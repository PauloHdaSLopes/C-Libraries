using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Paulo.Console.util.models
{
    public class HtmlUtil
    {
        HtmlDocument doc = new HtmlDocument();

        public void SetHtmlDocument(string html)
        {
            doc.LoadHtml(html);
        }

        public List<string> GetElementAttribute(string tag, string attribute)
        {
            List<string> lstElements = new List<string>();

            try
            {
                var nodes = doc.DocumentNode.Descendants(tag);

                foreach (var item in nodes)
                {
                    if (item.Attributes[attribute]?.Value != "")
                        lstElements.Add(item.Attributes[attribute]?.Value);
                }
              
                return lstElements;
            }
            catch (NullReferenceException)
            {
                throw new Exception(string.Format("Attribute '{0}' not found on tag '{1}'",attribute,tag));
            }
        }

        public List<string> GetTagValue(string tag)
        {
            List<string> lstElements = new List<string>();

            try
            {
                var nodes = doc.DocumentNode.Descendants(tag);

                foreach (var item in nodes)
                {
                    lstElements.Add(item.InnerText);
                }
                return lstElements;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetInnerText()
        {
            return doc.DocumentNode.InnerText;
        }

    }
}