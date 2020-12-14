// Copyright (c) 2020 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System.Threading.Tasks;

namespace TIS.BlazorApp.Shared
{
    public partial class RedirectToLogin
    {
        private string _administratorEmail;
        protected override async Task OnInitializedAsync()
        {
            var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false);
            var identity = authenticationState.User?.Identity;
            if (identity != null && !identity.IsAuthenticated)
            {
                _navigationManager.NavigateTo($"authentication/login?returnUrl={_navigationManager.Uri}");
                return;
            }
            _administratorEmail = _settings.AdministratorEmail;
        }

    }
}
