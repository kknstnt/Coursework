using System;
using System.Collections.Generic;

namespace Расчеты_для_курсовой
{
    public class Station
    {
        public List<(double X, double Y)> ReliefPoints = new();

        //нахождение площади живого сечения по заданным точкам рельефа и уровню воды методом трапеций
        public double CalculateSquare(double H)
        {
            int leftPointIdx = 0;
            while (ReliefPoints[leftPointIdx].Y > H)
                leftPointIdx++;

            int rightPointIdx = ReliefPoints.Count - 1;
            while (ReliefPoints[rightPointIdx].Y > H)
                rightPointIdx--;

            var innerPointsCount = rightPointIdx - leftPointIdx + 1;
            //получение массива всех точек рельефа ниже заданного уровня воды H
            var points = ReliefPoints.GetRange(leftPointIdx, innerPointsCount);

            if (H != ReliefPoints[leftPointIdx].Y)
            {
                var y0 = H;
                var ratioLeft = (H - ReliefPoints[leftPointIdx].Y) / (ReliefPoints[leftPointIdx - 1].Y - ReliefPoints[leftPointIdx].Y);
                var x0 = ReliefPoints[leftPointIdx].X - ratioLeft * (ReliefPoints[leftPointIdx].X - ReliefPoints[leftPointIdx - 1].X);
                //добавление в массив крайней левой точки рельефа
                points.Insert(0, (X: x0, Y: y0));
            }

            if (H != ReliefPoints[rightPointIdx].Y)
            {
                var yN = H;
                var ratioRight = (H - ReliefPoints[rightPointIdx].Y) / (ReliefPoints[rightPointIdx + 1].Y - ReliefPoints[rightPointIdx].Y);
                var xN = ReliefPoints[rightPointIdx].X + ratioRight * (ReliefPoints[rightPointIdx + 1].X - ReliefPoints[rightPointIdx].X);
                //добавление в массив крайней правой точки рельефа
                points.Add((X: xN, Y: yN));
            }

            //крайние площади являются прямоугольными треугольниками, ищем их по формуле a*b/2
            var S = 0.0;
            var Str0 = 0.5 * (points[0].Y - points[1].Y) * (points[1].X - points[0].X);
            var StrN = 0.5 * (points[^1].X - points[^2].X) * (points[^1].Y - points[^2].Y);
            S += Str0 + StrN;
            //остальные площади ищутся по формуле площади трапеции
            for (int i = 2; i < points.Count - 1; i++)
            {
                var a = H - points[i - 1].Y;
                var b = H - points[i].Y;
                var h = points[i].X - points[i - 1].X;
                S += 0.5 * (a + b) * h;
            }
            return S;
        }
    }
}
