using System;
using System.Collections.Generic;

namespace Расчеты_для_курсовой
{
    public static class Functions
    {
        public static List<(Func<double, double> Func, string Name)> Funcs;
        private static void Add(Func<double, double> func, string str)
        {
            Funcs.Add((func, str));
        }
        public static void Initialize()
        {
            Funcs = new();
            var alphas = new double[]{
                0.001, - 0.001,
                0.01, -0.01,
                0.1, -0.1,
                0.5, -0.5,
                1, -1,
                1.5, -1.5,
                2, -2,
                2.5, -2.5,
                3, -3,
                3.5, -3.5,
                4, -4,
                4.5, -4.5,
                5, -5,
                6, -6,
                7, -7,
                8, -8,
                9, -9,
                10,  -10};

            foreach (var alpha in alphas)
            {
                Add(x => Math.Pow(x, alpha), $"x^{alpha}");
                Add(x => Math.Pow(Math.E, alpha * x), $"e^({alpha}*x)");
                Add(x => Math.Log(alpha * x), $"log({alpha}*x)");
                Add(x => Math.Sin(alpha * x), $"sin({alpha}*x)");
                Add(x => Math.Cos(alpha * x), $"cos({alpha}*x)");
                Add(x => Math.Sinh(alpha * x), $"sh({alpha}*x)");
                Add(x => Math.Cosh(alpha * x), $"ch({alpha}*x)");
                Add(x => Math.Tan(alpha * x), $"tg({alpha}*x)");
                Add(x => Math.Asin(alpha * x), $"arcsin({alpha}*x)");
                Add(x => Math.Acos(alpha * x), $"arccos({alpha}*x)");
                Add(x => Math.Asinh(alpha * x), $"arcsh({alpha}*x)");
                Add(x => Math.Acosh(alpha * x), $"arcch({alpha}*x)");
                Add(x => Math.Atan(alpha * x), $"arctg({alpha}*x)");
                Add(x => Math.Atanh(alpha * x), $"arcth({alpha}*x)");
                Add(x => Math.Pow(Math.E, alpha * Math.Pow(x, 2)), $"e^({alpha}*x^2)");
                Add(x => 1 / (1 + Math.Pow(Math.E, alpha * x)), $"1 / (1 + e^({alpha}*x))");
                Add(x => (Math.Pow(Math.E, alpha * x) - 1) / (Math.Pow(Math.E, alpha * x) + 1), $"(e^({alpha}*x) - 1) / (e^({alpha}*x) + 1");
                Add(x => (Math.Pow(Math.E, alpha * x) - Math.Pow(Math.E, -alpha * x)) / 2, $"((e^({alpha}*x) - e^({-alpha}*x)) / 2");
            }
        }
    }
}
