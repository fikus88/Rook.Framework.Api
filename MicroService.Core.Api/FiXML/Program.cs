using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FiXML
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlDocument document = new XmlDocument();
            document.Load(args[0]);
            XmlNode node = document.SelectSingleNode("/Project/PropertyGroup/Version");
            DateTime utcNow = DateTime.UtcNow;
            string currentVersion = node.InnerText;
            string[] split = currentVersion.Split('.');
            if (split[0] != utcNow.Year.ToString() || split[1] != utcNow.Month.ToString() ||
                split[2] != utcNow.Day.ToString())
                split[3] = "0";
            node.InnerText = $"{utcNow.Year}.{utcNow.Month}.{utcNow.Day}.{int.Parse(split[3]) + 1}";
            document.Save(args[0]);
        }
    }
}
