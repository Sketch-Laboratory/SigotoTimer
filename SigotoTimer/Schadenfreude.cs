using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SigotoTimer
{
    class Schadenfreude
    {
        private string[] procNames;
        public event Action onActivated, onDeactivated;
        public event Action<bool> onWatch;

        public Schadenfreude(string[] procNames)
        {
            this.procNames = procNames;
        }

        private bool? lastActivatedState = null;
        public bool Watch()
        {
            bool activated = false;
            foreach (var procName in procNames)
            {
                var procs = Process.GetProcessesByName(procName);
                foreach (var proc in procs)
                {
                    if (this.ApplicationIsActivated(proc.Id))
                    {
                        activated = true;
                        break;
                    }
                }
            }

            if (lastActivatedState == null || lastActivatedState != activated)
            {
                if (activated) onActivated?.Invoke();
                else onDeactivated?.Invoke();
                lastActivatedState = activated;
            }
            onWatch?.Invoke(activated);

            return activated;
        }


        #region DLL Import

        /// <summary>Returns true if the current application has focus, false otherwise</summary>
        public bool ApplicationIsActivated(int procId)
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        #endregion
    }
}
