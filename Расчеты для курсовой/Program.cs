﻿namespace Расчеты_для_курсовой
{
    class Program
    {
        static void Main()
        {
            Preprocessing.MakeDataSetsFile(
                inputDaysCount: 3,
                outputDaysCount: 5,
                useSmoothing: false,
                PreprocessingType.StandardizationH);
        }
    }
}
