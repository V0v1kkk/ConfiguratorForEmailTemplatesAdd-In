using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MugenMvvmToolkit;

namespace MHConfigurator
{
    public static class Helper
    {
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        public static string ReplaceMacros(string input) //Метод для замены макросов
        {
            //Добавлять длинные шаблоны только сверху. Короткие могут ломать длинные
            input = input.Replace(@"#%prevdaymonth", DateTime.Today.AddDays(-1).ToString("d MMMM",
                    CultureInfo.CreateSpecificCulture("ru-RU"))); // #%prevdaymonth - предыдущий день месяц и год в формате "14 мая"
            input = input.Replace(@"#%yearshortprevday", DateTime.Today.AddDays(-1).ToString("dd.MM.yy",
                    CultureInfo.CreateSpecificCulture("ru-RU"))); // #%yearshortprevday - предыдущий день месяц и год в формате "08.10.15"
            input = input.Replace(@"#%shortprevday", DateTime.Today.AddDays(-1).ToString("dd.MM",
                    CultureInfo.CreateSpecificCulture("ru-RU"))); // #%prevdayshort - предыдущий день месяц и год в формате "08.10"
            input = input.Replace(@"#%onlymonthprevday", DateTime.Today.AddDays(-1).ToString("MMMM",
                    CultureInfo.CreateSpecificCulture("ru-RU")).ToLower(CultureInfo.CreateSpecificCulture("ru-RU"))); // месяц к которому относится предыдущий день в формате "май"
            input = input.Replace(@"#%onlydateprevday", DateTime.Today.AddDays(-1).ToString("dd",
                    CultureInfo.CreateSpecificCulture("ru-RU"))); // #%prevdayonlydate - предыдущий день месяц и год в формате "08"
            input = input.Replace(@"#%tomorrowdaymonth", DateTime.Today.AddDays(1).ToString("d MMMM",
                    CultureInfo.CreateSpecificCulture("ru-RU"))); // #%tomorrowdaymonth - завтрашний день и месяц в формате "14 мая"
            input = input.Replace(@"#%daymonth", DateTime.Now.ToString("d MMMM",
                    CultureInfo.CreateSpecificCulture("ru-RU"))); // #%daymonth - день и месяц в формате "14 мая"
            input = input.Replace(@"#%prevday", DateTime.Today.AddDays(-1).ToString("dd.MM.yyyy",
                    CultureInfo.CreateSpecificCulture("ru-RU"))); // #%prevday - предыдущий день месяц и год в формате "01.05.2015"
            input = input.Replace(@"#%tomorrow", DateTime.Today.AddDays(1).ToString("dd.MM.yyyy",
                    CultureInfo.CreateSpecificCulture("ru-RU"))); // #%tomorrow - завтрашний день месяц и год в формате "01.05.2015"

            return input;
        }

        public static string DeleteLinksInHtml(string html)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                doc.DocumentNode.SelectNodes("//link").ForEach(x => x.Remove());
                return doc.DocumentNode.OuterHtml;
            }
            catch (Exception exception)
            {
                //todo: log
                return "";
            }
        }
    }
}
