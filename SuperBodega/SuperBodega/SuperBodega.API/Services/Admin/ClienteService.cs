using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Services.Admin;

public class ClienteService
{
    private readonly IClienteRepository _clienteRepository;
    
    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }
    
    public async Task<IEnumerable<ClienteDTO>> GetAllClientesAsync()
    {
        var clientes = await _clienteRepository.GetAllAsync();
        return clientes.Select(c => new ClienteDTO
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Apellido = c.Apellido,
            Email = c.Email,
            Telefono = c.Telefono,
            Direccion = c.Direccion,
            Estado = c.Estado,
            FechaDeRegistro = c.FechaDeRegistro
        });
    }
    
    public async Task<ClienteDTO> GetClienteByIdAsync(int id)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        if (cliente == null)
        {
            return null;
        }
        return new ClienteDTO
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Apellido = cliente.Apellido,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            Direccion = cliente.Direccion,
            Estado = cliente.Estado,
            FechaDeRegistro = cliente.FechaDeRegistro
        };
    }
    
    public async Task<ClienteDTO> CreateClienteAsync(CreateClienteDTO createClienteDto)
    {
        var cliente = new Cliente
        {
            Nombre = createClienteDto.Nombre,
            Apellido = createClienteDto.Apellido,
            Email = createClienteDto.Email,
            Telefono = createClienteDto.Telefono,
            Direccion = createClienteDto.Direccion,
            Estado = createClienteDto.Estado,
            FechaDeRegistro = createClienteDto.FechaDeRegistro
        };
        var newCliente = await _clienteRepository.AddAsync(cliente);
        return new ClienteDTO
        {
            Id = newCliente.Id,
            Nombre = newCliente.Nombre,
            Apellido = newCliente.Apellido,
            Email = newCliente.Email,
            Telefono = newCliente.Telefono,
            Direccion = newCliente.Direccion,
            Estado = newCliente.Estado,
            FechaDeRegistro = newCliente.FechaDeRegistro
        };
    }
    
    public async Task<ClienteDTO> UpdateClienteAsync(int id, UpdateClienteDTO updateClienteDto)
    {
        var cliente = await _clienteRepository.GetByIdAsync(id);
        if (cliente == null)
        {
            return null;
        }
        
        cliente.Nombre = updateClienteDto.Nombre;
        cliente.Apellido = updateClienteDto.Apellido;
        cliente.Email = updateClienteDto.Email;
        cliente.Telefono = updateClienteDto.Telefono;
        cliente.Direccion = updateClienteDto.Direccion;
        cliente.Estado = updateClienteDto.Estado;

        var updatedCliente = await _clienteRepository.UpdateAsync(cliente);
        return new ClienteDTO
        {
            Id = updatedCliente.Id,
            Nombre = updatedCliente.Nombre,
            Apellido = updatedCliente.Apellido,
            Email = updatedCliente.Email,
            Telefono = updatedCliente.Telefono,
            Direccion = updatedCliente.Direccion,
            Estado = updatedCliente.Estado,
            FechaDeRegistro = updatedCliente.FechaDeRegistro
        };
    }
    
    public async Task<bool> DeleteClienteAsync(int id)
    {
        return await _clienteRepository.DeleteAsync(id);
    }
}