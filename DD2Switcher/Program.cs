using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace DD2Switcher {
    internal static class Program {
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr handle, ref Rectangle rect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        static Process[] games = Process.GetProcessesByName("Dundefgame");
        static Process activeGame = games[0];
        static int defaultAffinity = 0b100000000000;

        private static void AdjustAffinities() {
            int fullAffinity = 0b111111111111;
            int i = 0;
            foreach (Process game in games) {
                if (game != activeGame) {
                    var processAffinty = defaultAffinity >> i;
                    fullAffinity = fullAffinity & ~processAffinty;
                    game.ProcessorAffinity = new IntPtr(processAffinty);
                    i++;                    
                }
            }
            activeGame.ProcessorAffinity = new IntPtr(fullAffinity);
        }

        private static void AdjustPriorities() {
            foreach (Process game in games) {
                game.PriorityClass = ProcessPriorityClass.Idle;
            }
            activeGame.PriorityClass = ProcessPriorityClass.High;
        }

        private static void NerfAll() {
            int i = 0;
            foreach (Process game in games) {
                game.ProcessorAffinity = new IntPtr(defaultAffinity >> i);
                game.PriorityClass = ProcessPriorityClass.Idle;
                i++;
            }
        }

        private static void SwitchToGame(int index) {
            SetForegroundWindow(games[index].MainWindowHandle);
            activeGame = games[index];
            AdjustAffinities();
            AdjustPriorities();
        }

        private static void SwitchMainGame() {
            IntPtr foregroundWindow = GetForegroundWindow();
            Process foregroundGame = null;
            int foregroundGameIndex = -1;
            bool exists = false;
            
            foreach (Process game in games) {
                if (foregroundWindow == game.MainWindowHandle) {
                    exists = true;
                    foregroundGame = game;
                    foregroundGameIndex = Array.IndexOf(games, game);
                    break;
                }
            }

            if (exists) {
                Process tempGame = games[0];
                games[0] = foregroundGame;
                games[foregroundGameIndex] = tempGame;
            }
        }

        [STAThread]
        private static void Main() {
            HotKeyManager.RegisterHotKey(Keys.D1, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.D2, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.D3, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.D4, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.D5, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.Q, KeyModifiers.Alt);
            HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

            static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e) {
                switch (e.Key) {
                    case Keys.D1:
                        SwitchToGame(0);
                        break;
                    case Keys.D2:
                        SwitchToGame(1);
                        break;
                    case Keys.D3:
                        SwitchToGame(2);
                        break;
                    case Keys.D4:
                        SwitchToGame(3);
                        break;
                    case Keys.D5:
                        SwitchMainGame();
                        break;
                    case Keys.Q:
                        NerfAll();
                        break;
                }
            }
            
            while (true) {
                Thread.Sleep(2000);
            }
        }
    }
}