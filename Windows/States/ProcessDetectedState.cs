using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWinUI.States
{
    public class ProcessDetectedState : IProcessState
    {
        public void Handle(HomePage context)
        {
            context.ProcessDetected = true;
            context.SunValueUpdateTimer.Start();
            context.DispatcherQueue.TryEnqueue(() =>
            {
                context.InfoBar.Title = "成功";
                context.InfoBar.Message = "进程读取成功！";
                context.InfoBar.Severity = InfoBarSeverity.Success;
                context.InfoBar.IsOpen = true;
                context.UpdateControlsState(true);
            });
        }
    }
}
