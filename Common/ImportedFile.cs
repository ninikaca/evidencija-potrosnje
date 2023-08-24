using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    [Serializable]
    public class ImportedFile
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string FileName { get; set; }

        public ImportedFile() { }
    }
}
