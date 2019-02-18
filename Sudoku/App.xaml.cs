using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Sudoku
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Following code from...
        // https://code.msdn.microsoft.com/windowsdesktop/Create-a-splash-screen-for-3b994ee2

        private const int MINIMUM_SPLASH_TIME = 1500; // Milliseconds 
        private const int SPLASH_FADE_TIME = 600;     // Milliseconds 

        protected override void OnStartup(StartupEventArgs e)
        {
            // Load the splash screen 
            SplashScreen splash = new SplashScreen("Resources/Splash.bmp");
            splash.Show(false, true);

            // Start a stop watch 
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            // Load the MainWindow
            base.OnStartup(e);
            MainWindow main = new MainWindow();

            // Ensure the SplashScreen lasts as long as th specified time 
            timer.Stop();
            int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
            if (remainingTimeToShowSplash > 0)
            {
                System.Threading.Thread.Sleep(remainingTimeToShowSplash);
            }

            // Show the MainWindow 
            splash.Close(TimeSpan.FromMilliseconds(SPLASH_FADE_TIME));
            main.Show();
        }
    }
}
