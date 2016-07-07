using System;
using System.Security;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    public class SftpConnector : ConnectorBase
    {
        public SftpConnector(string server, string username, SecureString password) : base(server, username, password)
        {
            this.SetHelper(new SftpHelper());
        }
    }
}
