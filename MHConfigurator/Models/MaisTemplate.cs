using System;
using System.Text;
using MugenMvvmToolkit.Models;

namespace MHConfigurator.Models
{
    [Serializable]
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
            get
            {
                if (string.IsNullOrWhiteSpace(TemplateBody)) return "";
                return TemplateBody.Replace(@"charset=windows-1251", @"charset=UTF-8");
            }
            set
            {
                TemplateBody = value.Replace(@"charset=UTF-8", @"charset=windows-1251");
                OnPropertyChanged();
                OnPropertyChanged(nameof(TemplateBody));
            }
        }

        public string FullDescription () => TemplateDescription + " (" + TemplateId + ")";


        public bool Useful
        {
            get { return _useful; }
            set
            {
                if (value == _useful) return;
                _useful = value;
                OnPropertyChanged();
            }
        }


        private readonly int _guid = new Guid().GetHashCode();
        private int _templateId;
        private string _templateDescription;
        private string _templateBody;
        private bool _useful;

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
            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (obj is MailTemplate) return Equals((MailTemplate) obj);
            if (obj.GetType() != GetType()) return false;
            return Equals((MailTemplate) obj);
        }

        public override int GetHashCode()
        {
            return _guid;
        }

        public static bool operator ==(MailTemplate left, MailTemplate right)
        {
            if (ReferenceEquals(null, left) && ReferenceEquals(null, right)) return true;
            if (ReferenceEquals(null, left)) return false;
            if (ReferenceEquals(null, right)) return false;
            if (ReferenceEquals(left, right)) return true;
            return left.Equals(right);
        }

        public static bool operator !=(MailTemplate left, MailTemplate right)
        {
            return !(left == right);
        }
    }
}
