using System;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    public class FtpDisconnector : DisconnectorBase
    {
        public FtpDisconnector() : base()
        {
            this.SetHelper(new FtpHelper());
        }
    }
}
