using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    public class SftpConnector : IResult
    {
        Logger logger = Logger.GetLogger();

        #region IResult Members
        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        //public async void Execute(ActionExecutionContext context)
        public void Execute(ActionExecutionContext context)
        {
            ResultCompletionEventArgs args = new ResultCompletionEventArgs();
            try
            {
                //await Task.Run(() => System.Threading.Thread.Sleep(2000));
                SshHelper.Connect();
            }
            catch (Exception e)
            {
                Loader.Hide().Execute(context);
                args.Error = e;
                args.WasCancelled = true;
            }

            Completed(this, args);
        }

        #endregion
    }
}
