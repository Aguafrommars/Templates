// Copyright (c) 2021 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Aguacongas.TheIdServer.IntegrationTest
{
    class TestUserService
    {
        public ClaimsPrincipal User { get; set; }

        public void SetTestUser(bool isAuthenticated = true, IEnumerable<Claim> claims = null)
        {
            var identityMock = new Mock<IIdentity>();
            if (isAuthenticated)
            {
                identityMock.SetupGet(m => m.AuthenticationType).Returns("mock");
                identityMock.SetupGet(m => m.IsAuthenticated).Returns(true);
                var name = claims?.FirstOrDefault(c => c.Type == "name")?.Value ?? "test";
                identityMock.SetupGet(m => m.Name).Returns(name);
            }

            User = new ClaimsPrincipal(new ClaimsIdentity(identityMock.Object, claims, "Bearer", "name", "role"));
        }
    }
}
