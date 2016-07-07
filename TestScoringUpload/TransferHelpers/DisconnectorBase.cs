using System;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    public class DisconnectorBase : IResult
    {
        ITransferHelper helper;

        #region IResult Members
        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        protected void SetHelper(ITransferHelper helper)
        {
            this.helper = helper;
        }

        public void Execute(CoroutineExecutionContext context)
        {
            helper.Disconnect();

            Completed(this, new ResultCompletionEventArgs());
        }

        #endregion
    }
}
