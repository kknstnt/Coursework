using System;
using System.IO;
using OfficeOpenXml;
using Расчеты_для_курсовой;

namespace Нахождение_H_через_Q_и_U
{
    class Program
    {
        //разница между уровнем моря и уровнем воды в реке (см)
        const int DeltaH = 12294;
        //минимальный и максимальный уровни реки
        const double A0 = 123.69, B0 = 129.79,
            //точность метода половинного деления
            Eps = 0.01;
        public static void FindHError(string filePath, Station station)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var inputFile = new FileInfo(filePath);
            var inputPackage = new ExcelPackage(inputFile);
            var inputSheet = inputPackage.Workbook.Worksheets[0];

            var i = 1;
            while (inputSheet.Cells[i, 1].Value != null)
            {
                var Q = (double)inputSheet.Cells[i, 1].Value;
                var U = (double)inputSheet.Cells[i, 2].Value;
                //высота в метрах над уровнем моря
                var hMPredict = FindH(Q, U, station);
                //получение уровня воды реки в сантиметрах
                var hSmPredict = hMPredict * 100 - DeltaH;
                var hReal = (double)inputSheet.Cells[i, 3].Value;
                var hError = Math.Pow(hSmPredict - hReal, 2);

                inputSheet.Cells[i, 4].Value = hSmPredict;
                inputSheet.Cells[i, 5].Value = hError;
                i++;
            }
            var hErrorAver = FindAverageColumnValue(inputSheet, 5);
            var hRealAver = FindAverageColumnValue(inputSheet, 3);
            var hAverPercentError = Math.Sqrt(hErrorAver) / hRealAver;
            inputSheet.Cells[i + 1, 5].Value = hAverPercentError;

            inputPackage.Save();
        }

        private static double FindAverageColumnValue(ExcelWorksheet sheet, int columnNum)
        {
            var i = 1;
            var buf = 0.0;
            while (sheet.Cells[i, columnNum].Value != null)
            {
                buf += (double)sheet.Cells[i, columnNum].Value;
                i++;
            }
            return buf / (i - 1);
        }

        //поиск H через Q и U методом половинного деления
        private static double FindH(double Q, double U, Station station)
        {
            var a0 = A0;
            var b0 = B0;
            var c0 = (a0 + b0) / 2;
            var S_c0 = station.CalculateSquare(c0);
            var S = Q / U;

            while (Math.Abs(S - S_c0) > Eps)
            {
                if (S < S_c0)
                {
                    b0 = c0;
                }
                else
                {
                    a0 = c0;
                }
                c0 = (a0 + b0) / 2;
                S_c0 = station.CalculateSquare(c0);
            }
            return Math.Round(c0, 2);
        }

        static void Main(string[] args)
        {
            var kosa6 = new Station();
            Preprocessing.ReadReliefPoints(kosa6);

            FindHError("Нахождение H через Q и U.xlsx", kosa6);
        }
    }
}
