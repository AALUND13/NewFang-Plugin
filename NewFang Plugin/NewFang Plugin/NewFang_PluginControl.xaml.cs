using System.Windows;
using System.Windows.Controls;

namespace NewFang_Plugin
{
    public partial class NewFang_PluginControl : UserControl
    {

        private NewFang_Plugin Plugin { get; }

        private NewFang_PluginControl()
        {
            InitializeComponent();
        }

        public NewFang_PluginControl(NewFang_Plugin plugin) : this()
        {
            Plugin = plugin;
            DataContext = plugin.Config;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin.Save();
        }
    }
}
