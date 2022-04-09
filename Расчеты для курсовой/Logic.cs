using System;
using System.Collections.Generic;
using System.Linq;

namespace Расчеты_для_курсовой
{
    static class Logic
    {
        //точность фукциональной предобработки
        const double F_Eps = 0.0001;

        private static double FindPirsonCoeff(List<double> x, List<double> y)
        {
            var x_aver = x.Average();
            var y_aver = y.Average();
            double a, b, c = 0, d = 0, e = 0;
            for (int i = 0; i < x.Count; i++)
            {
                a = x[i] - x_aver;
                b = y[i] - y_aver;
                c += a * b;
                d += a * a;
                e += b * b;
            }
            return c / Math.Sqrt(d * e);
        }

        private static List<double> ConvertFactorByFunction(List<double> input, Func<double, double> function)
        {
            var convertedFactor = new List<double>();
            for (int i = 0; i < input.Count; i++)
                convertedFactor.Add(function(input[i]));
            return convertedFactor;
        }

        private static int FindBestConvertion(List<double> input, List<double> output)
        {
            double newPirsonCoeff, maxPirsonCoeff = 0;
            int bestFunctionIdx = -1;
            for (int i = 0; i < Functions.Funcs.Count; i++)
            {
                var convertedFactor = ConvertFactorByFunction(input, Functions.Funcs[i].Func);
                newPirsonCoeff = FindPirsonCoeff(convertedFactor, output);
                if (Math.Abs(newPirsonCoeff) > maxPirsonCoeff)
                {
                    maxPirsonCoeff = newPirsonCoeff;
                    bestFunctionIdx = i;
                }
            }
            return bestFunctionIdx;
        }

        private static void ConvertToSection(List<double> data, int a, int b)
        {
            var max = data.Max();
            var min = data.Min();
            for (int i = 0; i < data.Count; i++)
                data[i] = (data[i] - min) / (max - min) * (b - a) + 2;
        }

        public static void MakeBestConvertion(ref List<double> input, List<double> output)
        {
            var pirsonCoeff = FindPirsonCoeff(input, output);
            var conversionCount = 0;
            var functions = "";
            while (true)
            {
                ConvertToSection(input, 2, 102);
                var bestFunctionIdx = FindBestConvertion(input, output);
                if (bestFunctionIdx == -1)
                {
                    Console.WriteLine(conversionCount);
                    break;
                }
                conversionCount++;
                functions += Functions.Funcs[bestFunctionIdx].Name + "     " + bestFunctionIdx;
                input = ConvertFactorByFunction(input, Functions.Funcs[bestFunctionIdx].Func);
                functions += $"   ({input.Max()}  {input.Min()})     ";
                var newPirsonCoeff = FindPirsonCoeff(input, output);
                if (Math.Abs(newPirsonCoeff) > Math.Abs(pirsonCoeff) + F_Eps)
                    pirsonCoeff = newPirsonCoeff;
                else
                {
                    Console.WriteLine(conversionCount + "   " + functions);
                    break;
                }
            }
        }
    }
}
