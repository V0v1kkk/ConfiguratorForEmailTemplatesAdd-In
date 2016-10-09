using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DataAccessLibrary;
//using MailsTemplate = MHConfigurator.Models.MailsTemplate;

namespace Configurator
{
    // ReSharper disable once InconsistentNaming
    public class DAL
    {
        private static readonly object Locker;
        static private DAL _instance;

        private DAL()
        {
            //Mapper.Initialize(cfg => cfg.CreateMap<DataAccessLibrary.MailPropertys, MailPropertys>().ForMember(destinationMember => destinationMember.Useful, x => x.MapFrom(m => GetDAL().GetUsedTemplatesNumbers().Contains(m.ButtonID))));
            
            //Mapper.AssertConfigurationIsValid();

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<DataAccessLibrary.MailProperty, MailProperty>()
                    .ForMember(x => x.BodyID, expression => expression.MapFrom(property => (int) property.BodyID))
                    .ForMember(x => x.ButtonID, expression => expression.MapFrom(property => (int) property.ButtonID))
                    .ForMember(x => x.ReminderTime, expression => expression.MapFrom(property => DateTime.Parse(property.ReminderTime)))
                    .ForMember(x => x.Useful, expression => expression.MapFrom(property => GetDAL().UsedTemplates.Contains(property.ButtonID)));
                cfg.CreateMap<MailProperty, DataAccessLibrary.MailProperty>();
                cfg.CreateMap<DataAccessLibrary.MailsTemplate, MailsTemplate>();
            });
            //Mapper.Initialize(cfg => cfg.CreateMap<MailProperty, DataAccessLibrary.MailProperty>());
        }

        public static DAL GetDAL()
        {
            if (_instance == null)
            {
                _instance = new DAL();
            }
            
            return _instance;
        }


        private bool _mailPropertyChanged = false; //Будет устанавливатся в случае записи в таблицу
        private List<MailProperty> _mailPropertys = new List<MailProperty>(); //Возможно нужно модель унаследовать от модели
        public List<MailProperty> MailPropertys
        {
            get
            {
                if ((_mailPropertys == null)||(_mailPropertys.Count==0)||(_mailPropertyChanged))
                {
                    _mailPropertys = GetMailPropertys();
                }
                return _mailPropertys;
            }
            set
            {
                _mailPropertys = value;
                //Тут писать в базу?
            }
        }


        private bool _GeneratedChanged = false; //От этого поля будет зависеть несколько полей (список задействованных шаблонов, сами генератед объекты (м.б. в виде дерева))
        private List<long> _usedTemplates = new List<long>();
        public List<long> UsedTemplates
        {
            get
            {
                if ((_usedTemplates == null) || (_usedTemplates.Count == 0) || (_GeneratedChanged))
                {
                    _usedTemplates = GetUsedTemplatesNumbers();
                }
                return _usedTemplates; 
                
            }
            set
            {
                _usedTemplates = value;
                //Писать в базу?
            }
        }


        public MailProperty GetMailPropertyById(int id)
        {
            using (var uow = new UnitOfWork())
            {
                MailProperty t = null;
                try
                {
                    t = Mapper.Map<DataAccessLibrary.MailProperty, MailProperty>(uow.Propertys.Find(x => x.ButtonID == id).First());

                }
                catch (Exception)
                {
                    //todo: Залогировать
                }
                return t;
            }
        }
        /*
        public MailPropertys FindMailProperty(Expression<Func<MailPropertys, bool>> predicate) ////
        {
            using (var uow = new UnitOfWork())
            {
                MailPropertys t = null;
                try
                {
                    t = Mapper.Map<DataAccessLibrary.MailPropertys, MailPropertys>(uow.Propertys.Find(x => x.ButtonID == id).First());

                }
                catch (Exception)
                {
                    //todo: Залогировать
                }
                return t;
            }
        }*/
        private List<MailProperty> GetMailPropertys()
        {
            using (var uow = new UnitOfWork())
            {
                List<MailProperty> answer = new List<MailProperty>();
                try
                {
                    foreach (DataAccessLibrary.MailProperty mailProperty in uow.Propertys.GetAll())
                    {
                        try
                        {
                            MailProperty temp = Mapper.Map<MailProperty>(mailProperty);
                            answer.Add(temp);
                        }
                        catch (Exception)
                        {
                            //todo: Залогировать ошибку при маппинге
                        }
                    }
                }
                catch (Exception)
                {
                    //todo: Залогировать
                }
                return answer;
            }
        }

        public List<long> GetUsedTemplatesNumbers()
        {
            List<long> answer = new List<long>();
            using (var uow = new UnitOfWork())
            {
                try
                {
                    answer = uow.Generated.GetUsedTemplates();
                }
                catch (Exception)
                {
                    //todo: Залогировать
                }
                return answer;
            }
        }

        void SaveMailProperty(MailProperty mailProperty)
        {
            using (var uow = new UnitOfWork())
            {
                try
                {
                    var dbProp = uow.Propertys.Find(property => property.ButtonID == mailProperty.ButtonID).FirstOrDefault();
                    if (dbProp == null)
                    {
                        //uow.Propertys.Add(new);
                    }
                    else
                    {
                        
                    }
                }
                catch (Exception)
                {
                    //todo: Залогировать
                }
            }
        }

        public List<MailsTemplate> GetEmptyMailTemplates()
        {
            //todo: Заменить автомаппером с использованием профилей. Или не нужно? Просто другой медод для получения непустых объектов. А при загрузке формы шаблонов перевыгружать то, что передано в конструктор
            using (var uow = new UnitOfWork())
            {
                List<MailsTemplate> list = new List<MailsTemplate>();
                try
                {
                    var templatesEnititys = uow.Templates.GetListOfEmptyTemplates();
                    foreach (var enitity in templatesEnititys)
                    {
                        list.Add(new MailsTemplate()
                        {
                            Templateid = enitity.Item1,
                            Templadescription = enitity.Item2,
                            TemplateBody = null
                        });
                    }
                }
                catch (Exception)
                {
                    //todo: Залогировать
                }
                return list;
            }
        }

    }
}
