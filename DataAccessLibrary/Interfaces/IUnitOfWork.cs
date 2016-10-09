using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Interfaces
{
    interface IUnitOfWork
    {
        ITemplatesRepository Templates { get; }
        IPropertyRepositorys Propertys { get; }
        IGeneratedRepository Generated { get; }
        IWorkTTRepository WorkTT { get; }
        int Complete();
    }
}
