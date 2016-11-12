using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public static class ConnectionTools
    {
        public static void ChangeDatabase(
            this MassHelperEntities source,
            string metadata = "",
            string provider = "",
            string providerConnectionString = "",
            string configConnectionStringName = "")
        {
            try
            {
                // use the const name if it's not null, otherwise
                // using the convention of connection string = EF contextname
                // grab the type name and we're done
                var configNameEf = string.IsNullOrEmpty(configConnectionStringName)
                    ? ""
                    : configConnectionStringName;

                // add a reference to System.Configuration
                var entityCnxStringBuilder = new EntityConnectionStringBuilder
                    (System.Configuration.ConfigurationManager
                        .ConnectionStrings[configNameEf].ConnectionString);

                // init the sqlbuilder with the full EF connectionstring cargo
                var sqlCnxStringBuilder = new SqlConnectionStringBuilder
                    (entityCnxStringBuilder.ProviderConnectionString);

                // only populate parameters with values if added
                if (!string.IsNullOrEmpty(metadata))
                    sqlCnxStringBuilder.InitialCatalog = metadata;
                if (!string.IsNullOrEmpty(provider))
                    sqlCnxStringBuilder.DataSource = provider;
                if (!string.IsNullOrEmpty(providerConnectionString))
                    sqlCnxStringBuilder.UserID = providerConnectionString;

                // set the integrated security status

                // now flip the properties that were changed
                //source.Configuration.
                //source.Connection.ConnectionString
                //    = sqlCnxStringBuilder.ConnectionString;
            }
            catch (Exception ex)
            {
                // set log item if required
            }
        }
    }
}
