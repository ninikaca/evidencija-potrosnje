using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public enum MessageType
    {
        [EnumMember] Info,
        [EnumMember] Warning,
        [EnumMember] Error
    }

    [DataContract]
    [Serializable]
    public class Audit
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public MessageType MessageType { get; set; }

        [DataMember]
        public string Message { get; set; }

        public Audit() { }

        public Audit(DateTime timestamp, MessageType messageType, string message)
        {
            Id = -1;
            Timestamp = timestamp;
            MessageType = messageType;
            Message = message;
        }
    }
}
