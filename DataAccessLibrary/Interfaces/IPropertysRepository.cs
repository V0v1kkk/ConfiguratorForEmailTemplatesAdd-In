using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary.Interfaces
{
    public interface IPropertyRepositorys : IRepository<MailProperty>
    {
        void SaveMailProprty(MailProperty mailProperty);
    }
}
