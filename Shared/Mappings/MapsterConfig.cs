using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Mappings;



public static class MapsterConfig
{
    public static TypeAdapterConfig RegisterMappings(Assembly? assemblyToScan = null)
    {
        var config = new TypeAdapterConfig();

        // Scan registers (IRegister) from assembly
        assemblyToScan ??= Assembly.GetExecutingAssembly();
        config.Scan(assemblyToScan);

        // Optional (future-proof): avoid overriding destination with nulls on update mappings
        // config.Default.IgnoreNullValues(true);

        // Optional: preserve references if you have cycles (usually not needed)
        // config.Default.PreserveReference(true);

        return config;
    }
}
