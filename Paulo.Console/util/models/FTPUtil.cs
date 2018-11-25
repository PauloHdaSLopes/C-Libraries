using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Paulo.Console.util.models
{
    public class FTPUtil:IDisposable
    {
        public string _host { get; set; }
        public string _username { get; set; }
        public string _password { get; set; }

        private SftpClient sftpClient;

        //test.rebex.net:22
        //demo
        //password

        public FTPUtil(string _host, string _username, string _password)
        {
            this._host = _host;
            this._username = _username;
            this._password = _password;
            this.sftpClient = new SftpClient(_host, _username, _password);
        }

        public void Connect()
        {
            this.sftpClient.Connect();
        }

        public void Disconnect()
        {
            this.sftpClient.Disconnect();
        }

        public void ListItemsInDirectory(string dirName)
        {
            var dir = this.sftpClient.ListDirectory(dirName);

        }

        public List<string> ListAllDirectories()
        {
            List<string> lstReturn = new List<string>();

            var dir = this.sftpClient.ListDirectory("");
            foreach (var item in dir)
            {
                if (item.IsDirectory)
                {
                    var deepPath = this.sftpClient.ListDirectory(item.FullName);
                    foreach (var deep in deepPath)
                    {
                        lstReturn.Add(deep.FullName);
                    }
                }
            }
            return lstReturn;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
