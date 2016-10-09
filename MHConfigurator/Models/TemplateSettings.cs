namespace MHConfigurator.Models
{
    public class MailsTemplate
    {
        public int Templateid;
        public string Templadescription;
        public byte[] TemplateBody;

        public MailsTemplate()
        {
            Templateid = 0;
            Templadescription = "";
            TemplateBody = null;
        }

    }
}
