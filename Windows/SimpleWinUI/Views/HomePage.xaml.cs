using System;
using System.Diagnostics;
using System.Timers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimpleWinUI.Helpers;
using SimpleWinUI.States;

namespace SimpleWinUI
{
    public sealed partial class HomePage : Page
    {
        private int baseAddress = 0x006A9EC0;
        private string processName = "PlantsVsZombies";
        public Timer ProcessCheckTimer { get; private set; }
        public Timer SunValueUpdateTimer { get; private set; }
        public bool ProcessDetected { get; set; }
        public bool isSunLocked = false;
        private int lockedSunValue = 99999;
        private IProcessState currentState;

        public HomePage()
        {
            this.InitializeComponent();

            // 创建和初始化 SunValueUpdateTimer
            SunValueUpdateTimer = new Timer(1000); // 每秒触发一次
            SunValueUpdateTimer.Elapsed += SunValueUpdateTimer_Elapsed; // 绑定事件处理程序

            // 检查进程状态定时器
            ProcessCheckTimer = new Timer(1000); // 每秒检查一次是否检测到进程
            ProcessCheckTimer.Elapsed += ProcessCheckTimer_Elapsed; // 绑定事件处理程序
            ProcessCheckTimer.Start(); // 启动检查进程状态的定时器

            // 初始状态为进程未检测到
            SetState(new ProcessNotDetectedState());
        }

        public void SetState(IProcessState newState)
        {
            currentState = newState;
            currentState.Handle(this);
        }

        private void CheckProcessStatus()
        {
            bool isProcessDetected = IsProcessRunning(processName);
            if (isProcessDetected)
            {
                SetState(new ProcessDetectedState());
            }
            else
            {
                SetState(new ProcessNotDetectedState());
            }
        }

        private void ProcessCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                CheckProcessStatus();
            });
        }

        private void SunValueUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ProcessDetected && isSunLocked)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    int address = MemoryHelper.ReadMemoryValue(baseAddress, processName);
                    address = address + 0x768;
                    address = MemoryHelper.ReadMemoryValue(address, processName);
                    address = address + 0x5560;
                    MemoryHelper.WriteMemoryValue(address, processName, lockedSunValue);
                    UpdateSunValue();
                });
            }
        }

        private async void ModifySun_Click(object sender, RoutedEventArgs e)
        {
            if (!ProcessDetected)
            {
                SetInfoBarTitle("错误");
                SetInfoBarMessage("未能检测到进程！");
                SetInfoBarSeverity(InfoBarSeverity.Error);
                SetInfoBarIsOpen(true);
                return;
            }

            if (int.TryParse(SunInputTextBox.Text, out int sunValue))
            {
                lockedSunValue = sunValue;
                int address = MemoryHelper.ReadMemoryValue(baseAddress, processName);
                address = address + 0x768;
                address = MemoryHelper.ReadMemoryValue(address, processName);
                address = address + 0x5560;
                MemoryHelper.WriteMemoryValue(address, processName, sunValue);

                SunValueTextBlock.Text = $"阳光值已修改为: {sunValue}";
            }
            else
            {
                ContentDialog invalidValueDialog = new ContentDialog
                {
                    Title = "错误",
                    Content = "请输入有效的阳光值！",
                    CloseButtonText = "确定",
                    XamlRoot = this.Content.XamlRoot
                };

                _ = await invalidValueDialog.ShowAsync();
            }
        }

        private void LockSunCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!ProcessDetected)
            {
                SetInfoBarTitle("错误");
                SetInfoBarMessage("未能检测到进程！");
                SetInfoBarSeverity(InfoBarSeverity.Error);
                SetInfoBarIsOpen(true);
                LockSunCheckBox.IsChecked = false;
                return;
            }

            isSunLocked = true;
            SunInputTextBox.IsEnabled = false;
            ModifySunButton.IsEnabled = false;
            lockedSunValue = 99999;
            int address = MemoryHelper.ReadMemoryValue(baseAddress, processName);
            address = address + 0x768;
            address = MemoryHelper.ReadMemoryValue(address, processName);
            address = address + 0x5560;
            MemoryHelper.WriteMemoryValue(address, processName, lockedSunValue);
        }

        private void LockSunCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isSunLocked = false;
            SunInputTextBox.IsEnabled = true;
            ModifySunButton.IsEnabled = true;

            // 更新状态以确保控件状态正确
            if (ProcessDetected)
            {
                UpdateControlsState(true);
            }
        }

        public void UpdateControlsState(bool isEnabled)
        {
            SunInputTextBox.IsEnabled = isEnabled;
            ModifySunButton.IsEnabled = isEnabled;
            LockSunCheckBox.IsEnabled = isEnabled;
        }

        private void UpdateSunValue()
        {
            if (ProcessDetected)
            {
                int address = MemoryHelper.ReadMemoryValue(baseAddress, processName);
                address = address + 0x768;
                address = MemoryHelper.ReadMemoryValue(address, processName);
                address = address + 0x5560;
                int sunValue = MemoryHelper.ReadMemoryValue(address, processName);

                SunValueTextBlock.Text = $"当前阳光值: {sunValue}";
            }
        }

        private void InfoBar_CloseButtonClick(InfoBar sender, object args)
        {
            InfoBar.IsOpen = false;
        }

        private bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        // 在这里添加公共方法，用于更新InfoBar标题
        public void SetInfoBarTitle(string title)
        {
            DispatcherQueue.TryEnqueue(() => {
                InfoBar.Title = title;
            });
        }

        // 添加公共方法，用于更新InfoBar消息
        public void SetInfoBarMessage(string message)
        {
            DispatcherQueue.TryEnqueue(() => {
                InfoBar.Message = message;
            });
        }

        // 添加公共方法，用于更新InfoBar严重性
        public void SetInfoBarSeverity(InfoBarSeverity severity)
        {
            DispatcherQueue.TryEnqueue(() => {
                InfoBar.Severity = severity;
            });
        }

        // 添加公共方法，用于打开或关闭InfoBar
        public void SetInfoBarIsOpen(bool isOpen)
        {
            DispatcherQueue.TryEnqueue(() => {
                InfoBar.IsOpen = isOpen;
            });
        }
    }
}