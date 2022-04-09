using System.IO;
using OfficeOpenXml;

namespace Удаление_символов_из_данных
{
    class Program
    {
        static string[] symbols = { "\"", "#", "&", "(", ")", "*", "/", ":", ";", "@", 
            "[", "]", "^", "_", "~", "+", "<", "=", ">", "E", "F", "I", "L", "N", "Q", "S", "V", "W", 
            "Z", "А", "Б", "В", "Г", "Д", "Е", "И", "К", "Л", "Н", "П", "прмз", "прсх", "Р", "С", 
            "Т", "Ф", "Х", "Ш", "Ъ", "ю",};
        static string DeleteSymbols(string input)
        {
            foreach (var symbol in symbols)
                if (input.Contains(symbol))
                    input = input.Replace(symbol, "");

            return input.Trim();
        }
        static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var inputFile = new FileInfo("input.xlsx");
            var inputPackage = new ExcelPackage(inputFile);
            var inputSheet = inputPackage.Workbook.Worksheets[0];

            var outputFile = new FileInfo("output.xlsx");
            if (outputFile.Exists)
            {
                outputFile.Delete();
                outputFile = new FileInfo("output.xlsx");
            }
            var outputPackage = new ExcelPackage(outputFile);
            var outputSheet = outputPackage.Workbook.Worksheets.Add("Данные без символов");

            var i = 1;
            var j = 1;
            while (inputSheet.Cells[1, j].Value != null)
            {
                while (inputSheet.Cells[i, j].Value != null)
                {
                    outputSheet.Cells[i, j].Value = DeleteSymbols(inputSheet.Cells[i, j].Value.ToString());
                    i++;
                }
                j++;
                i = 1;
            }

            outputPackage.Save();
        }
    }
}
