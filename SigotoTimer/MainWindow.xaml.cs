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
            startParallelThread();
        }

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
                    Thread.Sleep(1000);
                }
            });
        }

        private void Client_onDeactivated()
        {
            runOnUIThread(delegate {
                this.Title = "Dectivated";
                Console.WriteLine(this.Title);
            });
        }

        private void Client_onActivated()
        {
            runOnUIThread (delegate {
                this.Title = "Activated";
                Console.WriteLine(this.Title);
            });
        }

        private string[] readTargetProcessNames()
        {
            var lines = File.ReadAllLines("./proc.txt");
            return lines;
        }

        private void runOnUIThread(Action func)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, func);
        }

    }
}
