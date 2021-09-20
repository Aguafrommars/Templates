// Copyright (c) 2021 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Threading.Tasks;

namespace TIS.Services
{
    public class RemoteAuthenticationService : ServerAuthenticationStateProvider, IRemoteAuthenticationService<RemoteAuthenticationState>
    {

        public Task<RemoteAuthenticationResult<RemoteAuthenticationState>> CompleteSignInAsync(RemoteAuthenticationContext<RemoteAuthenticationState> context)
        {
            return Success(context);
        }

        public Task<RemoteAuthenticationResult<RemoteAuthenticationState>> CompleteSignOutAsync(RemoteAuthenticationContext<RemoteAuthenticationState> context)
        {
            return Success(context);
        }

        public Task<RemoteAuthenticationResult<RemoteAuthenticationState>> SignInAsync(RemoteAuthenticationContext<RemoteAuthenticationState> context)
        {
            return Success(context);
        }

        public Task<RemoteAuthenticationResult<RemoteAuthenticationState>> SignOutAsync(RemoteAuthenticationContext<RemoteAuthenticationState> context)
        {
            return Success(context);
        }

        private static Task<RemoteAuthenticationResult<RemoteAuthenticationState>> Success(RemoteAuthenticationContext<RemoteAuthenticationState> context)
        {
            return Task.FromResult(new RemoteAuthenticationResult<RemoteAuthenticationState>
            {
                State = context.State,
                Status = RemoteAuthenticationStatus.Success
            });
        }
    }
}
