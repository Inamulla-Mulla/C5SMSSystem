using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;
using System.Management;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Threading;
using System.Globalization;
using System.Collections.ObjectModel;

namespace C5SMSSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Field
        private Thread readThread;
        private Thread sendThread;
        private AutoResetEvent receiveNow = new AutoResetEvent(false);
        private SMSdb smsDB;
        private  SMSReadWrite msgReadWrite;
        //public static readonly string NotFoundMsg = @"Requested data {0} not found.";
        #endregion

        #region pubic & private Properties
        private SerialPort _C5SerialPort;
        public SerialPort C5SerialPort
        {
            get { return _C5SerialPort; }
            set { _C5SerialPort = value; }
        }

        private ObservableCollection<SMS> mySMSCollection;
        public ObservableCollection<SMS> MYSMSCollection
        {
            get { return mySMSCollection; }
            set { mySMSCollection = value; }
        }

        private ObservableCollection<SMSLog> logCollection;
        public ObservableCollection<SMSLog> LogCollection
        {
            get { return logCollection; }
            set { logCollection = value; }
        }

        private Queue<SMS> smsQueue;
        public Queue<SMS> SMSQueue
        {
            get { return smsQueue; }
            set { smsQueue = value; }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        void C5SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Chars)
            {
                receiveNow.Set();
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnConnect.Content.ToString() == "Connect")
                {
                    //Open communication port 
                    this.C5SerialPort = OpenPort("COM1", 9600, 8, 300, 300);
                    if (this.C5SerialPort != null && this.C5SerialPort.IsOpen)
                    {
                        btnConnect.Content = "Disconnect";
                        lblConnectionStatus.Content = "C";
                        lblConnectionStatus.ToolTip = "Connected";
                        lblConnectionStatus.Background = Brushes.Green;
                        msgReadWrite = new SMSReadWrite(receiveNow);
                        smsDB = new SMSdb();
                        MYSMSCollection = smsDB.GetSMSs();
                        LogCollection = smsDB.GetLogs();
                        dgDisplayMessages.ItemsSource = MYSMSCollection;
                        dgDisplayLog.ItemsSource = LogCollection;
                        SMSQueue = new Queue<SMS>(MYSMSCollection.Where(x => x.Status == SMSStatus.Pending.ToString()));
                        readThread = new Thread(new ThreadStart(ReadMessages));
                        readThread.Start();

                        //sendThread = new Thread(new ThreadStart(SendMessages));
                        //sendThread.Start();
                    }
                    else
                    {
                        MessageBox.Show("Invalid port settings");
                    }
                }
                else
                {
                    ClosePort(this.C5SerialPort);
                    btnConnect.Content = "Connect";
                    lblConnectionStatus.Content = "D";
                    lblConnectionStatus.ToolTip = "Disconnected";
                    lblConnectionStatus.Background = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Open and Close Ports
        //Open Port
        public SerialPort OpenPort(string p_strPortName, int p_uBaudRate, int p_uDataBits, int p_uReadTimeout, int p_uWriteTimeout)
        {
            SerialPort port = new SerialPort();
            try
            {
                port.PortName = p_strPortName;                 //COM1
                port.BaudRate = p_uBaudRate;                   //9600
                port.DataBits = p_uDataBits;                   //8
                port.StopBits = StopBits.One;                  //1
                port.Parity = Parity.None;                     //None
               // port.ReadTimeout = p_uReadTimeout;             //300
                //port.WriteTimeout = p_uWriteTimeout;           //300
                port.DataReceived += new SerialDataReceivedEventHandler(C5SerialPort_DataReceived);
                port.Open();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return port;
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
                throw ex;
            }
        }
        #endregion

        private void ReadMessages()
        {
            try
            {
               // msgReadWrite = new SMSReadWrite(receiveNow);
                while (C5SerialPort.IsOpen)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action<ObservableCollection<SMS>>(SetSMSCollection), msgReadWrite.Read(this.C5SerialPort, AtComands.UnreadMessages));
                    SendMessages();
                }
            }
            catch (ThreadAbortException tax)
            {
                MessageBox.Show(tax.Message);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendMessages()
        {
            try
            {
                if (SMSQueue.Count > 0)
                {
                    CreateSendSMS(SMSQueue.Dequeue());
                }
            }
            catch (ThreadAbortException tax)
            {
                MessageBox.Show(tax.Message);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CreateSendSMS(SMS _sms)
        {
            bool result = false;
            bool UpdateMaster = true;
            try
            {
                string getDataOf = string.Empty;
                string[] msgArray = _sms.Message.Trim().Split(new char[] { ' ' });
                int arrayCount = msgArray.Count() - 1;
                int logCount = LogCollection.Count(x => x.MessageID == _sms.MessageID && x.SentStatus != SentSMSStatus.Passed.ToString() && x.SentStatus != SentSMSStatus.NotFound.ToString());
                if (logCount != null && logCount > 0)
                {
                    arrayCount = logCount;
                }
                SentSMSStatus[] arrayOfPassed = new SentSMSStatus[arrayCount];

                if (msgArray[0].ToUpper() == SMSRequestType.SDR.ToString())
                {
                    for (int i = 1; i <= arrayCount; i++)
                    {
                        result = false;
                        string requestPhoneNo = msgArray[i];
                        requestPhoneNo = requestPhoneNo.Length == 10 ? "91" + requestPhoneNo : requestPhoneNo;
                       // if (phoneNo.Length == 10) { phoneNo = "91" + phoneNo; }
                        SDRData sdrData = smsDB.GetData<SDRData>(SqlQueries.SDR_sp, requestPhoneNo, 1);
                        if (sdrData != null && !sdrData.Equals(null) && !string.IsNullOrEmpty(sdrData.MobileNumber) && sdrData.DateOfActivation != null)
                        {
                            string _msgString = CreateString<SDRData>(sdrData, SMSRequestType.SDR);
                            // Data Found
                            if (msgReadWrite.Send(C5SerialPort, _sms.SenderNumber = _sms.SenderNumber.Length == 12 ? "+" + _sms.SenderNumber : _sms.SenderNumber, _msgString))
                            {
                                //Data Found msg Sent.
                                result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, _msgString, SentSMSStatus.Passed, System.DateTime.Now);
                                LogCollection.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = _msgString, SentStatus = SentSMSStatus.Passed.ToString() });
                                arrayOfPassed[i - 1] = SentSMSStatus.Passed;
                            }
                            else
                            {
                                //Data Found msg Sending failed.
                                result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, _msgString, SentSMSStatus.Failed, System.DateTime.Now);
                                LogCollection.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = _msgString, SentStatus = SentSMSStatus.Failed.ToString() });
                                arrayOfPassed[i - 1] = SentSMSStatus.Failed;
                            }
                        }
                        else
                        {
                            //Data Not found
                            if (msgReadWrite.Send(C5SerialPort, _sms.SenderNumber = _sms.SenderNumber.Length == 12 ? "+" + _sms.SenderNumber : _sms.SenderNumber, DataNotFound(_sms.RequestType + " " + requestPhoneNo)))
                            {
                                //Data not Found msg sent.
                                result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, DataNotFound(_sms.RequestType + " " + requestPhoneNo), SentSMSStatus.NotFound, System.DateTime.Now);
                                LogCollection.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = DataNotFound(_sms.RequestType + " " + requestPhoneNo), SentStatus = SentSMSStatus.NotFound.ToString() });
                                arrayOfPassed[i - 1] = SentSMSStatus.NotFound;
                            }
                            else
                            {
                                //Data not Found msg Sending failed.
                                result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, DataNotFound(_sms.RequestType + " " + requestPhoneNo), SentSMSStatus.NotFoundAndFailed, System.DateTime.Now);
                                LogCollection.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = DataNotFound(_sms.RequestType + " " + requestPhoneNo), SentStatus = SentSMSStatus.NotFoundAndFailed.ToString() });
                                arrayOfPassed[i - 1] = SentSMSStatus.NotFoundAndFailed;
                            }
                        }
                    }

                    foreach (SentSMSStatus item in arrayOfPassed)
                    {
                        if (item != SentSMSStatus.Passed && item != SentSMSStatus.NotFound)
                        {
                            UpdateMaster = false;
                            break;
                        }
                    }
                    if (UpdateMaster)
                    {
                        // Update Pending Status... in Master.
                        _sms.Status = SMSStatus.Success.ToString();
                        if(smsDB.Update(_sms))
                        {
                            var item = MYSMSCollection.FirstOrDefault(x => x.MessageID == _sms.MessageID);
                            if (item != null)
                            {
                                item.Status = _sms.Status;
                                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(UpdateSMSCollection));
                            }
                            
                        }

                    }
                    else
                    {
                        // Update Pending Status... in Master.
                        _sms.Status = SMSStatus.Failed.ToString();
                        if (smsDB.Update(_sms))
                        {
                            var item = MYSMSCollection.FirstOrDefault(x => x.MessageID == _sms.MessageID);
                            if (item != null)
                            {
                                item.Status = _sms.Status;
                                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(UpdateSMSCollection));
                            }
                        }
                        // If failed to send msg add again to the queue...
                        //SMSQueue.Enqueue(_sms);
                        //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(UpdateSMSCollection));
                    }
                    
                }
                else if (msgArray[0].ToUpper() == SMSRequestType.CELLID.ToString())
                {
                    getDataOf = SqlQueries.CellID_sp;
                }
                else if (msgArray[0].ToUpper() == SMSRequestType.STD.ToString() || msgArray[0].ToUpper() == SMSRequestType.ISD.ToString() || msgArray[0].ToUpper() == SMSRequestType.SERIES.ToString())
                {
                    getDataOf = SqlQueries.SSI_sp;
                }

                //return result;
            }
            catch (Exception ex)
            {
                //return result;
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateSMSCollection()
        {
            dgDisplayMessages.Items.Refresh();
            dgDisplayLog.Items.Refresh();
        }

        public static string DataNotFound(string value)
        {
            return string.Format("Requested data {0} not found.", value);
        }

        private string CreateString<T>( T _t, SMSRequestType _requestType) where T : class
        {
            string msgString = string.Empty;
            if (_requestType == SMSRequestType.SDR)
            {
               msgString =  String.Format("Mob: {0}, Name: {1}, Addr: {2}, DOA: {3}, SP_Circle: {4}",
                              (_t as SDRData).MobileNumber, 
                              (_t as SDRData).FirstName + " " + (_t as SDRData).MiddleName + " " + (_t as SDRData).LastName, 
                              (_t as SDRData).Address1 + " " + (_t as SDRData).Address2 + " " + (_t as SDRData).Address3 + " " + (_t as SDRData).Place,
                              (_t as SDRData).DateOfActivation, (_t as SDRData).ServiceProviderName);
            }
            return msgString;
        }

        private void SetSMSCollection(ObservableCollection<SMS> smsCollection)
        {
            if (smsCollection != null && smsCollection.Count > 0)
            {
                foreach (var item in smsCollection)
                {
                    if (smsDB.Save(item))
                    {

                    }
                    else
                    {
                    }
                }
                bool result = msgReadWrite.Delete(C5SerialPort, AtComands.DeleteReadMessages);
                SMS lastSMS = smsDB.GetSMS();
                MYSMSCollection.Add(lastSMS);
                if (lastSMS.Status == SMSStatus.Pending.ToString()) { SMSQueue.Enqueue(lastSMS); }
                dgDisplayMessages.Items.Refresh();
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
             Thread.Sleep(1000);
                if (!C5SerialPort.IsOpen)
                {
                    C5SerialPort.Open();
                }
                Thread.Sleep(1000);
                C5SerialPort.Write("AT+CMGF=1\r");
                txtMessage.Text = C5SerialPort.ReadExisting();
                Thread.Sleep(1000);
                C5SerialPort.Write("AT+CSCA=\"" + "+919032055002" + "\"\r\n");
                txtMessage.Text = C5SerialPort.ReadExisting();
                Thread.Sleep(1000);
                C5SerialPort.Write("AT+CMGS=\"" + txtNumber.Text + "\"\r\n");
                txtMessage.Text = C5SerialPort.ReadExisting();
                Thread.Sleep(1000);
                C5SerialPort.Write(txtMessage.Text + char.ConvertFromUtf32(26) + "\r");
               // txtMessage.Text = C5SerialPort.ReadExisting();
                Thread.Sleep(1000);
                txtMessage.Text = C5SerialPort.ReadExisting();
               // C5SerialPort.Close();
            }
            catch (Exception ex)
            {
 
            }
        }

        private void btnBalance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtBalanceMsg.Text = string.Empty;
                txtBalanceMsg.Text = msgReadWrite.Balance(C5SerialPort, AtComands.CheckBalance(txtServiceCode.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
