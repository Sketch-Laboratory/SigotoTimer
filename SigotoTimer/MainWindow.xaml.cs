using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            InitializeConsole();
            startParallelThread();
        }

        private void InitializeConsole()
        {
            #if DEBUG
            ConsoleManager.Init();
            ConsoleManager.Show();
            #endif
        }

        private void startParallelThread()
        {
            Task.Factory.StartNew(delegate
            {
                var client = new Schadenfreude();
                var procName = "devenv";
                while (true)
                {
                    bool activated = false;
                    var procs = Process.GetProcessesByName(procName);
                    foreach (var proc in procs)
                    {
                        if(client.ApplicationIsActivated(proc.Id))
                        {
                            activated = true;
                            break;
                        }
                    }

                    if(activated)
                    {
                        Console.WriteLine("Activated.");
                    }
                    else
                    {
                        Console.WriteLine("De-Activated.");
                    }

                    Thread.Sleep(1000);
                }
            });
        }
    }
}
