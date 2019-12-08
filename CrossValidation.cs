using System;
using System.Collections.Generic;
using System.Linq;
using AI.Calculators;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AI
{
    public class CrossValidation
    {
        const int MAX_K_NUMBER = 6;
        const int MIN_K_NUMBER = 1;
        const int FILE_NUMBER = 20;
        const double threshold = 2;
        private const int defaultK = 15;


        public void StartValidation(List<StockData> data)
        {
            List<StockData> trainingData = new List<StockData>();
            List<StockData> testingData = new List<StockData>();
            List<List<StockData>> totalTrainingData = new List<List<StockData>>();
            List<List<StockData>> totalTestingData = new List<List<StockData>>();

            int foldSize = data.Count / Constants.FOLD_NUMBER;

            Console.WriteLine(data.Count());

            for (int x = 0; x < Constants.FOLD_NUMBER; x++)
            {
                for (int k = 0; k < data.Count - data.Count % Constants.FOLD_NUMBER; k++) {
                    if (k >= x * foldSize && k < (x+1) * foldSize)
                    {
                        testingData.Add(data[k]);
                    }
                    else
                    {
                        trainingData.Add(data[k]);
                    }
                }

                totalTestingData.Add(testingData.ToList());
                totalTrainingData.Add(trainingData.ToList());

                testingData.Clear();
                trainingData.Clear();
            }

            var res = totalTrainingData.Zip(totalTestingData, (n, w) => new { Train = n, Test = w });

            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 4
            };

            Parallel.ForEach(res, options, i =>
            {
                TestAccuracyWithMultipleGuesses(i.Train, i.Test);
            });
        }

        static void TestAccuracyWithMultipleGuesses(List<StockData> trainingData, List<StockData> testingData)
        {
            var predictor = new KNNPredictor(defaultK, testingData[0].GetName(), trainingData, new EuclideanDistance(), false);

            var guessDates = testingData[0].GetMultipleGuessData();
            double errorSum = 0;
            double absoluteErrorSum = 0;

            foreach(var guessDate in guessDates)
            {
                Console.WriteLine($"Spėjamas {guessDate.AddDays(1).ToString("yyyy-MM-dd")} dienos pokytis");
                var result = GuessWithMonthsData(testingData[0].GetName(), guessDate, predictor, testingData, defaultK);
                var day = testingData.FirstOrDefault(s => s.GetName().Equals(testingData[0].GetName())).GetDayData(guessDate)[trainingData[0].PredictingCollumn];
                var dayAbsoluteError = Math.Abs(day - result.Amount);
                var dayError = day == 0 ? dayAbsoluteError / Math.Abs(result.Amount) : dayAbsoluteError / Math.Abs(day);
                Console.WriteLine($"Dienos erroras {dayError}");
                errorSum += dayError;
                absoluteErrorSum += dayAbsoluteError;
            }

            var averageError = errorSum / guessDates.Count;
            var averageAbsoluteError = absoluteErrorSum / guessDates.Count;
            Console.WriteLine($"Absoliuti paklaida: {averageAbsoluteError}%; Santikinė paklaida: {100 * averageError}%");
        }

        static void TestAccuracy(List<StockData> trainingData, List<StockData> testingData)
        {
            var predictor = new KNNPredictor(defaultK, trainingData[0].GetName(), trainingData, new EuclideanDistance(), true);

            var guessDate = trainingData[0].GetGuessData();

            Console.WriteLine($"Spėjamas {guessDate.AddDays(1).ToString("yyyy-MM-dd")} dienos pokytis");
            var result = GuessWithMonthsData(trainingData[0].GetName(), guessDate, predictor, trainingData, defaultK);
            Console.WriteLine($"Rezultatas: Akcija pakis {result.Amount}%");
            var day = trainingData.FirstOrDefault(s => s.GetName().Equals(trainingData[0].GetName())).GetDayData(guessDate)[trainingData[0].PredictingCollumn];
            Console.WriteLine($"Tikras pokytis: {day}%");

            var absoluteError = Math.Abs(day - result.Amount);
            var error = absoluteError / Math.Abs(day);
            Console.WriteLine($"Absoliuti paklaida: {absoluteError}%; Santikinė paklaida: {100 * error}%");
        }

        static Prediction GuessWithMonthsData(string stock, DateTime date, KNNPredictor predictor, List<StockData> stocks, int kNumber)
        {
            var guessedStock = stocks.FirstOrDefault(s => s.GetName().Equals(stock));
            var monthsData = guessedStock.GetDaysData(date.AddDays(-30), date);

            return predictor.Predict(kNumber, monthsData);
        }

        public void StartVoting(List<StockData> trainingData, List<StockData> testData)
        {
            Console.WriteLine("Daugumos balsavimas");
            VotingMethod(trainingData, testData, true);
            Console.WriteLine("Svorinis balsavimas");
            VotingMethod(trainingData, testData, false);
        }

        static void VotingMethod(List<StockData> trainingData, List<StockData> testData, bool majorityVoting)
        {
            double totalCorrect=0;
            for (int fn = 0; fn <= FILE_NUMBER; fn++)
            {
                List<double> variations = new List<double>();
                var guessDate = testData[fn].GetGuessData();
                Console.WriteLine("Failo pavadinimas {0} ", trainingData[fn].GetName());
                var predictor = new KNNPredictor(defaultK, trainingData[fn].GetName(), trainingData, new EuclideanDistance(), true);

                var day = testData.FirstOrDefault(s => s.GetName().Equals(trainingData[fn].GetName())).GetDayData(guessDate)[testData[fn].PredictingCollumn];
                var trueVariation = day;
                Console.WriteLine($"Tikras pokytis: {trueVariation}%");
                for (int i = MIN_K_NUMBER; i <= MAX_K_NUMBER; i++)
                {
                    var result = GuessWithMonthsData(trainingData[fn].GetName(), guessDate, predictor, testData, i);
                    if (!majorityVoting)
                        result.Amount *= GetWeight(i);

                    Console.WriteLine("Kai k yra {0} pokytis lygus {1} %", i, result.Amount);
                    variations.Add(result.Amount);
                }
                var closest = variations[0];
                var dif = Difference(trueVariation, closest);
                foreach (var a in variations)
                {
                    var newDif = Difference(trueVariation, a);
                    if (newDif < dif)
                    {
                        closest = a; dif = newDif;
                    }
                }
                if (dif <= threshold)
                    totalCorrect++;
                Console.WriteLine("Spejimas naudojant balsavima {0}", closest);
            }
            Console.WriteLine("BENDRAS TIKSLUMAS LYGUS {0}% ",Math.Round(totalCorrect /FILE_NUMBER * 100));
        }

        static double GetWeight(int kNumber)
        {
            if (kNumber > 6)
                return 1;
            else if (kNumber < 3)
                return 0.3;
            else return 0.5;
        }

        static double Difference(double trueVariation, double variation) {

            double result = trueVariation > 0 ? variation > 0 ?
                result = Math.Abs(trueVariation - variation) : result = trueVariation + Math.Abs(variation) : variation > 0 ?
                result = Math.Abs(trueVariation) + Math.Abs(variation) : Math.Abs(Math.Abs(trueVariation) - Math.Abs(variation));

            return result;
        }
    }
}
