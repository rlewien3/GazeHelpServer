
using System;
using System.Windows;

namespace GazeHelpServer
{
    /**
     * Interaction logic for App.xaml
     */
    public partial class App : Application
    {
        public Tobii.InteractionLib.Wpf.InteractionLibWpfHost IntlibWpf { get; private set; }
        protected override void OnStartup(StartupEventArgs e) => IntlibWpf = new Tobii.InteractionLib.Wpf.InteractionLibWpfHost();

        protected override void OnExit(ExitEventArgs e)
        {
            // Cleans up the Interaction Library WPF host instance on exit
            IntlibWpf?.Dispose();

            base.OnExit(e);
        }
    }
}
