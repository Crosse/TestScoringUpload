using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JMU.TestScoring
{
    public class ConnectorBase : IResult
    {
        Logger logger = Logger.GetLogger();
        string server;
        string username;
        SecureString password;
        ITransferHelper helper;

        public ConnectorBase(string server, string username, SecureString password)
        {
            this.server = server;
            this.username = username;
            this.password = password;
        }

        protected void SetHelper(ITransferHelper helper)
        {
            this.helper = helper;
        }

        #region IResult Members
        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        //public async void Execute(ActionExecutionContext context)
        public void Execute(CoroutineExecutionContext context)
        {
            ResultCompletionEventArgs args = new ResultCompletionEventArgs();
            try
            {
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
