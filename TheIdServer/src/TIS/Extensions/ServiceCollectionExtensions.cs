using Aguacongas.IdentityServer;
using Aguacongas.IdentityServer.Admin.Http.Store;
using Aguacongas.IdentityServer.Store;
using Aguacongas.TheIdServer.BlazorApp.Infrastructure.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TIS.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPrerendeingServices(this IServiceCollection services)
        {
            services.AddRemoteAuthentication<RemoteAuthenticationState, RemoteUserAccount, OidcProviderOptions>();
            return services.AddScoped<LazyAssemblyLoader>()
                 .AddScoped<AuthenticationStateProvider, RemoteAuthenticationService>()
                 .AddScoped<SignOutSessionStateManager>()
                 .AddScoped<ISharedStringLocalizerAsync, PreRenderStringLocalizer>()
                 .AddTransient<IAccessTokenProvider, AccessTokenProvider>()
                 .AddTransient<JSInterop.IJSRuntime, JSRuntime>()
                 .AddTransient<IKeyStore<RsaEncryptorDescriptor>>(p => new KeyStore<RsaEncryptorDescriptor>(p.CreateApiHttpClient(p.GetRequiredService<IOptions<IdentityServerOptions>>().Value),
                         p.GetRequiredService<ILogger<KeyStore<RsaEncryptorDescriptor>>>()))
                 .AddTransient<IKeyStore<IAuthenticatedEncryptorDescriptor>>(p => new KeyStore<IAuthenticatedEncryptorDescriptor>(p.CreateApiHttpClient(p.GetRequiredService<IOptions<IdentityServerOptions>>().Value),
                         p.GetRequiredService<ILogger<KeyStore<IAuthenticatedEncryptorDescriptor>>>()));
        }

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
