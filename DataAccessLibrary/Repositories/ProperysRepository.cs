using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary.Interfaces;

namespace DataAccessLibrary.Repositories
{
    public class ProperysRepository : Repository<MailProperty>, IPropertyRepositorys
    {
        public ProperysRepository(DbContext context) : base(context)
        {
        }

        public MassHelperEntities MHContext
        {
            get
            { return Context as MassHelperEntities; }
        }

        public void SaveMailProprty(MailProperty mailProperty)
        {
            //MHContext.MailPropertys.
        }

        public List<long> GetUsedTemplates()
        {
            return MHContext.MailPropertys.Select(x => x.BodyID).Distinct().ToList();
        }
    }
}
