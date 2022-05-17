namespace Расчеты_для_курсовой
{
    class Program
    {
        static void Main()
        {
            Preprocessing.MakeDataSetsFile(
                inputDaysCount: 3,
                outputDaysCount: 1,
                useSmoothing: true,
                PreprocessingType.StandardizationH,
                ClusterName.Spring1);
        }
    }
}
