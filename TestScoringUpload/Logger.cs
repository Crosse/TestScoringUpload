using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crosse.ExtensionMethods;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    internal class Logger : PropertyChangedBase
    {
        private static string messages;
        private static Logger logger;

        public static Logger GetLogger()
        {
            if (logger == null)
                logger = new Logger();

            return logger;
        }

        public static Logger Instance { get { return GetLogger(); } }

        public string Messages
        {
            get { return messages; }
        }

        public void Clear()
        {
            messages = String.Format("Log cleared at {0}\n\n", DateTime.Now.ToIsoDateTimeString());
        }

        public void AppendLine(string message)
        {
            Debug.WriteLine(message);

            messages += String.Format("{0}: {1}", DateTime.Now.ToIsoDateTimeString(),
                                                  (message.EndsWith("\n") ? message : message + "\n"));

            NotifyOfPropertyChange(() => Messages);
        }

        public void AppendLine(string format, object arg0)
        {
            AppendLine(String.Format(format, arg0));
        }

        public void AppendLine(string format, object arg0, object arg1)
        {
            AppendLine(String.Format(format, arg0, arg1));
        }

        public void AppendLine(string format, params object[] args)
        {
            AppendLine(String.Format(format, args));
        }

        public void AppendError(string format, params object[] args)
        {
            AppendLine(String.Format("[ERROR] {0}", format), args);
        }

        public void AppendWarning(string format, params object[] args)
        {
            AppendLine(String.Format("[WARNING] {0}", format), args);
        }
    }
}
