﻿using Aguacongas.IdentityServer.EntityFramework.Store;
using Aguacongas.TheIdServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApp;
using WebApp.Models;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder ConfigureInitialData(this IApplicationBuilder app, IConfiguration configuration)
        {
            var dbType = configuration.GetValue<DbTypes>("DbType");
            if (configuration.GetValue<bool>("Migrate") &&
                dbType != DbTypes.InMemory)
            {
                using var scope = app.ApplicationServices.CreateScope();
                var configContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                configContext.Database.Migrate();

                var opContext = scope.ServiceProvider.GetRequiredService<OperationalDbContext>();
                opContext.Database.Migrate();

                var appcontext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                appcontext.Database.Migrate();
            }

            if (configuration.GetValue<bool>("Seed"))
            {
                using var scope = app.ApplicationServices.CreateScope();
                SeedData.SeedConfiguration(scope, configuration);
                SeedData.SeedUsers(scope, configuration);
            }

            return app;
        }
    }
}
