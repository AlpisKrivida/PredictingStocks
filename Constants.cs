using System;

namespace AI
{
    public static class Constants
    {
        public const string ORIGINAL_FOLDER = "Stocks";
        public const string TRAINING_FOLDER = "TrainingStocks";
        public const string MINIMIZED_FOLDER = "MinimizedStocks";
        public const string TEST_FOLDER = "TestStocks";
        public const int FOLD_NUMBER = 10;
        public static readonly DateTime DATE_FILTER = new DateTime(2010, 1, 1);
        public static readonly DateTime TEST_FROM = new DateTime(2017, 1, 1);
    }
}
