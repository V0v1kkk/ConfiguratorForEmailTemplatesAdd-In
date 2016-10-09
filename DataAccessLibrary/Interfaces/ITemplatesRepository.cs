using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Interfaces
{
    public interface ITemplatesRepository : IRepository<MailsTemplate>
    {
        IEnumerable<Tuple<int, string>> GetListOfEmptyTemplates();

    }
}
