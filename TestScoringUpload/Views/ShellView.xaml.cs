using System;
using System.Windows;
using System.Windows.Controls;

namespace JMU.TestScoring
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class ShellView : UserControl
    {
        public ShellView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ((Window)Parent).SizeToContent = SizeToContent.Manual;
        }
    }
}
