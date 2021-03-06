using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ModuleLauncher.Example.ViewModels;
using ModuleLauncher.Example.ViewModels.Authenticators;

namespace ModuleLauncher.Example.Views
{
    public partial class AuthenticatorsView : UserControl
    {
        public AuthenticatorsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
