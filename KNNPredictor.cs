using AI.Calculators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI
{
    class KNNPredictor
    {
        private readonly int kPoints;
        private readonly string stock;
        private readonly List<StockData> trainingData;
        private readonly IPairDistance pairDistance;

        /// <summary>
        /// Creates predictor class with given training set
        /// </summary>
        /// <param name="trainingSet">Training set</param>
        /// <param name="stock">Stock which will be predicted</param>
        public KNNPredictor(int k, string stockName, List<StockData> trainingSet, IPairDistance distanceCalculator, bool filter)
        {
            if (k <= 0)
                throw new ArgumentException("k turi būti teigiamas");

            kPoints = k;
            stock = stockName;
            pairDistance = distanceCalculator;

            if(filter)
                trainingData = FilterRelevent(trainingSet);
            else
                trainingData = trainingSet;
        }

        public Prediction Predict(StockData historyData)
        {
            return Predict(kPoints, historyData);
        }

        public Prediction Predict(int k, StockData historyData)
        {
            if (k <= 0)
                throw new ArgumentException("k turi būti teigiamas");
            if (!historyData.GetName().Equals(stock))
                throw new ArgumentException("akcijos vardas nesutampa");

            var distances = trainingData
                .AsParallel()
                .Select(t => GetMinimumDistance(t, historyData))
                .AsSequential()
                .OrderBy(x => x.Key)
                .Take(k)
                .ToList();

            //distances.ForEach(d => Console.WriteLine(d));
            var average = distances.Average(d => d.Value);
            var type = PredictionType.Still;
            if (average.CompareTo(0) > 0)
                type = PredictionType.Rise;
            else if (average.CompareTo(0) < 0)
                type = PredictionType.Fall;

            return new Prediction
            {
                Amount = average,
                Direction = type
            };
        }

        private KeyValuePair<double, double> GetMinimumDistance(StockData training, StockData sample)
        {
            var sampleData = sample.GetAllData();
            var traininghistory = training.GetAllData();
            var distances = new List<KeyValuePair<double, double>>();
            var tasks = new List<Task>();

            for(int i = 0; i < traininghistory.Count - sampleData.Count - 1; i++)
            {
                var distance = 0.0;
                for(int j = 0; j < sampleData.Count; j++)
                {
                    distance += CalculateDayDistance(traininghistory[i + j].Value, sampleData[j].Value);
                }
                var predictionDay = traininghistory[i + sampleData.Count];
                distances.Add(new KeyValuePair<double, double>(pairDistance.OuterFunction(distance), predictionDay.Value[sample.PredictingCollumn]));
            }

            return distances.OrderBy(d => d.Key).FirstOrDefault();
        }

        private double CalculateDayDistance(List<double> left, List<double> right)
        {
            return left.Zip(right, (l, r) => new KeyValuePair<double, double>(l, r))
                .Select(pairDistance.CalculateDistance)
                .Sum();
        }

        private List<StockData> FilterRelevent(List<StockData> set)
        {
            Console.WriteLine("Filtruojamos akcijos");
            var predictingStock = set.FirstOrDefault(s => s.GetName().Equals(stock));
            var stockData = predictingStock.GetAllDataAsDoubleList();

            var filterCount = Math.Pow(kPoints, 0.75);
            var correlations = new List<KeyValuePair<int, double>>();

            for(int i = 0; i < set.Count(); ++i)
            {
                //Console.WriteLine($"Skaičiuojama {set[i].GetName()}");
                var comparedStockData = set[i].GetAllDataAsDoubleList();

                var stockNValues = stockData.Take(Math.Min(stockData.Count, comparedStockData.Count)).Select(l => l[predictingStock.PredictingCollumn]).ToList();
                var stockAverage = stockNValues.Average();
                var comparedStockNValues = comparedStockData.Take(Math.Min(stockData.Count, comparedStockData.Count)).Select(l => l[predictingStock.PredictingCollumn]).ToList();
                var comparedAverage = comparedStockNValues.Average();
                
                // calculates Pearson's correlation parameter
                var numerator = stockNValues
                    .Zip(comparedStockNValues, (l, r) => new KeyValuePair<double, double>(l, r))
                    .Sum(p => (double)((p.Key - stockAverage) * (p.Value - comparedAverage)));
                var firstDenumerator = Math.Sqrt(stockNValues.Sum(v => Math.Pow(v - stockAverage, 2)));
                var secondDenumerator = Math.Sqrt(comparedStockNValues.Sum(v => Math.Pow(v - comparedAverage, 2)));

                correlations.Add(new KeyValuePair<int, double>(i, numerator / (firstDenumerator * secondDenumerator)));
            }

            correlations = correlations.OrderBy(p => p.Value).ToList();

            var filtered = new List<StockData>();
            foreach(var c in correlations)
            {
                filtered.Add(set[c.Key]);
            }

            Console.WriteLine("Akcijų filtravimas baigtas");
            return filtered;
        }
    }
}
