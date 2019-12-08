using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AI
{
    public static class StockDataReader
    {
        public static List<StockData> ReadFolder(string sourcePath)//for reading all dates
        {
            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine("Nerastas folderis {0} skaitymui", sourcePath);
                return null;
            }

            List<StockData> list = new List<StockData>();

            foreach (var file in Directory.EnumerateFiles(sourcePath))
            {
                Console.WriteLine("Nuskaitomas failas {0}", file);
                using (var reader = new StreamReader(file))
                {
                    string line;
                    StockData stockdata = new StockData(file)//one stock
                    {
                        PredictingCollumn = 3 // predicting Close value
                    };
                    reader.ReadLine();//skip first line
                    while ((line = reader.ReadLine()) != null )
                    {
                        string[] args = line.Split(',');//Date,Open,High,Low,Close,Volume,OpenInt
                        if (args.Length > 1)
                        {
                            DateTime day = DateTime.Parse(args[0]);//Date
                            List<double> data = new List<double>();//Open,High,Low,Close,Volume,OpenInt
                            for (int i = 1; i < args.Length; i++)
                            {
                                data.Add(double.Parse(args[i]));
                            }
                            stockdata.AddDayData(day, data);//add day's data to stock 
                        }
                    }
                    //Console.WriteLine(stockdata.ToString());//for testing
                    list.Add(stockdata);//add to the list of stocks
                }
            }
            return list;
        }

        public static List<StockData> ReadAndPrepareFolder(string sourcePath, DateTime date)
        {
            var readData = ReadFolder(sourcePath, date);
            return PrepareData(readData);
        }

        private static List<StockData> ReadFolder(string sourcePath, DateTime date)//for reading only after a certain date
        {
            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine("Nerastas folderis {0} skaitymui", sourcePath);
                return null;
            }

            List<StockData> list = new List<StockData>();

            foreach (var file in Directory.EnumerateFiles(sourcePath))
            {
                Console.WriteLine("Nuskaitomas failas {0}", file);
                using (var reader = new StreamReader(file))
                {
                    string line;
                    StockData stockdata = new StockData(file);//one stock
                    reader.ReadLine();//skip first line
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] args = line.Split(',');//Date,Open,High,Low,Close,Volume,OpenInt
                        if (args.Length > 1)
                        {
                            DateTime day = DateTime.Parse(args[0]);//Date
                            if (date.CompareTo(day) <= 0)
                            {
                                List<double> data = new List<double>();//Open,High,Low,Close,Volume,OpenInt
                                if (args.Length > 1)
                                    for (int i = 1; i < args.Length; i++)
                                    {
                                        data.Add(double.Parse(args[i]));
                                    }
                                stockdata.AddDayData(day, data);//add day's data to stock 
                            }
                        }
                    }
                    //Console.WriteLine(stockdata.ToString());//for testing
                    list.Add(stockdata);//add to the list of stocks
                }
            }
            return list;
        }

        private static List<StockData> PrepareData(List<StockData> data)
        {
            Console.WriteLine("Konvertuojami duomenys");
            var preparedData = new List<StockData>();
            foreach (var stock in data)
            {
                //Console.WriteLine($"Skaičiuojama {stock.GetName()}");
                var preparedStock = new StockData("aa\\" + stock.GetName());
                var history = stock.GetAllData();

                List<double> lastDay = null;

                foreach (var day in history)
                {
                    var changeList = new List<double>();

                    for (int i = 0; i < day.Value.Count; ++i)
                    {
                        changeList.Add(lastDay is null || lastDay[i] == 0 ? 0 : 100 * (day.Value[i] - lastDay[i]) / lastDay[i]);
                    }

                    preparedStock.AddDayData(day.Key, changeList);
                    lastDay = day.Value.ToList();
                }

                preparedData.Add(preparedStock);
            }

            Console.WriteLine("Duomenų konvertavimas baigtas");
            return preparedData;
        }

    }
}

