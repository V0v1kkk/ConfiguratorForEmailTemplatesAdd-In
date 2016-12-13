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
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantNameQualifier

namespace MHConfigurator
{
    //TODO: Передалать на обсервабл коллекциях
    // ReSharper disable once InconsistentNaming
    public class DAL
    {
        private DAL()
        {
            //Mapper.Initialize(cfg => cfg.CreateMap<DataAccessLibrary.MailProperties, MailProperties>().ForMember(destinationMember => destinationMember.Useful, x => x.MapFrom(m => GetDAL().GetUsedPropertyNumbers().Contains(m.ButtonID))));
            
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


        #region Properties

        #region Fields

        private static readonly object Locker;
        static private DAL _instance;

        private List<MailProperty> _mailPropertys; //Возможно нужно модель унаследовать от модели
        private List<MailTemplate> _mailTemplates;
        private List<long> _usedProperties;
        private List<long> _usedTemplates;
        private List<MailTemplate> _emptyMailTemplates;

        private bool _generatedChanged; //От этого поля будет зависеть несколько полей (список задействованных шаблонов, сами генератед объекты (м.б. в виде дерева))
        private bool _mailPropertiesChanged; //Будет устанавливатся в случае записи в таблицу
        private bool _mailTemplatesChanged;

        private string _databasePath;
        private List<PanelElement> _panelElements;
        private bool _panelElementsChanged;

        #endregion

        public string DatabasePath
        {
            get { return _databasePath; }
            set
            {
                if (_databasePath != value)
                {
                    //todo: сделать так для всех получаемых сущностей
                    _generatedChanged = true;
                    _mailPropertiesChanged = true;
                    _mailTemplatesChanged = true;
                }
                _databasePath = value;
                
            }
        }

        public List<MailProperty> MailProperties
        {
            get
            {
                if (_mailPropertys == null)
                {
                    _mailPropertys = GetMailProperties();
                    _mailPropertiesChanged = false;
                }
                else if (_mailPropertiesChanged)
                {
                    _mailPropertys = GetMailProperties();
                    _mailPropertiesChanged = false;
                }
                return _mailPropertys;
            }
            private set { _mailPropertys = value; }
        }

        public List<MailTemplate> MailTemplates
        {
            get
            {
                if (_mailTemplates == null)
                {
                    _mailTemplates = GetMailsTemplates();
                    _mailTemplatesChanged = false;
                }
                else if (_mailTemplatesChanged)
                {
                    _mailTemplates = GetMailsTemplates();
                    _emptyMailTemplates = GetEmptyMailTemplates();
                    _mailTemplatesChanged = false;
                }
                return _mailTemplates;
            }
        }

        public List<PanelElement> PanelElements
        {
            get
            {
                if (_panelElements == null)
                {
                    _panelElements = GetPanelElements();
                    _panelElementsChanged = false;
                }
                else if (_panelElementsChanged)
                {
                    _panelElements = GetPanelElements();
                    _panelElementsChanged = false;
                }
                return _panelElements;
            }
        }

        


        public List<long> UsedProperties
        {
            get
            {
                if (_usedProperties == null)
                {
                    _usedProperties = GetUsedPropertyNumbers();
                    _generatedChanged = false;
                }
                else if (_generatedChanged)
                {
                    _usedProperties = GetUsedPropertyNumbers();
                    //MailProperties = GetMailProperties(); //todo: Update Generated
                    _generatedChanged = false;
                }
                return _usedProperties;                
            }
        }

        public List<long> UsedTemplates
        {
            get
            {
                if (_usedTemplates == null)
                {
                    _usedTemplates = GetUsedTemplateNumbers();
                    _mailPropertiesChanged = false;
                }
                else if (_mailPropertiesChanged)
                {
                    _usedTemplates = GetUsedTemplateNumbers();
                    MailProperties = GetMailProperties();
                    _mailPropertiesChanged = false;
                }
                return _usedTemplates;
            }
        }



        public List<MailTemplate> EmptyMailTemplates
        {
            get
            {
                if (_emptyMailTemplates == null)
                {
                    _emptyMailTemplates = GetEmptyMailTemplates();
                    _mailTemplatesChanged = false;
                }
                else if (_mailTemplatesChanged)
                {
                    _emptyMailTemplates = GetEmptyMailTemplates();
                    _mailTemplates = GetMailsTemplates();
                    _mailTemplatesChanged = false;
                }
                return _emptyMailTemplates;
            }
        }
        #endregion


        #region Work with PanelElements


        private List<PanelElement> GetPanelElements()
        {
            using (var uow = new UnitOfWork(DatabasePath))
            {
                List<PanelElement> answer = new List<PanelElement>();
                try
                {
                    foreach (Generated panelElement in uow.Generated.GetAll())
                    {
                        if (panelElement.IsMenu)
                        {
                            answer.Add(new MenuElement(panelElement.Name,panelElement.ParentName,panelElement.Label,
                                panelElement.SuperTip,panelElement.ScreenTip,panelElement.Image));
                        }
                        else if(panelElement.IsButton)
                        {
                            answer.Add(new ButtonElement(panelElement.Name,panelElement.ParentName,panelElement.Label,panelElement.SuperTip,
                                panelElement.ScreenTip,panelElement.Image,(int?)panelElement.TemplateNO));
                        }
                        else if(panelElement.IsGroup)
                        {
                            answer.Add(new GroupElement(panelElement.Name,panelElement.ParentName,panelElement.Label));
                        }
                        else if(panelElement.IsSeparator)
                        {
                            answer.Add(new SeparatorElement(panelElement.Name,panelElement.ParentName));
                        }
                    }
                }
                catch (Exception)
                {
                    //todo: Залогировать
                }
                return PanelElement.CreateTree(answer); //convert flat list to tree
            }
        }



        #endregion


        #region Work with MailsProperties

        public MailProperty GetMailPropertyById(int id)
        {
            using (var uow = new UnitOfWork(DatabasePath))
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

        private List<MailProperty> GetMailProperties()
        {
            using (var uow = new UnitOfWork(DatabasePath))
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
            using (var uow = new UnitOfWork(DatabasePath))
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
                    _mailPropertiesChanged = true;
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
            using (var uow = new UnitOfWork(DatabasePath))
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
                    _mailPropertiesChanged = true;
                    return true;
                }
                catch (Exception)
                {
                    //todo: Залогировать
                    return false;
                }
            }
        }

        public List<long> GetUsedPropertyNumbers()
        {
            using (var uow = new UnitOfWork(DatabasePath))
            {
                try
                {
                    var answer = uow.Generated.GetUsedProperties();
                    return answer;
                }
                catch (Exception)
                {
                    //todo: Залогировать
                    return null;
                }

            }
        }

        #endregion

        #region MailTemplates

        public MailTemplate GetMailTemplateById(int id)
        {
            using (var uow = new UnitOfWork(DatabasePath))
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

        private List<MailTemplate> GetMailsTemplates()
        {
            //todo: Заменить автомаппером с использованием профилей. Или не нужно? Просто другой медод для получения непустых объектов. А при загрузке формы шаблонов перевыгружать то, что передано в конструктор
                List<MailTemplate> list = new List<MailTemplate>();
                try
                {
                    using (var uow = new UnitOfWork(DatabasePath))
                    { 
                        var templates = uow.Templates.GetAll(); //test where

                        list.AddRange(from mailsTemplate in templates where mailsTemplate.Templateid > 0 select Mapper.Map<DataAccessLibrary.MailsTemplate, MailTemplate>(mailsTemplate));

                        //list.AddRange(templates.Where(x => x.Templateid != 0).Select(Mapper.Map<DataAccessLibrary.MailsTemplate, MailTemplate>));
                        //list.AddRange(templates.Select(Mapper.Map<DataAccessLibrary.MailsTemplate, MailTemplate>));
                        return list;
                    }
                }
                catch (Exception)
                {
                    //todo: Залогировать
                    return null;
                }
        }

        private List<MailTemplate> GetEmptyMailTemplates()
        {
            //todo: Заменить автомаппером с использованием профилей. Или не нужно? Просто другой медод для получения непустых объектов. А при загрузке формы шаблонов перевыгружать то, что передано в конструктор
            using (var uow = new UnitOfWork(DatabasePath))
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

        public bool SaveMailTemplate(MailTemplate mailsTemplate, bool newItem = false)
        {
            if (mailsTemplate.TemplateId == 0) return false;
            using (var uow = new UnitOfWork(DatabasePath))
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
                    _mailTemplatesChanged = true;
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
            using (var uow = new UnitOfWork(DatabasePath))
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
                    _mailTemplatesChanged = true;
                    return true;
                }
                catch (Exception exception)
                {
                    //todo: Залогировать
                    return false;
                }
            }
        }

        public List<long> GetUsedTemplateNumbers()
        {
            try
            {
                List<long> answer;
                using (var uow = new UnitOfWork(DatabasePath))
                {
                
                    answer = uow.Propertys.GetUsedTemplates();
                    return answer;
                }
            }
            catch (Exception)
            {
                //todo: Залогировать
                return null;
            }
        }

        #endregion

        #region Work with connection to DB

        public bool TestConnection()
        {
            using (var uow = new UnitOfWork(DatabasePath))
            {
                return (uow.TestDbExist(DatabasePath)) && (GetEmptyMailTemplates() != null);
            }
        }

        public bool TestConnection(string databasePath)
        {
            var temp = DatabasePath;
            DatabasePath = databasePath;

            using (var uow = new UnitOfWork(DatabasePath))
            {
                if (uow.TestDbExist(databasePath) && GetEmptyMailTemplates() != null)
                {
                    DatabasePath = temp;
                    return true;
                }
                DatabasePath = temp;
                return false;
            }
        }

        #endregion

        #region FileSystemIO

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

        public bool MakeBackUpDb(string pathToDb, string pathToBackup = null)
        {
            //todo:ведение журнала бекапов
            try
            {
                if (pathToBackup == null)
                    pathToBackup = Environment.GetEnvironmentVariable("LOCALAPPDATA") + @"\MailsTemplatesAddinConfigurator\backup";
                Directory.CreateDirectory(pathToBackup);
                pathToBackup += "\\";
                pathToBackup += $"{DateTime.Now:dd-MM-yyyy_hh-mm-ss}";
                pathToBackup += "_";
                pathToBackup += Path.GetFileName(pathToDb);
                File.Copy(pathToDb,pathToBackup);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
