// Copyright (c) 2020 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Aguacongas.IdentityServer.Store;
using Aguacongas.IdentityServer.Store.Entity;
using Aguacongas.TheIdServer.BlazorApp.Infrastructure.Services;
using Aguacongas.TheIdServer.BlazorApp.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace TIS.Services
{
    public class PreRenderStringLocalizer : StringLocalizer
    {
        public PreRenderStringLocalizer(IReadOnlyLocalizedResourceStore store, IReadOnlyCultureStore cultureStore, ILogger<StringLocalizer> logger) : base(store, cultureStore, logger)
        {
            GetSupportedCulturesAsync().GetAwaiter().GetResult();
        }

        protected override LocalizedString GetLocalizedString(string name, params object[] arguments)
        {
            if (!KeyValuePairs.TryAdd(name, GetStringAsync(name).GetAwaiter().GetResult()))
            {
                var value = KeyValuePairs[name];
                var localizedString = new LocalizedString(name, string.Format(value ?? name, arguments), value == null);
                if (localizedString.ResourceNotFound && CurrentCulture.Name != "en")
                {
                    Logger.LogWarning($"Localized value for key '{name}' not found for culture '{CurrentCulture.Name}'");
                }
                return localizedString;
            }
            return new LocalizedString(name, string.Format(KeyValuePairs[name], arguments), true);
        }
    }
}
