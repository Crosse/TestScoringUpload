using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JMU.TestScoring
{
    public interface ITransferHelper
    {
        bool Connect(string server, string username, SecureString password);
        void Disconnect();

        bool UploadFile(string localPath, string remotePath);
        bool WriteTextToFile(string remoteFilePath, string text);
        bool CreateDirectory(string path);
        string[] ListDirectory(string path);
    }
}
