using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Uaaa.Data.Sql.Tests
{
    public static class Database
    {
        public static void Create()
        {
            const string createDbScript = @"Scripts/CreateTestDb.sql";
            File.ReadAllText(createDbScript);
        }

        public static void Clear()
        {
            const string clearDbScript = @"Scripts/CreateDb.sql";
            File.ReadAllText(clearDbScript);
        }
    }
}
