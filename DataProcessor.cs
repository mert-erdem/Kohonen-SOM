using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Kohonen_SOM
{
    class DataProcessor
    {
        private string _path;
        private List<double[]> _inputs = new List<double[]>();

        public DataProcessor(string path) => _path = path;

        public List<double[]> GetNormalizedData()
        {
            try
            {
                string line; int lineCount = 0;

                var streamReader = new StreamReader(_path);

                line = streamReader.ReadLine();

                while (line != null)
                {
                    if (lineCount != 0)
                    {
                        string[] columns = line.Split(',');
                        List<double> row = new List<double>();

                        foreach (var item in columns)
                        {
                            string value = item.Replace('.', ',');//double değere parse için ',' gerekiyor
                            try
                            {
                                row.Add(double.Parse(value));
                            }
                            catch (FormatException e)
                            {
                                row.Add(DummyCoding(item));
                            }
                        }
                        _inputs.Add(row.ToArray());
                    }
                    line = streamReader.ReadLine();
                    lineCount++;
                }
                streamReader.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            //veriler alındıktan ve double değerlere dönüştürüldükten sonra normalizasyon yapılır;
            DataNormalization();

            return _inputs;
        }

        //gelen harflerin alfabe sıralarına göre dummy coding yapan metod;
        private double DummyCoding(string input)
        {
            double letterIndex = 0;

            if (input != "TT")
            {
                char[] cArray = input.ToCharArray();
                letterIndex = cArray[0] - 64;//harfin alfabedeki değerini verir               
            }
            else// TT gelirse;
            {
                letterIndex = 9;
            }

            return letterIndex;
        }

        //min-max normalizasyonu
        private void DataNormalization()
        {
            List<double> columnValues = new List<double>();

            for (int i = 0; i < _inputs[0].Length; i++)
            {
                foreach (var row in _inputs)
                {
                    columnValues.Add(row[i]);
                }
                foreach (var row in _inputs)
                {
                    //min-max normalization
                    row[i] = (row[i] - columnValues.Min()) /
                        (columnValues.Max() - columnValues.Min());
                }
                columnValues.Clear();
            }
        }
    }
}
