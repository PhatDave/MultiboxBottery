using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DD2Switcher;

internal static class Program {
    private static List<Process> processes = new();

    private static Process activeProcess;
    private static readonly IntPtr defaultAffinity = new(0xFF000000);
    private static readonly IntPtr fullAffinity = new(0xFFFFFFFF);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    private static void AdjustAffinities() {
        List<Process> fuckedProcesses = new();

        foreach (var process in processes)
            if (process != activeProcess) {
                try {
                    process.ProcessorAffinity = defaultAffinity;
                }
                catch (Exception e) {
                    fuckedProcesses.Add(process);
                }
            }

        try {
            activeProcess.ProcessorAffinity = fullAffinity;
        }
        catch (Exception e) {
            fuckedProcesses.Add(activeProcess);
        }

        foreach (var fucked in fuckedProcesses)
            processes.Remove(fucked);
    }

    private static void AdjustPriorities() {
        List<Process> fuckedProcesses = new();

        foreach (var process in processes) {
            try {
                process.PriorityClass = ProcessPriorityClass.Idle;
            }
            catch (Exception e) {
                fuckedProcesses.Add(process);
            }
        }

        try {
            activeProcess.PriorityClass = ProcessPriorityClass.High;
        }
        catch (Exception e) {
            fuckedProcesses.Add(activeProcess);
        }

        foreach (var fucked in fuckedProcesses)
            processes.Remove(fucked);
    }

    private static void SwitchToProcess(int index) {
        Console.WriteLine("Switching to process at index " + index);
        if (index >= processes.Count) return;
        var targetWindowHandle = processes[processes.Count - 1 - index].MainWindowHandle;
        if (targetWindowHandle == IntPtr.Zero) {
            processes.RemoveAt(processes.Count - 1 - index);
            return;
        }
        SetForegroundWindow(targetWindowHandle);
        activeProcess = processes[processes.Count - 1 - index];
        AdjustAffinities();
        AdjustPriorities();
    }

    private static void SwitchMainGame() {
        var foregroundWindow = GetForegroundWindow();
        Process foregroundGame = null;
        var foregroundGameIndex = -1;
        var exists = false;

        foreach (var process in processes)
            if (foregroundWindow == process.MainWindowHandle) {
                exists = true;
                foregroundGame = process;
                foregroundGameIndex = processes.IndexOf(process);
                break;
            }

        if (exists) {
            var tempGame = processes[0];
            processes[0] = foregroundGame;
            processes[foregroundGameIndex] = tempGame;
        }
    }

    private static void ToggleGame() {
        Console.WriteLine("Toggling foreground window as tracked...");
        var foregroundWindow = GetForegroundWindow();
        var systemProcesses = Process.GetProcesses();
        Process foregroundProcess = null;

        foreach (var process in systemProcesses)
            if (foregroundWindow == process.MainWindowHandle) {
                foregroundProcess = process;
                break;
            }

        if (foregroundProcess == null) return;
        Console.WriteLine("Foreground process: " + foregroundProcess.ProcessName);
        var existingProcess = processes.Find(process => process.Id == foregroundProcess.Id);
        if (existingProcess != null) {
            Console.WriteLine("Removing foreground process from tracked...");
            processes.Remove(existingProcess);
        }
        else {
            Console.WriteLine("Adding foreground process to tracked...");
            processes.Add(foregroundProcess);
        }
    }

    [STAThread]
    private static void Main() {
        // AllocConsole();

        var processes = Process.GetProcesses();
        var currentProcess = Process.GetCurrentProcess();

        foreach (var process in processes)
            if (process.Id != currentProcess.Id && process.ProcessName == currentProcess.ProcessName) {
                process.Kill();
                Process.GetCurrentProcess().Kill();
            }

        
        HotKeyManager.RegisterHotKey(Keys.D1, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D2, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D3, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D4, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D5, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D6, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D7, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D8, KeyModifiers.Alt);
        HotKeyManager.RegisterHotKey(Keys.D9, KeyModifiers.Alt);

        HotKeyManager.RegisterHotKey(Keys.Oemtilde, KeyModifiers.Alt);

        // HotKeyManager.RegisterHotKey(Keys.Q, KeyModifiers.Alt);
        // HotKeyManager.RegisterHotKey(Keys.W, KeyModifiers.Alt);
        // HotKeyManager.RegisterHotKey(Keys.R, KeyModifiers.Alt);
        HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;

        var pixelList = new System.Collections.Generic.List<Pixel>();
        // pixelList.Add(new Pixel(1401, 1234, 224, 224, 224));
        pixelList.Add(new Pixel(1359, 1235, 220, 220, 220));

        static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e) {
            switch (e.Key) {
                case Keys.D1:
                    SwitchToProcess(0);
                    break;
                case Keys.D2:
                    SwitchToProcess(1);
                    break;
                case Keys.D3:
                    SwitchToProcess(2);
                    break;
                case Keys.D4:
                    SwitchToProcess(3);
                    break;
                case Keys.D5:
                    SwitchToProcess(4);
                    break;
                case Keys.D6:
                    SwitchToProcess(5);
                    break;
                case Keys.D7:
                    SwitchToProcess(6);
                    break;
                case Keys.D8:
                    SwitchToProcess(7);
                    break;
                case Keys.D9:
                    SwitchToProcess(8);
                    break;
                case Keys.Oemtilde:
                    ToggleGame();
                    break;
            }
        }


        Console.CancelKeyPress += (sender, e) => {
            Process.GetCurrentProcess().Kill();
        };
        while (true)
            System.Threading.Thread.Sleep(100000);

    }
}
