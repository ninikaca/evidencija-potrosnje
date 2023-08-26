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
    
        public static List<int> LoadedIds = new List<int>();

        public static void Save(List<Load> loads, Audit audit)
        {
            if (loads != null)
            {
                ImportedFile importedFile = new ImportedFile() { FileName = CurrentFile, Id = ++ImportedFilesCounter };
                bool exists = false;

                foreach (Load load in loads)
                {
                    foreach (Load l in Loads.Values)
                    {
                        if (l.Timestamp == load.Timestamp)
                        {
                            // update forecast and measured values
                            if (load.ForecastValue != -1)
                                l.ForecastValue = load.ForecastValue;

                            if (load.MeasuredValue != -1)
                                l.MeasuredValue = load.MeasuredValue;

                            l.ImportedFileId = importedFile.Id;
                            exists = true;
                            break;
                        }
                    }

                    // create a new record
                    if (exists == false) // it doesn't exist
                    {
                        load.Id = ++LoadsCounter;
                        load.ImportedFileId = importedFile.Id;
                        LoadedIds.Add(load.Id);
                        Loads.Add(load.Id, load);
                    }
                }

                // save log to imported files dictonary
                ImportedFiles.Add(ImportedFilesCounter, importedFile);

                // save audit about successfully imported file
                Audit a = new Audit(DateTime.Now, MessageType.Info, "Datoteka " + In.CurrentFile + " je uspešno pročitana");
                a.Id = ++AuditsCounter;
                Audits.Add(a.Id, a);
            }
            else
            {
                // save audit do dictonary
                audit.Id = ++AuditsCounter;
                Audits.Add(audit.Id, audit);
            }
        }

        // for delegate & event
        public void SaveDb(List<Load> loads)
        {
           
        }
    }
}
