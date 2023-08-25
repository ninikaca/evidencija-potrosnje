using Common;
using System;
using System.IO;
using System.ServiceModel;

namespace Konzola
{
    public class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Unos direktorijuma CSV datoteka: ");
                string putanja = Console.ReadLine();


                if (Directory.Exists(putanja))
                {
                    
                    else
                    {
                        foreach (var file in files)
                        {
                            MemoryStream stream = new MemoryStream();
                            using (FileStream csv = new FileStream(file, FileMode.Open, FileAccess.Read))
                            {
                                csv.CopyTo(stream);
                                csv.Dispose();
                                csv.Close();

                               
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Direktorijum nije validan");
                }
            }
        }
    }
}
