using System;
using System.Configuration;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Caliburn.Micro;
using Crosse.ExtensionMethods;

namespace JMU.TestScoring
{
    public class ConfigModel : PropertyChangedBase
    {
        static readonly byte[] salt = System.Text.Encoding.Unicode.GetBytes("Salt is Healthy");

        private string defaultSourcePath;
        private string filePrefix;
        private string remoteServer;
        private string remoteServerUser;
        private SecureString remoteServerPassword;
        private string remoteServerBaseDirectory;
        private string studentReportsSubdirectory;

        private Configuration config;
        private AppSettingsSection appSettings;
        public static ConfigModel Instance = new ConfigModel();

        private ConfigModel()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            appSettings = config.AppSettings;
        }

        private string SetAppSetting(string key, string value)
        {
            if (appSettings.Settings[key] == null)
                appSettings.Settings.Add(key, value);
            else
                appSettings.Settings[key].Value = value;

            config.Save();

            return value;
        }

        private string GetAppSetting(string key)
        {
            return appSettings.Settings[key] == null ? String.Empty : appSettings.Settings[key].Value;
        }

        public string DefaultSourcePath
        {
            get
            {
                if (String.IsNullOrEmpty(defaultSourcePath))
                    DefaultSourcePath = GetAppSetting("DefaultSourcePath");

                return defaultSourcePath;
            }
            set
            {
                defaultSourcePath = SetAppSetting("DefaultSourcePath", value);
                NotifyOfPropertyChange(() => DefaultSourcePath);
            }
        }

        public string FilePrefix
        {
            get
            {
                if (String.IsNullOrEmpty(filePrefix))
                    FilePrefix = GetAppSetting("FilePrefix");

                return filePrefix;
            }
            set
            {
                filePrefix = SetAppSetting("FilePrefix", value);
                NotifyOfPropertyChange(() => FilePrefix);
            }
        }

        public string RemoteServer
        {
            get
            {
                if (String.IsNullOrEmpty(remoteServer))
                    RemoteServer = GetAppSetting("RemoteServer");

                return remoteServer;
            }
            set
            {
                remoteServer = value = SetAppSetting("RemoteServer", value);
                NotifyOfPropertyChange(() => RemoteServer);
            }
        }

        public string RemoteServerUser
        {
            get
            {
                if (String.IsNullOrEmpty(remoteServerUser))
                    RemoteServerUser = GetAppSetting("RemoteServerUser");

                return remoteServerUser;
            }
            set
            {
                remoteServerUser = SetAppSetting("RemoteServerUser", value);
                NotifyOfPropertyChange(() => RemoteServerUser);
            }
        }

        public SecureString RemoteServerPassword
        {
            get
            {
                if (remoteServerPassword == null)
                {
                    remoteServerPassword = new SecureString();

                    string encrypted = GetAppSetting("RemoteServerPassword");

                    if (String.IsNullOrEmpty(encrypted))
                        return null;

                    byte[] decrypted = ProtectedData.Unprotect(
                        Convert.FromBase64String(encrypted),
                        salt,
                        DataProtectionScope.CurrentUser);

                    char[] d = Encoding.Unicode.GetChars(decrypted);
                    decrypted.Apply(b => b = 0);

                    for (int i = 0; i < d.Length; i++)
                    {
                        remoteServerPassword.AppendChar(d[i]);
                        d[i] = '\0';
                    }
                    remoteServerPassword.MakeReadOnly();
                }
                return remoteServerPassword;
            }

            set
            {
                remoteServerPassword = value;
                byte[] encrypted = ProtectedData.Protect(
                    Encoding.Unicode.GetBytes(remoteServerPassword.ConvertToUnsecureString()),
                    salt,
                    DataProtectionScope.CurrentUser);
                SetAppSetting("RemoteServerPassword", Convert.ToBase64String(encrypted));

                NotifyOfPropertyChange(() => RemoteServerPassword);
            }
        }

        public string RemoteServerBaseDirectory
        {
            get
            {
                if (String.IsNullOrEmpty(remoteServerBaseDirectory))
                    RemoteServerBaseDirectory = GetAppSetting("RemoteServerBaseDirectory");

                return remoteServerBaseDirectory;
            }
            set
            {
                remoteServerBaseDirectory = SetAppSetting("RemoteServerBaseDirectory", value);
                NotifyOfPropertyChange(() => RemoteServerBaseDirectory);
            }
        }

        public string StudentReportsSubdirectory
        {
            get
            {
                if (String.IsNullOrEmpty(studentReportsSubdirectory))
                    StudentReportsSubdirectory = GetAppSetting("StudentReportsSubdirectory");

                return studentReportsSubdirectory;
            }
            set
            {
                studentReportsSubdirectory = SetAppSetting("StudentReportsSubdirectory", value);
                NotifyOfPropertyChange(() => StudentReportsSubdirectory);
            }
        }

        public bool IsValid()
        {
            return (!String.IsNullOrEmpty(DefaultSourcePath) &&
                    !String.IsNullOrEmpty(FilePrefix) &&
                    !String.IsNullOrEmpty(RemoteServer) &&
                    !String.IsNullOrEmpty(RemoteServerUser) &&
                    RemoteServerPassword != null &&
                    !String.IsNullOrEmpty(RemoteServerBaseDirectory) &&
                    !String.IsNullOrEmpty(StudentReportsSubdirectory));
        }
    }
}
