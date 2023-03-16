// Copyright (c) 2021 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Aguacongas.TheIdServer.BlazorApp.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TIS.BlazorApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.AddTheIdServerApp();
            var configuration = builder.Configuration;
            var settings = configuration.Get<Settings>();
            if (settings?.Prerendered == false)
            {
                builder.RootComponents.Add<App>("app");
            }

            var host = builder.Build();
            var runtime = host.Services.GetRequiredService<IJSRuntime>();
            var cultureName = await runtime.InvokeAsync<string>("localStorage.getItem", "culture").ConfigureAwait(false);
            if (!string.IsNullOrEmpty(cultureName))
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .FirstOrDefault(c => c.Name == cultureName) ?? CultureInfo.CurrentCulture;
            }
            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
