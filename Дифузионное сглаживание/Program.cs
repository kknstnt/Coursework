using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System;

namespace Дифузионное_сглаживание
{
    public static class Smoothing
    {
        //параметр сглаживания
        const double ALPHA = 0.1;
        //необходимое число смены знаков второй производной для каждого параметра
        static Dictionary<string, int> K = new()
        {
            { "Q 1956", 8 },
            { "Q 1957", 8 },
            { "Q 1958", 8 },
            { "Q 1960", 8 },
            { "Q 1964", 8 },
            { "Q 1966", 8 },
            { "Q 1967", 11 },
            { "Q 1969", 6 },
            { "Q 1972", 6 },
            { "Q 1974", 8 },
            { "Q 1975", 4 },
            { "Q 1976", 7 },
            { "Q 1979", 5 },
            { "Q 1980", 7 },
            { "Q 1981", 8 },
            { "Q 1982", 8 },
            { "Q 1983", 6 },
            { "Q 1985", 6 },
            { "Q 2008", 8 },
            { "Q 2009", 8 },
            { "Q 2010", 7 },
            { "Q 2011", 8 },
            { "Q 2012", 6 },
            { "Q 2013", 9 },
            { "Q 2014", 8 },
            { "Q 2015", 10 },
            { "Q 2016", 10 },
            { "Q 2017", 11 },
            { "Q 2018", 8 },
            { "Q 2019", 9 },
        };
        static int CalcDiffChangeCount(List<double> input)
        {
            //значение второй производной
            var prev_dy = input[2] - 2 * input[1] + input[0];
            var count = 0;
            for (int i = 2; i < input.Count - 1; i++)
            {
                var dy = input[i + 1] - 2 * input[i] + input[i - 1];
                if (dy * prev_dy < 0)
                    count++;
                prev_dy = dy;
            }
            Console.Write($"{count}  ");
            return count;
        }
        public static void MakeSmooth(List<double> input, string name)
        {
            Console.Write($"{name}:   ");
            while (CalcDiffChangeCount(input) > K[name])
            {
                var prevVal = input[0];
                for (int i = 1; i < input.Count - 1; i++)
                {
                    var curVal = input[i];
                    input[i] = input[i] + ALPHA * (input[i + 1] - 2 * input[i] + prevVal);
                    prevVal = curVal;
                }
            }
            Console.WriteLine();
        }
    }
    public class Program
    {        
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
            var outputSheet = outputPackage.Workbook.Worksheets.Add("Сглаженные данные");

            var i = 1;
            while (inputSheet.Cells[1, i].Value != null)
            {
                var j = 2;
                var Q = new List<double>();
                var name = inputSheet.Cells[1, i].Value.ToString();
                while (inputSheet.Cells[j, i].Value != null)
                {
                    Q.Add((double)inputSheet.Cells[j, i].Value);
                    j++;
                }
                Smoothing.MakeSmooth(Q, name);

                for (int k = 0; k < Q.Count; k++)
                    outputSheet.Cells[k + 1, i].Value = Q[k];
                i++;
            }
            outputPackage.Save();
        }
    }
}
