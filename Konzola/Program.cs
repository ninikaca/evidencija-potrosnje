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
                    string[] files = Directory.GetFiles(putanja);

                    if (files.Length == 0)
                        Console.WriteLine("Nema nijedan CSV");
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

                                stream.Position = 0;
                                using (FileHandler fl = new FileHandler() { Stream = stream, FileName = Path.GetFileName(file) })
                                {
                                    // call service actions
                                    ChannelFactory<IService> service = new ChannelFactory<IService>("Service");
                                    IService service_proxy = service.CreateChannel();
                                    service_proxy.LoadSaveDatabase(fl.Stream, fl.FileName);

                                    fl.Dispose();
                                }
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
