// Copyright (c) 2020 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Aguacongas.IdentityServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http;
using TIS.Areas.Identity.Services;

[assembly: HostingStartup(typeof(TIS.Areas.Identity.IdentityHostingStartup))]
namespace TIS.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.Configure<EmailOptions>(context.Configuration.GetSection("EmailApiAuthentication"))
                    .AddSingleton<OAuthTokenManager<EmailOptions>>()
                    .AddTransient<IEmailSender>(p =>
                    {
                        var factory = p.GetRequiredService<IHttpClientFactory>();
                        var options = p.GetRequiredService<IOptions<EmailOptions>>();
                        return new EmailApiSender(factory.CreateClient(options.Value.HttpClientName), options);
                    })
                    .AddTransient<OAuthDelegatingHandler<EmailOptions>>()
                    .AddHttpClient(context.Configuration.GetValue<string>("EmailApiAuthentication:HttpClientName"))
                    .ConfigurePrimaryHttpMessageHandler(p => p.GetRequiredService<HttpClientHandler>())
                    .AddHttpMessageHandler<OAuthDelegatingHandler<EmailOptions>>();
            });
        }
    }
}