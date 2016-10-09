using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary.Interfaces;

namespace DataAccessLibrary.Repositories
{
    public class WorkTimeRepository : Repository<WorkTime>, IWorkTTRepository
    {
        public WorkTimeRepository(DbContext context) : base(context)
        {
        }


        public MassHelperEntities MHContext
        {
            get
            { return Context as MassHelperEntities; }
        }
    }
}
