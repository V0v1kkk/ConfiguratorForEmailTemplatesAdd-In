using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.IO;
using DataAccessLibrary.Interfaces;
using DataAccessLibrary.Repositories;

namespace DataAccessLibrary
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        //private readonly MassHelperEntities _context;
        private readonly MassHelperEntities _context;
        public UnitOfWork()
        {
            _context = new MassHelperEntities(); //test
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
        private string _connectionString;

        public int Complete()
        {
            return _context.SaveChanges();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="databasePath"></param>
        /// <returns></returns>
        public bool TestDbExist(string databasePath)
        {
            FileInfo databaseInfo = new FileInfo(databasePath);
            return databaseInfo.Exists;
        }

        static private string ConnectionStringBulder(string databasePath)
        {
            if(databasePath != null && databasePath.StartsWith(@"\\")) databasePath  =  @"\\" + databasePath; //fix for databases in shared folder
            return $@"metadata = res://*/DataModel.csdl|res://*/DataModel.ssdl|res://*/DataModel.msl;
                    provider=System.Data.SQLite.EF6;provider connection string='data source=""{databasePath}""'";
        }

        public void Dispose()
        {
            _context.Dispose(); //test
            GC.Collect(); //test
            GC.WaitForPendingFinalizers(); //test
        }

    }
}
