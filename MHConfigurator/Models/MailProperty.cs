using System;
using MugenMvvmToolkit.Models;

namespace MHConfigurator.Models
{
    [Serializable]
    public class MailProperty //: NotifyPropertyChangedBase
    {
        private int _buttonId;
        private string _description;

        public readonly int Guid = System.Guid.NewGuid().GetHashCode();
        //private int _buttonId;

        public int ButtonID
        {
            get { return _buttonId; }
            set
            {
                _buttonId = value;
                //PropertyChanged()
            }
        }


        public string Description { get; set; }
        public bool Only6565 { get; set; } //Изменять ли ящик на 6565 при заполнении по шаблону
        public bool Only6690 { get; set; } //Изменять ли ящик на 6690 при заполнении по шаблону
        public bool FillTO { get; set; } //Заполнять адресатов
        public string TO { get; set; } //Адресаты
        public bool FillCopy { get; set; } //Заполнять копию
        public string Copy { get; set; } // Копия
        public bool FillHideCopy { get; set; } //Заполнять скрытую копию
        public string HideCopy { get; set; } //Скрытая копия
        public bool FillSubject { get; set; } //Заполнять тему (на случай ответа на письмо)
        public string Subject { get; set; } //Тема
        public bool HighImportance { get; set; } //Высокая важность
        public bool Reminder { get; set; } //Ставить ли напоминание
        public DateTime ReminderTime { get; set; } //Время напоминания
        public bool FillBody { get; set; } //Заполнять текст письма
        public int BodyID { get; set; } //Текст письма
        public string Zametka1 { get; set; } //Заметка 1я строка
        public string Zametka2 { get; set; } //Заметка 2я строка
        public string Zametka3 { get; set; } //Заметка 3я строка
        public bool Useful { get; set; } //Используется ли шаблон

        public MailProperty()
        {
            ButtonID = 0;
            Description = "";
            Only6565 = true;
            Only6690 = false;
            FillTO = false;
            TO = "";
            FillCopy = false;
            Copy = "";
            FillHideCopy = false;
            HideCopy = "";
            FillSubject = false;
            Subject = "";
            HighImportance = false;
            Reminder = false;
            ReminderTime = DateTime.Now;
            FillBody = false;
            BodyID = 0;
            Zametka1 = "";
            Zametka2 = "";
            Zametka3 = "";
            Useful = false;

        }
        public MailProperty(int buttonid, string description, bool only6565, bool only6690, bool fillTO, string tO, bool fillCopy, string copy, bool fillHideCopy, string hideCopy,
            bool fillsubject, string subject, bool highimportance, bool reminder, DateTime remindertime, bool fillBody,
            string zametka1, string zametka2, string zametka3, int bodyID, bool useful)
        {
            ButtonID = buttonid;
            Description = description;
            Only6565 = only6565;
            Only6690 = only6690;
            FillTO = fillTO;
            TO = tO;
            FillCopy = fillCopy;
            Copy = copy;
            FillHideCopy = fillHideCopy;
            HideCopy = hideCopy;
            FillSubject = fillsubject;
            Subject = subject;
            HighImportance = highimportance;
            Reminder = reminder;
            ReminderTime = remindertime;
            FillBody = fillBody;
            BodyID = bodyID;
            Zametka1 = zametka1;
            Zametka2 = zametka2;
            Zametka3 = zametka3;
            Useful = useful;
        }


        protected bool Equals(MailProperty other)
        {
            return ButtonID == other.ButtonID && string.Equals(Description, other.Description) && Only6565 == other.Only6565 && Only6690 == other.Only6690 && FillTO == other.FillTO && string.Equals(TO, other.TO) && FillCopy == other.FillCopy && string.Equals(Copy, other.Copy) && FillHideCopy == other.FillHideCopy && string.Equals(HideCopy, other.HideCopy) && FillSubject == other.FillSubject && string.Equals(Subject, other.Subject) && HighImportance == other.HighImportance && Reminder == other.Reminder && ReminderTime.Equals(other.ReminderTime) && FillBody == other.FillBody && BodyID == other.BodyID && string.Equals(Zametka1, other.Zametka1);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MailProperty) obj);
        }

        
        public override int GetHashCode()
        {
            return Guid;
            /*
            unchecked
            {
                var hashCode = ButtonID;
                hashCode = (hashCode*397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Only6565.GetHashCode();
                hashCode = (hashCode*397) ^ Only6690.GetHashCode();
                hashCode = (hashCode*397) ^ FillTO.GetHashCode();
                hashCode = (hashCode*397) ^ (TO != null ? TO.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ FillCopy.GetHashCode();
                hashCode = (hashCode*397) ^ (Copy != null ? Copy.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ FillHideCopy.GetHashCode();
                hashCode = (hashCode*397) ^ (HideCopy != null ? HideCopy.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ FillSubject.GetHashCode();
                hashCode = (hashCode*397) ^ (Subject != null ? Subject.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ HighImportance.GetHashCode();
                hashCode = (hashCode*397) ^ Reminder.GetHashCode();
                hashCode = (hashCode*397) ^ ReminderTime.GetHashCode();
                hashCode = (hashCode*397) ^ FillBody.GetHashCode();
                hashCode = (hashCode*397) ^ BodyID;
                hashCode = (hashCode*397) ^ (Zametka1 != null ? Zametka1.GetHashCode() : 0);
                return hashCode;
            }
            */
        }
        
        public static bool operator ==(MailProperty left, MailProperty right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MailProperty left, MailProperty right)
        {
            return !Equals(left, right);
        }
    }
}