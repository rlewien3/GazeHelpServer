using Fleck;
using System;
using Tobii.Interaction;
using Tobii.Interaction.Client;
using System.Windows;
using Server;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GazeHelpServer
{
    /**
     * The main window of the GazeHelpServer GUI. Handles the interaction logic, shows messages, and starts the server.
     */
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ((App)Application.Current).IntlibWpf?.WpfBinding?.AddWindow(this);

            InitializeComponent();
            this.DataContext = this;

            toggleServer(null, null);
            portInput.Text = port.ToString();
        }

        bool isRunning = false;
        int port = 8898;
        string address = "127.0.0.1";
        SocketServer server;

        private void toggleServer(object sender, RoutedEventArgs e)
        {

            if (!isRunning)
            {
                // Start the server
                try
                {
                    switch (Host.EyeXAvailability)
                    {
                        case EyeXAvailability.NotAvailable:
                            showError("Please install the EyeX Engine and try again.");
                            return;

                        case EyeXAvailability.NotRunning:
                            showError("Please make sure that the EyeX Engine is started.");
                            return;
                    }

                    startServer();
                }
                catch (Exception ex)
                {
                    showError("Failed to start server on port " + port.ToString() + ": " + ex.Message);
                }
            }
            else 
            {
                // Stop the server
                isRunning = false;
                showError("Server is not running!");
                server.stop();                
            }
        }

        /**
         * Updates the server's port, when changed in the advanced panel
         */
        private void updatePort(object sender, RoutedEventArgs e) {

            if (Int32.TryParse(portInput.Text, out int tempPort) && tempPort > 0 && tempPort < 9999)
            {
                port = tempPort;

                if (isRunning) {
                    server.stop();
                    startServer();
                }
                
                // Update the toast to show success
                advToastText.Text = "Successfully changed the port to: " + port.ToString();
                Application.Current.Resources["advToastBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#405848"));
                Application.Current.Resources["advToastBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B5343"));
            }
            else
            {
                // Update the toast to show failure
                advToastText.Text = "Port number not valid! Try again.";
                Application.Current.Resources["advToastBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7B4139"));
                Application.Current.Resources["advToastBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#70362E"));
            }
        }

        /**
         * Starts the server
         */
        private void startServer() 
        {
            showNeutral("Server is running.");
            server = new SocketServer(port, address, this);
            server.start();
            isRunning = true;
        }

        /**
         * Shows an error message on the main tab of the GUI
         */
        public void showError(string msg) 
        {
            this.Dispatcher.Invoke(() => 
            {
                Application.Current.Resources["msgBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7B4139"));
                Application.Current.Resources["msgBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#70362E"));
                msgText.Text = msg;
                serverButton.Content = "Start Server";
                msgImg.Source = new BitmapImage(new Uri(@"/img/warning-icon.png", UriKind.Relative));
            });
        }

        /**
         * Shows a neutral message on the main tab of the GUI
         */
        public void showNeutral(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                Application.Current.Resources["msgBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#505050"));
                Application.Current.Resources["msgBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4D4D4D"));
                msgText.Text = msg;
                serverButton.Content = "Stop Server";
                msgImg.Source = new BitmapImage(new Uri(@"/img/tick-icon.png", UriKind.Relative));
            });
        }

        /**
         * Shows a success message on the main tabe of the GUI
         */
        public void showSuccess(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                Application.Current.Resources["msgBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#405848"));
                Application.Current.Resources["msgBorder"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B5343"));
                msgText.Text = msg;
                serverButton.Content = "Stop Server";
                msgImg.Source = new BitmapImage(new Uri(@"/img/tick-icon.png", UriKind.Relative));
            });
        }
    }
}
