using System.Security;

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
