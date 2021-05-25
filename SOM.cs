using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohonen_SOM
{
    class SOM
    {
        private double[][] _inputs;
        private double[,][] _weigthMap;//ağırlıkların tutulduğu harita
        private double[,] _map;//en son işlem görüp konsolda gösterilecek olan harita
        private List<double>[,] _commonIndexes;//her node un en çok eşleştiği input un index ini belirtmek için bir list matrisi
        private int _attributeCount, _latticeLenght;

        private Random _random = new Random();


        public SOM(double[][] inputs,int latticeLength, int attributeCount)
        {
            _inputs = inputs;
            _attributeCount = attributeCount;
            _latticeLenght = latticeLength;
            _weigthMap = new double[latticeLength, latticeLength][];
            _map = new double[latticeLength, latticeLength];

            //ağırlık matrisi 0-1 aralığında rastgele olarak dolduruluyor
            InitializingWeights();
        }

        //ordering phase;
        public void Run(double timeConstant2, double learningRate0, double effectiveWidth0)
        {
            //Ordering Phase(1000 iterasyon, learningRate(başlangıç) 0.1, learningRate alt sınırı 0.01, effectiveWidth 5);
            TheAlgorithm(timeConstant2, learningRate0, effectiveWidth0, 0.01, false);
            CreateAndShowTheMap();//o anda oluşan harita konsolda gösterilir

            //Convergence Phase(50000 iterasyon, learningRate 0.01, learningRate alt sınırı 0, effectiveWidth 5);
            TheAlgorithm(500*Math.Pow(_latticeLenght, 2), 0.01, effectiveWidth0, 0, true);
            CreateAndShowTheMap();
        }

        
        private void TheAlgorithm(double timeConstant2, double learningRate0, 
            double effectiveWidth0, double lRLowerBound, bool convergence)
        {
            //her işlem öncesi node lara ait en çok tekrar eden index listeleri yenilenir(ordering-convergence)
            _commonIndexes = new List<double>[_latticeLenght,_latticeLenght];

            int iteration = 0;
            double effectiveWidth = effectiveWidth0;
            double learningRate = learningRate0;
            double timeConstant1 = timeConstant2 / Math.Log10(effectiveWidth0);
            int randomIndex;


            while (iteration != timeConstant2)
            {
                //input vektörlerinden rastgele biri seçilir;
                randomIndex = _random.Next(0, 500);
                double[] inputVector = _inputs[randomIndex];

                double minDistance = 10000, distance;
                int winnerNeuronX = -1, winnerNeuronY = -1;//node un koordinatları

                // kazanan node ve lokasyonu bulunur;
                for (int i = 0; i < _latticeLenght; i++)
                {
                    for (int j = 0; j < _latticeLenght; j++)
                    {
                        distance = EuclideanDistance(_weigthMap[i, j], inputVector);
                        if (distance < minDistance)
                        {
                            if (_commonIndexes[i, j] == null)// o node a ait commonIndexes listesi yok ise
                            {
                                _commonIndexes[i, j] = new List<double>();
                            }
                            minDistance = distance;
                            winnerNeuronX = i;
                            winnerNeuronY = j;                            
                            _commonIndexes[i, j].Add(randomIndex);/*kazanan node un koordinatlarına karşılık gelen
                            başka bir listedeki index e node un kazandığı input setinin bir göstergesi eklenir
                            (bu durumda o input setinin satır index i)*/
                        }
                    }
                }
                
                // haritadaki tüm node ların ağırlıkları hji ye bağlı olarak güncellenir;
                double hji;
                for (int i = 0; i < _latticeLenght; i++)
                {
                    for (int j = 0; j < _latticeLenght; j++)
                    {
                        hji = Gaussian(winnerNeuronX, winnerNeuronY, i, j, effectiveWidth);

                        for (int k = 0; k < _weigthMap[i, j].Length; k++)
                        {
                            //haritada bulunan ij node unun ağırlık vektörü güncelleniyor;
                            _weigthMap[i, j][k] = _weigthMap[i, j][k] + learningRate * hji * (inputVector[k] - _weigthMap[i, j][k]);
                        }
                    }
                }

                //Iterasyon sonu işlemleri;
                effectiveWidth = effectiveWidth0 * Math.Exp(-iteration / timeConstant1);

                if (learningRate > lRLowerBound)//öğrenme hızı alt sınır kontrolü
                {
                    learningRate = learningRate0 * Math.Exp(-iteration / timeConstant2);
                }

                iteration++;
            }

            
            if(convergence)
            {
                Console.WriteLine("-------------------------------------------------------------------------------------");
                Console.WriteLine("Convergence Phase"+Environment.NewLine);                
            }
            else
            {
                Console.WriteLine("Ordering Phase" + Environment.NewLine);
            }
            Console.WriteLine("LearningRate ve EffectiveWidth son değerleri: ");
            Console.WriteLine("LearningRate = " + learningRate + "  EffectiveWidth = " + effectiveWidth+Environment.NewLine);
            Console.WriteLine("Harita: " + Environment.NewLine);
            
        }

        //hji değerini bulmak için Gaussian fonksiyonu kullanıldı;
        private double Gaussian(int winnerX, int winnerY, int neuronX, int neuronY, double effectiveWidth)
        {
            double[] vector1 = { winnerX, winnerY };
            double[] vector2 = { neuronX, neuronY };

            double result = Math.Exp(-(Math.Pow(EuclideanDistance(vector1, vector2), 2) / (2 * Math.Pow(effectiveWidth, 2))));
            
            return result;
        }

        private double EuclideanDistance(double[] vector1, double[] vector2)
        {
            double sum = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                sum += Math.Pow(vector1[i] - vector2[i], 2);
            }

            double result = Math.Sqrt(sum);

            return result;
        }

        //sonuç haritasını oluşturan ve konsolda gösteren metod;
        private void CreateAndShowTheMap()
        {
            //harita oluşturulur;
            for (int i = 0; i < _latticeLenght; i++)
            {
                for (int j = 0; j < _latticeLenght; j++)
                {
                    if (_commonIndexes[i, j] != null)
                    {
                        //her node a karşılık gelen _commonIndexes[i,j] listesinin en çok tekrar eden değeri
                        //sonuç haritasının o indexlerdeki değeri olur;
                        _map[i, j] = _commonIndexes[i, j].GroupBy(item => item).
                            OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
                    }
                    else//o node da hiç eşleşme olmamış ise;
                    {
                        _map[i, j] = -1;
                    }
                }
            }

            //harita konsola yazdırılır;
            for (int i = 0; i < _latticeLenght; i++)
            {
                for (int j = 0; j < _latticeLenght; j++)
                {
                    Console.Write(_map[i, j] + "      ");
                }
                Console.WriteLine(Environment.NewLine);
            }
        }

        //başlangıçta ağırlıklar 0-1 aralığında rastgele dağıtılır;
        private void InitializingWeights()
        {            
            for (int i = 0; i < _latticeLenght; i++)
            {
                for (int j = 0; j < _latticeLenght; j++)
                {
                    double[] weightArray = new double[_attributeCount];

                    for (int k = 0; k < weightArray.Length; k++)
                    {
                        double randomWeight= _random.NextDouble();    

                        weightArray[k] = randomWeight;
                    }
                    _weigthMap[i, j] = weightArray;
                }
            }
        }
    }
}
