using System;
using System.ServiceModel;
namespace Servis
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ServiceHost host = new ServiceHost(typeof(Service));
                host.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Servis je uspesno pokrenut...");
            Console.ReadLine();
        }
    }
}
