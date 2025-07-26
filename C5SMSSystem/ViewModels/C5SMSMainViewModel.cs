using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Management;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Commands;
using System.Windows;
using System.Threading;
using System.Windows.Input;
using System.Resources;
using C5SMSSystem.Properties;
using C5SMSSystem.ViewModels;

namespace C5SMSSystem
{
    public class C5SMSMainViewModel : ViewModelBase
    {
        #region Private & public data Members (Fields)
        private const int DEFAULT_BAUD_RATE = 9600;
        private AutoResetEvent receiveNow = new AutoResetEvent(false);
        private Thread readThread;
        #endregion

        #region Private data members & public properties
        private SerialPort c5SerialPort;
        public SerialPort C5SerialPort
        {
            get { return c5SerialPort; }
            set { c5SerialPort = value; }
        }

        private int comIndex;
        public int ComIndex
        {
            get { return comIndex; }
            set
            {
                if (value != comIndex)
                {
                    comIndex = value;
                    RaisePropertyChanged("ComIndex");
                }

            }
        }

        private bool lockStatus;
        public bool LockStatus
        {
            get { return lockStatus; }
            set
            {
                if (value != lockStatus)
                {
                    lockStatus = value;
                    RaisePropertyChanged("LockStatus");
                }
            }
        }


        private bool connectionStatus;
        public bool ConnectionStatus
        {
            get { return connectionStatus; }
            set
            {
                if (value != connectionStatus)
                {
                    connectionStatus = value;
                    RaisePropertyChanged("ConnectionStatus");
                }

            }
        }

        private ObservableCollection<string> comPorts;
        public ObservableCollection<string> ComPorts
        {
            get { return comPorts; }
            set
            {
                if (value != null && value != comPorts)
                {
                    comPorts = value;
                    RaisePropertyChanged("ComPorts");
                }

            }
        }

        private LoginViewModel unlockDialog;
        public LoginViewModel UnlockDialog
        {
            get { return unlockDialog; }
            set
            {
                unlockDialog = value;
                RaisePropertyChanged("UnlockDialog");
            }
        }

        private PortSettingsViewModel _portSettingsVM;
        public PortSettingsViewModel PortSettingsVM
        {
            get { return _portSettingsVM; }
            set { _portSettingsVM = value; RaisePropertyChanged("PortSettingsVM"); }
        }
        
        private int _baudRate = DEFAULT_BAUD_RATE;  // 115200
        public int BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; RaisePropertyChanged("BaudRate"); }
        }

        #endregion

        #region Command properties
        public DelegateCommand ConnectCommand { get; set; }
        #endregion

        #region Open and Close Ports
        //Open Port
        public Dictionary<bool, object> OpenPort(string p_strPortName, int p_uBaudRate, int p_uDataBits, int p_uReadTimeout, int p_uWriteTimeout)
        {
            SerialPort port = new SerialPort();
            try
            {
                port.PortName = p_strPortName;                 //COM1
                port.BaudRate = p_uBaudRate;                   //9600
                port.DataBits = p_uDataBits;                   //8
                port.StopBits = StopBits.One;                  //1
                port.Parity = Parity.None;                     //None
                port.Handshake = Handshake.XOnXOff; // port.Handshake = Handshake.RequestToSendXOnXOff;
                // port.ReadTimeout = p_uReadTimeout;             //300
                // port.WriteTimeout = p_uWriteTimeout;           //300
                port.DataReceived += new SerialDataReceivedEventHandler(C5SerialPort_DataReceived);
                port.Open();
                port.DtrEnable = true;
                port.RtsEnable = true;
                return new Dictionary<bool, object>() { { true, port } };
            }
            catch (Exception ex)
            {
                return new Dictionary<bool, object>() { { false, ex.Message } };
            }
        }

        //Close Port
        public void ClosePort(SerialPort port)
        {
            try
            {
                port.Close();
                port.DataReceived -= new SerialDataReceivedEventHandler(C5SerialPort_DataReceived);
                port = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Constructors
        public C5SMSMainViewModel()
        {
            LoadPorts();
            ConnectCommand = new DelegateCommand(Connect);
            ConnectionStatus = false;
            ComIndex = 0;
            
            LockStatus = string.IsNullOrEmpty(Settings.Default.SMSAppPassword) ? false : true;

            GetBaudRateFromSettings();

            UnlockDialog = new LoginViewModel();
            PortSettingsVM = new PortSettingsViewModel();
            PortSettingsVM.BaudRate = BaudRate.ToString();
            
            ProsoftCommons.CreateLogFile(@"C5SMSSystemXmlErrorLog_.xml");
            //UnlockDialog.IsOpen = true;
        }
        #endregion

        #region Public & private methods
        private void LoadPorts()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_POTSModem");
            var collection = searcher.Get();
            ComPorts = new ObservableCollection<string>();
            ComPorts.Add("Select Port");
            foreach (ManagementObject modemObject in collection)
            {
                ComPorts.Add(modemObject["AttachedTo"].ToString());
            }
        }

        private void Connect()
        {
            if (ComIndex > 0)
            {
                if (ConnectionStatus)
                {
                    ConnectionStatus = false;
                    ClosePort(this.C5SerialPort);
                }
                else
                {
                    //Open communication port 
                    Dictionary<bool, object> portStatus = OpenPort(ComPorts[ComIndex], BaudRate, 8, 300, 300);
                    if (portStatus.ContainsKey(true))
                    {
                        this.C5SerialPort = (SerialPort)portStatus.Single(c => c.Key == true).Value;
                        if (this.C5SerialPort != null && this.C5SerialPort.IsOpen)
                        {
                            ConnectionStatus = true;
                            SMSMonitorViewModel.Instance.Start(C5SerialPort, receiveNow);
                        }
                        else
                        {
                            MessageBox.Show("Invalid port settings");
                        }
                    }
                    else
                    {
                        MessageBox.Show((string)portStatus.Single(c => c.Key == false).Value);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select com port.", "C5 SMS System");
            }
        }

        private void C5SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Chars)
            {
                receiveNow.Set();
            }
        }

        public void LockIconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UnlockDialog.IsOpen = UnlockDialog.IsOpen == false ? true : UnlockDialog.IsOpen;
                if (!UnlockDialog.IsOpen)
                {
                    SMSMonitorViewModel.Instance.LoadDateFilters();
                    SMSMonitorViewModel.Instance.LoadData();
                }
            }
        }

        public void PortSettingsMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (ConnectionStatus)
                {
                    ConnectionStatus = false;
                    ClosePort(this.C5SerialPort);
                }

                PortSettingsVM.IsOpen = PortSettingsVM.IsOpen == false ? true : PortSettingsVM.IsOpen;
                if (!PortSettingsVM.IsOpen)
                {
                    GetBaudRateFromSettings();
                }
            }
        }

        private void GetBaudRateFromSettings()
        {
            try
            {
                string strBaudRate = DEFAULT_BAUD_RATE.ToString();
                if (string.IsNullOrEmpty(Settings.Default.BaudRate))
                {
                    Settings.Default.BaudRate = DEFAULT_BAUD_RATE.ToString();
                    Settings.Default.Save();
                }
                else
                {
                    strBaudRate = Settings.Default.BaudRate;
                }

                int intBaudRate = DEFAULT_BAUD_RATE;
                if (int.TryParse(strBaudRate, out intBaudRate))
                {
                    BaudRate = intBaudRate;
                }
            }
            catch (Exception ex)
            {
                BaudRate = DEFAULT_BAUD_RATE;
            }
        }
        #endregion
    }
}
