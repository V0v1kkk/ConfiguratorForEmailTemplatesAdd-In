using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace MHConfigurator.Models
{
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
                if(_downIndent==value) return;
                if ((value == _downIndent + 1) && (CutDown()))
                {
                    _downIndent = value;
                    OnPropertyChanged();
                }
                else if (value == _downIndent - 1)
                {
                    BackDown();
                    _downIndent = value;
                    OnPropertyChanged();
                }
            }

        }

        public int UpIndent
        {
            get { return _upIndent; }
            set
            {
                if(_upIndent==value) return;
                if ((value == _upIndent + 1)&&(CutUp()))
                {
                    _upIndent = value;
                    OnPropertyChanged();
                }
                else if (value == _upIndent - 1)
                {
                    BackUp();
                    _upIndent = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly Stack<Tuple<string, string>> _upPartsStack;
        private Stack<Tuple<string, string>> _downPartsStack;
        private int _upIndent;
        private int _downIndent;


        public bool CutUp()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(TemplateBody);

            var paragraphs = doc.DocumentNode.SelectNodes("//p");
            if (paragraphs != null)
            {
                foreach (var tag in paragraphs)
                {
                    if (tag.Attributes["class"] != null && string.Compare(tag.Attributes["class"].Value, "MsoNormal", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        string replasestring = $"< !--0{UpIndent}-- >";
                        _upPartsStack.Push(new Tuple<string, string>(tag.ToString(), replasestring));

                        var parent = tag.ParentNode;
                        HtmlNode mark = doc.CreateTextNode(replasestring);
                        parent.ReplaceChild(mark, tag);

                        doc.Save(TemplateBody);

                        return true;
                    }
                }
            }
            return false;
        }


        public void BackUp()
        {
            var tuple = _upPartsStack.Pop();
            TemplateBody = TemplateBody.Replace(tuple.Item2, tuple.Item1);
        }



        public bool CutDown()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(TemplateBody);

            var paragraphs = doc.DocumentNode.SelectNodes("//p");
            if (paragraphs != null)
            {
                foreach (var tag in paragraphs.Reverse())
                {
                    if (tag.Attributes["class"] != null && string.Compare(tag.Attributes["class"].Value, "MsoNormal", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        string replasestring = $"< !--1{DownIndent}-- >";
                        _downPartsStack.Push(new Tuple<string, string>(tag.ToString(), replasestring));

                        var parent = tag.ParentNode;
                        HtmlNode mark = doc.CreateTextNode(replasestring);
                        parent.ReplaceChild(mark, tag);

                        doc.Save(TemplateBody);

                        return true;
                    }
                }
            }
            return false;
        }


        public void BackDown()
        {
            var tuple = _downPartsStack.Pop();
            TemplateBody = TemplateBody.Replace(tuple.Item2, tuple.Item1);
        }
    }
}