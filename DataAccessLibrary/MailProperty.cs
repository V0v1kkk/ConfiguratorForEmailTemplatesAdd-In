//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataAccessLibrary
{
    using System;
    using System.Collections.Generic;
    
    public partial class MailProperty
    {
        public long ButtonID { get; set; }
        public string Description { get; set; }
        public bool Only6565 { get; set; }
        public bool Only6690 { get; set; }
        public bool FillTO { get; set; }
        public string TO { get; set; }
        public bool FillCopy { get; set; }
        public string Copy { get; set; }
        public bool FillHideCopy { get; set; }
        public string HideCopy { get; set; }
        public bool FillSubject { get; set; }
        public string Subject { get; set; }
        public bool HighImportance { get; set; }
        public bool Reminder { get; set; }
        public string ReminderTime { get; set; }
        public bool FillBody { get; set; }
        public long BodyID { get; set; }
        public string Zametka1 { get; set; }
        public string Zametka2 { get; set; }
        public string Zametka3 { get; set; }
    
        public virtual MailsTemplate MailsTemplate { get; set; }
    }
}
