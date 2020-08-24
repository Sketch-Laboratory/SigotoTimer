using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SigotoTimer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            loadSettings();
            startParallelThread();
            initializeNoBorderWindow();
        }

        private void runOnUIThread(Action func)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, func);
        }

        #region Load Settings

        private void loadSettings()
        {
            try
            {
                IniFile ini = new IniFile();
                ini.Load("./config.ini");
                this.Topmost = ini["Window"]["TopMost"].ToBool();
                notificationDuration = ini["Schadenfreude"]["PleaseDoWorkNotiDuration"].ToInt();
            }
            catch
            {

            }
        }

        #endregion

        #region Schadenfreude

        private void Client_onDeactivated()
        {
            Console.WriteLine("Deactivated");
            tick = 1;
            isActivated = false;

            runOnUIThread(delegate {
                StateDesc.Content = "머리 식히는 중";
            });
        }

        private void Client_onActivated()
        {
            Console.WriteLine("Activated");
            tick = 1;
            isActivated = true;

            runOnUIThread (delegate {
                StateDesc.Content = "일에 집중하는 중";
            });
        }

        private string[] readTargetProcessNames()
        {
            var lines = File.ReadAllLines("./proc.txt");
            return lines;
        }

        bool isActivated = false;
        long tick = 0;
        long notificationDuration = 0;
        private void startParallelThread()
        {
            Task.Factory.StartNew(delegate
            {
                var procNames = readTargetProcessNames();
                var client = new Schadenfreude(procNames);
                client.onActivated += Client_onActivated;
                client.onDeactivated += Client_onDeactivated;
                while (true)
                {
                    client.Watch();

                    runOnUIThread(delegate
                    {
                        StateTImer.Content = $"{tick++}초 째";
                    });


                    Thread.Sleep(1000);
                }
            });
        }

        #endregion

        #region No-Border Window

        private void initializeNoBorderWindow()
        {
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.MouseDown += MainWindow_MouseDown;
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #endregion
    }
}
