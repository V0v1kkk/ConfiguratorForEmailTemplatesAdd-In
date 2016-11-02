using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace MHConfigurator.Models
{
    [Serializable]
    public class MailTemplateEditable : MailTemplate //Декоратор над MailTemplate для удобного редактирования HTML шаблона
    {
        public MailTemplateEditable()
        {
            _upPartsStack = new Stack<Tuple<string, string>>();
            _downPartsStack = new Stack<Tuple<string, string>>();
        }

        public MailTemplateEditable(MailTemplate mailTemplate) : this()
        {
            TemplateId = mailTemplate.TemplateId;
            TemplateDescription = mailTemplate.TemplateDescription;
            TemplateBody = mailTemplate.TemplateBody;


        }

        public bool PartsExist
        {
            get
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(TemplateBody);

                var paragraphs = doc.DocumentNode.SelectNodes("//p");
                if (paragraphs != null)
                {
                    return paragraphs.Any(tag => tag.Attributes["class"] != null && string.Compare(tag.Attributes["class"].Value, "MsoNormal", StringComparison.InvariantCultureIgnoreCase) == 0);
                }
                return false;
            }
        }

        public int DownIndent
        {
            get { return _downIndent; }
            set
            {
                if (value < 0) value = 0;

                if (value - _downIndent > 0) //Если значение увеличилось
                {
                    _downIndent += CutDown(value - _downIndent);
                    OnPropertyChanged();
                }
                else if (_downIndent - value > 0) //если уменьшилось
                {
                    _downIndent -= BackDown(_downIndent - value);
                    OnPropertyChanged();
                }
            }

        }

        public int UpIndent
        {
            get { return _upIndent; }
            set
            {
                if (value < 0) value = 0;

                if (value - _upIndent > 0) //Если значение увеличилось
                {
                    _upIndent += CutUp(value - _upIndent);
                    OnPropertyChanged();
                }
                else if (_upIndent - value > 0) //если уменьшилось
                {
                    _upIndent -= BackUp(_upIndent - value);
                    OnPropertyChanged();
                }
            }
        }

        private readonly Stack<Tuple<string, string>> _upPartsStack;
        private Stack<Tuple<string, string>> _downPartsStack;
        private int _upIndent;
        private int _downIndent;


        public int CutUp(int count = 1)
        {
            int cuttingcounter = 0;

            var doc = new HtmlDocument();
            doc.LoadHtml(TemplateBody);

            var paragraphs = doc.DocumentNode.SelectNodes("//p");
            if (paragraphs != null)
            {
                
                foreach (var tag in paragraphs)
                {
                    //if (tag.Attributes["class"] != null && string.Compare(tag.Attributes["class"].Value, "MsoNormal", StringComparison.InvariantCultureIgnoreCase) == 0)
                    //if (tag.Attributes["class"] != null && !((tag.Attributes["class"].Value.Contains("WordSection")) || (tag.Attributes["class"].Value.Contains("Table"))))
                    if (cuttingcounter < count)
                    {
                        string replasestring = $"<!-- 0{UpIndent + cuttingcounter} -->";
                        _upPartsStack.Push(new Tuple<string, string>(tag.OuterHtml, replasestring));

                        var parent = tag.ParentNode;
                        HtmlNode mark = doc.CreateTextNode(replasestring);
                        parent.ReplaceChild(mark, tag);

                        TemplateBody = doc.DocumentNode.OuterHtml;

                        cuttingcounter++;
                    }
                }
            }
            return cuttingcounter;
        }


        public int BackUp(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (_upPartsStack.Count == 0) return i;
                var tuple = _upPartsStack.Pop();
                TemplateBody = TemplateBody.Replace(tuple.Item2, tuple.Item1);
            }
            return count;
        }



        public int CutDown(int count = 1)
        {
            int cuttingcounter = 0;

            var doc = new HtmlDocument();
            doc.LoadHtml(TemplateBody);

            var paragraphs = doc.DocumentNode.SelectNodes("//p");
            if (paragraphs != null)
            {
                foreach (var tag in paragraphs.Reverse())
                {
                    if (cuttingcounter < count)
                    {
                        string replasestring = $"<!-- 1{DownIndent + cuttingcounter} -->";
                        _downPartsStack.Push(new Tuple<string, string>(tag.OuterHtml, replasestring));

                        var parent = tag.ParentNode;
                        HtmlNode mark = doc.CreateTextNode(replasestring);
                        parent.ReplaceChild(mark, tag);

                        TemplateBody = doc.DocumentNode.OuterHtml;

                        cuttingcounter++;
                    }
                }
            }
            return cuttingcounter;
        }


        public int BackDown(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (_downPartsStack.Count == 0) return i;
                var tuple = _downPartsStack.Pop();
                TemplateBody = TemplateBody.Replace(tuple.Item2, tuple.Item1);
            }
            return count;
        }


        public void ClearIndents()
        {
            _upIndent = 0;
            _downIndent = 0;
            _upPartsStack.Clear();
            _downPartsStack.Clear();
            OnPropertyChanged("UpIndent");
            OnPropertyChanged("DownIndent");
        }

        public string GetCleanHtmlForSave()
        {
            if (string.IsNullOrWhiteSpace(TemplateBody)) return "";
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(TemplateBody);
            doc.DocumentNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Comment).ToList().ForEach(n => n.Remove());
            return doc.DocumentNode.OuterHtml;
        }
    }
}