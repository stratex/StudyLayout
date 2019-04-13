using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WindowPositionRestore
{
    class WindowPOS
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        public struct Rect
        {
            public Rect(int L, int T, int R, int B)
            {
                Left = L;
                Top = T;
                Right = R;
                Bottom = B;
            }
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        public struct ProcRect
        {
            public ProcRect(Process process, Rect rectangle)
            {
                proc = process;
                rect = rectangle;
            }
            public Process proc { get; set; }
            public Rect rect { get; set; }
        }

        public static List<ProcRect> GetWindowsInfo()
        {

            List<ProcRect> rects = new List<ProcRect>();
            Rect currentrect = new Rect();
            Process[] processes = Process.GetProcesses();
            for (int i = 0; i < processes.Length; i++)
            {
                Process proc = processes[i];
                IntPtr ptr = proc.MainWindowHandle;
                if (proc.MainWindowTitle != string.Empty)
                {
                    GetWindowRect(ptr, ref currentrect);
                    rects.Add(new ProcRect(proc, currentrect));
                }
            }
            return rects;
        }

        //Filename:Processname:x:y:w:h
        public static void SaveWindowStates(List<ProcRect> pr, ListView.SelectedIndexCollection indices)
        {
            using (StreamWriter sw = new StreamWriter("settings.cfg"))
                foreach (int item in indices)
                    sw.WriteLine(String.Format("{0}:{1}:{2}:{3}:{4}:{5}", pr[item].proc.MainModule.FileName, pr[item].proc.ProcessName, pr[item].rect.Left, pr[item].rect.Top, pr[item].rect.Right, pr[item].rect.Bottom));
        }

        public static void RestoreWindowStates()
        {
            string[] line;
            using (StreamReader sr = new StreamReader("settings.cfg"))
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine().Split(':');
                    if (ProcRunning(line[2]))
                    {
                        var pr = Process.GetProcessesByName(line[2]);
                        foreach (var proc in pr)
                            if (proc.MainWindowTitle != "")
                                MoveWindow(proc.MainWindowHandle, Convert.ToInt32(line[3]), Convert.ToInt32(line[4]), Convert.ToInt32(line[5]) - Convert.ToInt32(line[3]), Convert.ToInt32(line[6]) - Convert.ToInt32(line[4]), true);
                    }
                    else
                    {
                        Process pr = new Process();
                        pr.StartInfo.FileName = line[0] + ':' + line[1];
                        pr.Start();
                        pr.WaitForInputIdle();
                        Thread.Sleep(2500);
                        MoveWindow(pr.MainWindowHandle, Convert.ToInt32(line[3]), Convert.ToInt32(line[4]), Convert.ToInt32(line[5]) - Convert.ToInt32(line[3]), Convert.ToInt32(line[6]) - Convert.ToInt32(line[4]), true);
                    }
                }
                     
        }

        public static bool ProcRunning(string name)
        {
            bool isRunning = false;
            var pr = Process.GetProcessesByName(name);
            if (pr.Length > 0)
                foreach (var item in pr)
                    if (item.MainWindowTitle != "")
                        isRunning = true;
            return isRunning;
        }
    }
}