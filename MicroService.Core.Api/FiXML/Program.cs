using System;
using System.IO;
using System.Xml;

namespace FiXML
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Path: " + args[0]);
            foreach (string directory in Directory.GetDirectories(args[0]))
                foreach (string filename in Directory.GetFiles(directory, "*.csproj"))
                    UpdateProjectFile(filename);
        }

        private static void UpdateProjectFile(string filename)
        {
            Console.WriteLine($"Opening project file {filename}");
            XmlDocument document = new XmlDocument();
            document.Load(filename);
            Console.WriteLine($"Getting Version node");
            XmlNode node = document.SelectSingleNode("/Project/PropertyGroup/Version");
            if (node == null)
            {
                Console.WriteLine("No Version node");
                return;
            }
            DateTime utcNow = DateTime.UtcNow;
            string currentVersion = node.InnerText;
            Console.WriteLine($"Found current version {currentVersion}");
            string[] split = currentVersion.Split('.');
            if (split[0] != utcNow.Year.ToString() || split[1] != utcNow.Month.ToString() ||
                split[2] != utcNow.Day.ToString())
            {
                Console.WriteLine("Date changed so resetting build");
                split[3] = "0";
            }
            node.InnerText = $"{utcNow.Year}.{utcNow.Month}.{utcNow.Day}.{int.Parse(split[3]) + 1}";
            Console.WriteLine($"Changed version to {node.InnerText}");
            Console.WriteLine("Saving csproj file");
            document.Save(filename);
            Console.WriteLine("Done");
        }
    }
}
