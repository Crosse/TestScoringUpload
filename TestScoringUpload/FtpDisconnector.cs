using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    public class FtpDisconnector : IResult
    {
        #region IResult Members
        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        public void Execute(ActionExecutionContext context)
        {
            ITransferHelper helper = new FtpHelper();
            helper.Disconnect();

            Completed(this, new ResultCompletionEventArgs());
        }

        #endregion
    }
}
