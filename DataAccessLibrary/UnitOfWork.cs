using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Repositories;

namespace DataAccessLibrary
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly MassHelperEntities _context;
        public UnitOfWork()
        {
            _context = new MassHelperEntities();
            Templates = new TemplatesRepository(_context);
            Propertys = new ProperysRepository(_context);
            Generated = new GeneratedRepository(_context);
            WorkTT = new WorkTimeRepository(_context);
        }
        public UnitOfWork(string databasePath)
        {
            _context = new MassHelperEntities(ConnectionStringBulder(databasePath));
            //if (databasePath != null && databasePath.StartsWith(@"\\")) databasePath = @"\\" + databasePath; //fix for databases in shared folder
            //_context.Database.Connection.ConnectionString = _context.Database.Connection.ConnectionString.Replace("path", databasePath);

            Templates = new TemplatesRepository(_context);
            Propertys = new ProperysRepository(_context);
            Generated = new GeneratedRepository(_context);
            WorkTT = new WorkTimeRepository(_context);

            //_context.Configuration.AutoDetectChangesEnabled = false;
            //_context.Configuration.ValidateOnSaveEnabled = false;
        }

        public ITemplatesRepository Templates { get; private set; }
        public IPropertyRepositorys Propertys { get; private set; }
        public IGeneratedRepository Generated { get; private set; }
        public IWorkTTRepository WorkTT { get; private set; }
        private string _connectionString;

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public bool TestConnection(string databasePath = @"C:\Users\Владимир\AppData\Local\Mass helper\base.sqlite3")
        {
            DbConnection conn = _context.Database.Connection;
            try
            {
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if(conn.State==ConnectionState.Open) conn.Close();
            }
        }

        static private string ConnectionStringBulder(string databasePath)
        {
            if(databasePath != null && databasePath.StartsWith(@"\\")) databasePath  =  @"\\" + databasePath; //fix for databases in shared folder
            return $@"metadata = res://*/DataModel.csdl|res://*/DataModel.ssdl|res://*/DataModel.msl;
                    provider=System.Data.SQLite.EF6;provider connection string='data source=""{databasePath}""'";
        }

        private void SetConnectionString(string databasePath)
        {
            //todo: Проверка строки
            _connectionString = databasePath;
            ConfigurationManager.ConnectionStrings["MassHelperEntities"].ConnectionString = ConfigurationManager.ConnectionStrings["MassHelperEntities"].ConnectionString.Replace("path", databasePath);
        }
        private void BackConnectionString()
        {
            //todo: Проверка резервной строки
            ConfigurationManager.ConnectionStrings["MassHelperEntities"].ConnectionString = ConfigurationManager.ConnectionStrings["MassHelperEntities"].ConnectionString.Replace(_connectionString, "path");
        }


        static private void ConnectionBulder(string databasePath)
        {
            ConfigurationManager.ConnectionStrings["MassHelperEntities"].ConnectionString = ConfigurationManager.ConnectionStrings["MassHelperEntities"].ConnectionString.Replace("path",databasePath);

            /*
            var ecsBuilder = new EntityConnectionStringBuilder(originalConnectionString);
            var sqlCsBuilder = new SqlConnectionStringBuilder(ecsBuilder.ProviderConnectionString)
            {
                DataSource = "newDBHost"
            };
            var providerConnectionString = sqlCsBuilder.ToString();
            ecsBuilder.ProviderConnectionString = providerConnectionString;

            string contextConnectionString = ecsBuilder.ToString();
            using (var db = new SMSContext(contextConnectionString))
            {
            }





            SQLiteConnectionStringBuilder conString = new SQLiteConnectionStringBuilder();
            conString.DataSource = databasePath;
            conString.DefaultTimeout = 5000;
            conString.SyncMode = SynchronizationModes.Off;
            conString.JournalMode = SQLiteJournalModeEnum.Memory;
            conString.PageSize = 65536;
            conString.CacheSize = 16777216;
            conString.FailIfMissing = false;
            conString.ReadOnly = false;

            EntityConnectionStringBuilder efBuilder = new EntityConnectionStringBuilder();
            
            efBuilder.ProviderConnectionString = conString.ConnectionString;

            return efBuilder.ConnectionString;*/
        }

        /*
        public DbConnection GetSqlConn4DbName(string dataSource, string dbName)
        {
            var sqlConnStringBuilder = new SQLiteConnectionStringBuilder();
            sqlConnStringBuilder.DataSource = String.IsNullOrEmpty(dataSource) ? DefaultDataSource : dataSource;
            sqlConnStringBuilder.IntegratedSecurity = true;
            sqlConnStringBuilder.MultipleActiveResultSets = true;
            // NOW MY PROVIDER FACTORY OF CHOICE, switch providers here 
            var sqlConnFact = new SQLiteProviderFactory();
            var sqlConn = sqlConnFact.CreateConnection();
            return sqlConn;
        }*/


        static private SQLiteConnection ConnectionBulder1(string databasePath)
        {
            var connection = new SQLiteConnection()
            {
                ConnectionString = new SQLiteConnectionStringBuilder()
                { DataSource =databasePath, ForeignKeys = true, SyncMode = SynchronizationModes.Full, Pooling = true, Flags = SQLiteConnectionFlags.NoConvertSettings}
                        .ConnectionString,
                ParseViaFramework = true,
                
            };
            return connection;
        }





        public void Dispose()
        {
            //BackConnectionString();
            _context.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

    }
}
