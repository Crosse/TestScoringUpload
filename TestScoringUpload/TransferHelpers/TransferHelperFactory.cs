using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMU.TestScoring
{
    public class TransferHelperFactory
    {
        public static ITransferHelper GetHelper(string transferProtocol)
        {
            switch (transferProtocol)
            {
                case "FTP":
                    return new FtpHelper();
                case "SFTP":
                    return new SftpHelper();
                default:
                    throw new NotSupportedException(transferProtocol);
            }
        }
    }
}
