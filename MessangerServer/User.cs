using System.Collections.Generic;

namespace MessangerServer
{
    public class User
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Host { get; set; }

        public virtual ICollection<Message>? SentMessages { get; set; }
        public virtual ICollection<Message>? ReceivedMessages { get; set; }
    }
}

