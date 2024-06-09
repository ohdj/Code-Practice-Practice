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
        private int baseAddress = 0x006A9EC0;          // ��Ϸ�ڴ��ַ
        private string processName = "PlantsVsZombies";// ��Ϸ��������
        private Timer processCheckTimer;
        private Timer sunValueUpdateTimer;
        private bool processDetected = false;

        public HomePage()
        {
            this.InitializeComponent();

            // ��ʼ����ʱ����ÿ����һ�ν���
            processCheckTimer = new Timer(1000);
            processCheckTimer.Elapsed += ProcessCheckTimer_Elapsed;
            processCheckTimer.Start();

            // ��ʼ����ʱ����ÿ�����һ������ֵ
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
                    InfoBar.Title = "�ɹ�";
                    InfoBar.Message = "���̶�ȡ�ɹ���";
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
                    InfoBar.Title = "����";
                    InfoBar.Message = "δ�ܼ�⵽���̣�";
                    InfoBar.Severity = InfoBarSeverity.Error;
                    InfoBar.IsOpen = true;
                });
            }
        }

        private void SunValueUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateSunValue();
            });
        }

        private void ReadSun_Click(object sender, RoutedEventArgs e)
        {
            UpdateSunValue();
        }

        private void ModifySun_Click(object sender, RoutedEventArgs e)
        {
            if (!processDetected)
            {
                InfoBar.Title = "����";
                InfoBar.Message = "δ�ܼ�⵽���̣�";
                InfoBar.IsOpen = true;
                return;
            }

            int address = ReadMemoryValue(baseAddress); // ��ȡ��ַ
            address = address + 0x768;                  // ��ȡ2����ַ
            address = ReadMemoryValue(address);
            address = address + 0x5560;                 // ��ȡ���������ֵ�ĵ�ַ
            WriteMemory(address, 9000);                // д������ֵ
        }

        private void UpdateSunValue()
        {
            if (processDetected)
            {
                int address = ReadMemoryValue(baseAddress); // ��ȡ��ַ
                address = address + 0x768;                  // ��ȡ2����ַ
                address = ReadMemoryValue(address);
                address = address + 0x5560;                 // ��ȡ���������ֵ�ĵ�ַ
                int sunValue = ReadMemoryValue(address);    // ��ȡ����ֵ

                SunValueTextBlock.Text = $"��ǰ����ֵ: {sunValue}";
            }
        }

        // InfoBar�رհ�ť�¼�
        private void InfoBar_CloseButtonClick(InfoBar sender, object args)
        {
            if (processDetected)
            {
                InfoBar.IsOpen = false;
            }
        }

        // �������Ƿ�����
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

        public void WriteMemory(int baseAdd, int value)
        {
            try
            {
                Helper.WriteMemoryValue(baseAdd, processName, value);
            }
            catch
            {
                InfoBar.Title = "����";
                InfoBar.Message = "д�ڴ�ʧ�ܣ�";
                InfoBar.IsOpen = true;
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
