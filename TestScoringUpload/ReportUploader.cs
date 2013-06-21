using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using Caliburn.Micro;
using Crosse.ExtensionMethods;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Renci.SshNet.Common;

namespace JMU.TestScoring
{
    public class ReportUploader : IResult
    {
        private Logger logger = Logger.GetLogger();
        readonly string studentReportPath;
        readonly string username;
        readonly string testCode;

        ConfigModel config = ConfigModel.Instance;

        public ReportUploader(string username, string testCode)
        {
            this.studentReportPath = Path.Combine(config.DefaultSourcePath, config.StudentReportsSubdirectory);
            this.username = username;
            this.testCode = testCode;
        }

        #region IResult Members

        public event EventHandler<ResultCompletionEventArgs> Completed = delegate { };

        public void Execute(ActionExecutionContext context)
        {
            ResultCompletionEventArgs args = new ResultCompletionEventArgs();

            string remoteDir = UnixPath.Combine(config.RemoteServerBaseDirectory, username);
            string remoteStudentDir = UnixPath.Combine(remoteDir, config.StudentReportsSubdirectory);

            var files = Directory.GetFiles(config.DefaultSourcePath, String.Format("{0}*.*", config.FilePrefix));
            logger.AppendLine(String.Format("Found {0} files with prefix \"{1}\"", files.Length, config.FilePrefix));

            var studentFiles = Directory.GetFiles(studentReportPath, String.Format("{0}*.*", config.FilePrefix));
            logger.AppendLine(String.Format("Found {0} student files with prefix \"{1}\"", studentFiles.Length, config.FilePrefix));

            IEnumerable<SftpFile> allRemoteFiles;
            try
            {
                logger.AppendLine(String.Format("Verifying that files with prefix \"{0}\" don't already exist in destination", testCode));
                allRemoteFiles = SshHelper.Client.ListDirectory(remoteDir);
            }
            catch (SftpPathNotFoundException e)
            {
                logger.AppendLine(String.Format("Invalid remote remotePath: {0}", remoteDir));

                Loader.Hide().Execute(context);
                args.Error = e;
                args.WasCancelled = true;
                Completed(this, args);

                return;
            }

            if (allRemoteFiles.Any(f => f.Name.StartsWith(testCode + " ")))
            {
                logger.AppendLine("WARNING:  Files with this test code prefix already exist!");

                Loader.Hide().Execute(context);
                args.WasCancelled = true;
                Completed(this, args);

                return;
            }


            if (allRemoteFiles.Any(d => d.IsDirectory && d.FullName == remoteStudentDir) == false)
            {
                logger.AppendLine(string.Format("Student subdirectory \"{0}\" doesn't exist.  Creating it...", config.StudentReportsSubdirectory));
                try
                {
                    SshHelper.Client.CreateDirectory(remoteStudentDir);
                }
                catch (Exception e)
                {
                    logger.AppendLine(String.Format("WARNING: Error creating directory: {0}", e.Message));
                    Loader.Hide().Execute(context);
                    args.Error = e;
                    args.WasCancelled = true;
                    Completed(this, args);

                    return;
                }
            }

            UploadFiles(files, remoteDir, context);
            UploadFiles(studentFiles, remoteStudentDir, context);
            WriteLogFile(remoteDir, context);

            logger.AppendLine("Upload was successful.");
            Completed(this, args);
        }

        private void UploadFiles(string[] files, string remoteDirectory, ActionExecutionContext context)
        {
            foreach (var file in files)
            {
                FileInfo f = new FileInfo(file);
                string remoteFilePath = UnixPath.Combine(remoteDirectory, f.Name.Replace(config.FilePrefix, testCode));
                logger.AppendLine(String.Format("Uploading file \"{0}\" as \"{2}\"", file, config.RemoteServer, remoteFilePath));

                try
                {
                    SshHelper.Client.UploadFile(new FileStream(f.FullName, FileMode.Open), remoteFilePath, false);
                }
                catch (Exception e)
                {
                    logger.AppendLine(String.Format("WARNING: An error occurred uploading file \"{0}\"", file));

                    Loader.Hide().Execute(context);
                    Completed(this, new ResultCompletionEventArgs { Error = e, WasCancelled = true });
                    return;
                }
            }
        }

        private void WriteLogFile(string remotePath, ActionExecutionContext context)
        {
            string logFile = String.Format("{0}_{1}.log", testCode, DateTime.Now.ToIsoDateTimeString(useSeparators:true));
            logFile = logFile.SanitizeFileName("_");
            string logFilePath = UnixPath.Combine(remotePath, logFile);
            logFilePath = logFilePath.SanitizePath("_");

            var files = SshHelper.Client.ListDirectory(remotePath);

            bool valid = false;
            int i = 0;
            while (!valid)
            {
                string test = logFile + (i == 0 ? "" : "_" + i);
                if (!files.Any(f => f.Name == test))
                {
                    valid = true;
                    logFile = test;
                    break;
                }
            }

            logger.AppendLine(String.Format("Writing log file: {0}", logFilePath));
            try
            {
                SshHelper.Client.WriteAllText(logFilePath, logger.Messages);
            }
            catch (Exception e)
            {
                logger.AppendLine("WARNING: An error occurred while uploading the log file \"{0}\"", logFilePath);

                Loader.Hide().Execute(context);
                Completed(this, new ResultCompletionEventArgs { Error = e, WasCancelled = true });
                return;
            }

        }

        #endregion
    }
}
