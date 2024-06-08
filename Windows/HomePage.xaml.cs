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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleWinUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            LoadForegroundProcesses();
        }

        private void LoadForegroundProcesses()
        {
            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle))
                .OrderBy(p => p.ProcessName)
                .ToList();

            ProcessComboBox.ItemsSource = processes;
            ProcessComboBox.DisplayMemberPath = "MainWindowTitle";
            ProcessComboBox.SelectedValuePath = "Id";
        }

        private void OnReadMemoryClick(object sender, RoutedEventArgs e)
        {
            var selectedProcess = ProcessComboBox.SelectedItem as Process;
            if (selectedProcess != null && int.TryParse(AddressTextBox.Text, System.Globalization.NumberStyles.HexNumber, null, out int address))
            {
                try
                {
                    IntPtr processHandle = OpenProcess(ProcessAccessFlags.VirtualMemoryRead | ProcessAccessFlags.VirtualMemoryWrite, false, selectedProcess.Id);
                    if (processHandle == IntPtr.Zero)
                    {
                        CurrentValueTextBlock.Text = "Error opening process";
                        return;
                    }

                    IntPtr baseAddress = new IntPtr(address);
                    byte[] buffer = new byte[4]; // 假设读取4字节 (一个整数)
                    if (ReadProcessMemory(processHandle, baseAddress, buffer, buffer.Length, out int bytesRead) && bytesRead == buffer.Length)
                    {
                        int currentValue = BitConverter.ToInt32(buffer, 0);
                        CurrentValueTextBlock.Text = $"Current Value: {currentValue}";
                    }
                    else
                    {
                        CurrentValueTextBlock.Text = "Error reading memory";
                    }

                    CloseHandle(processHandle);
                }
                catch (Exception ex)
                {
                    CurrentValueTextBlock.Text = $"Error: {ex.Message}";
                }
            }
        }

        private void OnWriteMemoryClick(object sender, RoutedEventArgs e)
        {
            var selectedProcess = ProcessComboBox.SelectedItem as Process;
            if (selectedProcess != null && int.TryParse(AddressTextBox.Text, System.Globalization.NumberStyles.HexNumber, null, out int address) && int.TryParse(NewValueTextBox.Text, out int newValue))
            {
                try
                {
                    IntPtr processHandle = OpenProcess(ProcessAccessFlags.VirtualMemoryRead | ProcessAccessFlags.VirtualMemoryWrite, false, selectedProcess.Id);
                    if (processHandle == IntPtr.Zero)
                    {
                        CurrentValueTextBlock.Text = "Error opening process";
                        return;
                    }

                    IntPtr baseAddress = new IntPtr(address);
                    byte[] buffer = BitConverter.GetBytes(newValue);
                    if (WriteProcessMemory(processHandle, baseAddress, buffer, buffer.Length, out int bytesWritten) && bytesWritten == buffer.Length)
                    {
                        CurrentValueTextBlock.Text = "Memory write successful";
                    }
                    else
                    {
                        CurrentValueTextBlock.Text = "Error writing memory";
                    }

                    CloseHandle(processHandle);
                }
                catch (Exception ex)
                {
                    CurrentValueTextBlock.Text = $"Error: {ex.Message}";
                }
            }
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            VirtualMemoryRead = 0x0010,
            VirtualMemoryWrite = 0x0020,
            VirtualMemoryOperation = 0x0008,
            QueryInformation = 0x0400,
            ReadWrite = VirtualMemoryRead | VirtualMemoryWrite | VirtualMemoryOperation
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);
    }
}
