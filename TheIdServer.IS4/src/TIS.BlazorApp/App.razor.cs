﻿// Copyright (c) 2021 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Aguacongas.TheIdServer.BlazorApp.Pages;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TIS.BlazorApp
{
    public partial class App
    {
        private readonly List<Assembly> _lazyLoadedAssemblies = new List<Assembly>
        {
            typeof(Index).Assembly
        };

        private readonly string[] _pageKindList = new[]
        {
            "Api",
            "ApiScope",
            "Client",
            "Culture",
            "ExternalProvider",
            "Identities",
            "Identity",
            "Import",
            "Key",
            "Role",
            "User"
        };

        private Task OnNavigateAsync(NavigationContext args)
        {
            _logger.LogDebug($"OnNavigateAsync {args.Path}");
            var path = args.Path.Split("/")[0];
            if (path == "protectresource")
            {
                return LoadAssemblyAsync("Aguacongas.TheIdServer.BlazorApp.Pages.Api.dll");
            }

            if (path == "identityresource")
            {
                return LoadAssemblyAsync("Aguacongas.TheIdServer.BlazorApp.Pages.Identity.dll");
            }

            var pageKind = _pageKindList.FirstOrDefault(k => path == $"{k.ToLower()}s");
            if (pageKind != null)
            {
                return LoadAssemblyAsync($"Aguacongas.TheIdServer.BlazorApp.Pages.{pageKind}s.dll");
            }

            pageKind = _pageKindList.FirstOrDefault(k => path == k.ToLower());
            if (pageKind != null)
            {
                return LoadAssemblyAsync($"Aguacongas.TheIdServer.BlazorApp.Pages.{pageKind}.dll");
            }

            return Task.CompletedTask;
        }

        private async Task LoadAssemblyAsync(string assemblyName)
        {
            _logger.LogDebug($"LoadAssemblyAsync {assemblyName}");
            var assemblies = await _assemblyLoader.LoadAssembliesAsync(
                new[] { assemblyName }).ConfigureAwait(false);
            _lazyLoadedAssemblies.AddRange(assemblies.Where(a => !_lazyLoadedAssemblies.Any(l => l.FullName == a.FullName)));
        }
    }
}
