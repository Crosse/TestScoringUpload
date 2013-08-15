using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Xceed.Wpf.Toolkit;

namespace JMU.TestScoring
{
    public class Loader : IResult
    {
        private static Logger logger = Logger.GetLogger();
        readonly string message;
        readonly bool hide;

        public Loader(string message)
        {
            this.message = message;
        }

        public Loader(bool hide)
        {
            this.hide = hide;
        }

        #region IResult Members

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        public void Execute(ActionExecutionContext context)
        {
            var view = context.View as FrameworkElement;
            while (view != null)
            {
                var busyIndicator = view as BusyIndicator;
                if (busyIndicator != null)
                {
                    if (!String.IsNullOrEmpty(message))
                        busyIndicator.BusyContent = message;
                    busyIndicator.IsBusy = !hide;
                    break;
                }

                view = view.Parent as FrameworkElement;
            }

            try
            {
                Completed(this, new ResultCompletionEventArgs());
            }
            catch (ArgumentNullException e)
            {
                logger.AppendLine(e.Message);
            }
        }
        #endregion

        public static IResult Show(string message = null)
        {
            logger.AppendLine(message);
            return new Loader(message);
        }

        public static IResult Hide()
        {
            return new Loader(true);
        }

        public static bool Query(string question)
        {
            var result = Xceed.Wpf.Toolkit.MessageBox.Show(question, "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return (result == MessageBoxResult.Yes ? true : false);
        }

        public static void Notify(string text)
        {
            var result = Xceed.Wpf.Toolkit.MessageBox.Show(text, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
