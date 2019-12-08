using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace AI
{
    static class DimensionMinimization
    {

        public static void RewriteData(List<StockData> list, string folder)
        {
            foreach (StockData stockData in list)
            {
                using (StreamWriter writer = new StreamWriter(folder + "/" + stockData.GetName()))
                {
                    writer.WriteLine("Date,High,Low,Close");
                    foreach(var pair in stockData.GetAllData())
                    {
                        writer.WriteLine("{0},{1},{2},{3}", pair.Key.ToString("yyyy-MM-dd"),pair.Value[0], pair.Value[1], pair.Value[2]);
                    }
                }
            }

        }

        public static double GetAverage(List<double> list)
        {
            double average = 0;
            foreach (double item in list)
            {
                average = average + item;
            }
            return average / list.Count;
        }

        public static List<double> GetVariance(List<List<double>> list)
        {
            list = Normalize(MapByParameters(list));
            List<double> variancelist = new List<double>();
            for (int i = 0; i < list.Count; i++)
            {
                double average = GetAverage(list[i]);
                double variance = 0;
                variancelist.Add(average);
                foreach (double item in list[i])
                {
                    variance = variance + (item - average) * (item - average);
                }
                variancelist[0] = (variance / list[i].Count);
            }
            return variancelist;
        }

        public static List<List<double>> MapByParameters(List<List<double>> list)
        {
            List<List<double>> mappedlist = new List<List<double>>();
            foreach (double item in list[0])
            {
                mappedlist.Add(new List<double>());
            }
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < mappedlist.Count; j++)
                {
                    mappedlist[j].Add(list[i][j]);
                }
            }
            return mappedlist;
        }

        public static List<List<double>> Normalize(List<List<double>> list)
        {
            List<List<double>> newlist = new List<List<double>>();
            for (int i = 0; i < list.Count; i++)
            {
                newlist.Add(new List<double>());
                double max = list[i].Max();
                double min = list[i].Min();
                foreach (double item in list[i])
                {
                    double z = (item - min) / (max - min);
                    newlist[i].Add(z);
                }
            }
            return newlist;
        }

    }
}
