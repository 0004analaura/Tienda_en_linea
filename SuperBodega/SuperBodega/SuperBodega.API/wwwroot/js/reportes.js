
document.addEventListener('DOMContentLoaded', () => {

    document.getElementById('btnFiltrar')
        .addEventListener('click', loadReportes);


    document.querySelectorAll('#reporteTabs button')
        .forEach(b => b.addEventListener('shown.bs.tab', loadReportes));

    loadReportes();
});

/* --- helpers ------------------------------------------------------------- */
function buildUrl(base, inicio, fin) {
    const params = new URLSearchParams();
    if (inicio) params.append('inicio', inicio);
    if (fin) params.append('fin', fin);
    return params.toString() ? `${base}?${params.toString()}` : base;
}

/* almacena instancias en un diccionario para no sobrescribir variables globales */
const charts = {};

function drawChart(canvasId, labels, valores, label) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    if (charts[canvasId]) charts[canvasId].destroy();

    charts[canvasId] = new Chart(canvas, {
        type: 'bar',
        data: {
            labels,
            datasets: [{ label, data: valores }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true
        }
    });

    setTimeout(() => {
        charts[canvasId].resize();
        charts[canvasId].update();
    }, 50);
}
function renderPeriodo(data) {
    const tbody = document.querySelector('#tablaPeriodo tbody');
    tbody.innerHTML = '';

    const labels = [];
    const valores = [];

    data.forEach(r => {
        const fecha = r.fecha.split('T')[0];
        labels.push(fecha);
        valores.push(r.montoTotal);

        tbody.innerHTML += `
            <tr>
                <td>${fecha}</td>
                <td class="text-end">${r.totalTransacciones}</td>
                <td class="text-end">${r.montoTotal.toFixed(2)}</td>
            </tr>`;
    });

    drawChart('chartPeriodo', labels, valores, 'Monto (Q)');
}

function renderProducto(data) {
    const tbody = document.querySelector('#tablaProducto tbody');
    tbody.innerHTML = '';

    const labels = [];
    const valores = [];

    data.forEach(r => {
        labels.push(r.producto);
        valores.push(r.montoTotal);

        tbody.innerHTML += `
            <tr>
                <td>${r.producto}</td>
                <td class="text-end">${r.cantidadVendida}</td>
                <td class="text-end">${r.montoTotal.toFixed(2)}</td>
            </tr>`;
    });

    drawChart('chartProducto', labels, valores, 'Monto (Q)');
}

function renderCliente(data) {
    const tbody = document.querySelector('#tablaCliente tbody');
    tbody.innerHTML = '';

    const labels = [];
    const valores = [];

    data.forEach(r => {
        labels.push(r.cliente);
        valores.push(r.montoTotal);

        tbody.innerHTML += `
            <tr>
                <td>${r.cliente}</td>
                <td class="text-end">${r.totalTransacciones}</td>
                <td class="text-end">${r.montoTotal.toFixed(2)}</td>
            </tr>`;
    });

    drawChart('chartCliente', labels, valores, 'Monto (Q)');
}

function renderProveedor(data) {
    const tbody = document.querySelector('#tablaProveedor tbody');
    tbody.innerHTML = '';

    const labels = [];
    const valores = [];

    data.forEach(r => {
        labels.push(r.proveedor);
        valores.push(r.montoTotal);

        tbody.innerHTML += `
            <tr>
                <td>${r.proveedor}</td>
                <td class="text-end">${r.cantidadProductosVendidos}</td>
                <td class="text-end">${r.montoTotal.toFixed(2)}</td>
            </tr>`;
    });

    drawChart('chartProveedor', labels, valores, 'Monto (Q)');
}

/* --- carga principal ----------------------------------------------------- */
async function loadReportes() {
    const inicio = document.getElementById('inicio').value;
    const fin = document.getElementById('fin').value;
    const active = document
        .querySelector('#reporteTabs .nav-link.active')
        .dataset.bsTarget;

    let endpoint = '';
    switch (active) {
        case '#tabPeriodo':
            endpoint = buildUrl('api/Reporte/PorPeriodo', inicio, fin);
            break;
        case '#tabProducto':
            endpoint = buildUrl('api/Reporte/PorProducto', inicio, fin);
            break;
        case '#tabCliente':
            endpoint = buildUrl('api/Reporte/PorCliente', inicio, fin);
            break;
        case '#tabProveedor':
            endpoint = buildUrl('api/Reporte/PorProveedor', inicio, fin);
            break;
    }

    const res = await fetch('/' + endpoint);
    const data = await res.json();

    if (active === '#tabPeriodo') renderPeriodo(data);
    if (active === '#tabProducto') renderProducto(data);
    if (active === '#tabCliente') renderCliente(data);
    if (active === '#tabProveedor') renderProveedor(data);
}
