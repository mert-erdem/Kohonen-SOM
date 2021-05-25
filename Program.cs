using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohonen_SOM
{
    class Program
    {
        static void Main(string[] args)
        {
            List<double[]> inputs;

            //veriler, dosyadan alınıp normalize edilir(min-max normalizasyonu).
            var dataProcessor = new DataProcessor(Directory.GetCurrentDirectory() + "\\emlak-veri.txt");
            inputs = dataProcessor.GetNormalizedData();

            var som = new SOM(inputs.ToArray(), 10, 8);//10x10 lattice e sahip, 8 attribute için bir SOM oluşturulur
            som.Run(1000, 0.1,5);//1000 iterasyon//0.1 learningRate//5 effectiveWidth
            //Convergence Phase iterasyon sayısı fonksiyonun içinde hesaplanır
            //Topolojik fonksiyon olarak Gaussian fonksiyonu kullanılmıştır
        }
    }
}
