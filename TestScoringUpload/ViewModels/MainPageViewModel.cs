using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.DirectoryServices.AccountManagement;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    [Export(typeof(MainPageViewModel))]
    class MainPageViewModel : Screen, IDataErrorInfo
    {
        private Logger logger = Logger.GetLogger();
        private string facultyUsername;
        private string testCode;
        private bool isValidating;
        private bool isValid;
        private ConfigModel config = ConfigModel.Instance;

        #region Properties
        public bool IsValidating
        {
            get { return isValidating; }
            set
            {
                isValidating = value;
                NotifyOfPropertyChange(() => IsValidating);
            }
        }

        public string FacultyUsername
        {
            get { return facultyUsername; }
            set
            {
                facultyUsername = value;
                NotifyOfPropertyChange(() => FacultyUsername);
                NotifyOfPropertyChange(() => CanTransfer);
            }
        }

        public string TestCode
        {
            get { return testCode; }
            set
            {
                testCode = value;
                NotifyOfPropertyChange(() => TestCode);
                NotifyOfPropertyChange(() => CanTransfer);
            }
        }

        public bool CanTransfer
        {
            get
            {
                return (
                    !String.IsNullOrEmpty(facultyUsername) &&
                    !String.IsNullOrEmpty(testCode) &&
                    config.IsValid());
            }
        }
        #endregion Properties

        public IEnumerable<IResult> Transfer()
        {
            logger.Clear();

            yield return Loader.Show("Validating...");

            if (Validate())
            {
                yield return Loader.Show("Connecting...");

                ITransferHelper helper = TransferHelperFactory.GetHelper(config.TransferProtocol);
                yield return helper.GetConnector(config.RemoteServer, config.RemoteServerUser, config.RemoteServerPassword);

                yield return Loader.Show("Uploading files...");
                yield return new ReportUploader(FacultyUsername, TestCode);

                yield return Loader.Show("Disconnecting...");

                yield return helper.GetDisconnector();
            }

            yield return Loader.Hide();
            logger.AppendLine("Finished.");
        }

        public bool Validate()
        {
            isValid = true;

            IsValidating = true;
            NotifyOfPropertyChange(() => FacultyUsername);
            NotifyOfPropertyChange(() => TestCode);
            IsValidating = false;

            return isValid;
        }

        public bool ValidateFacultyUsername()
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            try
            {
                UserPrincipal princ = UserPrincipal.FindByIdentity(ctx, facultyUsername);
                return !(princ == null);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region IDataErrorInfo Members
        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                if (!IsValidating)
                    return null;

                bool valid = true;
                string message = null;

                switch (columnName)
                {
                    case "FacultyUsername":
                        if (!ValidateFacultyUsername())
                        {
                            message = String.Format("Invalid Faculty e-ID \"{0}\".", FacultyUsername);
                            logger.AppendLine(message);
                            valid = false;
                        }
                        break;
                    case "TestCode":
                        if (TestCode.Contains(" "))
                        {
                            message = "File Name (Test Code) must not contain spaces.";
                            logger.AppendLine(message);
                            valid = false;
                        }
                        break;
                    default:
                        break;
                }

                if (!valid)
                    isValid = valid;

                return message;
            }
        }
        #endregion
    }
}
