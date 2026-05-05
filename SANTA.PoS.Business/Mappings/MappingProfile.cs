using AutoMapper;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Business.Mappings;

/// <summary>
/// AutoMapper profile for entity to DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entity to DTO mappings
        CreateMap<Descuento, DescuentoDto>();
        CreateMap<Producto, ProductDto>();
        CreateMap<CreateUpdateProductDto, Producto>();
    }
}
