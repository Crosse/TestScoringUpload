using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using Crosse.ExtensionMethods;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace JMU.TestScoring
{
    public class SftpHelper : ITransferHelper
    {
        private static Logger logger = Logger.GetLogger();
        static SftpClient client;
        static ConfigModel config = ConfigModel.Instance;

        public bool Connect(string server, string username, SecureString password)
        {
            if (client != null && client.IsConnected)
                Disconnect();

            var addrs = Dns.GetHostAddresses(server);
            if (addrs.Length == 0)
            {
                logger.AppendLine("Connect error:  could not find IP address of {0}!", server);
                return false;
            }

            logger.AppendLine("Connect:  connecting to {0} ({1})", server, String.Join<IPAddress>(", ", addrs));

            KeyboardInteractiveAuthenticationMethod kbd = new KeyboardInteractiveAuthenticationMethod(username);
            kbd.AuthenticationPrompt += (s, e) =>
            {
                foreach (var prompt in e.Prompts)
                {
                    if (prompt.Request.IndexOf("assword:") > 0)
                    {
                        prompt.Response = password.ConvertToUnsecureString();
                    }
                }
            };

            ConnectionInfo connInfo = new ConnectionInfo(server, username,
                new PasswordAuthenticationMethod(username, password.ConvertToUnsecureString()), kbd);

            client = new SftpClient(connInfo);
            try
            {
                client.Connect();
                if (client.IsConnected)
                    logger.AppendLine("Connected.");
                else
                {
                    logger.AppendLine("ERROR:  An unspecified error occurred while connecting to {0}", server);
                    client.Dispose();
                }
            }
            catch (Exception e)
            {
                logger.AppendLine("ERROR:  {0}", e.Message);
                client.Dispose();
            }

            return client.IsConnected;
        }

        public void Disconnect()
        {
            if (client != null)
            {
                if (client.IsConnected)
                {
                    logger.AppendLine("Disconnect:  disconnecting from {0}", config.RemoteServer);
                    client.Disconnect();
                }
                client.Dispose();
            }
            else
                logger.AppendLine("Disconnect:  not connected to {0}", config.RemoteServer);
        }

        public bool UploadFile(string localFilePath, string remoteFilePath)
        {
            FileInfo f = new FileInfo(localFilePath);

            if (!f.Exists)
            {
                logger.AppendLine("ERROR:  local file \"{0}\" does not exist.", localFilePath);
                return false;
            }

            logger.AppendLine(String.Format("Upload:  \"{0}\" --> \"{1}\"", localFilePath, remoteFilePath));

            try
            {
                client.UploadFile(new FileStream(f.FullName, FileMode.Open), remoteFilePath, false);
            }
            catch (Exception e)
            {
                logger.AppendLine(String.Format("WARNING: An error occurred uploading file \"{0}\":  {1}", localFilePath, e.Message));
                return false;
            }

            return true;
        }

        public bool WriteTextToFile(string remoteFilePath, string text)
        {
            bool result = false;
            try
            {
                client.WriteAllText(remoteFilePath, text);
                result = true;
            }
            catch (Exception e)
            {
                logger.AppendError("Write to text file {0} failed:  {1}", remoteFilePath, e.Message);
            }

            return result;
        }

        public bool CreateDirectory(string path)
        {
            bool result = false;
            try
            {
                client.CreateDirectory(path);
                result = true;
            }
            catch (Exception e)
            {
                logger.AppendError("Remote directory creation failed: {0}.", e.Message);
            }

            return result;
        }

        public string[] ListDirectory(string remotePath)
        {
            IEnumerable<SftpFile> files;
            try
            {
                files = client.ListDirectory(remotePath);
            }
            catch (SftpPathNotFoundException e)
            {
                logger.AppendError(e.Message);
                throw new DirectoryNotFoundException(e.Message);
            }

            return files.Select(f => f.FullName).ToArray();
        }
    }
}
