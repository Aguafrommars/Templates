// Copyright (c) 2021 @Olivier Lefebvre. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Aguacongas.TheIdServer.Test
{
    public class DbContextOptionsBuilderExtensionsTest
    {
        [Fact]
        [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "for test purpose")]
        public void UseDatabaseFromConfiguration_should_add_in_memory_options()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = Guid.NewGuid().ToString(),
                ["DbType"] = "InMemory",
            }).Build();
            var sut = new DbContextOptionsBuilder();

            sut.UseDatabaseFromConfiguration(configuration);

            Assert.Contains(sut.Options.Extensions, e => typeof(InMemoryOptionsExtension) == e.GetType());
        }

        [Fact]
        [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "For test purpose")]
        public void UseDatabaseFromConfiguration_should_add_sqlite_options()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = Guid.NewGuid().ToString(),
                ["DbType"] = "Sqlite",
            }).Build();
            var sut = new DbContextOptionsBuilder();

            sut.UseDatabaseFromConfiguration(configuration);

            Assert.Contains(sut.Options.Extensions, e => typeof(SqliteOptionsExtension) == e.GetType());
        }


        [Fact]
        [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "For test purpose")]
        public void UseDatabaseFromConfiguration_should_add_sql_server_options()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = Guid.NewGuid().ToString(),
                ["DbType"] = "SqlServer",
            }).Build();
            var sut = new DbContextOptionsBuilder();

            sut.UseDatabaseFromConfiguration(configuration);

            Assert.Contains(sut.Options.Extensions, e => typeof(SqlServerOptionsExtension) == e.GetType());
        }
    }
}
