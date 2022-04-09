using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace Расчеты_для_курсовой
{
    //Наборы данных по годам
    class Data
    {
        //Номер года
        public int Year;
        public List<double> Q;
        public List<double> H;
        //Количество измерений
        public int Count;
    }

    //Сформированные данные для обучения
    class DataSet
    {
        public int InputDaysCount;
        public int OutputDaysCount;
        public List<(List<double> Q, List<double> H)> Input;
        public List<(List<double> Q, List<double> H)> Output;

        public void MakePreprocessing(params PreprocessingType[] types)
        {
            foreach (var type in types)

                switch (type)
                {
                    case PreprocessingType.StandardizationH:
                        StandardizeH();
                        break;
                    case PreprocessingType.FullFunctional:
                        MakeFunctionalPreprocess();
                        break;
                    case PreprocessingType.RelativeIncreaseQ:
                        CalcRelativeQ();
                        break;
                }
        }

        private void MakeFunctionalPreprocess()
        {
            Functions.Initialize();

            for (int i = 0; i < InputDaysCount; i++)
            {
                var newQ = Input[i].Q;
                //Преобразовываем относительно последнего выходного параметра
                Logic.MakeBestConvertion(ref newQ, Output[^1].Q);

                var newH = Input[i].H;
                Logic.MakeBestConvertion(ref newH, Output[^1].H);

                var newQH = (new List<double>(newQ), new List<double>(newH));
                Input[i] = newQH;
            }
        }

        private void CalcRelativeQ()
        {
            for (int i = Input[0].Q.Count - 1; i > 0; i--)
            {
                for (int j = InputDaysCount - 1; j > 0; j--)
                    Input[j].Q[i] = (Input[j].Q[i] - Input[j - 1].Q[i]) / Input[j - 1].Q[i];
                Input[0].Q[i] = (Input[0].Q[i] - Input[0].Q[i - 1]) / Input[0].Q[i - 1];
            }
            //Q(n) считается через Q(n-1), поэтому Q(0) мы посчитать не можем
            for (int i = 0; i < InputDaysCount; i++)
            {
                Input[i].H.RemoveAt(0);
                Input[i].Q.RemoveAt(0);
            }
            for (int i = 0; i < OutputDaysCount; i++)
            {
                Output[i].H.RemoveAt(0);
                Output[i].Q.RemoveAt(0);
            }
        }

        private void StandardizeH()
        {
            var minH = Output[^1].H.Min();
            for (int i = 0; i < InputDaysCount; i++)
                for (int j = 0; j < Input[i].H.Count; j++)
                    Input[i].H[j] -= minH;

            for (int i = 0; i < OutputDaysCount; i++)
                for (int j = 0; j < Output[i].H.Count; j++)
                    Output[i].H[j] -= minH;
        }

        public void WriteFile(string dataSetsFilePath)
        {
            var outputFile = new FileInfo(dataSetsFilePath);
            if (outputFile.Exists)
            {
                outputFile.Delete();
                outputFile = new FileInfo(dataSetsFilePath);
            }
            var outputPackage = new ExcelPackage(outputFile);
            var outputSheet = outputPackage.Workbook.Worksheets.Add("Тестирующие наборы");

            //Q и H
            for (int i = 0; i < InputDaysCount; i++)
            {
                outputSheet.Cells[1, 2 * i + 1].Value = $"q{i + 1}";
                outputSheet.Cells[1, 2 * i + 2].Value = $"h{i + 1}";
            }
            for (int i = 0; i < OutputDaysCount; i++)
            {
                outputSheet.Cells[1, 2 * (InputDaysCount + i) + 1].Value = $"q{InputDaysCount + i + 1} (пр.)";
                outputSheet.Cells[1, 2 * (InputDaysCount + i) + 2].Value = $"h{InputDaysCount + i + 1} (пр.)";
            }
            for (int i = 0; i < Input[0].Q.Count; i++)
            {
                for (int j = 0; j < InputDaysCount; j++)
                {
                    outputSheet.Cells[i + 2, 2 * j + 1].Value = Input[j].Q[i];
                    outputSheet.Cells[i + 2, 2 * j + 2].Value = Input[j].H[i];
                }
                for (int j = 0; j < OutputDaysCount; j++)
                {
                    outputSheet.Cells[i + 2, 2 * (InputDaysCount + j) + 1].Value = Output[j].Q[i];
                    outputSheet.Cells[i + 2, 2 * (InputDaysCount + j) + 2].Value = Output[j].H[i];
                }
            }

            ////Q
            //for (int i = 0; i < InputDaysCount; i++)
            //    outputSheet.Cells[1, i + 1].Value = $"q{i + 1}";
            //outputSheet.Cells[1, InputDaysCount + 1].Value = $"q{InputDaysCount + 1} (пр.)";

            //for (int i = 0; i < Input[0].Q.Count; i++)
            //{
            //    for (int j = 0; j < InputDaysCount; j++)
            //        outputSheet.Cells[i + 2, j + 1].Value = Input[j].Q[i];
            //    outputSheet.Cells[i + 2, InputDaysCount + 1].Value = Output.Q[i];
            //}

            ////H
            //for (int i = 0; i < InputDaysCount; i++)
            //    outputSheet.Cells[1, i + 1].Value = $"h{i + 1}";
            //outputSheet.Cells[1, InputDaysCount + 1].Value = $"h{InputDaysCount + 1} (пр.)";

            //for (int i = 0; i < Input[0].H.Count; i++)
            //{
            //    for (int j = 0; j < InputDaysCount; j++)
            //        outputSheet.Cells[i + 2, j + 1].Value = Input[j].H[i];
            //    outputSheet.Cells[i + 2, InputDaysCount + 1].Value = Output.H[i];
            //}

            outputPackage.Save();
        }
    }
}
