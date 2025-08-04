using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Collections.ObjectModel;

namespace C5SMSSystem
{
    public interface ISMSReadWrite
    {
        ObservableCollection<SMS> Read(SerialPort _port, string _command);
        List<PartSMSs> createSMS(SerialPort C5SerialPort, string _phoneNo, string _msgString);
        bool Send(SerialPort _port, string _phoneNo, string _message, int totalParts, int Part);
        bool Delete(SerialPort _port, string _command);
        string Balance(SerialPort _port, string _command);
    }
}
