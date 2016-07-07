using System;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    public class SftpDisconnector : DisconnectorBase
    {
        public SftpDisconnector() : base()
        {
            this.SetHelper(new SftpHelper());
        }
    }
}
