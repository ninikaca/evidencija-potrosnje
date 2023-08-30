using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace BazaPodataka
{
    public class Xml : IBaza
    {
        public static void Save(List<Load> loads, Audit audit)
        {
            string load_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TBL_LOAD.xml");
            string audit_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TBL_AUDIT.xml");
            string imported_file_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TBL_IMPORTED_FILE.xml");

            if (loads != null)
            {
                int max_id_imported = 1;
                ImportedFile importedFile = new ImportedFile() { FileName = In.CurrentFile };

                // if imported file xml doesn't exist create a new one
                if (File.Exists(imported_file_path) == false)
                {
                    string root_element = "STAVKE";
                    XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "no"), new XElement(root_element));
                    xml.Save(imported_file_path);
                }

                // if audit xml doesn't exist create a new one
                if (File.Exists(audit_path) == false)
                {
                    string root_element = "STAVKE";
                    XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "no"), new XElement(root_element));
                    xml.Save(audit_path);
                }

                // if loads xml doesn't exist create a new one
                if (File.Exists(load_path) == false)
                {
                    // create a new empty file
                    string root_element = "rows";
                    XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "no"), new XElement(root_element));
                    xml.Save(load_path);
                }

                // now we can write imported data to xml file
                MemoryStream stream = new MemoryStream();
                using (FileStream fs = new FileStream(imported_file_path, FileMode.Open, FileAccess.Read))
                {
                    fs.CopyTo(stream);
                    fs.Dispose();
                    fs.Close();

                    stream.Position = 0;
                    using (FileHandler fl = new FileHandler() { Stream = stream, FileName = Path.GetFileName(imported_file_path) })
                    {
                        XDocument xml = XDocument.Load(fl.Stream);
                        XElement stavke = xml.Element("STAVKE");
                        var elements = xml.Descendants("ID");

                        try
                        {
                            max_id_imported = elements.Max(e => int.Parse(e.Value));
                            importedFile.Id = ++max_id_imported;
                        }
                        catch
                        {
                            importedFile.Id = 1;
                        }

                        // log imported file info into xml
                        var im_file = new XElement("row");
                        im_file.Add(new XElement("ID", (importedFile.Id).ToString()));
                        im_file.Add(new XElement("FILE_NAME", importedFile.FileName));

                        stavke.Add(im_file);
                        xml.Save(imported_file_path);

                        fl.Dispose();
                    }
                }

                // now save data from load to xml
                stream = new MemoryStream();
                using (FileStream fs = new FileStream(load_path, FileMode.Open, FileAccess.Read))
                {
                    fs.CopyTo(stream);
                    fs.Dispose();
                    fs.Close();

                    stream.Position = 0;
                    using (FileHandler fl = new FileHandler() { Stream = stream, FileName = Path.GetFileName(load_path) })
                    {
                        XmlDocument xml_load = new XmlDocument();
                        xml_load.Load(fl.Stream);
                        fl.Stream.Position = 0;
                        XDocument xml = XDocument.Load(fl.Stream);
                        XElement rows = xml.Element("rows");
                        var elements = xml.Descendants("ID");
                        int max_load_id;

                        try
                        {
                            max_load_id = elements.Max(e => int.Parse(e.Value));
                        }
                        catch
                        {
                            max_load_id = 0;
                        }

                        foreach (Load l in loads)
                        {
                            XmlNode xe;

                            try
                            {
                                xe = xml_load.SelectSingleNode("//row[TIME_STAMP='" + l.Timestamp.ToString("yyyy-MM-dd HH:mm") + "']");
                            }
                            catch
                            {
                                xe = null;
                            }

                            if (xe == null)
                            {
                                string forecast, measured, absolute, quadric;
                                if (l.ForecastValue == -1) forecast = "N/A"; else forecast = l.ForecastValue.ToString().Replace(',', '.');
                                if (l.MeasuredValue == -1) measured = "N/A"; else measured = l.MeasuredValue.ToString().Replace(',', '.');
                                if (l.SquaredDeviation == -1) quadric = "N/A"; else quadric = l.SquaredDeviation.ToString().Replace(',', '.');
                                if (l.AbsolutePercentageDeviation == -1) absolute = "N/A"; else absolute = l.AbsolutePercentageDeviation.ToString().Replace(',', '.');

                                XElement xen = new XElement("row");
                                xen.Add(new XElement("ID", (max_load_id + 1).ToString()));
                                xen.Add(new XElement("TIME_STAMP", l.Timestamp.ToString("yyyy-MM-dd HH:mm")));
                                xen.Add(new XElement("FORECAST_VALUE", forecast));
                                xen.Add(new XElement("MEASURED_VALUE", measured));
                                xen.Add(new XElement("ABSOLUTE_PERCENTAGE_DEVIATION", absolute));
                                xen.Add(new XElement("SQUARED_DEVIATION", quadric));
                                xen.Add(new XElement("IMPORTED_FILE_ID", importedFile.Id.ToString()));

                                rows.Add(xen);

                                max_load_id++;
                                In.LoadedIds.Add(max_load_id);
                                xml.Save(load_path);
                            }
                            else
                            {
                                if (l.ForecastValue != -1)
                                    xe.SelectSingleNode("FORECAST_VALUE").InnerText = l.ForecastValue.ToString().Replace(',', '.');

                                if (l.MeasuredValue != -1)
                                    xe.SelectSingleNode("MEASURED_VALUE").InnerText = l.MeasuredValue.ToString().Replace(',', '.');

                                l.Id = int.Parse(xe.SelectSingleNode("ID").InnerText);
                                In.LoadedIds.Add(l.Id);
                                xml_load.Save(load_path);
                            }
                        }

                        fl.Dispose();
                    }
                }

                // if audit xml doesn't exist create a new one
                if (File.Exists(audit_path) == false)
                {
                    string root_element = "STAVKE";
                    XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "no"), new XElement(root_element));
                    xml.Save(audit_path);
                }

                // save audit about successfull read of csv
                Audit a = new Audit(DateTime.Now, MessageType.Info, "Datoteka " + In.CurrentFile + " je uspešno pročitana");

                stream = new MemoryStream();
                using (FileStream csv = new FileStream(audit_path, FileMode.Open, FileAccess.Read))
                {
                    csv.CopyTo(stream);
                    csv.Dispose();
                    csv.Close();

                    stream.Position = 0;
                    using (FileHandler fl = new FileHandler() { Stream = stream, FileName = Path.GetFileName(audit_path) })
                    {
                        XDocument xml = XDocument.Load(fl.Stream);
                        XElement stavke = xml.Element("STAVKE");
                        var elements = xml.Descendants("ID");
                        int max_id_audit;

                        try
                        {
                            max_id_audit = elements.Max(e => int.Parse(e.Value));
                        }
                        catch
                        {
                            max_id_audit = 0;
                        }

                        // log imported file info into xml
                        var au_file = new XElement("row");
                        au_file.Add(new XElement("ID", (max_id_audit + 1).ToString()));
                        au_file.Add(new XElement("TIME_STAMP", a.Timestamp.ToString("yyyy-MM-dd HH:mm.fff")));
                        au_file.Add(new XElement("MESSAGE_TYPE", a.MessageType.ToString()));
                        au_file.Add(new XElement("MESSAGE", a.Message));

                        stavke.Add(au_file);
                        xml.Save(audit_path);

                        fl.Dispose();
                    }
                }
            }
            else
            {
                // if audit xml doesn't exist create a new one
                if (File.Exists(audit_path) == false)
                {
                    string root_element = "STAVKE";
                    XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "no"), new XElement(root_element));
                    xml.Save(audit_path);
                }

                Audit a = audit;
                // save audit to xml
                MemoryStream stream = new MemoryStream();
                using (FileStream fs = new FileStream(audit_path, FileMode.Open, FileAccess.Read))
                {
                    fs.CopyTo(stream);
                    fs.Dispose();
                    fs.Close();

                    stream.Position = 0;
                    using (FileHandler fl = new FileHandler() { Stream = stream, FileName = Path.GetFileName(audit_path) })
                    {
                        XDocument xml = XDocument.Load(fl.Stream);
                        XElement stavke = xml.Element("STAVKE");
                        var elements = xml.Descendants("ID");
                        int max_id_audit;

                        try
                        {
                            max_id_audit = elements.Max(e => int.Parse(e.Value));
                        }
                        catch
                        {
                            max_id_audit = 0;
                        }

                        // log imported file info into xml
                        var au_file = new XElement("row");
                        au_file.Add(new XElement("ID", (max_id_audit + 1).ToString()));
                        au_file.Add(new XElement("TIME_STAMP", a.Timestamp.ToString("yyyy-MM-dd HH:mm.fff")));
                        au_file.Add(new XElement("MESSAGE_TYPE", a.MessageType.ToString()));
                        au_file.Add(new XElement("MESSAGE", a.Message));

                        stavke.Add(au_file);
                        xml.Save(audit_path);

                        fl.Dispose();
                    }
                }
            }
        }


        // for delegate & event
        public void SaveDb(List<Load> loads)
        {
            string load_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TBL_LOAD.xml");
            MemoryStream stream = new MemoryStream();

            using (FileStream fs = new FileStream(load_path, FileMode.Open, FileAccess.Read))
            {
                fs.CopyTo(stream);
                fs.Dispose();
                fs.Close();

                stream.Position = 0;
                using (FileHandler fl = new FileHandler() { Stream = stream, FileName = Path.GetFileName(load_path) })
                {
                    XmlDocument xml_load = new XmlDocument();
                    xml_load.Load(fl.Stream);
                    fl.Stream.Position = 0;
                    XDocument xml = XDocument.Load(fl.Stream);
                    XElement rows = xml.Element("rows");
                    var elements = xml.Descendants("ID");

                    foreach (Load load in loads)
                    {
                        XmlNode xe;

                        try
                        {
                            xe = xml_load.SelectSingleNode("//row[TIME_STAMP='" + load.Timestamp.ToString("yyyy-MM-dd HH:mm") + "']");
                        }
                        catch
                        {
                            xe = null;
                        }

                        if (xe != null)
                        {
                            string absolute, quadric;
                            if (load.SquaredDeviation == -1) quadric = "N/A"; else quadric = load.SquaredDeviation.ToString().Replace(',', '.');
                            if (load.AbsolutePercentageDeviation == -1) absolute = "N/A"; else absolute = load.AbsolutePercentageDeviation.ToString().Replace(',', '.');

                            if (load.SquaredDeviation != -1)
                                xe.SelectSingleNode("SQUARED_DEVIATION").InnerText = load.SquaredDeviation.ToString().Replace(',', '.');

                            if (load.AbsolutePercentageDeviation != -1)
                                xe.SelectSingleNode("ABSOLUTE_PERCENTAGE_DEVIATION").InnerText = load.AbsolutePercentageDeviation.ToString().Replace(',', '.');
                        }

                        xml_load.Save(load_path);
                    }

                    fl.Dispose();
                }
            }
        }
    }
}
