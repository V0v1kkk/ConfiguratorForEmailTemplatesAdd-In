using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public partial class MassHelperEntities
    {
        /// <summary>
        /// Need for change database in runtime
        /// </summary>
        /// <param name="connectionString"></param>
        public MassHelperEntities(string connectionString)
            : base(connectionString)
        {
        }

        public MassHelperEntities(SQLiteConnection connection) : base(connection, true)
        {
        }

        private static string CheckConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return "name=MassHelperEntities";
            return connectionString;
        }
    }
}
