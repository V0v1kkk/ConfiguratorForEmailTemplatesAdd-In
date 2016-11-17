using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary.Interfaces;

namespace DataAccessLibrary.Repositories
{
    public class TemplatesRepository : Repository<MailsTemplate>, ITemplatesRepository
    {
        public TemplatesRepository(DbContext context) : base(context)
        {
        }

        public IEnumerable<Tuple<int,string>> GetListOfEmptyTemplates()
        {
            return MHContext.MailsTemplates.ToList().Select(x => Tuple.Create((int)x.Templateid, x.Templadescription));
        }

        public MassHelperEntities MHContext
        {
            get
            { return Context as MassHelperEntities; }
        }
    }
}
