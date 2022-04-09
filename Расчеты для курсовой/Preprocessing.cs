using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using Дифузионное_сглаживание;

namespace Расчеты_для_курсовой
{
    public enum PreprocessingType
    {
        StandardizationH,
        FullFunctional,
        RelativeIncreaseQ
    }

    public static class Preprocessing
    {
        public const string BASE_DIR = @"C:\Users\akimz\source\repos\Расчеты для курсовой\Данные для расчетов\";

        const string RELIEF_POINTS_KOSA6_PATH = BASE_DIR + @"Точки рельефа, Коса гидроствор №6.txt",
            DATA_PATH = BASE_DIR + @"Измерения Q, H, t во время половодья. Коса, 2008-2019 (две недели после пика).xlsx",
            DATASETS_PATH = BASE_DIR + @"Данные для обучения.xlsx";

        public static void ReadReliefPoints(Station station)
        {
            var lines = File.ReadAllLines(RELIEF_POINTS_KOSA6_PATH);
            foreach (var line in lines)
            {
                double[] points = Array.ConvertAll(line.Split('\t'), double.Parse);
                station.ReliefPoints.Add((X: points[0], Y: points[1]));
            }
        }

        private static void ReadInput(out List<Data> data)
        {
            data = new();

            var inputFile = new FileInfo(DATA_PATH);
            var inputPackage = new ExcelPackage(inputFile);
            var inputSheet = inputPackage.Workbook.Worksheets[0];

            for (int i = 0; inputSheet.Cells[1, i * 8 + 1].Value != null; i++)
            {
                var year = Convert.ToInt32(inputSheet.Cells[1, i * 8 + 1].Value);
                var q = new List<double>();
                var h = new List<double>();
                for (int j = 0; inputSheet.Cells[j + 2, i * 8 + 4].Value != null; j++)
                {
                    q.Add((double)inputSheet.Cells[j + 2, i * 8 + 4].Value);
                    h.Add((double)inputSheet.Cells[j + 2, i * 8 + 6].Value);
                }
                data.Add(new Data
                {
                    Year = (int)year,
                    Q = q,
                    H = h,
                    Count = q.Count
                });
            }
        }

        public static void MakeDataSetsFile(int inputDaysCount, int outputDaysCount, bool useSmoothing, params PreprocessingType[] preprocessingType)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ReadInput(out var data);

            if (useSmoothing)
                foreach (var oneYearData in data)
                    Smoothing.MakeSmooth(input: oneYearData.Q, name: $"Q {oneYearData.Year}");

            var dataSet = new DataSet
            {
                InputDaysCount = inputDaysCount,
                OutputDaysCount = outputDaysCount,
                Input = new(),
                Output = new(),
            };

            for (int i = 0; i < inputDaysCount; i++)
                dataSet.Input.Add((new List<double>(), new List<double>()));
            for (int i = 0; i < outputDaysCount; i++)
                dataSet.Output.Add((new List<double>(), new List<double>()));

            for (int i = 0; i < data.Count; i++)
                for (int j = 0; j < data[i].Count - inputDaysCount - outputDaysCount + 1; j++)
                {
                    for (int k = 0; k < inputDaysCount; k++)
                    {
                        dataSet.Input[k].Q.Add(data[i].Q[j + k]);
                        dataSet.Input[k].H.Add(data[i].H[j + k]);
                    }
                    for (int k = 0; k < outputDaysCount; k++)
                    {
                        dataSet.Output[k].Q.Add(data[i].Q[j + inputDaysCount + k]);
                        dataSet.Output[k].H.Add(data[i].H[j + inputDaysCount + k]);
                    }
                }

            dataSet.MakePreprocessing(preprocessingType);
            dataSet.WriteFile(DATASETS_PATH);
        }
    }
}
