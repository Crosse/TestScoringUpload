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
