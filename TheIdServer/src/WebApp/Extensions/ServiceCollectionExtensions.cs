using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureDataProtection(this IServiceCollection services, IConfiguration configuration)
        {
            var dataprotectionSection = configuration.GetSection(nameof(DataProtectionOptions));
            if (dataprotectionSection != null)
            {
                services.AddDataProtection(options => dataprotectionSection.Bind(options)).ConfigureDataProtection(configuration.GetSection(nameof(DataProtectionOptions)));
            }

            return services;
        }
    }
}
