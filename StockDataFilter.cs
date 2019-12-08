using System;
using System.IO;

namespace AI
{
    public class StockDataFilter
    {
        public void PrepareFiles(string sourcePath, string trainingPath, string testPath, DateTime dataAtleast, DateTime testDate)
        {
            if(!(Directory.Exists(trainingPath) || Directory.Exists(testPath)))
            {
                Directory.CreateDirectory(trainingPath);
                Directory.CreateDirectory(testPath);
            }
            else if(Directory.GetFiles(trainingPath).Length != 0 || Directory.GetFiles(testPath).Length != 0)
            {
                Console.WriteLine("Duomenys atfiltruoti jau anksčiau");
                return;
            }
            
            foreach(var file in Directory.EnumerateFiles(sourcePath))
            {
                if(IsDataAtleastFrom(file, dataAtleast))
                {
                    Console.WriteLine("Kopijuojamas failas {0}", file);
                    SplitData(file, trainingPath, testPath, dataAtleast, testDate);
                }
            }
        }

        private bool IsDataAtleastFrom(string filename, DateTime date)
        {
            using (var reader = new StreamReader(filename))
            {
                var headers = reader.ReadLine(); // skip header line
                if (string.IsNullOrWhiteSpace(headers)) // empty file
                    return false;

                var oldestData = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(oldestData)) // file with only the header line
                    return false;

                var oldestDate = DateTime.Parse(oldestData.Split(',')[0]);
                return date.CompareTo(oldestDate) >= 0;
            }
        }

        /// <summary>
        /// Splits data to training data and test data by filtering everything up to the date (not included)
        /// as training data and from specified date (included) as test data
        /// </summary>
        /// <param name="trainingPath">Folder for training data</param>
        /// <param name="testPath">Folder for test data</param>
        /// <param name="filename">File to split</param>
        /// <param name="testsFrom">Filter date</param>
        public void SplitData(string source, string trainingPath, string testPath, DateTime dataAtleast, DateTime testsFrom)
        {
            using (var reader = new StreamReader(source))
            {
                string header = reader.ReadLine();
                string line = null;

                using (var writer = new StreamWriter(Path.Combine(trainingPath, Path.GetFileName(source))))
                {
                    writer.WriteLine(header);
                    
                    while ((line = reader.ReadLine()) != null)
                    {
                        var date = DateTime.Parse(line.Split(',')[0]);
                        if (date.CompareTo(dataAtleast) >= 0)
                            break;
                    }

                    writer.WriteLine(line);
                    line = CopyFilePart(testsFrom, reader, writer);
                }

                using (var writer = new StreamWriter(Path.Combine(testPath, Path.GetFileName(source))))
                {
                    writer.WriteLine(header);
                    writer.WriteLine(line);     // first line is already read

                    line = CopyFilePart(DateTime.Now, reader, writer);
                }
            }
        }

        private string CopyFilePart(DateTime testsFrom, StreamReader reader, StreamWriter writer)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var date = DateTime.Parse(line.Split(',')[0]);
                if (date.CompareTo(testsFrom) >= 0)
                    break;
                else
                    writer.WriteLine(line);
            }

            return line;
        }
    }
}
