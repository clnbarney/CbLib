using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;

namespace CbLib
{
    public static class HtmlAgilityPackExtensions
    {
        public static List<HtmlNode> GetNodeByClassName(this HtmlNode node, string className)
        {
            var nodes = new List<HtmlNode>();

            try
            {
                nodes = node.Descendants(0).Where(x => x.HasClass(className)).ToList();
            }
            catch { }

            return nodes;
        }
    }
}
