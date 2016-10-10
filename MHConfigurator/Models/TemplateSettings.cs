namespace MHConfigurator.Models
{
    public class MailsTemplate
    {
        public int TemplateId;
        public string TemplateDescription;
        public byte[] TemplateBody;

        public MailsTemplate()
        {
            TemplateId = 0;
            TemplateDescription = "";
            TemplateBody = null;
        }

    }
}
