using AutoMapper;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Business.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Producto
        CreateMap<Descuento, DescuentoDto>()
            .ForMember(dest => dest.IdProducto, opt => opt.MapFrom(src => src.Producto.IdProducto));
        CreateMap<Producto, ProductDto>();
        CreateMap<CreateUpdateProductDto, Producto>();

        // Venta
        CreateMap<Venta, VentaDto>()
            .ForMember(dest => dest.RegistroVentas, opt => opt.MapFrom(src => src.RegistroVentas));
        CreateMap<CreateVentaDto, Venta>();
        CreateMap<UpdateVentaDto, Venta>();
        CreateMap<RegistroVenta, RegistroVentaDto>()
            .ForMember(dest => dest.IdProducto, opt => opt.MapFrom(src => src.Producto.IdProducto));
        CreateMap<CreateRegistroVentaDto, RegistroVenta>();
        CreateMap<UpdateRegistroVentaDto, RegistroVenta>();

        // Categoria
        CreateMap<Categoria, CategoriaDto>();

        // Usuario
        CreateMap<Usuario, UsuarioDto>();

        // Cliente
        CreateMap<Cliente, ClienteDto>();
        CreateMap<CreateClienteDto, Cliente>();

        // Nota de Crédito
        CreateMap<NotasCredito, NotaCreditoDto>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());
        CreateMap<RegistroNotasCredito, RegistroNotaCreditoDto>()
            .ForMember(dest => dest.IdProducto, opt => opt.MapFrom(src => src.Producto.IdProducto));
        CreateMap<PagosNotasCredito, PagoNotaCreditoDto>();

        // Venta del Día
        CreateMap<VentaDia, VentaDiaDto>();

        // Notificación
        CreateMap<Notificacione, NotificacionDto>();

        // Requisición
        CreateMap<RequisicionItem, RequisicionItemDto>()
            .ForMember(dest => dest.IdProducto, opt => opt.MapFrom(src => src.Producto.IdProducto))
            .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto.Nombre));
    }
}
