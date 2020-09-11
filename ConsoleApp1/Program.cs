using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;

namespace ConsoleApp1
{
    class Program
    {
        private static readonly string filesDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Files");
        private static readonly string categoriesDirectory = Path.Combine(filesDirectory, "Categories");
        

        static void Main(string[] args)
        {
            GenerateOutputForWiki();
        }

        static void GenerateUncategorizedCodes()
        {
            // This file is manually created from https://api.github.com/emojis whenever we want to update the list
            // Last update was from v8 of the api
            var codes = File.ReadAllLines(Path.Combine(filesDirectory, "Codes.txt"));

            var categorizedCodes = new List<string>();
            var fileInfos = Directory.GetFiles(categoriesDirectory).Select(fileName => new FileInfo(fileName));
            foreach (var fileInfo in fileInfos)
            {
                var lines = File.ReadAllLines(fileInfo.FullName);
                categorizedCodes.AddRange(lines);
            }

            var uncategorizedCodes = new List<string>();
            foreach (var code in codes)
                if (!categorizedCodes.Contains(code))
                    uncategorizedCodes.Add(code);

            File.WriteAllLines(Path.Combine(filesDirectory, "UncategorizedCodes.txt"), uncategorizedCodes);
        }

        static void GenerateOutputForWiki()
        {
            var fileInfos = Directory.GetFiles(categoriesDirectory).Select(fileName => new FileInfo(fileName));
            
            var result = new List<string>
            {
                ":warning: If you see any emoji in the wrong categories or in the wrong order/place, please see [How to change](https://github.com/itecompro/markdown-emoji-cheatsheet#how-to-change). It will be fixed.",
                "",
                "# Categories",
            };

            foreach (var fileInfo in fileInfos)
            {
                var name = GetFileNameWithoutExtension(fileInfo);
                result.Add($"- [{name}](#{name})");
            }
            result.Add("");

            foreach (var fileInfo in fileInfos)
            {
                var name = GetFileNameWithoutExtension(fileInfo);
                result.Add($"## {name}");
                result.Add("");

                var lines = File.ReadAllLines(fileInfo.FullName);
                result.AddRange(GetTable(lines));
                
                result.Add("");
            }

            var wikiDirectory = Path.Combine(filesDirectory, "Wiki");
            Directory.CreateDirectory(wikiDirectory);
            File.WriteAllLines(Path.Combine(wikiDirectory, "Home.md"), result);
        }

        static string GetFileNameWithoutExtension(FileInfo fileInfo)
        {
            return fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.'));
        }

        static IEnumerable<string> GetTable(IEnumerable<string> codes)
        {
            var result = new List<string>();
            result.Add("|   |   |   |");
            result.Add("|---|---|---|");

            var counter = 1;
            var resultLine = new StringBuilder();
            foreach (var code in codes)
            {
                resultLine.Append($"| {GetCellContent(code)} ");
                if (counter == 3)
                {
                    resultLine.Append("|");
                    result.Add(resultLine.ToString());
                    resultLine = new StringBuilder();
                    counter = 1;
                }
                else
                    counter++;
            }

            if (counter != 1)
                result.Add(resultLine.ToString());

            return result;
        }

        static string GetCellContent(string code)
        {
            return $":{code}: `:{code}:`";
        }
    }
}
