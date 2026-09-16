using System;
using System.Runtime.Serialization;

namespace ClassLibrary_Message
{
    [DataContract]
    public class MessageTCP
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Host { get; set; }

        [DataMember]
        public string User { get; set; }

        [DataMember]
        public string Receiver { get; set; }

        [DataMember]
        public DateTime SendAt { get; set; }

        public MessageTCP()
        {
        }
    }
}
