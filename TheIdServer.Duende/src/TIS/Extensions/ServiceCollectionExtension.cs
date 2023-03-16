// Copyright (c) 2021 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Aguacongas.DynamicConfiguration.Redis;
using Aguacongas.IdentityServer;
using Aguacongas.IdentityServer.Abstractions;
using Aguacongas.IdentityServer.Admin.Http.Store;
using Aguacongas.IdentityServer.Admin.Options;
using Aguacongas.IdentityServer.Admin.Services;
using Aguacongas.IdentityServer.EntityFramework.Store;
using Aguacongas.IdentityServer.Services;
using Aguacongas.IdentityServer.Store;
using Aguacongas.TheIdServer.Authentication;
using Aguacongas.TheIdServer.BlazorApp.Infrastructure.Services;
using Aguacongas.TheIdServer.BlazorApp.Models;
using Aguacongas.TheIdServer.BlazorApp.Services;
using Aguacongas.TheIdServer.Data;
using Aguacongas.TheIdServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TIS;
using TIS.Models;
using TIS.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddTheIdServer(this IServiceCollection services, ConfigurationManager configurationManager)
        {
            var isProxy = configurationManager.GetValue<bool>("Proxy");

            void configureOptions(IdentityServerOptions options)
                => configurationManager.GetSection("PrivateServerAuthentication").Bind(options);

            services.AddTransient<ISchemeChangeSubscriber, SchemeChangeSubscriber<SchemeDefinition>>()
                .AddIdentityProviderStore()
                .AddConfigurationStores()
                .AddOperationalStores()
                .AddIdentity<ApplicationUser, IdentityRole>(
                    options => configurationManager.Bind(nameof(AspNetCore.Identity.IdentityOptions), options))
                .AddTheIdServerStores()
                .AddDefaultTokenProviders();

            var dbType = configurationManager.GetValue<DbTypes>("DbType");
            if (isProxy)
            {
                services.AddAdminHttpStores(configureOptions);
            }
            else
            {
                AddDefaultServices(services, dbType, configurationManager);
            }

            ConfigureDataProtection(services, configurationManager);

            var identityServerBuilder = services.AddClaimsProviders(configurationManager)
                .Configure<ForwardedHeadersOptions>(configurationManager.GetSection(nameof(ForwardedHeadersOptions)))
                .Configure<AccountOptions>(configurationManager.GetSection(nameof(AccountOptions)))
                .Configure<DynamicClientRegistrationOptions>(configurationManager.GetSection(nameof(DynamicClientRegistrationOptions)))
                .Configure<TokenValidationParameters>(configurationManager.GetSection(nameof(TokenValidationParameters)))
                .Configure<SiteOptions>(configurationManager.GetSection(nameof(SiteOptions)))
                .ConfigureNonBreakingSameSiteCookies()
                .AddOidcStateDataFormatterCache()
                .Configure<Duende.IdentityServer.Configuration.IdentityServerOptions>(configurationManager.GetSection(nameof(Duende.IdentityServer.Configuration.IdentityServerOptions)))
                .AddIdentityServerBuilder()
                .AddRequiredPlatformServices()
                .AddCookieAuthentication()
                .AddCoreServices()
                .AddDefaultEndpoints()
                .AddPluggableServices()
                .AddKeyManagement()
                .AddValidators()
                .AddResponseGenerators()
                .AddDefaultSecretParsers()
                .AddDefaultSecretValidators()
                .AddInMemoryPersistedGrants()
                .AddCiba(configurationManager.GetSection(nameof(BackchannelAuthenticationUserNotificationServiceOptions)))
                .AddAspNetIdentity<ApplicationUser>()
                .AddDynamicClientRegistration()
                .ConfigureKey(configurationManager.GetSection("IdentityServer:Key"));


            identityServerBuilder.AddJwtRequestUriHttpClient();

            if (isProxy)
            {
                identityServerBuilder.Services.AddTransient<IProfileService>(p =>
                {
                    var options = p.GetRequiredService<IOptions<IdentityServerOptions>>().Value;
                    var httpClient = p.GetRequiredService<IHttpClientFactory>().CreateClient(options.HttpClientName);
                    return new ProxyProfilService<ApplicationUser>(httpClient,
                        p.GetRequiredService<UserManager<ApplicationUser>>(),
                        p.GetRequiredService<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                        p.GetRequiredService<IEnumerable<IProvideClaims>>(),
                        p.GetRequiredService<ILogger<ProxyProfilService<ApplicationUser>>>());
                });
            }
            else
            {
                identityServerBuilder.AddProfileService<ProfileService<ApplicationUser>>();
                if (!configurationManager.GetValue<bool>("DisableTokenCleanup"))
                {
                    identityServerBuilder.AddTokenCleaner(configurationManager.GetValue<TimeSpan?>("TokenCleanupInterval") ?? TimeSpan.FromMinutes(1));
                }
            }

            services.AddTransient(p =>
            {
                var handler = new HttpClientHandler();
                if (configurationManager.GetValue<bool>("DisableStrictSsl"))
                {
#pragma warning disable S4830 // Server certificates should be verified during SSL/TLS connections
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, policy) => true;
#pragma warning restore S4830 // Server certificates should be verified during SSL/TLS connections
                }
                return handler;
            })
                .AddHttpClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName)
                .ConfigurePrimaryHttpMessageHandler(p => p.GetRequiredService<HttpClientHandler>());

            services.Configure<ExternalLoginOptions>(configurationManager.GetSection("Google"))
                .AddAuthorization(options =>
                    options.AddIdentityServerPolicies())
                .AddAuthentication()
                .AddJwtBearer("Bearer", options => ConfigureIdentityServerAuthenticationOptions(options, configurationManager))
                // reference tokens
                .AddOAuth2Introspection("introspection", options => ConfigureIdentityServerAuthenticationOptions(options, configurationManager));

            var mvcBuilder = services.Configure<SendGridOptions>(configurationManager)
                .AddLocalization()
                .AddControllersWithViews(options =>
                    options.AddIdentityServerAdminFilters())
                .AddViewLocalization()
                .AddDataAnnotationsLocalization()
                .AddNewtonsoftJson(options =>
                {
                    var settings = options.SerializerSettings;
                    settings.NullValueHandling = NullValueHandling.Ignore;
                    settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                .AddIdentityServerWsFederation();

            ConfigureDynamicProviderManager(mvcBuilder, isProxy, dbType);

            services.AddRemoteAuthentication<RemoteAuthenticationState, RemoteUserAccount, OidcProviderOptions>();
            services.AddScoped<LazyAssemblyLoader>()
                 .AddScoped<ThemeService>()
                 .AddScoped<AuthenticationStateProvider, RemoteAuthenticationService>()
                 .AddScoped<ISharedStringLocalizerAsync, Aguacongas.TheIdServer.BlazorApp.Infrastructure.Services.StringLocalizer>()
                 .AddTransient<IReadOnlyCultureStore, PreRenderCultureStore>()
                 .AddTransient<IReadOnlyLocalizedResourceStore, PreRenderLocalizedResourceStore>()
                 .AddTransient<IAccessTokenProvider, AccessTokenProvider>()
                 .AddTransient<JSInterop.IJSRuntime, JSRuntime>()
                 .AddTransient<IKeyStore<RsaEncryptorDescriptor>, KeyStore<RsaEncryptorDescriptor, Aguacongas.IdentityServer.KeysRotation.RsaEncryptorDescriptor>>()
                 .AddTransient<IKeyStore<IAuthenticatedEncryptorDescriptor>, KeyStore<IAuthenticatedEncryptorDescriptor, Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.IAuthenticatedEncryptorDescriptor>>()
                 .AddAdminApplication(new Settings())
                 .AddDatabaseDeveloperPageExceptionFilter()
                 .AddRazorPages(options => options.Conventions.AuthorizeAreaFolder("Identity", "/Account"));

            ConfigureHealthChecks(services, dbType, isProxy, configurationManager);

            return services;
        }

        private static void ConfigureDynamicProviderManager(IMvcBuilder mvcBuilder, bool isProxy, DbTypes dbType)
        {
            var dynamicBuilder = mvcBuilder.AddIdentityServerAdmin<ApplicationUser, SchemeDefinition>();
            if (isProxy)
            {
                dynamicBuilder.AddTheIdServerStore();
            }
            else if (dbType == DbTypes.MongoDb)
            {
                dynamicBuilder.AddTheIdServerEntityMongoDbStore();
            }
            else if (dbType == DbTypes.RavenDb)
            {
                dynamicBuilder.AddTheIdServerStoreRavenDbStore();
            }
            else
            {
                dynamicBuilder.AddTheIdServerEntityFrameworkStore();
            }
        }
        private static void AddForceHttpsSchemeMiddleware(IApplicationBuilder app, ConfigurationManager configurationManager)
        {
            var forceHttpsScheme = configurationManager.GetValue<bool>("ForceHttpsScheme");

            if (forceHttpsScheme)
            {
                app.Use((context, next) =>
                {
                    context.Request.Scheme = "https";
                    return next();
                });
            }
        }

        private static void ConfigureIdentityServerAuthenticationOptions(JwtBearerOptions options, ConfigurationManager configurationManager)
        {
            configurationManager.GetSection("ApiAuthentication").Bind(options);
            if (configurationManager.GetValue<bool>("DisableStrictSsl"))
            {
                options.BackchannelHttpHandler = new HttpClientHandler
                {
#pragma warning disable S4830 // Server certificates should be verified during SSL/TLS connections
                    ServerCertificateCustomValidationCallback = (message, cert, chain, policy) => true
#pragma warning restore S4830 // Server certificates should be verified during SSL/TLS connections
                };
            }
            options.Audience = configurationManager["ApiAuthentication:ApiName"];
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var request = context.HttpContext.Request;
                    var path = request.Path;
                    var accessToken = TokenRetrieval.FromQueryString()(request);
                    if (path.StartsWithSegments("/providerhub") && !string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                    var oneTimeToken = TokenRetrieval.FromQueryString("otk")(request);
                    if (!string.IsNullOrEmpty(oneTimeToken))
                    {
                        context.Token = request.HttpContext
                            .RequestServices
                            .GetRequiredService<IRetrieveOneTimeToken>()
                            .GetOneTimeToken(oneTimeToken);
                        return Task.CompletedTask;
                    }
                    context.Token = TokenRetrieval.FromAuthorizationHeader()(request);
                    return Task.CompletedTask;
                }
            };

            options.ForwardDefaultSelector = context =>
            {
                var authHeader = context.Request.Headers[HttpRequestHeader.Authorization.ToString()];
                if (string.IsNullOrEmpty(authHeader))
                {
                    return null;
                }

                var parts = authHeader.First().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    return null;
                }

                if (!parts[1].Contains("."))
                {
                    return "introspection";
                }

                return null;
            };
        }

        private static void ConfigureIdentityServerAuthenticationOptions(OAuth2IntrospectionOptions options, ConfigurationManager configurationManager)
        {
            configurationManager.GetSection("ApiAuthentication").Bind(options);
            options.ClientId = configurationManager.GetValue<string>("ApiAuthentication:ApiName");
            options.ClientSecret = configurationManager.GetValue<string>("ApiAuthentication:ApiSecret");
            static string tokenRetriever(HttpRequest request)
            {
                var path = request.Path;
                var accessToken = TokenRetrieval.FromQueryString()(request);
                if (path.StartsWithSegments("/providerhub") && !string.IsNullOrEmpty(accessToken))
                {
                    return accessToken;
                }
                var oneTimeToken = TokenRetrieval.FromQueryString("otk")(request);
                if (!string.IsNullOrEmpty(oneTimeToken))
                {
                    return request.HttpContext
                        .RequestServices
                        .GetRequiredService<IRetrieveOneTimeToken>()
                        .GetOneTimeToken(oneTimeToken);
                }
                return TokenRetrieval.FromAuthorizationHeader()(request);
            }

            options.TokenRetriever = tokenRetriever;
        }

        private static void AddDefaultServices(IServiceCollection services, DbTypes dbType, ConfigurationManager configurationManager)
        {
            services.Configure<IdentityServerOptions>(options => configurationManager.GetSection("ApiAuthentication").Bind(options))
                .AddIdentityProviderStore();

            if (dbType == DbTypes.RavenDb)
            {
                services.Configure<RavenDbOptions>(options => configurationManager.GetSection(nameof(RavenDbOptions)).Bind(options))
                    .AddSingleton(p =>
                    {
                        var options = p.GetRequiredService<IOptions<RavenDbOptions>>().Value;
                        var documentStore = new DocumentStore
                        {
                            Urls = options.Urls,
                            Database = options.Database,
                        };
                        if (!string.IsNullOrWhiteSpace(options.CertificatePath))
                        {
                            documentStore.Certificate = new X509Certificate2(options.CertificatePath, options.CertificatePassword);
                        }
                        documentStore.SetFindIdentityPropertyForIdentityServerStores();
                        return documentStore.Initialize();
                    })
                    .AddTheIdServerRavenDbStores();

            }
            else if (dbType == DbTypes.MongoDb)
            {
                var connectionString = configurationManager.GetConnectionString("DefaultConnection");
                services.AddTheIdServerMongoDbStores(connectionString);
            }
            else
            {
                services.AddTheIdServerAdminEntityFrameworkStores(options => options.UseDatabaseFromConfiguration(configurationManager))
                    .AddConfigurationEntityFrameworkStores(options => options.UseDatabaseFromConfiguration(configurationManager))
                    .AddOperationalEntityFrameworkStores(options => options.UseDatabaseFromConfiguration(configurationManager));
            }

            var signalRBuilder = services.AddSignalR(options => configurationManager.GetSection("SignalR:HubOptions").Bind(options));
            if (configurationManager.GetValue<bool>("SignalR:UseMessagePack"))
            {
                signalRBuilder.AddMessagePackProtocol();
            }

            var redisConnectionString = configurationManager.GetValue<string>("SignalR:RedisConnectionString");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                signalRBuilder.AddStackExchangeRedis(redisConnectionString, options => configurationManager.GetSection("SignalR:RedisOptions").Bind(options));
            }
        }

        private static void ConfigureInitialData(IApplicationBuilder app, ConfigurationManager configurationManager)
        {
            var dbType = configurationManager.GetValue<DbTypes>("DbType");
            if (configurationManager.GetValue<bool>("Migrate") &&
                dbType != DbTypes.InMemory && dbType != DbTypes.RavenDb && dbType != DbTypes.MongoDb)
            {
                using var scope = app.ApplicationServices.CreateScope();
                var configContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                configContext.Database.Migrate();

                var opContext = scope.ServiceProvider.GetRequiredService<OperationalDbContext>();
                opContext.Database.Migrate();

                var appcontext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                appcontext.Database.Migrate();
            }

            if (configurationManager.GetValue<bool>("Seed"))
            {
                using var scope = app.ApplicationServices.CreateScope();
                SeedData.SeedConfiguration(scope, configurationManager);
                SeedData.SeedUsers(scope, configurationManager);
            }

        }
        private static void ConfigureDataProtection(IServiceCollection services, ConfigurationManager configurationManager)
        {
            var dataprotectionSection = configurationManager.GetSection(nameof(DataProtectionOptions));
            if (dataprotectionSection != null)
            {
                services.AddDataProtection(options => dataprotectionSection.Bind(options)).ConfigureDataProtection(configurationManager.GetSection(nameof(DataProtectionOptions)));
            }
        }

        private static void ConfigureHealthChecks(IServiceCollection services, DbTypes dbTypes, bool isProxy, IConfiguration configuration)
        {
            var builder = services.AddHealthChecks();
            ConfigureDbHealthChecks(dbTypes, isProxy, configuration, builder);

            var dynamicConfigurationRedisConnectionString = configuration.GetValue<string>($"{nameof(RedisConfigurationOptions)}:{nameof(RedisConfigurationOptions.ConnectionString)}");
            var signalRRedisConnectionString = configuration.GetValue<string>("SignalR:RedisConnectionString");

            if (!string.IsNullOrEmpty(signalRRedisConnectionString))
            {
                builder.AddRedis(signalRRedisConnectionString, name: "signalRRedisConnectionString");
            }

            if (!string.IsNullOrEmpty(dynamicConfigurationRedisConnectionString))
            {
                builder.AddRedis(dynamicConfigurationRedisConnectionString, name: "dynamicConfigurationRedis");
            }
        }

        private static void ConfigureDbHealthChecks(DbTypes dbTypes, bool isProxy, IConfiguration configuration, IHealthChecksBuilder builder)
        {
            if (!isProxy)
            {
                var tags = new[] { "store" };
                switch (dbTypes)
                {
                    case DbTypes.MongoDb:
                        builder.AddMongoDb(configuration.GetConnectionString("DefaultConnection"), tags: tags);
                        break;
                    case DbTypes.RavenDb:
                        builder.AddRavenDB(options =>
                        {
                            var section = configuration.GetSection(nameof(RavenDbOptions));
                            section.Bind(options);
                            var path = section.GetValue<string>(nameof(RavenDbOptions.CertificatePath));
                            if (!string.IsNullOrWhiteSpace(path))
                            {
                                options.Certificate = new X509Certificate2(path, section.GetValue<string>(nameof(RavenDbOptions.CertificatePassword)));
                            }
                        }, tags: tags);
                        break;
                    default:
                        builder.AddDbContextCheck<ConfigurationDbContext>(tags: tags)
                            .AddDbContextCheck<OperationalDbContext>(tags: tags)
                            .AddDbContextCheck<ApplicationDbContext>(tags: tags);
                        break;
                }
                return;
            }

            builder.AddAsyncCheck("api", async () =>
            {
                using var client = new HttpClient();
                var reponse = await client.GetAsync(configuration.GetValue<string>($"{nameof(PrivateServerAuthentication)}:HeathUrl")).ConfigureAwait(false);
                return new HealthCheckResult(reponse.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Unhealthy);
            });
        }
    }
}
