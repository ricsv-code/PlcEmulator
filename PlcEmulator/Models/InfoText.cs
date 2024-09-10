using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlcEmulator
{
    public class InfoText
    {
        public InfoText()
        {

            TextBoxReceivedData = new StringBuilder();
            TextBoxSentData = new StringBuilder();
        }

        public string TextBoxImageData;
        public string TextBoxOperation;
        public StringBuilder TextBoxReceivedData;
        public StringBuilder TextBoxSentData;

    }
}
