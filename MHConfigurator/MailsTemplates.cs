//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MHConfigurator
{
    using System;
    using System.Collections.Generic;
    
    public partial class MailsTemplates
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MailsTemplates()
        {
            this.MailPropertys = new HashSet<MailPropertys>();
        }
    
        public long Templateid { get; set; }
        public string Templadescription { get; set; }
        public byte[] TemplateBody { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MailPropertys> MailPropertys { get; set; }
    }
}
