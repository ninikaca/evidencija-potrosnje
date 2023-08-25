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
        public void LoadSaveDatabase(MemoryStream stream, string filename)
        {
           
        }        
    }
}
