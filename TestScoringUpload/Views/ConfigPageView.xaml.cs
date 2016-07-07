using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace JMU.TestScoring
{
    /// <summary>
    /// Interaction logic for ConfigPageView.xaml
    /// </summary>
    [Export(typeof(ConfigPageView))]
    public partial class ConfigPageView : UserControl
    {
        public ConfigPageView()
        {
            InitializeComponent();
        }
    }
}
