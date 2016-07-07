using System.Windows.Controls;

namespace JMU.TestScoring
{
    /// <summary>
    /// Interaction logic for MainPageView.xaml
    /// </summary>
    public partial class MainPageView : UserControl
    {
        public MainPageView()
        {
            InitializeComponent();
            this.Height = 350;
            this.Width = 550;
        }

        private void Messages_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }
    }
}
