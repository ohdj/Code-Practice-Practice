using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWinUI.States
{
    public interface IProcessState
    {
        void Handle(HomePage context);
    }
}
