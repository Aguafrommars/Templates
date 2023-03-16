// Copyright (c) 2021 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Aguacongas.TheIdServer.BlazorApp.Infrastructure.Services;
using Aguacongas.TheIdServer.BlazorApp.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Testing;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using RichardSzalay.MockHttp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using TIS;
using Xunit.Abstractions;
using blazorApp = TIS.BlazorApp;

namespace Aguacongas.TheIdServer.IntegrationTest
{
    public static class TestUtils
    {
        public static IConfigurationRoot CreateApplicationConfiguration(HttpClient httpClient)
        {
            var appConfigurationDictionary = new Dictionary<string, string>
            {
                ["AdministratorEmail"] = "aguacongas@gmail.com",
                ["ApiBaseUrl"] = new Uri(httpClient.BaseAddress, "api").ToString(),
                ["ProviderOptions:Authority"] = httpClient.BaseAddress.ToString(),
                ["ProviderOptions:ClientId"] = "theidserveradmin",
                ["ProviderOptions:DefaultScopes[0]"] = "openid",
                ["ProviderOptions:DefaultScopes[1]"] = "profile",
                ["ProviderOptions:DefaultScopes[2]"] = "theidserveradminapi",
                ["ProviderOptions:PostLogoutRedirectUri"] = new Uri(httpClient.BaseAddress, "authentication/logout-callback").ToString(),
                ["ProviderOptions:RedirectUri"] = new Uri(httpClient.BaseAddress, "authentication/login-callback").ToString(),
                ["ProviderOptions:ResponseType"] = "code",
                ["WelcomeContenUrl"] = "/welcome-fragment.html"
            };
            var appConfiguration = new ConfigurationBuilder().AddInMemoryCollection(appConfigurationDictionary).Build();
            return appConfiguration;
        }

        public class FakeAuthenticationStateProvider : AuthenticationStateProvider, IAccessTokenProvider
        {
            private readonly AuthenticationState _state;

            public FakeAuthenticationStateProvider(string userName, IEnumerable<Claim> claims)
            {
                if (claims != null && !claims.Any(c => c.Type == "name"))
                {
                    var list = claims.ToList();
                    list.Add(new Claim("name", userName));
                    claims = list;
                }
                _state = new AuthenticationState(new FakeClaimsPrincipal(new FakeIdendity(userName, claims)));
            }

            public override Task<AuthenticationState> GetAuthenticationStateAsync()
            {
                return Task.FromResult(_state);
            }

            public ValueTask<AccessTokenResult> RequestAccessToken()
            {
                return new ValueTask<AccessTokenResult>(new AccessTokenResult(AccessTokenResultStatus.Success,
                    new AccessToken
                    {
                        Expires = DateTimeOffset.Now.AddDays(1),
                        GrantedScopes = new string[] { "openid", "profile", "theidseveradminaoi" },
                        Value = "test"
                    },
                    "http://exemple.com",
                    new InteractiveRequestOptions
                    {
                        Interaction = InteractionType.GetToken,
                        ReturnUrl = "http://exemple.com"
                    }));
            }

            public ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
            {
                throw new NotImplementedException();
            }
        }

        public class AccessTokenProviderAccessor : IAccessTokenProviderAccessor
        {
            public AccessTokenProviderAccessor(IAccessTokenProvider accessTokenProvider)
            {
                TokenProvider = accessTokenProvider;
            }

            public IAccessTokenProvider TokenProvider { get; }
        }

        public class FakeClaimsPrincipal : ClaimsPrincipal
        {
            public FakeClaimsPrincipal(FakeIdendity idendity) : base(idendity)
            {

            }

            public override bool IsInRole(string role)
            {
                return Identity.IsAuthenticated && Claims != null && Claims.Any(c => c.Type == "role" && c.Value == role);
            }
        }

        public class FakeIdendity : ClaimsIdentity
        {
            private readonly string _userName;
            private bool _IsAuthenticated = true;

            public FakeIdendity(string userName, IEnumerable<Claim> claims) : base(claims)
            {
                _userName = userName;
            }
            public override string AuthenticationType => "Bearer";

            public override bool IsAuthenticated => _IsAuthenticated;

            public void SetIsAuthenticated(bool value)
            {
                _IsAuthenticated = value;
            }

            public override string Name => _userName;
        }
    }

    class JSRuntimeImpl : JSRuntime
    {
        protected override void BeginInvokeJS(long taskId, string identifier, string argsJson)
        {
        }

        protected override void BeginInvokeJS(long taskId, string identifier, string argsJson, JSCallResultType resultType, long targetInstanceId)
        {
        }

        protected override void EndInvokeDotNet(DotNetInvocationInfo invocationInfo, in DotNetInvocationResult invocationResult)
        {
        }
    }

}
