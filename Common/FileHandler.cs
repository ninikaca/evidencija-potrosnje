using System;
using System.IO;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class FileHandler : IDisposable
    {
        [DataMember]
        public MemoryStream Stream { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        private bool disposed = false;

        

        public void Dispose()
        {
           
        }
    }
}
