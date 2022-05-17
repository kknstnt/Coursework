using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using Дифузионное_сглаживание;

namespace Расчеты_для_курсовой
{
    [Flags]
    public enum ClusterName
    {
        Winter1 = 1,
        Winter2 = 2,
        Winter3 = 4,
        Spring1 = 8,
        Spring2 = 16,
        Spring3 = 32,
        AllData
    }

    [Flags]
    public enum PreprocessingType
    {
        StandardizationH = 1,
        FullFunctional = 2,
        RelativeIncreaseQ = 4
    }

    public static class Preprocessing
    {
        public const string BASE_DIR = @"C:\Users\akimz\source\repos\Расчеты для курсовой\Данные для расчетов\";

        const string RELIEF_POINTS_KOSA6_PATH = BASE_DIR + @"Точки рельефа, Коса гидроствор №6.txt",
            DATA_PATH = BASE_DIR + @"Измерения Q, H, t во время половодья. Коса.xlsx",
            DATASETS_PATH = BASE_DIR + @"Данные для обучения.xlsx";

        // Минимальный уровень воды за все рассматриваемые годы для вычисления dH
        public const int MIN_H = 166;

        // Года, попавшие в каждый из кластеров
        public readonly static Dictionary<ClusterName, int[]> Clusters = new() { 
            { ClusterName.Winter1, new int[] { 1956, 1957, 1958, 1960, 1967 } },
            { ClusterName.Winter2, new int[] { 1964, 1966, 1969, 1974, 1975, 1976, 1982, 2011, 2012, 2017, 2018, 2019 } },
            { ClusterName.Winter3, new int[] { 1972, 1979, 1980, 1981, 1983, 1985, 2008, 2009, 2010, 2013, 2014, 2015, 2016 } },
            { ClusterName.Spring1, new int[] { 1956, 1958, 1960, 1964, 1967, 1974, 1975, 1976, 1980, 1983, 2008, 2010, 2011, 2012, 2018, 2019 } },
            { ClusterName.Spring2, new int[] { 1979, 1981, 1982, 1985, 2016 } },
            { ClusterName.Spring3, new int[] { 1957, 1966, 1969, 1972, 2009, 2013, 2014, 2015, 2017 } },
        };

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

            for (int i = 0; inputSheet.Cells[1, i * 6 + 1].Value != null; i++)
            {
                var year = Convert.ToInt32(inputSheet.Cells[1, i * 6 + 1].Value);
                var q = new List<double>();
                var h = new List<double>();
                for (int j = 0; inputSheet.Cells[j + 2, i * 6 + 4].Value != null; j++)
                {
                    q.Add((double)inputSheet.Cells[j + 2, i * 6 + 4].Value);
                    h.Add((double)inputSheet.Cells[j + 2, i * 6 + 5].Value);
                }
                data.Add(new Data
                {
                    Year = year,
                    Q = q,
                    H = h,
                    Count = q.Count
                });
            }
        }

        public static void MakeDataSetsFile(int inputDaysCount, int outputDaysCount, bool useSmoothing, PreprocessingType preprocessingType, ClusterName clusterName = ClusterName.AllData)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ReadInput(out var data);

            var resultYears = new List<int>();
            // Если кластер не выбран, то используются все данные
            if (clusterName != ClusterName.AllData)
            {
                // Иначе сохраним в resultYears используемые года
                var clusters = Enum.GetValues(typeof(ClusterName));
                foreach (var val in clusters)
                {
                    var cluster = (ClusterName)val;
                    if ((cluster & clusterName) == cluster)
                        resultYears = resultYears.Union(Clusters[cluster]).ToList();
                }
            }
            if (useSmoothing)
                foreach (var oneYearData in data)
                    if (clusterName == ClusterName.AllData || resultYears.Contains(oneYearData.Year))
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
            {
                if (clusterName != ClusterName.AllData && !resultYears.Contains(data[i].Year))
                    continue;

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
            }
            dataSet.MakePreprocessing(preprocessingType);
            dataSet.WriteFile(DATASETS_PATH);
        }
    }
}
