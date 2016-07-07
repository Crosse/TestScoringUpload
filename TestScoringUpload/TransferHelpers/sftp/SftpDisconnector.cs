using System;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    public class SftpDisconnector : IResult
    {
        #region IResult Members
        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        public void Execute(ActionExecutionContext context)
        {
            ITransferHelper helper = new SftpHelper();
            helper.Disconnect();

            Completed(this, new ResultCompletionEventArgs());
        }

        #endregion
    }
}
