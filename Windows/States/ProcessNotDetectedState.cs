using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWinUI.States
{
    public class ProcessNotDetectedState : IProcessState
    {
        public void Handle(HomePage context)
        {
            context.ProcessDetected = false;
            context.SunValueUpdateTimer.Stop();
            context.DispatcherQueue.TryEnqueue(() =>
            {
                context.InfoBar.Title = "错误";
                context.InfoBar.Message = "未能检测到进程！";
                context.InfoBar.Severity = InfoBarSeverity.Error;
                context.InfoBar.IsOpen = true;
                context.UpdateControlsState(false);
            });
        }
    }
}
