using BazaPodataka;
using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Servis
{
    public delegate void SaveDbDelegate(List<Load> loads);

    public class Service : IService
    {
        public static List<int> LoadedIds = In.LoadedIds;

        public void LoadSaveDatabase(MemoryStream stream, string filename)
        {
            bool okay = true;
            string error_message = string.Empty;
            string where_to_save_data = ConfigurationManager.AppSettings["baza"];
            List<Load> loads = new List<Load>();
            bool isOstv = filename.StartsWith("ostv") == true;

            if (filename.StartsWith("ostv") || filename.StartsWith("prog"))
            {
                using (StreamReader csv = new StreamReader(stream))
                {
                    string[] data = csv.ReadToEnd().Split('\n');
                    string[] rows = data.Take(data.Length - 1).ToArray();
                    int row_counter = 0;

                    foreach (var row in rows)
                    {
                        string[] splited_data = row.Split(',');

                        if (splited_data[0].Equals("TIME_STAMP"))
                        {
                            continue;
                        }

                        bool vdate = DateTime.TryParseExact(splited_data[0], "yyyy-MM-dd HH:mm",
                                     CultureInfo.InvariantCulture,
                                     DateTimeStyles.None, out DateTime date);

                        bool vvalue = double.TryParse(splited_data[1].Replace('.', ','), out double value);

                        if (vdate == false || vvalue == false || value < 0)
                        {
                            okay = false;
                            error_message = "U datoteci " + filename + " nalazi se nevalidni podaci";
                            break;
                        }

                        Load new_load;
                        if (isOstv)
                        {
                            new_load = new Load(date, -1, value, -1, -1);
                            loads.Add(new_load);
                        }
                        else
                        {
                            new_load = new Load(date, value, -1, -1, -1);
                            loads.Add(new_load);
                        }

                        row_counter++;
                    }

                    
                }
            }
            else
            {
                okay = false;
            }

            if (okay)
            {
                if (where_to_save_data.Equals("i"))
                {
                    In.Save(loads, null);
                }
                else
                {
                    Xml.Save(loads, null);
                }
            }
            else
            {
                Audit audit = new Audit(DateTime.Now, MessageType.Error, error_message);

                if (where_to_save_data.Equals("i"))
                {
                    In.Save(null, audit);
                }
                else
                {
                    Xml.Save(null, audit);
                }
            }

        }

        public void Calculation()
        {
            
        }
    }
}
