using System.ComponentModel.Composition;
using Caliburn.Micro;

namespace JMU.TestScoring
{
    [Export(typeof(ShellViewModel))]
    class ShellViewModel : Conductor<object>.Collection.OneActive
    {
        private Logger logger = Logger.GetLogger();
        private MainPageViewModel mainPage;
        private ConfigPageViewModel configPage;

        [ImportingConstructor]
        public ShellViewModel(MainPageViewModel mainPage, ConfigPageViewModel configPage)
        {
            this.mainPage = mainPage;
            this.configPage = configPage;

            base.DisplayName = "Test Score Uploader";
        }

        protected override void OnActivate()
        {
            ShowMainPage();

            if (ConfigModel.Instance.RemoteServerPassword.Length == 0)
            {
                ShowConfigPage();
                Loader.Notify("Some settings could not be loaded from the configuration file.\nPlease verify your configuration settings.");
            }

            base.OnActivate();
        }

        public void ShowMainPage()
        {
            ActivateItem(mainPage);
        }

        public void ShowConfigPage()
        {
            ActivateItem(configPage);
        }
    }
}
