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
        public event SaveDbDelegate SaveDelegate;

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

                    csv.Dispose();
                    csv.Close();

                    if (rows.Length < 23 || rows.Length > 25)
                    {
                        okay = false;
                        error_message = "U datoteci " + filename + " nalazi se neodgovarajući broj redova: " + rows.Length;
                    }
                }
            }
            else
            {
                okay = false;
            }

            if (okay)
            {
                In.CurrentFile = filename;
                if (where_to_save_data.Equals("i"))
                {
                    In.Save(loads, null);
                }
                else
                {
                    Xml.Save(loads, null);
                }
                In.CurrentFile = string.Empty;
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

            // run calculation
            Calculation();

            // write in memory output
            if (where_to_save_data.Equals("i"))
            {
                Console.WriteLine("====================================================== LOADS ==================================================");
                Console.WriteLine("{0,5} {1,30} {2,15} {3,15} {4,15} {5,15} {6,10}", "ID", "TIME", "PROG", "OSTV", "ABS", "SQR", "IFID");

                foreach (Load l in In.Loads.Values)
                {
                    Console.WriteLine("{0,5} {1,30} {2,15} {3,15} {4,15} {5,15} {6,10}",
                                          l.Id, l.Timestamp,
                                          l.ForecastValue == -1 ? "N/A" : l.ForecastValue.ToString("F6").Replace(',', '.'),
                                          l.MeasuredValue == -1 ? "N/A" : l.MeasuredValue.ToString("F6").Replace(',', '.'),
                                          l.AbsolutePercentageDeviation == -1 ? "N/A" : l.AbsolutePercentageDeviation.ToString("F6").Replace(',', '.'),
                                          l.SquaredDeviation == -1 ? "N/A" : l.SquaredDeviation.ToString("F6").Replace(',', '.'),
                                          l.ImportedFileId);
                }
                Console.WriteLine();

                Console.WriteLine("================================================ AUDITS ===============================================");
                Console.WriteLine("{0,-5} {1,-30}   {2,-10} {3,-50}", "ID", "TIME", "MSGT", "MSG");

                foreach (Audit a in In.Audits.Values)
                {
                    Console.WriteLine("{0,-5} {1,-30}   {2,-10} {3,-50}",
                                          a.Id, a.Timestamp, a.MessageType, a.Message);
                }
                Console.WriteLine();

                Console.WriteLine("============================================= IMPORTED FILES ==========================================");
                Console.WriteLine("{0,5} {1,50}", "ID", "FILENAME");

                foreach (ImportedFile i in In.ImportedFiles.Values)
                {
                    Console.WriteLine("{0,5} {1,50}", i.Id, i.FileName);
                }
                Console.WriteLine();
            }
        }

        public void Calculation()
        {
            string where_to_save_data = ConfigurationManager.AppSettings["baza"];
            string which_formula = ConfigurationManager.AppSettings["formula"];

            if (where_to_save_data.Equals("i"))
            {
                List<Load> loads = new List<Load>();

                foreach (int id in LoadedIds)
                {
                    if (In.Loads.TryGetValue(id, out Load load_dic))
                    {
                        Load load = new Load(load_dic.Timestamp, load_dic.ForecastValue, load_dic.MeasuredValue, load_dic.AbsolutePercentageDeviation, load_dic.SquaredDeviation);
                        load.Id = id;

                        if (load.ForecastValue != -1 && load.MeasuredValue != -1)
                        {
                            if (which_formula.Equals("q")) // quadric
                            {
                                load.SquaredDeviation = Math.Pow(((load.MeasuredValue - load.ForecastValue) / load.MeasuredValue), 2);
                            }
                            else
                            {
                                load.AbsolutePercentageDeviation = ((Math.Abs(load.MeasuredValue - load.ForecastValue) / load.MeasuredValue)) * 100;
                            }
                        }

                        loads.Add(load);
                    }
                }
            }
            else
            {
                List<Load> loads = new List<Load>();
                string tbl_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TBL_LOAD.xml");

                if (!File.Exists(tbl_path))
                    return;

                foreach (int id in LoadedIds)
                {
                    MemoryStream stream = new MemoryStream();
                    using (FileStream xml = new FileStream(tbl_path, FileMode.Open, FileAccess.Read))
                    {
                        xml.CopyTo(stream);
                        xml.Dispose();
                        xml.Close();
                    }

                    stream.Position = 0;

                    using (FileHandler fileHandler = new FileHandler() { Stream = stream, FileName = "TBL_LOAD.xml" })
                    {
                        XmlDocument db = new XmlDocument();
                        db.Load(fileHandler.Stream);

                        XmlNode node = db.SelectSingleNode("//row[ID='" + id + "']");

                        if (node != null)
                        {
                            double quadric, absolute, forecast, measured;
                            var abs = node.SelectSingleNode("ABSOLUTE_PERCENTAGE_DEVIATION").InnerText;
                            var sqr = node.SelectSingleNode("SQUARED_DEVIATION").InnerText;

                            if (!double.TryParse(abs.Replace('.', ','), out absolute))
                            {
                                absolute = -1;
                            }

                            if (!double.TryParse(sqr.Replace('.', ','), out quadric))
                            {
                                quadric = -1;
                            }

                            if (!double.TryParse(node.SelectSingleNode("FORECAST_VALUE").InnerText.Replace('.', ','), out forecast))
                            {
                                forecast = -1;
                            }

                            if (!double.TryParse(node.SelectSingleNode("MEASURED_VALUE").InnerText.Replace('.', ','), out measured))
                            {
                                measured = -1;
                            }


                            Load loaded = new Load()
                            {
                                Id = int.Parse(node.SelectSingleNode("ID").InnerText),
                                Timestamp = DateTime.Parse(node.SelectSingleNode("TIME_STAMP").InnerText),
                                ForecastValue = forecast,
                                MeasuredValue = measured,
                                AbsolutePercentageDeviation = absolute,
                                SquaredDeviation = quadric,
                                ImportedFileId = int.Parse(node.SelectSingleNode("IMPORTED_FILE_ID").InnerText)
                            };

                            if (loaded.ForecastValue != -1 && loaded.MeasuredValue != -1)
                            {
                                if (which_formula.Equals("q")) // quadric
                                {
                                    loaded.SquaredDeviation = Math.Pow(((loaded.MeasuredValue - loaded.ForecastValue) / loaded.MeasuredValue), 2);
                                }
                                else
                                {
                                    loaded.AbsolutePercentageDeviation = ((Math.Abs(loaded.MeasuredValue - loaded.ForecastValue) / loaded.MeasuredValue)) * 100;
                                }
                            }

                            loads.Add(loaded);
                        }

                        fileHandler.Dispose();
                    }
                }
            }
        }
    }
}
