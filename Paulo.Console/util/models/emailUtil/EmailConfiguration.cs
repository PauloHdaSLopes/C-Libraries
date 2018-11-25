using Paulo.Console.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulo.Console.model.emailUtil
{
    public class EmailConfiguration : IEmailConfiguration
    {
        public string   Server      { get; set; }
        public int      Port        { get; set; }
        public string   Username    { get; set; }
        public string   Password    { get; set; }

        //public string   PopServer   { get; set; }
        //public int      PopPort     { get; set; }
        //public string   PopUsername { get; set; }
        //public string   PopPassword { get; set; }

        //public string   ImapServer { get; set; }
        //public int      ImapPort { get; set; }
        //public string   ImapUsername { get; set; }
        //public string   ImapPassword { get; set; }
    }
}
