using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
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
        private SolidColorBrush commonBackgroundBrush;
        private SolidColorBrush accentBackgroundBrush;
        private SolidColorBrush commonTextBrush;
        private SolidColorBrush accentTextBrush;

        private bool _accentTick = false;

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

                commonBackgroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom($"#{ini["Window"]["Background"].ToString()}");
                commonBackgroundBrush.Opacity = ini["Window"]["BackgroundOpacity"].ToDouble();
                accentBackgroundBrush = (SolidColorBrush)new BrushConverter().ConvertFrom($"#{ini["Window"]["BackgroundAccent"].ToString()}");
                accentBackgroundBrush.Opacity = ini["Window"]["BackgroundOpacity"].ToDouble();
                this.Background = commonBackgroundBrush;

                commonTextBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom($"#{ini["Window"]["TextColor"].ToString()}"));
                accentTextBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom($"#{ini["Window"]["TextColorAccent"].ToString()}"));
                this.StateDesc.Foreground = commonTextBrush;
                this.StateTImer.Foreground = commonTextBrush;

                notificationDuration = ini["Schadenfreude"]["NotiDuration"].ToInt();
                notiSound = new SoundPlayer(ini["Schadenfreude"]["NotiSound"].ToString());
            }
            catch
            {

            }
        }

        #endregion

        #region Schadenfreude

        private string[] readTargetProcessNames()
        {
            var lines = File.ReadAllLines("./proc.txt");
            return lines;
        }

        long tick = 0;
        long notificationDuration = 600;
        private SoundPlayer notiSound;
        bool isNotificationSending = false;

        private void startParallelThread()
        {
            Task.Factory.StartNew(delegate
            {
                var procNames = readTargetProcessNames();
                var client = new Schadenfreude(procNames);
                client.onActivated += Client_onActivated;
                client.onDeactivated += Client_onDeactivated;
                client.onWatch += Client_onWatch;
                while (true)
                {
                    client.Watch();
                    Thread.Sleep(1000);
                }
            });
        }

        private void Client_onActivated()
        {
            Console.WriteLine("Activated");
            tick = 1;

            runOnUIThread(delegate {
                StateDesc.Content = "일에 집중하는 중";
            });
        }

        private void Client_onDeactivated()
        {
            Console.WriteLine("Deactivated");
            tick = 1;

            runOnUIThread(delegate {
                StateDesc.Content = "머리 식히는 중";
            });
        }

        private void Client_onWatch(bool isActivated)
        {
            runOnUIThread(delegate
            {
                StateTImer.Content = $"{tick++}초 째";

                if(isActivated && isNotificationSending)
                {
                    StopDoWorkNotification();
                }
                else if (!isActivated && !isNotificationSending && tick > notificationDuration )
                {
                    DoWorkNotification();
                }
            });
        }

        Thread bgBlinkThread;

        private void DoWorkNotification()
        {
            isNotificationSending = true;
            BlinkWindow.FlashWindow(this);
            bgBlinkThread = new Thread(new ThreadStart(delegate
            {
                while (true)
                {
                    runOnUIThread(delegate
                    {
                        this.Background = _accentTick? accentBackgroundBrush : commonBackgroundBrush;
                        this.StateTImer.Foreground = _accentTick? commonTextBrush : accentTextBrush;
                        _accentTick = !_accentTick;
                    });
                    Thread.Sleep(300);
                }
            }));
            bgBlinkThread.Start();
            try { notiSound.PlaySync(); }
            catch { }
        }

        private void StopDoWorkNotification()
        {
            isNotificationSending = false;
            BlinkWindow.StopFlashingWindow(this);
            bgBlinkThread.Abort();
            Background = commonBackgroundBrush;
            StateTImer.Foreground = commonTextBrush;
            try { notiSound.Stop(); }
            catch { }
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
