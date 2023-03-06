// Copyright (c) 2021 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.Authentication.Negotiate;
using System;

namespace IdentityServerHost.Quickstart.UI
{
    public class AccountOptions
    {
        public bool AllowLocalLogin { get; set; } = true;
        public bool AllowRememberLogin { get; set; } = true;
        public TimeSpan RememberMeLoginDuration { get; set; } = TimeSpan.FromDays(30);

        public bool ShowLogoutPrompt { get; set; } = true;
        public bool AutomaticRedirectAfterSignOut { get; set; } = false;

        // specify the Windows authentication scheme being used
        public string WindowsAuthenticationSchemeName { get; } = NegotiateDefaults.AuthenticationScheme;

        // if user uses windows auth, should we load the groups from windows
        public bool IncludeWindowsGroups { get; set; } = false;

        public string InvalidCredentialsErrorMessage { get; set; } = "Invalid username or password";

        public bool ShowForgotPassworLink { get; set; } = true;

        public bool ShowRegisterLink { get; set; } = true;

        public bool ShowResendEmailConfirmationLink { get; set; } = true;

    }
}
