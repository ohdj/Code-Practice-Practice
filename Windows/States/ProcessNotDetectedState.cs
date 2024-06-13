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
            context.SetInfoBarTitle("错误");
            context.SetInfoBarMessage("未能检测到进程！");
            context.SetInfoBarSeverity(InfoBarSeverity.Error);
            context.SetInfoBarIsOpen(true);
            context.UpdateControlsState(false);
        }
    }
}
