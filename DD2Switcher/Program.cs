using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace DD2Switcher; 

internal static class Program {
    private static Rectangle rect = new(0, 0, 1920, 1080);
    private static readonly Process[] games = Process.GetProcessesByName("Dundefgame");
    private static Process activeGame = games[0];
    private static Bitmap screenshot;
    private static Graphics graphics;
    private static readonly int defaultAffinity = 0b100000000000;
    private static bool paused = true;

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr handle, ref Rectangle rect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    public static Bitmap CaptureWindow(IntPtr handle) {
        if (screenshot == null)
            screenshot = new Bitmap(rect.Width, rect.Height);
        graphics = Graphics.FromImage(screenshot);
        var hdc = graphics.GetHdc();
        PrintWindow(handle, hdc, 0);
        graphics.ReleaseHdc(hdc);
        return screenshot;
    }

    private static void AdjustAffinities() {
        var fullAffinity = 0b111111111111;
        var i = 0;
        foreach (var game in games)
            if (game != activeGame) {
                var processAffinty = defaultAffinity >> i;
                fullAffinity = fullAffinity & ~processAffinty;
                game.ProcessorAffinity = new IntPtr(processAffinty);
                i++;
            }

        activeGame.ProcessorAffinity = new IntPtr(fullAffinity);
    }

    private static void AdjustPriorities() {
        foreach (var game in games) game.PriorityClass = ProcessPriorityClass.Idle;
        activeGame.PriorityClass = ProcessPriorityClass.High;
    }

    private static void NerfAll() {
        var i = 0;
        foreach (var game in games) {
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
        var foregroundWindow = GetForegroundWindow();
        Process foregroundGame = null;
        var foregroundGameIndex = -1;
        var exists = false;

        foreach (var game in games)
            if (foregroundWindow == game.MainWindowHandle) {
                exists = true;
                foregroundGame = game;
                foregroundGameIndex = Array.IndexOf(games, game);
                break;
            }

        if (exists) {
            var tempGame = games[0];
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
        HotKeyManager.RegisterHotKey(Keys.W, KeyModifiers.Alt);
        HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;
        
        List<Pixel> pixelList = new List<Pixel>();
        pixelList.Add(new Pixel(1062, 885, 240, 240, 240));

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
                case Keys.W:
                    if (paused) {
                        Console.Beep(1500, 500);
                        paused = false;
                    } else {
                        Console.Beep(500, 500);
                        paused = true;
                    }

                    break;
            }
        }

        while (true) {
            while (!paused) {
                screenshot = CaptureWindow(games[0].MainWindowHandle);
                foreach (Pixel p in pixelList) {
                    if (p.ProcessBitmap(screenshot)) {
                        Console.Beep(1500, 850);
                    }
                }
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
        }
    }
}