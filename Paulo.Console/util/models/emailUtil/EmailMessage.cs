using MailKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulo.Console.model.emailUtil
{
    public class EmailMessage
    {
        public UniqueId UID { get; set; }
        public List<EmailAddress> ToAddresses { get; set; }
        public List<EmailAddress> FromAddresses { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public List<Attachments> attachments;

        public EmailMessage()
        {
            ToAddresses = new List<EmailAddress>();
            FromAddresses = new List<EmailAddress>();
        }

        public class Attachments
        {
            public string Name { get; set; }
            public MemoryStream Archive { get; set; }
        }
    }
}
