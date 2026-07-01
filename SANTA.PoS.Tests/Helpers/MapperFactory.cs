using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using SANTA.PoS.Business.Mappings;

namespace SANTA.PoS.Tests.Helpers;

// AutoMapper 16 requires an ILoggerFactory — use NullLoggerFactory for tests.
internal static class MapperFactory
{
    public static IMapper Create() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance)
            .CreateMapper();
}
