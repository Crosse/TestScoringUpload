using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Crosse.ExtensionMethods;
using Renci.SshNet;

namespace JMU.TestScoring
{
    public class SshHelper
    {
        private static Logger logger = Logger.GetLogger();
        static SftpClient client;
        static ConfigModel config = ConfigModel.Instance;

        public static void Connect()
        {
            if (client != null && client.IsConnected)
            {
                logger.AppendLine("Connect:  already connected to {0}", config.RemoteServer);
                return;
            }

            var addrs = Dns.GetHostAddresses(config.RemoteServer);
            if (addrs.Length == 0)
            {
                logger.AppendLine("Connect error:  could not find IP address of {0}!", config.RemoteServer);
                return;
            }

            logger.AppendLine("Connect:  connecting to {0} ({1})", config.RemoteServer, String.Join<IPAddress>(", ", addrs));

            KeyboardInteractiveAuthenticationMethod kbd = new KeyboardInteractiveAuthenticationMethod(config.RemoteServerUser);
            kbd.AuthenticationPrompt += (s, e) =>
            {
                foreach (var prompt in e.Prompts)
                {
                    if (prompt.Request.IndexOf("assword:") > 0)
                    {
                        prompt.Response = config.RemoteServerPassword.ConvertToUnsecureString();
                    }
                }
            };

            ConnectionInfo connInfo = new ConnectionInfo(config.RemoteServer, config.RemoteServerUser,
                new PasswordAuthenticationMethod(config.RemoteServerUser, config.RemoteServerPassword.ConvertToUnsecureString()),
                kbd);

            client = new SftpClient(connInfo);
            client.Connect();
            logger.AppendLine("Connected.");
        }

        public static void Disconnect()
        {
            if (client != null && client.IsConnected)
            {
                logger.AppendLine("Disconnect:  disconnecting from {0}", config.RemoteServer);
                client.Disconnect();
            }
            else
                logger.AppendLine("Disconnect:  not connected to {0}", config.RemoteServer);
        }

        public static SftpClient Client
        {
            get { return client; }
        }
    }
}
