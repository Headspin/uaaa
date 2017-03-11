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

            info = ConnectionInfo.Create(@"Server=tcp:my.database.windows.net,1433;Initial Catalog=TestDb;Persist Security Info=False;User ID={your_username};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            Assert.Equal(@"tcp:my.database.windows.net,1433", info.Server);
            Assert.Equal(@"TestDb", info.Database);
            
        }

    }
}
