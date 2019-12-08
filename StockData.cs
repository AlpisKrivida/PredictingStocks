using System;
using System.Collections.Generic;
using System.Linq;

namespace AI
{
    public class StockData
    {
        string name;
        Dictionary<DateTime, List<double>> data;

        public int PredictingCollumn { get; set; } = 0;

        public StockData(string name)
        {
            this.name = name.Split('\\')[1];
            data = new Dictionary<DateTime, List<double>>();
        }

        public void AddDayData(DateTime date, List<double> list)
        {
            data.Add(date, list);
        }

        public List<double> GetDayData(DateTime day)
        {
            if (!data.TryGetValue(day, out List<double> list))//if there is no such record of the day
                return null;

            return list;
        }

        public string GetName()
        {
            return name;
        }

        public List<KeyValuePair<DateTime,List<double>>> GetAllData()
        {
            return data.ToList();
        }

        public List<List<double>> GetAllDataAsDoubleList()
        {
            var datatemp = data.ToList();
            List<List<double>> newlist = new List<List<double>>();
            foreach (KeyValuePair<DateTime,List<double>> item in datatemp)
            {
                newlist.Add(item.Value.ToList());
            }
            return newlist;
        }

        public StockData GetDaysData(DateTime from, DateTime to)
        {
            if (from.CompareTo(to) > 0)
                throw new ArgumentException("to negali būti mažesnis už from");

            var filtered = new StockData("aaa\\" + name);
            for (var d = from; d.CompareTo(to) <= 0; d = d.AddDays(1))
            {
                if(data.Keys.Contains(d))
                    filtered.AddDayData(d, data[d]);
            }
            return filtered;
        }

        public List<DateTime> GetMultipleGuessData()
        {
            List<DateTime> guessedDates = new List<DateTime>();
            DateTime temp = new DateTime();

            for (int x = data.Keys.Count() - 10; x > 0; x--)
            {
                temp = data.Keys.ElementAt(x);
                if (data.Keys.Contains(temp.AddDays(-1)))
                {
                    guessedDates.Add(temp.AddDays(-1));
                    temp.AddMonths(-1);
                }

                if (guessedDates.Count >= 3)
                    break;
            }

            return guessedDates;
        }

        public DateTime GetGuessData()
        {
            DateTime temp = new DateTime();

            for (int x = data.Keys.Count() - 10;x>0;x--)
            {
                temp = data.Keys.ElementAt(x);
                if (data.Keys.Contains(temp.AddDays(-1)))
                    return temp.AddDays(-1);
            }

            return new DateTime();
        }

        public static List<List<double>> GetAllDataAsDoubleList(List<StockData> data)
        {
            List<List<double>> newlist = new List<List<double>>();
            foreach (StockData stockdata in data)
            {
                var datatemp = stockdata.GetAllDataAsDoubleList();
                foreach (List<double> item in datatemp)
                {
                    newlist.Add(item.ToList());
                }
            }
            return newlist;
        }

        public override string ToString()
        {
            string line = name + "\n";
            foreach (KeyValuePair<DateTime,List<double>> pair in data)
            {
                line = line + pair.Key;
                foreach (double item in pair.Value)
                {
                    line = line + ", " + item;
                }
                line = line + "\n";
            }
            return line;
        }
    }
}
