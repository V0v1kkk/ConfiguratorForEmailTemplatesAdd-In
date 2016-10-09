using System;
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
        public ITemplatesRepository Templates { get; private set; }
        public IPropertyRepositorys Propertys { get; private set; }
        public IGeneratedRepository Generated { get; private set; }
        public IWorkTTRepository WorkTT { get; private set; }
        public int Complete()
        {
            return _context.SaveChanges();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
