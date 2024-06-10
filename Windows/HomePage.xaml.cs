using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleWinUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private int baseAddress = 0x006A9EC0;          // 游戏内存基址
        private string processName = "PlantsVsZombies";// 游戏进程名字
        private Timer processCheckTimer;
        private Timer sunValueUpdateTimer;
        private bool processDetected = false;
        private bool isSunLocked = false;
        private int lockedSunValue = 99999;

        public HomePage()
        {
            this.InitializeComponent();

            // 初始化定时器，每秒检测一次进程
            processCheckTimer = new Timer(1000);
            processCheckTimer.Elapsed += ProcessCheckTimer_Elapsed;
            processCheckTimer.Start();

            // 初始化定时器，每秒更新一次阳光值
            sunValueUpdateTimer = new Timer(1000);
            sunValueUpdateTimer.Elapsed += SunValueUpdateTimer_Elapsed;
        }

        private void ProcessCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool isProcessDetected = IsProcessRunning(processName);

            if (isProcessDetected && !processDetected)
            {
                processDetected = true;
                sunValueUpdateTimer.Start();
                DispatcherQueue.TryEnqueue(() =>
                {
                    InfoBar.Title = "成功";
                    InfoBar.Message = "进程读取成功！";
                    InfoBar.Severity = InfoBarSeverity.Success;
                    InfoBar.IsOpen = true;
                });
            }
            else if (!isProcessDetected && processDetected)
            {
                processDetected = false;
                sunValueUpdateTimer.Stop();
                DispatcherQueue.TryEnqueue(() =>
                {
                    InfoBar.Title = "错误";
                    InfoBar.Message = "未能检测到进程！";
                    InfoBar.Severity = InfoBarSeverity.Error;
                    InfoBar.IsOpen = true;
                });
            }
        }

        private void SunValueUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (processDetected && isSunLocked)
                {
                    int address = ReadMemoryValue(baseAddress); // 读取基址
                    address = address + 0x768;                  // 获取2级地址
                    address = ReadMemoryValue(address);
                    address = address + 0x5560;                 // 获取存放阳光数值的地址
                    WriteMemory(address, lockedSunValue);       // 锁定阳光值
                }
                UpdateSunValue();
            });
        }

        private async void ModifySun_Click(object sender, RoutedEventArgs e)
        {
            if (!processDetected)
            {
                InfoBar.Title = "错误";
                InfoBar.Message = "未能检测到进程！";
                InfoBar.IsOpen = true;
                return;
            }

            if (int.TryParse(SunInputTextBox.Text, out int sunValue))
            {
                lockedSunValue = sunValue;
                int address = ReadMemoryValue(baseAddress); // 读取基址
                address = address + 0x768;                  // 获取2级地址
                address = ReadMemoryValue(address);
                address = address + 0x5560;                 // 获取存放阳光数值的地址
                WriteMemory(address, sunValue);             // 写入阳光值

                SunValueTextBlock.Text = $"阳光值已修改为: {sunValue}";
            }
            else
            {
                ContentDialog invalidValueDialog = new ContentDialog
                {
                    Title = "错误",
                    Content = "请输入有效的阳光值！",
                    CloseButtonText = "确定",
                    XamlRoot = this.Content.XamlRoot  // 设置XamlRoot属性
                };

                _ = await invalidValueDialog.ShowAsync();
            }
        }

        private void LockSunCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            isSunLocked = true;
            // 禁用输入框和按钮
            SunInputTextBox.IsEnabled = false;
            ModifySunButton.IsEnabled = false;
            // 设置阳光值为99999
            lockedSunValue = 99999;
            int address = ReadMemoryValue(baseAddress); // 读取基址
            address = address + 0x768;                  // 获取2级地址
            address = ReadMemoryValue(address);
            address = address + 0x5560;                 // 获取存放阳光数值的地址
            WriteMemory(address, lockedSunValue);       // 写入阳光值
        }

        private void LockSunCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isSunLocked = false;
            // 启用输入框和按钮
            SunInputTextBox.IsEnabled = true;
            ModifySunButton.IsEnabled = true;
        }

        private void UpdateSunValue()
        {
            if (processDetected)
            {
                int address = ReadMemoryValue(baseAddress); // 读取基址
                address = address + 0x768;                  // 获取2级地址
                address = ReadMemoryValue(address);
                address = address + 0x5560;                 // 获取存放阳光数值的地址
                int sunValue = ReadMemoryValue(address);    // 读取阳光值

                SunValueTextBlock.Text = $"当前阳光值: {sunValue}";
            }
        }

        private void InfoBar_CloseButtonClick(InfoBar sender, object args)
        {
            if (processDetected)
            {
                InfoBar.IsOpen = false;
            }
        }

        private bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        public int ReadMemoryValue(int baseAdd)
        {
            try
            {
                return Helper.ReadMemoryValue(baseAdd, processName);
            }
            catch
            {
                return 0;
            }
        }

        public async void WriteMemory(int baseAdd, int value)
        {
            try
            {
                Helper.WriteMemoryValue(baseAdd, processName, value);
            }
            catch
            {
                ContentDialog writeMemoryErrorDialog = new ContentDialog
                {
                    Title = "错误",
                    Content = "写内存失败！",
                    CloseButtonText = "确定",
                    XamlRoot = this.Content.XamlRoot  // 设置XamlRoot属性
                };

                _ = await writeMemoryErrorDialog.ShowAsync();
            }
        }
    }

    public static class Helper
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int PROCESS_WM_READ = 0x0010;
        private const int PROCESS_WM_WRITE = 0x0020;
        private const int PROCESS_VM_OPERATION = 0x0008;

        public static int ReadMemoryValue(int baseAdd, string processName)
        {
            Process process = Process.GetProcessesByName(processName)[0];
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);

            byte[] buffer = new byte[4];
            int bytesRead = 0;
            ReadProcessMemory(processHandle, baseAdd, buffer, buffer.Length, ref bytesRead);
            CloseHandle(processHandle);

            return BitConverter.ToInt32(buffer, 0);
        }

        public static void WriteMemoryValue(int baseAdd, string processName, int value)
        {
            Process process = Process.GetProcessesByName(processName)[0];
            IntPtr processHandle = OpenProcess(PROCESS_VM_OPERATION | PROCESS_WM_WRITE, false, process.Id);

            byte[] buffer = BitConverter.GetBytes(value);
            int bytesWritten = 0;
            WriteProcessMemory(processHandle, baseAdd, buffer, buffer.Length, ref bytesWritten);
            CloseHandle(processHandle);
        }
    }
}
