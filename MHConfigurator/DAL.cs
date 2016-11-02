using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DataAccessLibrary;
using AutoMapper;
using MHConfigurator.Models;
using MailProperty = MHConfigurator.Models.MailProperty;
using MailTemplate = MHConfigurator.Models.MailTemplate;

namespace MHConfigurator
{
    //TODO: Передалать на обсервабл коллекциях
    // ReSharper disable once InconsistentNaming
    public class DAL
    {
        private static readonly object Locker;
        static private DAL _instance;


        private DAL()
        {
            //Mapper.Initialize(cfg => cfg.CreateMap<DataAccessLibrary.MailPropertys, MailPropertys>().ForMember(destinationMember => destinationMember.Useful, x => x.MapFrom(m => GetDAL().GetUsedPropertiesNumbers().Contains(m.ButtonID))));
            
            //Mapper.AssertConfigurationIsValid();

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<DataAccessLibrary.MailProperty, MailProperty>()
                    .ForMember(x => x.BodyID, expression => expression.MapFrom(property => (int) property.BodyID))
                    .ForMember(x => x.ButtonID, expression => expression.MapFrom(property => (int) property.ButtonID))
                    .ForMember(x => x.ReminderTime, expression => expression.MapFrom(property => DateTime.Parse(property.ReminderTime)))
                    .ForMember(x => x.Useful, expression => expression.MapFrom(property => GetDAL().UsedProperties.Contains(property.ButtonID)));
                cfg.CreateMap<MailProperty, DataAccessLibrary.MailProperty>()
                    .ForMember(x => x.MailsTemplate, o => o.UseDestinationValue());


                cfg.CreateMap<DataAccessLibrary.MailsTemplate, MailTemplate>()
                .ForMember(x=>x.TemplateDescription, o=>o.MapFrom(template => template.Templadescription))
                .ForMember(x=>x.TemplateBodyRusFix, o=>o.Ignore())
                .ForMember(x=>x.TemplateBody, o=>o.MapFrom(template => 
                template.TemplateBody != null ? Encoding.GetEncoding("windows-1251").GetString(Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("windows-1251"),
                        template.TemplateBody)) : "" ))
                .ForMember(x=>x.Useful, o=>o.MapFrom(template => GetDAL().UsedTemplates.Contains(template.Templateid)));
                cfg.CreateMap<MailTemplate, DataAccessLibrary.MailsTemplate>().ForMember(x=>x.MailPropertys, o=>o.UseDestinationValue())
                .ForMember(x=>x.Templadescription, o=>o.MapFrom(template => template.TemplateDescription))
                .ForMember(x=>x.TemplateBody,o=>o.MapFrom(template =>Encoding.Convert(Encoding.GetEncoding("windows-1251"), Encoding.UTF8, Encoding.GetEncoding("windows-1251").GetBytes(template.TemplateBody))))
                .ForMember(x=> x.MailPropertys, o=>o.UseDestinationValue());
            });
        }

        public static DAL GetDAL()
        {
            if (_instance == null)
            {
                _instance = new DAL();
            }
            
            return _instance;
        }


        private bool _mailPropertyChanged; //Будет устанавливатся в случае записи в таблицу
        private List<MailProperty> _mailPropertys = new List<MailProperty>(); //Возможно нужно модель унаследовать от модели
        public List<MailProperty> MailPropertys
        {
            get
            {
                if ((_mailPropertys == null)||(_mailPropertys.Count==0)||(_mailPropertyChanged))
                {
                    _mailPropertys = GetMailPropertys();
                    if (_mailPropertyChanged) _mailPropertyChanged = false;
                }
                return _mailPropertys;
            }
            private set
            {
                _mailPropertys = value;
                //Тут писать в базу? сдалать публичным, Вичислять разницу межу коллекциями и приводить базу в соответствующее состояние???
            }
        }


        private bool _GeneratedChanged = false; //От этого поля будет зависеть несколько полей (список задействованных шаблонов, сами генератед объекты (м.б. в виде дерева))
        private List<long> _usedProperties = new List<long>();
        public List<long> UsedProperties
        {
            get
            {
                if ((_usedProperties == null) || (_usedProperties.Count == 0) || (_GeneratedChanged))
                {
                    _usedProperties = GetUsedPropertiesNumbers();
                }
                return _usedProperties; 
                
            }
            private set
            {
                _usedProperties = value;
                //Писать в базу?
            }
        }


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
            private set
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

        

        public bool SaveMailProperty(MailProperty mailProperty)
        {
            if (mailProperty.ButtonID == 0) return false;
            using (var uow = new UnitOfWork())
            {
                try
                {
                    var dbProp = uow.Propertys.Find(property => property.ButtonID == mailProperty.ButtonID).FirstOrDefault();
                    if (dbProp == null)
                    {
                        uow.Propertys.Add(Mapper.Map<MailProperty,DataAccessLibrary.MailProperty>(mailProperty));
                    }
                    else
                    {
                        Mapper.Map(mailProperty, dbProp);
                    }
                    uow.Complete();
                    _mailPropertyChanged = true;
                    return true;
                }
                catch (Exception)
                {
                    //todo: Залогировать
                    return false;
                }
            }
        }

        public bool DeleteMailProperty(MailProperty mailProperty)
        {
            if (mailProperty.ButtonID == 0) return false;
            using (var uow = new UnitOfWork())
            {
                try
                {
                    var dbProp = uow.Propertys.Find(property => property.ButtonID == mailProperty.ButtonID).FirstOrDefault();
                    if (dbProp == null)
                    {
                        return false; 
                    }
                    else
                    {
                        uow.Propertys.Remove(dbProp);
                    }
                    uow.Complete();
                    _mailPropertyChanged = true;
                    return true;
                }
                catch (Exception)
                {
                    //todo: Залогировать
                    return false;
                }
            }
        }

        #region MailTemplates

        public List<long> GetUsedPropertiesNumbers()
        {
            List<long> answer = new List<long>();
            using (var uow = new UnitOfWork())
            {
                try
                {
                    answer = uow.Generated.GetUsedProperties();
                    return answer;
                }
                catch (Exception)
                {
                    //todo: Залогировать
                    return null;
                }
                
            }
        }

        public List<long> GetUsedTemplatesNumbers()
        {
            List<long> answer = new List<long>();
            using (var uow = new UnitOfWork())
            {
                try
                {
                    answer = uow.Propertys.GetUsedTemplates();
                    return answer;
                }
                catch (Exception)
                {
                    //todo: Залогировать
                    return null;
                }

            }
        }
        public List<MailTemplate> GetEmptyMailTemplates()
        {
            //todo: Заменить автомаппером с использованием профилей. Или не нужно? Просто другой медод для получения непустых объектов. А при загрузке формы шаблонов перевыгружать то, что передано в конструктор
            using (var uow = new UnitOfWork())
            {
                List<MailTemplate> list = new List<MailTemplate>();
                try
                {
                    var templatesEnititys = uow.Templates.GetListOfEmptyTemplates();
                    foreach (var enitity in templatesEnititys)
                    {
                        list.Add(new MailTemplate()
                        {
                            TemplateId = enitity.Item1,
                            TemplateDescription = enitity.Item2,
                            TemplateBody = null
                        });
                    }
                    return list;
                }
                catch (Exception)
                {
                    //todo: Залогировать
                    return null;
                }
            }
        }


        public List<MailTemplate> GetMailTemplates()
        {
            //todo: Заменить автомаппером с использованием профилей. Или не нужно? Просто другой медод для получения непустых объектов. А при загрузке формы шаблонов перевыгружать то, что передано в конструктор
            using (var uow = new UnitOfWork())
            {
                List<MailTemplate> list = new List<MailTemplate>();
                try
                {
                    var templates = uow.Templates.GetAll();
                    list.AddRange(templates.Select(Mapper.Map<DataAccessLibrary.MailsTemplate, MailTemplate>).Where(x=>x.TemplateId!=0));
                    return list;
                }
                catch (Exception)
                {
                    //todo: Залогировать
                    return null;
                }
            }
        }

        public MailTemplate GetMailTemplateById(int id)
        {
            using (var uow = new UnitOfWork())
            {
                MailTemplate t = null;
                try
                {
                    t = Mapper.Map<DataAccessLibrary.MailsTemplate, MailTemplate>(uow.Templates.Find(x => x.Templateid == id).First());

                }
                catch (Exception)
                {
                    //todo: Залогировать
                }
                return t;
            }
        }

        public bool SaveMailTemplate(MailTemplate mailsTemplate, bool newItem = false)
        {
            if (mailsTemplate.TemplateId == 0) return false;
            using (var uow = new UnitOfWork())
            {
                try
                {
                    var dbProp =
                        uow.Templates.Find(template => template.Templateid == mailsTemplate.TemplateId).FirstOrDefault();
                    if (dbProp == null)
                    {
                        uow.Templates.Add(Mapper.Map<MailTemplate, DataAccessLibrary.MailsTemplate>(mailsTemplate));
                    }
                    else
                    {
                        Mapper.Map(mailsTemplate, dbProp);
                    }
                    uow.Complete();
                    //_mailPropertyChanged = true;
                    return true;
                }
                catch (Exception exception)
                {
                    //todo: Залогировать
                    return false;
                }
            }
        }

        public bool DeleteMailTemplate(MailTemplate mailTemplate)
        {
            if (mailTemplate.TemplateId == 0) return false;
            using (var uow = new UnitOfWork())
            {
                try
                {
                    var dbTemplate = uow.Templates.Find(template => template.Templateid == mailTemplate.TemplateId).FirstOrDefault();
                    if (dbTemplate == null)
                    {
                        return false;
                    }
                    else
                    {
                        uow.Templates.Remove(dbTemplate);
                    }
                    uow.Complete();
                    _mailPropertyChanged = true;
                    return true;
                }
                catch (Exception exception)
                {
                    //todo: Залогировать
                    return false;
                }
            }
        }
        #endregion



        #region FSIO

        public string GetHtmlFromFile(string path,Encoding encoding)
        {
            if (String.IsNullOrWhiteSpace(path)) return "";
            encoding = encoding ?? Encoding.GetEncoding("windows-1251");
            try
            {
                return File.ReadAllText(path, encoding);
            }
            catch (Exception)
            {
                //todo: Залогировать
                return "";
            }
        }

        #endregion
    }
}
