using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DataAccessLibrary.Interfaces;

namespace DataAccessLibrary.Repositories
{
    public class GeneratedRepository: Repository<Generated>, IGeneratedRepository
    {
        public GeneratedRepository(DbContext context) : base(context)
        {
        }


        public MassHelperEntities MHContext
        {
            get
            { return Context as MassHelperEntities; }
        }

        public List<long> GetUsedProperties()
        {
            var temp = MHContext.Generateds.Where(l => l.TemplateNO.HasValue).Select(x=>x.TemplateNO.Value).Distinct().ToList();
            return temp;
            /*
            var temp = MHContext.Generateds.Select(x => x.TemplateNO);
            List<long> answer = new List<long>();
            foreach (long? number in temp)
            {
                if (number.HasValue)
                {
                    answer.Add(number.Value);
                }
                
            }
            answer = answer.Distinct().ToList();
            return answer;*/
        }
    }
}
