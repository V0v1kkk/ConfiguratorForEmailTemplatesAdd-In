using System;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
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
            Templates = new TemplatesRepository(_context);
            Propertys = new ProperysRepository(_context);
            Generated = new GeneratedRepository(_context);
            WorkTT = new WorkTimeRepository(_context);
        }

        public ITemplatesRepository Templates { get; private set; }
        public IPropertyRepositorys Propertys { get; private set; }
        public IGeneratedRepository Generated { get; private set; }
        public IWorkTTRepository WorkTT { get; private set; }
        public int Complete()
        {
            return _context.SaveChanges();
        }

        static public bool TestConnection(string databasePath = @"C:\Users\Владимир\AppData\Local\Mass helper\base.sqlite3")
        {
            using (var db = new MassHelperEntities(ConnectionStringBulder(databasePath)))
            {
                DbConnection conn = db.Database.Connection;
                try
                {
                    conn.Open();
                    
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        static private string ConnectionStringBulder(string databasePath)
        {
            if(databasePath != null && databasePath.StartsWith(@"\\")) databasePath  =  @"\\" + databasePath; //fix for databases in shared folder
            return $@"metadata = res://*/DataModel.csdl|res://*/DataModel.ssdl|res://*/DataModel.msl;
                    provider=System.Data.SQLite.EF6;provider connection string='data source=""{databasePath}""'";
        }

        public void Dispose()
        {
            _context.Dispose();
        }

    }
}
