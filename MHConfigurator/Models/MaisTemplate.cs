using System;
using System.Text;
using MugenMvvmToolkit.Models;

namespace MHConfigurator.Models
{
    public class MailTemplate : NotifyPropertyChangedBase
    {
        public int TemplateId
        {
            get { return _templateId; }
            set
            {
                if (TemplateId == value) return;
                _templateId = value;
                OnPropertyChanged();
            }
        }

        public string TemplateDescription
        {
            get { return _templateDescription; }
            set
            {
                if (_templateDescription == value) return;
                _templateDescription = value;
                OnPropertyChanged();
            }
        }

        public string TemplateBody
        {
            get { return _templateBody; }
            set
            {
                if (_templateBody == value) return;
                _templateBody = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TemplateBodyRusFix));
            }
        }

        public string TemplateBodyRusFix
        {
            get { return TemplateBody.Replace(@"charset=windows-1251", @"charset=UTF-8"); }
            set
            {
                TemplateBody = value.Replace(@"charset=UTF-8", @"charset=windows-1251");
                OnPropertyChanged();
                OnPropertyChanged(nameof(TemplateBody));
            }
        }

        /*
        private string GetBodyText()
        {
            return Encoding.GetEncoding("windows-1251")
                .GetString(Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("windows-1251"), TemplateBody))
                .Replace(@"charset=windows-1251", @"charset=UTF-8");
        }
        */


        public string FullDescription => TemplateDescription + " (" + TemplateId + ")";

        private readonly int _guid = new Guid().GetHashCode();
        private int _templateId;
        private string _templateDescription;
        private string _templateBody;

        public MailTemplate()
        {
            /*
            TemplateId = 0;
            TemplateDescription = "";
            TemplateBody = null;
            */
        }

        protected bool Equals(MailTemplate other)
        {
            return TemplateId == other.TemplateId && string.Equals(TemplateDescription, other.TemplateDescription) && Equals(TemplateBody, other.TemplateBody);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MailTemplate) obj);
        }

        public override int GetHashCode()
        {
            return _guid;
        }

        public static bool operator ==(MailTemplate left, MailTemplate right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MailTemplate left, MailTemplate right)
        {
            return !Equals(left, right);
        }
    }
}
