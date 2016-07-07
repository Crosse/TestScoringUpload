using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.FtpClient;

namespace JMU.TestScoring
{
    public class FtpHelper : ITransferHelper
    {
        static Logger logger = Logger.GetLogger();
        static FtpClient client;
        static ConfigModel config = ConfigModel.Instance;

        #region ITransferHelper Members

        public bool Connect(string server, string username, System.Security.SecureString password)
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

            client = new FtpClient();
            client.Host = server;
            client.Credentials = new NetworkCredential(username, password);

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

        public bool UploadFile(string localPath, string remotePath)
        {
            FileInfo f = new FileInfo(localPath);

            if (!f.Exists)
            {
                logger.AppendLine("ERROR:  local file \"{0}\" does not exist.", localPath);
                return false;
            }

            logger.AppendLine(String.Format("Upload:  \"{0}\" --> \"{1}\"", localPath, remotePath));

            try
            {
                using (Stream l = File.OpenRead(localPath))
                using (Stream r = client.OpenWrite(remotePath, FtpDataType.Binary))
                {
                    BinaryReader reader = new BinaryReader(l);
                    BinaryWriter writer = new BinaryWriter(r);
                    try
                    {
                        if (l.Length <= int.MaxValue)
                            writer.Write(reader.ReadBytes((int)l.Length));
                        else
                        {
                            long pos = 0;
                            while (pos < l.Length)
                            {
                                byte[] bytes = reader.ReadBytes(int.MaxValue);
                                writer.Write(bytes);
                                pos += bytes.Length;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        if (writer != null)
                            writer.Close();
                        if (reader != null)
                            reader.Close();
                    }
                }
            }
            catch (Exception e)
            {
                logger.AppendLine(String.Format("WARNING: An error occurred uploading file \"{0}\":  {1}", localPath, e.Message));
                return false;
            }

            return true;
        }

        public bool WriteTextToFile(string remoteFilePath, string text)
        {
            bool result = false;
            try
            {
                using (Stream s = client.OpenAppend(remoteFilePath, FtpDataType.ASCII))
                {
                    StreamWriter wr = new StreamWriter(s);
                    try
                    {
                        wr.WriteLine(text);
                        result = true;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        if (wr != null)
                            wr.Close();
                    }
                }
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

        public string[] ListDirectory(string path)
        {
            FtpListItem[] items;

            try
            {
                items = client.GetListing(path);
            }
            catch (Exception e)
            {
                logger.AppendError(e.Message);
                throw new InvalidOperationException(e.Message, e);
            }

            return items.Select(f => f.FullName).ToArray();
        }

        #endregion
    }
}
