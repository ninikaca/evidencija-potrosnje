using Common;
using System;
using System.Collections.Generic;

namespace BazaPodataka
{
    public class In : IBaza
    {
        public static Dictionary<int, Audit> Audits = new Dictionary<int, Audit>();
        public static Dictionary<int, Load> Loads = new Dictionary<int, Load>();
        public static Dictionary<int, ImportedFile> ImportedFiles = new Dictionary<int, ImportedFile>();

        public static int AuditsCounter = 0, LoadsCounter = 0, ImportedFilesCounter = 0;
        public static string CurrentFile = string.Empty;

        public static List<int> LoadedIds = new List<int>();
    }
}
