using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using Расчеты_для_курсовой;

namespace Функц._предобработка_для_долгосрочного_прогноза
{
    class Program
    {
        public static Dictionary<string, List<(double Max, double Min)>> Bounds = new()
        {
            { "Q1", new List<(double Max, double Min)>() { (640, 5.87), (0.8954452, 0.0199986) } },
            { "H1", new List<(double Max, double Min)>() { (439, 0), (0.4699451, 0.0099996) } },
            { "Q2", new List<(double Max, double Min)>() { (640, 5.87) } },
            { "H2", new List<(double Max, double Min)>() { (439, 0) } },
            { "Q3", new List<(double Max, double Min)>() { (640, 6.21) } },
            { "H3", new List<(double Max, double Min)>() { (439, 1) } }
        };

        public static Dictionary<string, List<int>> Conversions = new()
        {
            { "Q1", new List<int>() { 46, 13 } },
            { "H1", new List<int>() { 52, 13 } },
            { "Q2", new List<int>() { 69 } },
            { "H2", new List<int>() { 33 } },
            { "Q3", new List<int>() { 12 } },
            { "H3", new List<int>() { 12 } }
        };

        public static void ConvertParameter(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var inputFile = new FileInfo(filePath);
            var inputPackage = new ExcelPackage(inputFile);
            var inputSheet = inputPackage.Workbook.Worksheets[0];

            var i = 1;
            while (inputSheet.Cells[1, i].Value != null)
            {
                var paramName = inputSheet.Cells[1, i].Value.ToString();
                var j = 2;
                while (inputSheet.Cells[j, i].Value != null)
                {
                    var value = (double)inputSheet.Cells[j, i].Value;
                    //var min = Bounds[paramName][0].Min;
                    //var max = Bounds[paramName][0].Max;
                    //value = (value - min) / (max - min) * 100 + 2;

                    for (int k = 0; k < Conversions[paramName].Count; k++)
                    {
                        var min = Bounds[paramName][k].Min;
                        var max = Bounds[paramName][k].Max;
                        value = (value - min) / (max - min) * 100 + 2;
                        value = Functions.Funcs[Conversions[paramName][k]].Func(value);
                    }

                    inputSheet.Cells[j, i].Value = value;
                    j++;
                }
                i++;
            }
            inputPackage.Save();
        }
        static void Main(string[] args)
        {
            Functions.Initialize();
            ConvertParameter(Preprocessing.BASE_DIR + "Преобразование параметров.xlsx");
        }
    }
}
