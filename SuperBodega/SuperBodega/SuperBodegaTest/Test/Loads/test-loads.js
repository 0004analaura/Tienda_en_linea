import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Métricas personalizadas para comparar síncrono vs asíncrono
const errorRate = new Rate('errors');
const checkoutSyncDuration = new Trend('checkout_sync_duration');
const checkoutAsyncDuration = new Trend('checkout_async_duration');
const estadoUpdateSyncDuration = new Trend('estado_update_sync_duration');
const estadoUpdateAsyncDuration = new Trend('estado_update_async_duration');

// Configuración de la prueba - 20 segundos
export const options = {
    stages: [
        { duration: '5s', target: 5 },   // Ramp up gradual
        { duration: '10s', target: 30 },  // Mantener carga moderada
        { duration: '5s', target: 0 },    // Ramp down
    ],
    thresholds: {
        http_req_duration: ['p(95)<3000'], // 95% de requests < 3s
        http_req_failed: ['rate<0.2'],     // Error rate < 20%
        errors: ['rate<0.1'],             // Custom error rate < 10%
        checkout_sync_duration: ['p(95)<8000'],
        checkout_async_duration: ['p(95)<3000'],
        estado_update_sync_duration: ['p(95)<3000'],
        estado_update_async_duration: ['p(95)<2000'],
    },
};

// URL base de la API
const BASE_URL = __ENV.API || 'http://localhost:8080';

// Headers comunes
const headers = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
};

// Datos globales
let clientes = [];
let productos = [];
let ventasCreadas = [];

// Setup - Cargar datos iniciales
export function setup() {
    console.log('Iniciando setup de datos de prueba para SuperBodega...');
    
    // Verificar que la API esté disponible
    const healthCheck = http.get(`${BASE_URL}/health`, { timeout: '10s' });
    if (healthCheck.status !== 200) {
        console.error('API no está disponible');
        return { clientes: [], productos: [] };
    }
    
    console.log('API disponible');

    // Cargar clientes existentes usando tu ClienteController
    try {
        const clientesResponse = http.get(`${BASE_URL}/api/Cliente`, { timeout: '10s' });
        if (clientesResponse.status === 200) {
            clientes = JSON.parse(clientesResponse.body);
            console.log(`Clientes cargados: ${clientes.length}`);
        } else {
            console.log(`No se pudieron cargar clientes (status: ${clientesResponse.status})`);
            clientes = [];
        }
    } catch (error) {
        console.log(`Error al cargar clientes: ${error.message}`);
        clientes = [];
    }

    // Cargar productos existentes usando tu ProductoController
    try {
        const productosResponse = http.get(`${BASE_URL}/api/Producto/GetAll`, { timeout: '10s' });
        if (productosResponse.status === 200) {
            const productosData = JSON.parse(productosResponse.body);
            // Filtrar solo productos con stock > 0
            productos = productosData.filter(p => p.stock > 0);
            console.log(`Productos con stock cargados: ${productos.length}`);
        } else {
            console.log(`No se pudieron cargar productos (status: ${productosResponse.status})`);
            productos = [];
        }
    } catch (error) {
        console.log(`Error al cargar productos: ${error.message}`);
        productos = [];
    }

    if (clientes.length === 0 || productos.length === 0) {
        console.log('Datos insuficientes para ejecutar pruebas');
        console.log(`Clientes: ${clientes.length}, Productos: ${productos.length}`);
    }

    return { clientes, productos };
}

// Función principal de la prueba
export default function(data) {
    const { clientes, productos } = data;
    
    if (!clientes.length || !productos.length) {
        console.log('No hay datos suficientes para ejecutar pruebas');
        return;
    }

    const clienteRandom = clientes[Math.floor(Math.random() * clientes.length)];
    const productosSeleccionados = seleccionarProductosRandom(productos, Math.floor(Math.random() * 3) + 1);

    // 60% checkout, 40% actualización de estado
    if (Math.random() < 0.6) {
        // Prueba de creación de venta (checkout)
        if (Math.random() < 0.5) {
            testCheckoutSincrono(clienteRandom, productosSeleccionados);
        } else {
            testCheckoutAsincrono(clienteRandom, productosSeleccionados);
        }
    } else {
        // Prueba de actualización de estado (solo si hay ventas)
        if (ventasCreadas.length > 0) {
            const ventaRandom = ventasCreadas[Math.floor(Math.random() * ventasCreadas.length)];
            if (Math.random() < 0.5) {
                testActualizarEstadoSincrono(ventaRandom);
            } else {
                testActualizarEstadoAsincrono(ventaRandom);
            }
        } else {
            // Si no hay ventas, crear una
            testCheckoutSincrono(clienteRandom, productosSeleccionados);
        }
    }

    sleep(Math.random() * 2 + 0.5); // Sleep aleatorio entre 0.5-2.5 segundos
}

// ============================================================================
// PRUEBAS DE CHECKOUT (CREACIÓN DE VENTAS)
// ============================================================================

function testCheckoutSincrono(cliente, productos) {
    console.log(`[SYNC] Checkout para cliente ${cliente.id} - ${cliente.nombre}`);
    
    const startTime = Date.now();
    
    try {
        // 1. Limpiar carrito
        const limpiarResponse = http.del(`${BASE_URL}/api/Carrito/vaciar/${cliente.id}`, null, { headers });
        
        check(limpiarResponse, {
            '[SYNC] Carrito limpiado': (r) => r.status === 204 || r.status === 200,
        });

        // Esperar un poco entre operaciones
        sleep(0.5);

        // 2. Agregar productos secuencialmente con más espacio
        for (const producto of productos) {
            const cantidad = Math.floor(Math.random() * 2) + 1; // Máximo 2 productos
            const agregarPayload = {
                clienteId: cliente.id,
                productoId: producto.id,
                cantidad: cantidad
            };

            const agregarResponse = http.post(
                `${BASE_URL}/api/Carrito/agregar`, 
                JSON.stringify(agregarPayload),
                { headers }
            );

            const agregarOk = check(agregarResponse, {
                '[SYNC] Producto agregado': (r) => r.status === 200,
            });

            if (!agregarOk) {
                console.log(`[SYNC] Error agregando producto ${producto.id} (status: ${agregarResponse.status})`);
                if (agregarResponse.body) {
                    console.log(`[SYNC] Error detail: ${agregarResponse.body.substring(0, 200)}`);
                }
                errorRate.add(1);
                return;
            }

            // Más espacio entre productos
            sleep(0.3);
        }

        // 3. Verificar carrito con timeout mayor
        sleep(0.5);
        const carritoResponse = http.get(`${BASE_URL}/api/Carrito/cliente/${cliente.id}`, { 
            headers,
            timeout: '10s'
        });
        
        const carritoOk = check(carritoResponse, {
            '[SYNC] Carrito obtenido': (r) => r.status === 200,
        });

        if (!carritoOk) {
            console.log(`[SYNC] Error obteniendo carrito (status: ${carritoResponse.status})`);
            errorRate.add(1);
            return;
        }

        const carrito = JSON.parse(carritoResponse.body);
        
        if (!carrito.items || carrito.items.length === 0) {
            console.log(`[SYNC] Carrito vacío después de agregar productos`);
            errorRate.add(1);
            return;
        }

        // 4. Realizar checkout con timeout mayor
        sleep(1); // Esperar antes del checkout
        
        const checkoutResponse = http.post(
            `${BASE_URL}/api/RealizarCompra/confirmar/${cliente.id}`,
            JSON.stringify({
                notasAdicionales: `K6 Test SYNC - ${new Date().toISOString()}`
            }),
            { 
                headers,
                timeout: '15s' // Timeout mayor para transacciones complejas
            }
        );

        const checkoutOk = check(checkoutResponse, {
            '[SYNC] Checkout realizado': (r) => r.status === 200,
            '[SYNC] Tiempo razonable': (r) => r.timings.duration < 10000,
        });

        const duration = Date.now() - startTime;
        checkoutSyncDuration.add(duration);

        if (checkoutOk) {
            console.log(`[SYNC] Checkout completado en ${duration}ms`);
            
            // Extraer ID de venta de la respuesta
            try {
                const ventaData = JSON.parse(checkoutResponse.body);
                if (ventaData.ventaId) {
                    const ventaId = ventaData.ventaId;
                    ventasCreadas.push({ 
                        id: ventaId, 
                        estado: 1 // EstadoVenta.Recibido
                    });
                    console.log(`[SYNC] Venta ${ventaId} agregada para pruebas de estado`);
                }
            } catch (e) {
                console.log(`[SYNC] No se pudo extraer ID de venta: ${e.message}`);
            }
        } else {
            console.log(`[SYNC] Checkout falló (status: ${checkoutResponse.status})`);
            if (checkoutResponse.body) {
                console.log(`[SYNC] Error detail: ${checkoutResponse.body.substring(0, 200)}`);
            }
            errorRate.add(1);
        }

    } catch (error) {
        console.log(`[SYNC] Excepción en checkout: ${error.message}`);
        errorRate.add(1);
    }
}

function testCheckoutAsincrono(cliente, productos) {
    console.log(`[ASYNC] Checkout para cliente ${cliente.id} - ${cliente.nombre}`);
    
    const startTime = Date.now();
    
    try {
        // 1. Limpiar carrito
        const limpiarResponse = http.del(`${BASE_URL}/api/Carrito/vaciar/${cliente.id}`, null, { headers });
        
        check(limpiarResponse, {
            '[ASYNC] Carrito limpiado': (r) => r.status === 204 || r.status === 200,
        });

        // 2. Agregar productos en paralelo (modo asíncrono)
        const requests = productos.map(producto => {
            const cantidad = Math.floor(Math.random() * 3) + 1;
            return {
                method: 'POST',
                url: `${BASE_URL}/api/Carrito/agregar`,
                body: JSON.stringify({
                    clienteId: cliente.id,
                    productoId: producto.id,
                    cantidad: cantidad
                }),
                params: { headers }
            };
        });

        // Ejecutar todas las peticiones en paralelo
        const responses = http.batch(requests);
        
        const agregarOk = check(responses, {
            '[ASYNC] Todos los productos agregados': (responses) => 
                responses.every(r => r.status === 200),
        });

        if (!agregarOk) {
            console.log(`[ASYNC] Error agregando productos en paralelo`);
            errorRate.add(1);
            return;
        }

        // 3. Verificar carrito
        const carritoResponse = http.get(`${BASE_URL}/api/Carrito/cliente/${cliente.id}`, { headers });
        if (carritoResponse.status !== 200) {
            console.log(`[ASYNC] Error obteniendo carrito`);
            errorRate.add(1);
            return;
        }

        const carrito = JSON.parse(carritoResponse.body);
        
        if (!carrito.items || carrito.items.length === 0) {
            console.log(`[ASYNC] Carrito vacío`);
            errorRate.add(1);
            return;
        }

        // 4. Checkout usando Kafka (asíncrono)
        // Si tienes el endpoint v2/CarritoV/checkout, usarlo
        const checkoutPayload = {
            clienteId: cliente.id,
            items: carrito.items.map(item => ({
                productoId: item.productoId,
                cantidad: item.cantidad
            })),
            notasAdicionales: `Prueba K6 ASYNC - ${new Date().toISOString()}`
        };

        const checkoutResponse = http.post(
            `${BASE_URL}/api/v2/CarritoV/checkout`,
            JSON.stringify(checkoutPayload),
            { headers }
        );

        const checkoutOk = check(checkoutResponse, {
            '[ASYNC] Checkout enviado a Kafka': (r) => r.status === 202,
            '[ASYNC] Respuesta rápida': (r) => r.timings.duration < 2000,
        });

        const duration = Date.now() - startTime;
        checkoutAsyncDuration.add(duration);

        if (checkoutOk) {
            console.log(`[ASYNC] Checkout enviado a Kafka en ${duration}ms`);
            // Simular venta creada para pruebas de estado
            ventasCreadas.push({ 
                id: Math.floor(Math.random() * 10000) + 1000, 
                estado: 1 
            });
        } else {
            console.log(`[ASYNC] Checkout falló (status: ${checkoutResponse.status})`);
            errorRate.add(1);
        }

    } catch (error) {
        console.log(`[ASYNC] Excepción en checkout: ${error.message}`);
        errorRate.add(1);
    }
}

// ============================================================================
// PRUEBAS DE ACTUALIZACIÓN DE ESTADO
// ============================================================================

function testActualizarEstadoSincrono(venta) {
    console.log(`[SYNC] Actualizando estado venta ${venta.id}`);
    
    const startTime = Date.now();
    
    try {
        // Determinar siguiente estado válido según tu enum EstadoVenta
        const siguienteEstado = obtenerSiguienteEstado(venta.estado);
        if (!siguienteEstado) {
            console.log(`[SYNC] Venta ${venta.id} ya está en estado final`);
            return;
        }

        // Usar tu VentasEstadoController v2.0
        const updateResponse = http.put(
            `${BASE_URL}/api/v2/ventas/${venta.id}/estado/${siguienteEstado}`,
            null,
            { headers }
        );

        const updateOk = check(updateResponse, {
            '[SYNC] Estado actualizado': (r) => r.status === 204,
            '[SYNC] Tiempo razonable': (r) => r.timings.duration < 3000,
        });

        const duration = Date.now() - startTime;
        estadoUpdateSyncDuration.add(duration);

        if (updateOk) {
            console.log(`[SYNC] Estado actualizado en ${duration}ms`);
            venta.estado = siguienteEstado;
        } else {
            console.log(`[SYNC] Error actualizando estado (status: ${updateResponse.status})`);
            if (updateResponse.status === 400) {
                const errorText = updateResponse.body;
                console.log(`[SYNC] Detalle del error: ${errorText}`);
            }
            errorRate.add(1);
        }

    } catch (error) {
        console.log(`[SYNC] Excepción actualizando estado: ${error.message}`);
        errorRate.add(1);
    }
}

function testActualizarEstadoAsincrono(venta) {
    console.log(`[ASYNC] Actualizando estado venta ${venta.id}`);
    
    const startTime = Date.now();
    
    try {
        const siguienteEstado = obtenerSiguienteEstado(venta.estado);
        if (!siguienteEstado) {
            console.log(`[ASYNC] Venta ${venta.id} ya está en estado final`);
            return;
        }

        // Hacer verificaciones en paralelo (modo asíncrono)
        const requests = [
            {
                method: 'GET',
                url: `${BASE_URL}/api/v2/ventas/${venta.id}/estados-permitidos`,
                params: { headers }
            },
            {
                method: 'PUT',
                url: `${BASE_URL}/api/v2/ventas/${venta.id}/estado/${siguienteEstado}`,
                params: { headers }
            }
        ];

        const responses = http.batch(requests);
        
        const updateOk = check(responses, {
            '[ASYNC] Estados permitidos obtenidos': (responses) => 
                responses[0].status === 200,
            '[ASYNC] Estado actualizado': (responses) => 
                responses[1].status === 204,
            '[ASYNC] Respuesta rápida': (responses) => 
                responses.every(r => r.timings.duration < 2000),
        });

        const duration = Date.now() - startTime;
        estadoUpdateAsyncDuration.add(duration);

        if (updateOk) {
            console.log(`[ASYNC] Estado actualizado en ${duration}ms`);
            venta.estado = siguienteEstado;
        } else {
            console.log(`[ASYNC] Error en actualización paralela`);
            errorRate.add(1);
        }

    } catch (error) {
        console.log(`[ASYNC] Excepción en actualización asíncrona: ${error.message}`);
        errorRate.add(1);
    }
}

// ============================================================================
// FUNCIONES AUXILIARES
// ============================================================================

function seleccionarProductosRandom(productos, cantidad) {
    const productosDisponibles = productos.filter(p => p.stock && p.stock > 0);
    const seleccionados = [];
    
    for (let i = 0; i < Math.min(cantidad, productosDisponibles.length); i++) {
        const randomIndex = Math.floor(Math.random() * productosDisponibles.length);
        const producto = productosDisponibles.splice(randomIndex, 1)[0];
        seleccionados.push(producto);
    }
    
    return seleccionados;
}

function obtenerSiguienteEstado(estadoActual) {
    // Según tu enum EstadoVenta: Recibido=1, Despachado=2, Entregado=3
    switch (estadoActual) {
        case 1: return 2; // Recibido -> Despachado
        case 2: return 3; // Despachado -> Entregado
        case 3: return null; // Entregado (final)
        default: return null;
    }
}

// ============================================================================
// ESCENARIOS ESPECÍFICOS
// ============================================================================

export const checkoutOnly = {
    executor: 'constant-arrival-rate',
    rate: 20, // 20 checkouts por segundo
    timeUnit: '1s',
    duration: '20s',
    preAllocatedVUs: 30,
    maxVUs: 50,
    exec: 'testCheckoutScenario',
};

export const estadoOnly = {
    executor: 'constant-arrival-rate',
    rate: 30, // 30 actualizaciones por segundo
    timeUnit: '1s',
    duration: '20s',
    preAllocatedVUs: 20,
    maxVUs: 40,
    exec: 'testEstadoScenario',
};

export function testCheckoutScenario(data) {
    const { clientes, productos } = data;
    if (!clientes.length || !productos.length) return;
    
    const clienteRandom = clientes[Math.floor(Math.random() * clientes.length)];
    const productosSeleccionados = seleccionarProductosRandom(productos, 2);
    
    if (Math.random() < 0.5) {
        testCheckoutSincrono(clienteRandom, productosSeleccionados);
    } else {
        testCheckoutAsincrono(clienteRandom, productosSeleccionados);
    }
}

export function testEstadoScenario() {
    if (ventasCreadas.length === 0) {
        console.log('No hay ventas para actualizar estado');
        return;
    }
    
    const ventaRandom = ventasCreadas[Math.floor(Math.random() * ventasCreadas.length)];
    
    if (Math.random() < 0.5) {
        testActualizarEstadoSincrono(ventaRandom);
    } else {
        testActualizarEstadoAsincrono(ventaRandom);
    }
}

// ============================================================================
// REPORTE FINAL
// ============================================================================

export function teardown(data) {
    console.log('\n===== REPORTE FINAL SuperBodega =====');
    console.log(`Pruebas ejecutadas durante 20 segundos`);
    console.log(`Clientes utilizados: ${data.clientes.length}`);
    console.log(`Productos disponibles: ${data.productos.length}`);
    console.log(`Ventas procesadas: ${ventasCreadas.length}`);
    console.log('\nEndpoints utilizados:');
    console.log('   • GET  /api/Cliente - Cargar clientes');
    console.log('   • GET  /api/Producto - Cargar productos');
    console.log('   • POST /api/Carrito/agregar - Agregar al carrito');
    console.log('   • GET  /api/Carrito/cliente/{id} - Obtener carrito');
    console.log('   • DEL  /api/Carrito/vaciar/{id} - Limpiar carrito');
    console.log('   • POST /api/RealizarCompra/confirmar/{id} - Checkout síncrono');
    console.log('   • POST /api/v2/CarritoV/checkout - Checkout asíncrono (Kafka)');
    console.log('   • PUT  /api/v2/ventas/{id}/estado/{estado} - Actualizar estado');
    console.log('   • GET  /api/v2/ventas/{id}/estados-permitidos - Estados válidos');
    console.log('\nMétricas clave a revisar:');
    console.log('   • checkout_sync_duration vs checkout_async_duration');
    console.log('   • estado_update_sync_duration vs estado_update_async_duration');
    console.log('   • http_req_duration para rendimiento general');
    console.log('   • errors para tasa de errores');
    console.log('\nResultados esperados:');
    console.log('   • ASYNC checkout más rápido (Kafka)');
    console.log('   • SYNC checkout más confiable');
    console.log('   • ASYNC estado updates en paralelo');
    console.log('   • SYNC estado updates con mejor consistencia');
}

/*
INSTRUCCIONES DE USO:

1. Ejecutar prueba completa (20 segundos):
   k6 run --env API=http://localhost:8080 test-loads.js

2. Solo pruebas de checkout:
   k6 run --env API=http://localhost:8080 --scenario checkoutOnly test-loads.js

3. Solo pruebas de actualización de estado:
   k6 run --env API=http://localhost:8080 --scenario estadoOnly test-loads.js

4. Con Docker Compose:
   docker-compose --profile load run k6 run /scripts/test-loads.js

5. Con API en contenedor:
   k6 run --env API=http://api:8080 test-loads.js

MÉTRICAS ESPECÍFICAS PARA EL SISTEMA:
• checkout_sync_duration: Tiempo usando CarritoService.RealizarCompraAsync
• checkout_async_duration: Tiempo usando Kafka con KafkaConsumerHostedService  
• estado_update_sync_duration: Tiempo usando VentasEstadoController directo
• estado_update_async_duration: Tiempo con validaciones en paralelo
• errors: Errores específicos de tus controladores

ARQUITECTURA:
• Síncrono: CarritoController → CarritoService → Base de datos → EmailService
• Asíncrono: CarritoController → Kafka → KafkaConsumerHostedService → Base de datos → EmailService
• Estados: VentasEstadoController → VentaEstadoService → Base de datos → EmailService

CONTROLADORES UTILIZADOS:
• ClienteController (/api/Cliente)
• ProductoController (/api/Producto)  
• CarritoController (/api/Carrito)
• VentasEstadoController (/api/v2/ventas)
• RealizarCompraController (endpoint de checkout)
*/