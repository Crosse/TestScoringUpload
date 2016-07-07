using System;
using System.Security;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    public class FtpConnector : ConnectorBase
    {
        public FtpConnector(string server, string username, SecureString password) : base(server, username, password)
        {
            this.SetHelper(new FtpHelper());
        }
    }
}
