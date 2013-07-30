using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using Caliburn.Micro;
using Crosse.ExtensionMethods;

namespace JMU.TestScoring
{
    public class ReportUploader : IResult
    {
        private Logger logger = Logger.GetLogger();
        readonly string studentReportPath;
        readonly string username;
        readonly string testCode;

        ConfigModel config = ConfigModel.Instance;
        ITransferHelper helper;

        public ReportUploader(string username, string testCode)
        {
            this.studentReportPath = Path.Combine(config.DefaultSourcePath, config.StudentReportsSubdirectory);
            this.username = username;
            this.testCode = testCode;

            this.helper = new SshHelper();
            this.helper = new FtpHelper();
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

            string[] allRemoteFiles;
            try
            {
                logger.AppendLine(String.Format("Verifying that files with prefix \"{0}\" don't already exist in destination", testCode));
                allRemoteFiles = helper.ListDirectory(remoteDir);
            }
            catch (DirectoryNotFoundException e)
            {
                logger.AppendLine(String.Format("Invalid remote path: {0}", remoteDir));

                Loader.Hide().Execute(context);
                args.Error = e;
                args.WasCancelled = true;
                Completed(this, args);

                return;
            }

            if (allRemoteFiles.Any(f => UnixPath.GetFileName(f).StartsWith(testCode + " ")))
            {
                logger.AppendLine("WARNING:  Files with this test code prefix already exist!");

                bool result = Loader.Query("Files with this test code already exist.  Overwrite?");

                if (result)
                    logger.AppendWarning("Overwriting existing files per operator request.");
                else
                {
                    logger.AppendLine("Existing files will not be overwritten.  Cancelling operation.");
                    Loader.Hide().Execute(context);
                    args.WasCancelled = true;
                    Completed(this, args);
                    return;
                }
            }


            if (allRemoteFiles.Any(d => d == remoteStudentDir) == false)
            {
                logger.AppendLine(string.Format("Student subdirectory \"{0}\" doesn't exist.  Creating it...", config.StudentReportsSubdirectory));
                
                if (helper.CreateDirectory(remoteStudentDir) == false)
                {
                    Loader.Hide().Execute(context);
                    args.WasCancelled = true;
                    Completed(this, args);
                    return;
                }
            }

            UploadFiles(files, remoteDir, context);
            UploadFiles(studentFiles, remoteStudentDir, context);

            //TODO: Don't hardcode the Logs path.
            string logDir = UnixPath.Combine(config.RemoteServerBaseDirectory, "UploadLogs");
            WriteLogFile(logDir, context);

            logger.AppendLine("Upload was successful.");

            bool delete = Loader.Query("Delete source files?");
            if (delete)
            {
                logger.AppendLine("Deleting local files...");
                foreach (var file in (files.Concat(studentFiles)))
                {
                    try
                    {
                        File.Delete(file);
                        logger.AppendLine("Deleted {0}", file);
                    }
                    catch (Exception e)
                    {
                        logger.AppendError("Unable to delete \"{0}\": {1}", Path.GetFileName(file), e.Message);
                    }
                }
            }

            Completed(this, args);
        }

        private void UploadFiles(string[] files, string remoteDirectory, ActionExecutionContext context)
        {
            foreach (var file in files)
            {
                FileInfo f = new FileInfo(file);
                string remoteFilePath = UnixPath.Combine(remoteDirectory, Regex.Replace(f.Name, "^" + config.FilePrefix, testCode, RegexOptions.IgnoreCase));

                if (!helper.UploadFile(file, remoteFilePath))
                    return;
            }
        }

        private void WriteLogFile(string remotePath, ActionExecutionContext context)
        {
            string logFile = String.Format("{0}_{1}_{2}.log", DateTime.Now.ToIsoDateTimeString(useSeparators:true), testCode, username);
            logFile = logFile.SanitizeFileName("_");
            string logFilePath = UnixPath.Combine(remotePath, logFile);
            logFilePath = logFilePath.SanitizePath("_");

            var files = helper.ListDirectory(remotePath);

            bool valid = false;
            int i = 0;
            while (!valid)
            {
                string test = logFile + (i == 0 ? "" : "_" + i);
                if (!files.Any(f => f.EndsWith(test)))
                {
                    valid = true;
                    logFile = test;
                    break;
                }
            }

            logger.AppendLine(String.Format("Writing log file: {0}", logFilePath));
            try
            {
                helper.WriteTextToFile(logFilePath, logger.Messages);
            }
            catch (Exception e)
            {
                logger.AppendLine("WARNING: An error occurred while uploading the log file \"{0}\": {1}", logFilePath, e.Message);

                Loader.Hide().Execute(context);
                Completed(this, new ResultCompletionEventArgs { Error = e, WasCancelled = true });
                return;
            }
        }

        #endregion
    }
}
