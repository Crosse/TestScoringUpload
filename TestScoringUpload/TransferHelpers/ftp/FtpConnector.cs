using System;
using System.Security;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    public class FtpConnector : IResult
    {
        Logger logger = Logger.GetLogger();
        string server;
        string username;
        SecureString password;

        public FtpConnector(string server, string username, SecureString password)
        {
            this.server = server;
            this.username = username;
            this.password = password;
        }

        #region IResult Members
        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        //public async void Execute(ActionExecutionContext context)
        public void Execute(ActionExecutionContext context)
        {
            ResultCompletionEventArgs args = new ResultCompletionEventArgs();
            try
            {
                //await Task.Run(() => System.Threading.Thread.Sleep(2000));
                ITransferHelper helper = new FtpHelper();
                helper.Connect(server, username, password);
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
