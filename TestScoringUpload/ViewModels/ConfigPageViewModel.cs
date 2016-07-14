using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Crosse.ExtensionMethods;

namespace JMU.TestScoring
{
    [Export(typeof(ConfigPageViewModel))]
    public class ConfigPageViewModel : Screen, IDataErrorInfo
    {
        Logger logger = Logger.GetLogger();

        private bool formChanged = false;
        private bool isValidating;
        private bool isValid;
        private string defaultSourcePath;
        private string filePrefix;
        private string remoteServer;
        private string remoteServerUser;
        private string remoteServerPassword;
        private string remoteServerBaseDirectory;
        private string studentReportsSubdirectory;
        private string transferProtocol;

        private ConfigModel config = ConfigModel.Instance;

        #region Properties
        public bool IsValidating
        {
            get { return isValidating; }
            set
            {
                isValidating = value;
                formChanged = true;
                NotifyOfPropertyChange(() => IsValidating);
            }
        }
        public string DefaultSourcePath
        {
            get { return defaultSourcePath; }
            set
            {
                defaultSourcePath = value;
                formChanged = true;
                NotifyOfPropertyChange(() => DefaultSourcePath);
                NotifyOfPropertyChange(() => StudentReportsSubdirectory);
                NotifyOfPropertyChange(() => CanSaveAndClose);
            }
        }
        public string FilePrefix
        {
            get { return filePrefix; }
            set
            {
                filePrefix = value;
                formChanged = true;
                NotifyOfPropertyChange(() => FilePrefix);
                NotifyOfPropertyChange(() => CanSaveAndClose);
            }
        }
        public string RemoteServer
        {
            get { return remoteServer; }
            set
            {
                remoteServer = value;
                formChanged = true;
                NotifyOfPropertyChange(() => RemoteServer);
                NotifyOfPropertyChange(() => CanSaveAndClose);
            }
        }
        public string RemoteServerUser
        {
            get { return remoteServerUser; }
            set
            {
                remoteServerUser = value;
                formChanged = true;
                NotifyOfPropertyChange(() => RemoteServerUser);
                NotifyOfPropertyChange(() => CanSaveAndClose);
            }
        }
        public string RemoteServerPassword
        {
            get { return remoteServerPassword; }
            set
            {
                remoteServerPassword = value;
                formChanged = true;
                NotifyOfPropertyChange(() => RemoteServerPassword);
                NotifyOfPropertyChange(() => CanSaveAndClose);
            }
        }
        public string RemoteServerBaseDirectory
        {
            get { return remoteServerBaseDirectory; }
            set
            {
                remoteServerBaseDirectory = value;
                formChanged = true;
                NotifyOfPropertyChange(() => RemoteServerBaseDirectory);
                NotifyOfPropertyChange(() => CanSaveAndClose);
            }
        }
        public string StudentReportsSubdirectory
        {
            get { return studentReportsSubdirectory; }
            set
            {
                studentReportsSubdirectory = value;
                formChanged = true;
                NotifyOfPropertyChange(() => StudentReportsSubdirectory);
                NotifyOfPropertyChange(() => CanSaveAndClose);
            }
        }
        public string TransferProtocol
        {
            get { return transferProtocol; }
            set
            {
                transferProtocol = value;
                formChanged = true;
                NotifyOfPropertyChange(() => TransferProtocol_SFTP);
                NotifyOfPropertyChange(() => TransferProtocol_FTP);
                NotifyOfPropertyChange(() => CanSaveAndClose);
            }
        }
        public bool TransferProtocol_FTP
        {
            get { return TransferProtocol == "FTP"; }
            set { TransferProtocol = "FTP"; }
        }
        public bool TransferProtocol_SFTP
        {
            get { return TransferProtocol == "SFTP"; }
            set { TransferProtocol = "SFTP"; }
        }
        #endregion Properties

        public ConfigPageViewModel()
        {
            GetSettings();
            Validate();
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
        }
        
        public void SaveAndClose()
        {
            if (Validate())
            {
                SaveSettings();
                TryClose();
            }
        }

        public bool CanSaveAndClose
        {
            get
            {
                return formChanged && Validate();
            }
        }

        public void Cancel()
        {
            GetSettings();
            TryClose();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        public override void CanClose(Action<bool> callback)
        {
            if (CanSaveAndClose)
            {
                SaveSettings();
            }

            callback(true);
        }

        private void GetSettings()
        {
            DefaultSourcePath = config.DefaultSourcePath;
            FilePrefix = config.FilePrefix;
            RemoteServer = config.RemoteServer;
            RemoteServerUser = config.RemoteServerUser;
            RemoteServerPassword = config.RemoteServerPassword.ConvertToUnsecureString();
            RemoteServerBaseDirectory = config.RemoteServerBaseDirectory;
            StudentReportsSubdirectory = config.StudentReportsSubdirectory;
            TransferProtocol = config.TransferProtocol;
            formChanged = false;
        }

        private void SaveSettings()
        {
            config.DefaultSourcePath = defaultSourcePath;
            config.FilePrefix = filePrefix;
            config.RemoteServer = remoteServer;
            config.RemoteServerUser = remoteServerUser;
            config.RemoteServerPassword = remoteServerPassword.ConvertToSecureString();
            config.RemoteServerBaseDirectory = remoteServerBaseDirectory;
            config.StudentReportsSubdirectory = studentReportsSubdirectory;
            config.TransferProtocol = transferProtocol;
            formChanged = false;
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
                return ValidateProperty(columnName);
            }
        }
        #endregion

        private string ValidateProperty(string propertyName)
        {
            bool valid = true;
            string message = null;

            switch (propertyName)
            {
                case "DefaultSourcePath":
                    if (System.IO.Directory.Exists(DefaultSourcePath) == false)
                    {
                        message = String.Format("Path \"{0}\" does not exist or is invalid.", DefaultSourcePath);
                        valid = false;
                    }
                    break;
                case "FilePrefix":
                    if (String.IsNullOrEmpty(FilePrefix))
                        message = "A blank File Prefix is not allowed.";
                    else if (FilePrefix.EndsWith(" "))
                        message = "File Prefix cannot end with a space.";
                    else if (FilePrefix.StartsWith(" "))
                        message = "File Prefix cannot start with a space.";

                    if (!String.IsNullOrEmpty(message))
                        valid = false;
                    break;
                case "RemoteServerPassword":
                    if (RemoteServerPassword == null || RemoteServerPassword.Length == 0)
                    {
                        message = "Password cannot be blank.";
                        valid = false;
                    }
                    break;
                case "StudentReportsSubdirectory":
                    try
                    {
                        string fullstudentpath = System.IO.Path.Combine(DefaultSourcePath, StudentReportsSubdirectory);
                        if (System.IO.Directory.Exists(fullstudentpath) == false)
                        {
                            message = string.Format("Student Report Path ({0}) does not exist or is invalid.", fullstudentpath);
                            valid = false;
                        }
                    }
                    catch (Exception e)
                    {
                        message = e.Message;
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

        private bool Validate()
        {
            isValid = true;

            NotifyOfPropertyChange(() => DefaultSourcePath);
            ValidateProperty("DefaultSourcePath");

            NotifyOfPropertyChange(() => FilePrefix);
            ValidateProperty("FilePrefix");

            NotifyOfPropertyChange(() => RemoteServerPassword);
            ValidateProperty("RemoteServerPassword");

            NotifyOfPropertyChange(() => StudentReportsSubdirectory);
            ValidateProperty("StudentReportsSubdirectory");

            return isValid;
        }
    }
}
