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
                        
                    }

                    fl.Dispose();
                }
            }
        }
    }
}
