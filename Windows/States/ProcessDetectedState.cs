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
            context.SetInfoBarTitle("成功");
            context.SetInfoBarMessage("进程读取成功！");
            context.SetInfoBarSeverity(InfoBarSeverity.Success);
            context.SetInfoBarIsOpen(true);

            if (!context.isSunLocked)
            {
                context.UpdateControlsState(true);
            }
            else
            {
                context.UpdateControlsState(false);
                context.LockSunCheckBox.IsEnabled = true; // 只启用LockSunCheckBox控件
            }
        }
    }
}
