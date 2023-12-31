﻿using System;
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (Stream != null)
                    {
                        Stream.Dispose();
                        Stream.Close();
                        Stream = null;
                    }
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
