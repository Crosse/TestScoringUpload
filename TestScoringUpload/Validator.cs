using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    public class Validator : IResult
    {
        readonly string username;
        readonly string reportPath;

        public Validator(string username, string reportPath)
        {
            this.username = username;
            this.reportPath = reportPath;
        }

        public async Task<bool> ValidateFacultyUsername()
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            try
            {
                UserPrincipal princ = UserPrincipal.FindByIdentity(ctx, username);
                return !(princ == null);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region IResult Members

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        public async void Execute(ActionExecutionContext context)
        {
            bool valid = await ValidateFacultyUsername();
            if (!valid)
            {
                Loader.Hide().Execute(context);
            }

            Completed(this, new ResultCompletionEventArgs { WasCancelled = !valid });
        }

        #endregion
    }
}
