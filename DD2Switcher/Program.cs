using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace DD2Switcher;

internal static class Program {
    private static Rectangle rect = new(0, 0, 2560, 1440);
    private static readonly Process[] games = Process.GetProcessesByName("Dundefgame");

    private static readonly SoundPlayer beeper =
        new(@"C:\Users\Administrator\RiderProjects\DD2Switcher\DD2Switcher\beep.wav");

    private static Process activeGame = games[0];
    private static Bitmap screenshot;
    private static Graphics graphics;
    private static readonly IntPtr defaultAffinity = new(0xFF000000);
    private static readonly IntPtr fullAffinity = new(0xFFFFFFFF);
    private static bool paused = true;
    
    private static List<Point> relevantPoints = new List<Point>();
    private static List<Point> pointsToRemove = new List<Point>();

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
        foreach (var game in games)
            if (game != activeGame)
                game.ProcessorAffinity = defaultAffinity;
        activeGame.ProcessorAffinity = fullAffinity;
    }

    private static void AdjustPriorities() {
        foreach (var game in games) game.PriorityClass = ProcessPriorityClass.Idle;
        activeGame.PriorityClass = ProcessPriorityClass.High;
    }

    private static void NerfAll() {
        foreach (var game in games) {
            game.ProcessorAffinity = defaultAffinity;
            game.PriorityClass = ProcessPriorityClass.Idle;
        }
    }

    private static void BuffAll() {
        foreach (var game in games) {
            game.ProcessorAffinity = fullAffinity;
            game.PriorityClass = ProcessPriorityClass.Normal;
        }
    }

    private static void SwitchToGame(int index) {
        if (index >= games.Length) return;
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
        var processes = Process.GetProcesses();
        var currentProcess = Process.GetCurrentProcess();

        foreach (var process in processes)
            if (process.Id != currentProcess.Id && process.ProcessName == currentProcess.ProcessName)
                process.Kill();

        HotKeyManager.RegisterHotKey(Keys.D1, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D2, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D3, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D4, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D5, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D6, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.Q, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.W, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.R, KeyModifiers.Alt);
        HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

        var pixelList = new System.Collections.Generic.List<Pixel>();
        // pixelList.Add(new Pixel(1401, 1234, 224, 224, 224));
        pixelList.Add(new Pixel(1359, 1235, 220, 220, 220));

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
                case Keys.D6:
                    Environment.Exit(0);
                    break;
                case Keys.W:
                    if (paused) {
                        beeper.Play();
                        paused = false;
                    }
                    else {
                        beeper.Play();
                        Thread.Sleep(150);
                        beeper.Play();
                        paused = true;
                    }

                    break;
            }
        }

        while (true) {
            bool runOnce = false;
            bool AAA = false;
            relevantPoints.Clear();
            while (!paused) {
                screenshot = CaptureWindow(games[0].MainWindowHandle);
                // screenshot.Save("SS.png");

                // if (!runOnce) {
                //     runOnce = true;
                //     for (var y = 0; y < screenshot.Height; y++)
                //     for (var x = 0; x < screenshot.Width; x++) {
                //         var pixelColor = screenshot.GetPixel(x, y);
                //         if (pixelColor.R > 220 && pixelColor.G > 220 && pixelColor.B > 220) 
                //             relevantPoints.Add(new Point(x, y));
                //     }
                // }
                //
                // pointsToRemove.Clear();
                // foreach (var relevantPoint in relevantPoints) {
                //     var pixel = screenshot.GetPixel(relevantPoint.X, relevantPoint.Y);
                //     if (!(pixel.R > 220) || !(pixel.G > 220) || !(pixel.B > 220))
                //         pointsToRemove.Add(relevantPoint);
                // }
                //
                // foreach (var point in pointsToRemove) {
                //     relevantPoints.Remove(point);
                // }
                //
                // Debug.WriteLine(relevantPoints.Count);
                
                foreach (var p in pixelList)
                    if (p.ProcessBitmap(screenshot)) {
                        beeper.Play();
                        break;
                    }

                Thread.Sleep(250);
            }
            
            //     System.IO.TextWriter tw = new System.IO.StreamWriter("SavedList.txt");
            //     foreach (var point in relevantPoints) {
            //         tw.WriteLine(point.ToString());
            //     }
            //     tw.Close();   

            Thread.Sleep(250);
        }
    }
}