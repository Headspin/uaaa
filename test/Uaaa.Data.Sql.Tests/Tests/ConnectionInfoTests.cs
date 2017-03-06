using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Uaaa.Data.Sql.Tests.Tests
{
    public class ConnectionInfoTests
    {
        [Fact]
        public void ParseServerDatabase()
        {
            var info = ConnectionInfo.Create(
                @"Server=(localdb)\mssqllocaldb;Database=TestDb;Trusted_Connection=True;MultipleActiveResultSets=true");
            Assert.Equal(@"(localdb)\mssqllocaldb", info.Server);
            Assert.Equal(@"TestDb", info.Database);
        }
    }
}
