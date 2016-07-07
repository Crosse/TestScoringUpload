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
        private string _facultyusername;
        private string _testcode;
        private bool _isvalidating;
        private bool _isvalid;
        private ConfigModel config = ConfigModel.Instance;

        #region Properties
        public bool IsValidating
        {
            get { return _isvalidating; }
            set
            {
                _isvalidating = value;
                NotifyOfPropertyChange(() => IsValidating);
            }
        }

        public string FacultyUsername
        {
            get { return _facultyusername; }
            set
            {
                _facultyusername = value;
                NotifyOfPropertyChange(() => FacultyUsername);
                NotifyOfPropertyChange(() => CanTransfer);
            }
        }

        public string TestCode
        {
            get { return _testcode; }
            set
            {
                _testcode = value;
                NotifyOfPropertyChange(() => TestCode);
                NotifyOfPropertyChange(() => CanTransfer);
            }
        }
        #endregion Properties

        public bool CanTransfer
        {
            get
            {
                return (
                    !String.IsNullOrEmpty(_facultyusername) &&
                    !String.IsNullOrEmpty(_testcode) &&
                    config.IsValid());
            }
        }

        public IEnumerable<IResult> Transfer()
        {
            logger.Clear();

            yield return Loader.Show("Validating...");

            if (Validate())
            {
                yield return Loader.Show("Connecting...");
                //yield return new SftpConnector(config.RemoteServer, config.RemoteServerUser, config.RemoteServerPassword);
                yield return new FtpConnector(config.RemoteServer, config.RemoteServerUser, config.RemoteServerPassword);

                yield return Loader.Show("Uploading files...");
                yield return new ReportUploader(FacultyUsername, TestCode);

                yield return Loader.Show("Disconnecting...");
                //yield return new SftpDisconnector();
                yield return new FtpDisconnector();
            }

            yield return Loader.Hide();
            logger.AppendLine("Finished.");
        }

        public bool Validate()
        {
            _isvalid = true;

            IsValidating = true;
            NotifyOfPropertyChange(() => FacultyUsername);
            NotifyOfPropertyChange(() => TestCode);
            IsValidating = false;

            return _isvalid;
        }

        public bool ValidateFacultyUsername()
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            try
            {
                UserPrincipal princ = UserPrincipal.FindByIdentity(ctx, _facultyusername);
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
                    _isvalid = valid;

                return message;
            }
        }

        #endregion
    }
}
