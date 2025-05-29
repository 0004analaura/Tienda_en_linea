using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Services.Admin;

public class ProveedorService
{
    private readonly IProveedorRepository _proveedorRepository;
    
    public ProveedorService(IProveedorRepository proveedorRepository)
    {
        _proveedorRepository = proveedorRepository;
    }
    
    public async Task<IEnumerable<ProveedorDTO>> GetAllProveedoresAsync()
    {
        var proveedores = await _proveedorRepository.GetAllAsync();
        return proveedores.Select(p => new ProveedorDTO
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Email = p.Email,
            Telefono = p.Telefono,
            Direccion = p.Direccion,
            Estado = p.Estado,
            FechaDeRegistro = p.FechaDeRegistro
        });
    }
    
    public async Task<ProveedorDTO> GetProveedorByIdAsync(int id)
    {
        var proveedor = await _proveedorRepository.GetByIdAsync(id);
        if (proveedor == null)
        {
            return null;
        }
        return new ProveedorDTO
        {
            Id = proveedor.Id,
            Nombre = proveedor.Nombre,
            Email = proveedor.Email,
            Telefono = proveedor.Telefono,
            Direccion = proveedor.Direccion,
            Estado = proveedor.Estado,
            FechaDeRegistro = proveedor.FechaDeRegistro
        };
    }
    
    public async Task<ProveedorDTO> CreateProveedorAsync(CreateProveedorDTO proveedorDto)
    {
        var proveedor = new Proveedor
        {
            Nombre = proveedorDto.Nombre,
            Email = proveedorDto.Email,
            Telefono = proveedorDto.Telefono,
            Direccion = proveedorDto.Direccion,
            Estado = proveedorDto.Estado,
            FechaDeRegistro = proveedorDto.FechaDeRegistro
        };
        var newProveedor = await _proveedorRepository.AddAsync(proveedor);
        return new ProveedorDTO
        {
            Id = newProveedor.Id,
            Nombre = newProveedor.Nombre,
            Email = newProveedor.Email,
            Telefono = newProveedor.Telefono,
            Direccion = newProveedor.Direccion,
            Estado = newProveedor.Estado,
            FechaDeRegistro = newProveedor.FechaDeRegistro
        };
    }
    
    public async Task<ProveedorDTO> UpdateProveedorAsync(int id, UpdateProveedorDTO proveedorDto)
    {
        var proveedor = await _proveedorRepository.GetByIdAsync(id);
        if (proveedor == null)
        {
            return null;
        }
        
        proveedor.Nombre = proveedorDto.Nombre;
        proveedor.Email = proveedorDto.Email;
        proveedor.Telefono = proveedorDto.Telefono;
        proveedor.Direccion = proveedorDto.Direccion;
        proveedor.Estado = proveedorDto.Estado;

        var updatedProveedor = await _proveedorRepository.UpdateAsync(proveedor);
        return new ProveedorDTO
        {
            Id = updatedProveedor.Id,
            Nombre = updatedProveedor.Nombre,
            Email = updatedProveedor.Email,
            Telefono = updatedProveedor.Telefono,
            Direccion = updatedProveedor.Direccion,
            Estado = updatedProveedor.Estado,
            FechaDeRegistro = updatedProveedor.FechaDeRegistro
        };
    }
    
    public async Task<bool> DeleteProveedorAsync(int id)
    {
        return await _proveedorRepository.DeleteAsync(id);
    }
    
}