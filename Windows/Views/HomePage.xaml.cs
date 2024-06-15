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

            // �����ͳ�ʼ�� SunValueUpdateTimer
            SunValueUpdateTimer = new Timer(1000); // ÿ�봥��һ��
            SunValueUpdateTimer.Elapsed += SunValueUpdateTimer_Elapsed; // ���¼��������

            // ������״̬��ʱ��
            ProcessCheckTimer = new Timer(1000); // ÿ����һ���Ƿ��⵽����
            ProcessCheckTimer.Elapsed += ProcessCheckTimer_Elapsed; // ���¼��������
            ProcessCheckTimer.Start(); // ����������״̬�Ķ�ʱ��

            // ��ʼ״̬Ϊ����δ��⵽
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
                SetInfoBarTitle("����");
                SetInfoBarMessage("δ�ܼ�⵽���̣�");
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

                SunValueTextBlock.Text = $"����ֵ���޸�Ϊ: {sunValue}";
            }
            else
            {
                ContentDialog invalidValueDialog = new ContentDialog
                {
                    Title = "����",
                    Content = "��������Ч������ֵ��",
                    CloseButtonText = "ȷ��",
                    XamlRoot = this.Content.XamlRoot
                };

                _ = await invalidValueDialog.ShowAsync();
            }
        }

        private void LockSunCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!ProcessDetected)
            {
                SetInfoBarTitle("����");
                SetInfoBarMessage("δ�ܼ�⵽���̣�");
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

            // ����״̬��ȷ���ؼ�״̬��ȷ
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

                SunValueTextBlock.Text = $"��ǰ����ֵ: {sunValue}";
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

        // ��������ӹ������������ڸ���InfoBar����
        public void SetInfoBarTitle(string title)
        {
            DispatcherQueue.TryEnqueue(() => {
                InfoBar.Title = title;
            });
        }

        // ��ӹ������������ڸ���InfoBar��Ϣ
        public void SetInfoBarMessage(string message)
        {
            DispatcherQueue.TryEnqueue(() => {
                InfoBar.Message = message;
            });
        }

        // ��ӹ������������ڸ���InfoBar������
        public void SetInfoBarSeverity(InfoBarSeverity severity)
        {
            DispatcherQueue.TryEnqueue(() => {
                InfoBar.Severity = severity;
            });
        }

        // ��ӹ������������ڴ򿪻�ر�InfoBar
        public void SetInfoBarIsOpen(bool isOpen)
        {
            DispatcherQueue.TryEnqueue(() => {
                InfoBar.IsOpen = isOpen;
            });
        }
    }
}