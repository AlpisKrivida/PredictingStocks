using AI.Calculators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AI
{
    class Program
    {
        private const int defaultK = 4; // untested; determined by random dice roll
        private const string defaultStock = "intc.us.txt"; // intel stocks

        static void Main(string[] args)
        {
            WritePretty("Paruošiami duomenų failai");
            var dataFilter = new StockDataFilter();
            dataFilter.PrepareFiles(Constants.ORIGINAL_FOLDER, Constants.TRAINING_FOLDER, Constants.TEST_FOLDER, Constants.DATE_FILTER, Constants.TEST_FROM);
            WritePretty("Duomenų failai paruoši");

            //Console.WriteLine("Atrenkamos akcijos, kurios turi duomenis bent nuo {0}", Constants.DATE_FILTER.ToString("yyyy-MM-dd"));
            WritePretty("Nuskaitomi mokymo duomenys");
            List<StockData> trainingData = StockDataReader.ReadAndPrepareFolder(Constants.TRAINING_FOLDER, Constants.DATE_FILTER);
            WritePretty("Mokymo duomenys nuskaityti");

            WritePretty("Nuskaitomi testavimo duomenys");
            List<StockData> testData = StockDataReader.ReadAndPrepareFolder(Constants.TEST_FOLDER, Constants.DATE_FILTER);
            WritePretty("Testavimo duomenys nuskaityti");

            WritePretty("Sudaromas modelis");
            var predictor = new KNNPredictor(defaultK, defaultStock, trainingData, new EuclideanDistance(), true);
            WritePretty("Modelis sudarytas");

            WritePretty("Pradėdama kryžminė patikra");
            var crossValidation = new CrossValidation();
            crossValidation.StartValidation(trainingData);
            WritePretty("Kryžminė patikra baigta");
            WritePretty("Pradedami balsavimo ekspermentai");
            crossValidation.StartVoting(trainingData, testData);
            WritePretty("Balsavimo ekspermentai baigti");

        }

        static Prediction GuessWithMonthsData(string stock, DateTime date, KNNPredictor predictor, List<StockData> stocks)
        {
            var guessedStock = stocks.FirstOrDefault(s => s.GetName().Equals(stock));
            var monthsData = guessedStock.GetDaysData(date.AddDays(-30), date);

            return predictor.Predict(monthsData);
        }

        static void WritePretty(string value)
        {
            Console.WriteLine("===============================");
            Console.WriteLine(value);
            Console.WriteLine("===============================");
        }
    }
}
