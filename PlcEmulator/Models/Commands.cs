using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlcEmulator
{
    public class Commands
    {

        public ICommand StartCommand;
        public ICommand StopCommand;
        public ICommand CleanCommand;
        public ICommand RunScriptsCommand;
        public ICommand SetMotorPropertiesCommand;
        public ICommand DarkModeCommand;
        public ICommand StandardCommand;
        public ICommand NumberOfMotorsCommand;

        public ICommand SendSomeErrorsCommand;



    }
}
