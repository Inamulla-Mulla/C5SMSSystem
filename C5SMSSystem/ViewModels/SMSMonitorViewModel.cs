using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Commands;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;
using System.Threading;
using System.Windows.Threading;
using C5SMSSystem.Properties;

namespace C5SMSSystem
{
     public class SMSMonitorViewModel : ViewModelBase
    {
        #region Private & public data Members (Fields)
         private ISMSdb smsDB;
         private ISMSReadWrite msgReadWrite;
         private Thread readThread;
         static readonly object _locker = new object();
        #endregion

        #region Private data members & public properties
         private SerialPort c5SerialPort;
         public SerialPort C5SerialPort
         {
             get { return c5SerialPort; }
             set { c5SerialPort = value; }
         }

         private AutoResetEvent c5AutoResetEvent;
         public AutoResetEvent C5AutoResetEvent
         {
             get { return c5AutoResetEvent; }
             set { c5AutoResetEvent = value; }
         }

         private ObservableCollection<SMS> receivedMessages;
         public ObservableCollection<SMS> ReceivedMessages
        {
            get { return receivedMessages; }
            set 
            {
                if (value != null && value != receivedMessages)
                {
                    receivedMessages = value;
                    RaisePropertyChanged("ReceivedMessages");
                }
            }
        }

         private ObservableCollection<SMSLog> sentMessages;
         public ObservableCollection<SMSLog> SentMessages
        {
            get { return sentMessages; }
            set 
            {
                if (value != null && value != sentMessages)
                {
                    sentMessages = value;
                    RaisePropertyChanged("SentMessages");
                }
            }
        }

        private Queue<SMS> smsQueue;
        public Queue<SMS> SMSQueue
        {
            get { return smsQueue; }
            set
            {
                if (value != null && value != smsQueue)
                {
                    smsQueue = value;
                    RaisePropertyChanged("SMSQueue");
                }
            }
        }

        private static SMSMonitorViewModel instance;
        public static SMSMonitorViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SMSMonitorViewModel();
                }
                return instance;
            }
        }

        private DateTime inboxFromDate;
        public DateTime InboxFromDate
        {
            get { return inboxFromDate; }
            set 
            {
                if (value != null && value != inboxFromDate)
                {
                    inboxFromDate = value;
                    RaisePropertyChanged("InboxFromDate");
                } 
            }
        }

        private DateTime inboxToDate;
        public DateTime InboxToDate
        {
            get { return inboxToDate; }
            set
            {
                if (value != null && value != inboxToDate)
                {
                    inboxToDate = value;
                    RaisePropertyChanged("InboxToDate");
                }
            }
        }

        private DateTime stboxFromDate;
        public DateTime StboxFromDate
        {
            get { return stboxFromDate; }
            set
            {
                if (value != null && value != stboxFromDate)
                {
                    stboxFromDate = value;
                    RaisePropertyChanged("StboxFromDate");
                }
            }
        }

        private DateTime stboxToDate;
        public DateTime StboxToDate
        {
            get { return stboxToDate; }
            set
            {
                if (value != null && value != stboxToDate)
                {
                    stboxToDate = value;
                    RaisePropertyChanged("StboxToDate");
                }
            }
        }

        private string sentMsgsCount;
        public string SentMsgsCount
        {
            get
            {
                return sentMsgsCount;
            }
            set
            {
                if (value != null && value != sentMsgsCount)
                {
                    sentMsgsCount = value;
                    RaisePropertyChanged("SentMsgsCount");
                }
            }
        }

        private ObservableCollection<SMSUser> smsUsers;
        public ObservableCollection<SMSUser> SMSUsers
        {
            get { return smsUsers; }
            set
            {
                if (value != null && value != smsUsers)
                {
                    smsUsers = value;
                    RaisePropertyChanged("SMSUsers");
                }
            }
        }

        //private DateTime maxDate;
        //public DateTime MaxDate
        //{
        //    get { return inboxToDate; }
        //    set
        //    {
        //        if (value != null && value != maxDate)
        //        {
        //            maxDate = value;
        //            RaisePropertyChanged("MaxDate");
        //        }
        //    }
        //}
        
        #endregion

        #region Command properties
        public DelegateCommand<object> PrintExportCommand { get; set; }
        public DelegateCommand FilterInboxCommand { get; set; }
        public DelegateCommand FilterSentboxCommand { get; set; }
        #endregion

        #region Constructors
        private SMSMonitorViewModel()
        {
            LoadDateFilters();
            smsDB = new SMSdb();
            PrintExportCommand = new DelegateCommand<object>(PrintExport);
            FilterInboxCommand = new DelegateCommand(FilterInbox);
            FilterSentboxCommand = new DelegateCommand(FilterSentbox);
            SMSUsers = smsDB.GetSMSUsers();
        }
        #endregion

        #region Public & private methods
        private void PrintExport(object param)
        {
            var Tparameter = (Tuple<TableView, object>)param;
            ((TableView)Tparameter.Item1).PrintAutoWidth = false;
            var link = new PrintableControlLink((TableView)Tparameter.Item1);
            //link.PageHeaderTemplate = PrintFormat.PageHeader;
            //link.PageFooterTemplate = PrintFormat.PageFooter;
            link.VerticalContentSplitting = DevExpress.XtraPrinting.VerticalContentSplitting.Smart;
            link.PrintingSystem.Document.ScaleFactor = 0.90F;
            //link.PrintingSystem.Document.AutoFitToPagesWidth = int.Parse("1");

            link.DocumentName = "Details";
            link.PaperKind = System.Drawing.Printing.PaperKind.A4;
            link.Margins.Top = 50;
            link.Margins.Bottom = 50;
            link.Margins.Left = 50;
            link.Margins.Right = 50;
            link.Landscape = true;
            //link.CreateDocument(false);
            //link.ShowPrintPreview(ProsoftCommons.GetVisualParent<Window>((UserControl)Tparameter.Item2),"C5 SMS Preview");

            DocumentPreviewWindow preview = new DocumentPreviewWindow()
            {
                Owner = ProsoftCommons.GetVisualParent<Window>((UserControl)Tparameter.Item2),
                ShowInTaskbar = true,
                WindowState = System.Windows.WindowState.Maximized,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Model = new LinkPreviewModel(link),
                Title="C5 SMS Preview"
            };
            link.CreateDocument(true);
            preview.Show();
        }

        public void Start(SerialPort _port, AutoResetEvent _autoReset)
        {
            C5SerialPort = _port;
            C5AutoResetEvent = _autoReset;
            msgReadWrite = new SMSReadWrite(C5AutoResetEvent);
            LoadDateFilters();
            //LoadData();
            //SMSQueue = new Queue<SMS>(ReceivedMessages.Where(x => x.Status == SMSStatus.Pending.ToString()));

            readThread = new Thread(new ThreadStart(ConfirmPendingSMSsAndStartThread));
            readThread.IsBackground = true;
            readThread.Start();
        }

        private void ConfirmPendingSMSsAndStartThread()
        {
            LoadOnlyPendingSMSs();
            SMSQueue = new Queue<SMS>(ReceivedMessages.Where(x => x.Status == SMSStatus.Pending.ToString()));

            if (SMSQueue != null && SMSQueue.Count > 0)
            {
                MessageBoxResult msgResult = MessageBox.Show("There are " + SMSQueue.Count + " pending SMSs in queue, do you want to send those pending SMSs?\n\nPending SMSs will be cancelled if you choose \"No\".", "Pending SMSs", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (msgResult == MessageBoxResult.Yes)
                {
                    while (SMSQueue.Count > 0)
                        SendMessage();
                }
                else
                {
                    while (SMSQueue.Count > 0)
                    {
                        SMS _sms = SMSQueue.Dequeue();
                        _sms.Status = SMSStatus.Cancelled.ToString();
                        if (smsDB.Update(_sms))
                        {
                            var item = ReceivedMessages.FirstOrDefault(x => x.MessageID == _sms.MessageID);
                            if (item != null)
                                item.Status = _sms.Status;
                        }
                    }
                }
            }

            LoadData();
            StartReadMessagesThread();
        }

        private void StartReadMessagesThread()
        {
            readThread = new Thread(new ThreadStart(ReadMessages));
            readThread.IsBackground = true;
            readThread.Start();
        }

        public void LoadOnlyPendingSMSs()
        {
            ReceivedMessages = smsDB.GetPendingSMSs();
            SentMessages = new ObservableCollection<SMSLog>();
            //SentMsgsCount = "Sent Messages : " + SentMessages.Count(x => x.SentStatus != SentSMSStatus.Failed.ToString()).ToString();
        }

        public void LoadData()
        {
            ReceivedMessages = smsDB.GetSMSsByDate(System.DateTime.Now);
            SentMessages = smsDB.GetLogsByDate(System.DateTime.Now);
            SentMsgsCount = "Sent Messages : " + SentMessages.Count(x => x.SentStatus != SentSMSStatus.Failed.ToString()).ToString();
        }

        private void ReadMessages()
        {
            lock (_locker)
            {
                try
                {
                    while (C5SerialPort.IsOpen)
                    {
                        SetSMSCollection(msgReadWrite.Read(SMSMonitorViewModel.Instance.C5SerialPort, AtComands.UnreadMessages));
                        if (Settings.Default.isSendSMS)
                            SendMessage();
                    }
                }
                catch (ThreadAbortException tax)
                {
                   // MessageBox.Show(tax.Message);
                    ProsoftCommons.SaveExeptionToLog(tax);
                }

                catch (Exception ex)
                {
                   // MessageBox.Show(ex.Message);
                    ProsoftCommons.SaveExeptionToLog(ex);
                }
            }
        }

        private void SendMessage()
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
                //MessageBox.Show(tax.Message);
                ProsoftCommons.SaveExeptionToLog(tax);
            }

            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
            }
        }

        private void CreateSendSMS(SMS _sms)
        {
            try
            {
                string getDataOf = string.Empty;
                string[] msgArray = _sms.Message.Trim().Split(new char[] { ' ' });
                int arrayCount = msgArray.Count() - 1;
                
                if (msgArray[0].ToUpper().Contains(SMSRequestType.SDR.ToString()))
                {
                    CreateSendSMS<SDRData>(arrayCount, msgArray, _sms, SqlQueries.SDR_sp, msgArray[0].ToUpper());
                }
                else if (msgArray[0].ToUpper().Contains(SMSRequestType.TID.ToString()))
                {
                    CreateSendSMS<TIDData>(arrayCount, msgArray, _sms, SqlQueries.TID_sp, msgArray[0].ToUpper());
                }
                else if (msgArray[0].ToUpper().Contains(SMSRequestType.CELLID.ToString()))
                {
                    CreateSendSMS<CellsiteData>(arrayCount, msgArray, _sms, SqlQueries.CellID_sp, msgArray[0].ToUpper());
                }
                else if (msgArray[0].ToUpper().Contains(SMSRequestType.STD.ToString()))
                {
                    CreateSendSMS<STDData>(arrayCount, msgArray, _sms, SqlQueries.SSI_sp, msgArray[0].ToUpper());
                }
                else if (msgArray[0].ToUpper().Contains(SMSRequestType.SERIES.ToString()))
                {
                    CreateSendSMS<SERIESData>(arrayCount, msgArray, _sms, SqlQueries.SSI_sp, msgArray[0].ToUpper());
                }
                else if (msgArray[0].ToUpper().Contains(SMSRequestType.ISD.ToString()))
                {
                    CreateSendSMS<ISDData>(arrayCount, msgArray, _sms, SqlQueries.ISD_sp, msgArray[0].ToUpper());
                }
                else if (msgArray[0].ToUpper().Contains(SMSRequestType.SUS.ToString()))
                {
                    CreateSendSMS<SUSData>(arrayCount, msgArray, _sms, SqlQueries.SUS_sp, msgArray[0].ToUpper());
                }                
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
            }
        }

        public static string DataNotFound(string value)
        {
            return string.Format("Requested {0} data not found.", value);
        }

        private string CreateString<T>(T _t, string _requestType) where T : IBusinessEntity
        {
            string msgString = string.Empty;
            //if (_requestType.ToUpper() == SMSRequestType.SDR.ToString())
            if (_requestType.ToUpper().Contains(SMSRequestType.SDR.ToString()))
            {
                //"Category:" at the end of sms will create error while sending sms
                //Hence if the Category is empty then pass it to "NULL" string
                string category = (_t as SDRData).Category;                
                if (category == null || category.Equals("") || category.Trim().Equals(""))
                {
                    category= "NULL";
                }

                // 31_Dec_2018 - As per new requirements (Irshad Sir and Mahesh) 
                // Changes made by - Inamullah
                // Remove Address2 and Address3 from Address field
                //msgString = String.Format(ReplaceNULLValue("Mob: ", (_t as SDRData).MobileNumber) + "\r\n" + ReplaceNULLValue("Name: ", (_t as SDRData).FirstName + " " + (_t as SDRData).MiddleName + " " + (_t as SDRData).LastName) +
                //    "\r\n" + ReplaceNULLValue("Addr: ", (_t as SDRData).Address1 + " " + (_t as SDRData).Place) + //  + " " + (_t as SDRData).Address2 + " " + (_t as SDRData).Address3
                //    "\r\n" + ReplaceNULLValue("DOA: ", (_t as SDRData).DateOfActivation != null ? (_t as SDRData).DateOfActivation.Value.ToShortDateString() : string.Empty) +
                //    "\r\n" + ReplaceNULLValue("SP_Circle: ", (_t as SDRData).ServiceProviderName) + "\r\n" + ReplaceNULLValue("Category: ", category));                

                msgString = String.Format(ReplaceNULLValue("", (_t as SDRData).MobileNumber) + "\r\n" + ReplaceNULLValue("", (_t as SDRData).FirstName + " " + (_t as SDRData).MiddleName + " " + (_t as SDRData).LastName) +
                    "\r\n" + ReplaceNULLValue("", (_t as SDRData).Address1 + " " + (_t as SDRData).Place) + //  + " " + (_t as SDRData).Address2 + " " + (_t as SDRData).Address3
                    "\r\n" + ReplaceNULLValue("", (_t as SDRData).DateOfActivation) +
                    "\r\n" + ReplaceNULLValue("", (_t as SDRData).ServiceProviderName) + "\r\n" + ReplaceNULLValue("", category));                
            }
            else if (_requestType.ToUpper().Contains(SMSRequestType.TID.ToString()))
            {
                string towerInfo = (_t as TIDData).Latitude + "###" + (_t as TIDData).Longitude + "###" + (_t as TIDData).Azimuth;
                string encodedString = Convert.ToBase64String(Encoding.UTF8.GetBytes(towerInfo));

                msgString = String.Format(ReplaceNULLValue("TID: ", (_t as TIDData).TowerID) + "\r\n\r\n" + ReplaceNULLValue("Tower Name: ", (_t as TIDData).TowerName) + 
                    "\r\n\r\n" + ReplaceNULLValue("Addr: ", (_t as TIDData).Address1) + 
                    "\r\n\r\n" + ReplaceNULLValue("SP: ", (_t as TIDData).ServiceProviderName) + 
                    "\r\n\r\n" + ReplaceNULLValue("LAC: ", (_t as TIDData).LAC) + 
                    "\r\n\r\n" + ReplaceNULLValue("State: ", (_t as TIDData).State) +
                    "\r\nLink: http://www.prosoftesolutions.com/cellid_info.html?params=" + encodedString);
            }
            else if (_requestType.ToUpper().Contains(SMSRequestType.CELLID.ToString()))
            {
                //msgString = String.Format(ReplaceNULLValue("Provider: ", (_t as CellsiteData).ServiceProviderNameTD) +
                //    "\r\n" + ReplaceNULLValue("Tower ID : ", (_t as CellsiteData).TowerID) +
                //    "\r\n" + ReplaceNULLValue("Tower Name: ", (_t as CellsiteData).TowerName) +
                //    "\r\n" + ReplaceNULLValue("Latitude: ", (_t as CellsiteData).Latitude) +
                //    "\r\n" + ReplaceNULLValue("Longitude: ", (_t as CellsiteData).Longitude) +
                //    "\r\n" + ReplaceNULLValue("Addr.: ", (_t as CellsiteData).Address1) + // " " + (_t as CellsiteData).Address2) +
                //    "\r\n" + ReplaceNULLValue("City: ", (_t as CellsiteData).TowerCity) +
                //    "\r\n" + ReplaceNULLValue("State: ", (_t as CellsiteData).TowerState));

                msgString = String.Format(ReplaceNULLValue("Tower ID : ", (_t as CellsiteData).TowerID) +
                    "\r\n" + ReplaceNULLValue("Tower Name: ", (_t as CellsiteData).TowerName) +
                    "\r\n" + ReplaceNULLValue("Link: ", "http://www.google.com/maps/place/" + (_t as CellsiteData).Latitude + ","+ (_t as CellsiteData).Longitude + ""));                
            }
            else if (_requestType.ToUpper().Contains(SMSRequestType.STD.ToString()))
            {
                msgString = String.Format(ReplaceNULLValue("Code: ", (_t as STDData).nPhoneNo) +
                "\r\n" + ReplaceNULLValue("City: ", (_t as STDData).Place) +
                "\r\n" + ReplaceNULLValue("Dist.: ", (_t as STDData).District) +
                "\r\n" + ReplaceNULLValue("State: ", (_t as STDData).StateName) +
                "\r\n" + ReplaceNULLValue("Country: ", (_t as STDData).CountryName));
            }
            else if (_requestType.ToUpper().Contains(SMSRequestType.ISD.ToString()))
            {
                msgString = String.Format(ReplaceNULLValue("Code: ", (_t as ISDData).CountryCode) +
                            "\r\n" + ReplaceNULLValue("Country: ", (_t as ISDData).CountryName));
            }
            else if (_requestType.ToUpper().Contains(SMSRequestType.SERIES.ToString()))
            {
                msgString = String.Format(ReplaceNULLValue("Series: ", (_t as SERIESData).nPhoneNo) +
                "\r\n" + ReplaceNULLValue("Provider: ", (_t as SERIESData).ServiceProviderName) +
                "\r\n" + ReplaceNULLValue("State: ", (_t as SERIESData).StateName) +
                "\r\n" + ReplaceNULLValue("Country: ", (_t as SERIESData).CountryName) +
                "\r\n" + ReplaceNULLValue("Contact Person: ", (_t as SERIESData).ContactPersonName) +
                "\r\n" + ReplaceNULLValue("Designation: ", (_t as SERIESData).ContactPersonDesignation) +
                "\r\n" + ReplaceNULLValue("Phone No.: ", (_t as SERIESData).ContactPhoneNo1));
            }
            else if (_requestType.ToUpper().Contains(SMSRequestType.SUS.ToString()))
            {
                msgString = String.Format(ReplaceNULLValue("SusNo: ", (_t as SUSData).nPhoneNo) +
                            "\r\n" + ReplaceNULLValue("SusName: ", (_t as SUSData).SuspectListName) +
                            "\r\n" + ReplaceNULLValue("Remarks: ", (_t as SUSData).Remarks) +
                            "\r\n" + ReplaceNULLValue("CreatedBy: ", (_t as SUSData).CreatedBy));
            }

            if (msgString.Equals("NULL"))
                msgString = msgString.Replace("NULL", "");
            
            msgString = msgString.Replace("\r\n\r\n", "\r\n");

            return msgString;
        }

        private string ReplaceNULLValue(string caption, string value)
        {            
            if (value != null && !value.Equals("") && !value.Equals("NULL"))
            {
                return caption + value;
            }
            else
            {
                return "";
            }
        }

        private void SetSMSCollection(ObservableCollection<SMS> smsCollection)
        {
            if (smsCollection != null && smsCollection.Count > 0)
            {
                foreach (var item in smsCollection)
                {
                    if (smsDB.Save(item))
                    {
                       // MessageBox.Show("Message Saved to DB.");
                    }
                    else
                    {
                        //MessageBox.Show("Failed to save message.","C5 SMS System");
                        ProsoftCommons.SaveExeptionToLog(new Exception("Failed to save message."));
                    }
                }
                
                ObservableCollection<SMS> pendingSMS = smsDB.GetPendingSMSs();
                foreach (var item in pendingSMS)
                {
                    ReceivedMessages.Add(item);
                    SMSQueue.Enqueue(item); 
                }

                // bool result = msgReadWrite.Delete(C5SerialPort, AtComands.DeleteReadMessages);

            }
        }

#region Old code
        //private void CreateSendSMS<T>(int arrayCount, string[] msgArray, SMS _sms, SentSMSStatus[] arrayOfPassed, string _spName, string _smsRequestType) where T : IBusinessEntity, new()
        //{
        //    bool result = false;
        //    bool UpdateMaster = true;
        //    T t = new T();
        //    for (int i = 1; i <= arrayCount; i++)
        //    {
        //        result = false;
        //        string requestPhoneNo = msgArray[i];
        //        requestPhoneNo = requestPhoneNo.Length == 10 ? "91" + requestPhoneNo : requestPhoneNo;
        //        // if (phoneNo.Length == 10) { phoneNo = "91" + phoneNo; }
        //        T tData = smsDB.GetData<T>(_spName, requestPhoneNo, 1);
        //        if (tData != null && AreAnyPropertiesNotNull(tData))
        //        {
        //            string _msgString = CreateString<T>(tData, _smsRequestType);

        //            IEnumerable<string> multiPartMsg = SplitByLength(_msgString, 152);
        //            int partNumber = 0;
        //            foreach (string _part in multiPartMsg)
        //            {
        //                //string _partMsg = multiPartMsg.Count() > 1 ? _part + " <" + partNumber + " of 1>" : _part;
        //                partNumber++;
        //                // Data Found
        //                if (msgReadWrite.Send(C5SerialPort, _sms.SenderNumber = _sms.SenderNumber.Length == 12 ? "+" + _sms.SenderNumber : _sms.SenderNumber, _part, multiPartMsg.Count(), partNumber))
        //                {
        //                    //Data Found msg Sent.
        //                    result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, _part, SentSMSStatus.Passed, System.DateTime.Now);
        //                    SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = _part, SentStatus = SentSMSStatus.Passed.ToString() });
        //                    arrayOfPassed[i - 1] = SentSMSStatus.Passed;
        //                }
        //                else
        //                {
        //                    //Data Found msg Sending failed.
        //                    result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, _part, SentSMSStatus.Failed, System.DateTime.Now);
        //                    SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = _part, SentStatus = SentSMSStatus.Failed.ToString() });
        //                    arrayOfPassed[i - 1] = SentSMSStatus.Failed;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            //Data Not found
        //            if (msgReadWrite.Send(C5SerialPort, _sms.SenderNumber = _sms.SenderNumber.Length == 12 ? "+" + _sms.SenderNumber : _sms.SenderNumber, DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"),1,1))
        //            {
        //                //Data not Found msg sent.
        //                result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"), SentSMSStatus.NotFound, System.DateTime.Now);
        //                SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"), SentStatus = SentSMSStatus.NotFound.ToString() });
        //                arrayOfPassed[i - 1] = SentSMSStatus.NotFound;
        //            }
        //            else
        //            {
        //                //Data not Found msg Sending failed.
        //                result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"), SentSMSStatus.NotFoundAndFailed, System.DateTime.Now);
        //                SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"), SentStatus = SentSMSStatus.NotFoundAndFailed.ToString() });
        //                arrayOfPassed[i - 1] = SentSMSStatus.NotFoundAndFailed;
        //            }
        //        }
        //    }

        //    foreach (SentSMSStatus item in arrayOfPassed)
        //    {
        //        if (item != SentSMSStatus.Passed && item != SentSMSStatus.NotFound)
        //        {
        //            UpdateMaster = false;
        //            break;
        //        }
        //    }
        //    if (UpdateMaster)
        //    {
        //        // Update Pending Status... in Master.
        //        _sms.Status = SMSStatus.Success.ToString();
        //        if (smsDB.Update(_sms))
        //        {
        //            var item = ReceivedMessages.FirstOrDefault(x => x.MessageID == _sms.MessageID);
        //            if (item != null)
        //                item.Status = _sms.Status;
        //        }

        //    }
        //    else
        //    {
        //        // Update Pending Status... in Master.
        //        _sms.Status = SMSStatus.Failed.ToString();
        //        if (smsDB.Update(_sms))
        //        {
        //            var item = ReceivedMessages.FirstOrDefault(x => x.MessageID == _sms.MessageID);
        //            if (item != null)
        //                item.Status = _sms.Status;
        //        }
        //    }
        //}

#endregion

        private void CreateSendSMS<T>(int arrayCount, string[] msgArray, SMS _sms, string _spName, string _smsRequestType) where T : IBusinessEntity, new()
        {
            bool result = false;
            bool UpdateMaster = true;
            List<PartSMSs> status = new List<PartSMSs>();
            //T t = new T();
            try
            {
                Settings.Default.isReadSMS = false;
                
                for (int i = 1; i <= arrayCount; i++)
                {
                    result = false;
                    string requestPhoneNo = msgArray[i];
                    if (_smsRequestType.ToUpper().Contains(SMSRequestType.SDR.ToString()))
                    {
                        requestPhoneNo = requestPhoneNo.Length == 10 ? "91" + requestPhoneNo : requestPhoneNo;
                    }

                    // Get Sms Validity for Sender.
                    SMSUser smsUser = null;
                    try
                    {
                        smsUser = SMSUsers.Single(s => s.UserPhoneNumber == _sms.SenderNumber);
                    }
                    catch (Exception e)
                    {
                        Exception tempObject = new Exception(e.Message + " User Not Found in Sms Monitor Collection");
                        ProsoftCommons.SaveExeptionToLog(tempObject);
                        smsUser = null;
                    }

                    if (smsUser == null) return;
                    // Check SMS Validity
                    if (CheckSMSValidity(smsUser))
                    {
                        if (_smsRequestType.ToUpper().Contains(SMSRequestType.CELLID.ToString()) && requestPhoneNo.Length < ProsoftCommons.RequestedTowerIDLength)
                        {
                            //Data Not found
                            status = msgReadWrite.createSMS(c5SerialPort, _sms.SenderNumber, "Tower ID length should be greater or equal to " + ProsoftCommons.RequestedTowerIDLength + " charaters.");

                            if (status.ElementAt(0).PartMsgStatus == SentSMSStatus.Passed)
                            {
                                //Data not Found msg sent.
                                result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, "Tower ID length should be greater or equal to " + ProsoftCommons.RequestedTowerIDLength + " charaters.", SentSMSStatus.NotFound, System.DateTime.Now);
                                SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = "Tower ID length should be greater or equal to " + ProsoftCommons.RequestedTowerIDLength + " charaters.", SentStatus = SentSMSStatus.NotFound.ToString() });
                                SentMsgsCount = "Sent Messages : " + SentMessages.Count(x => x.SentStatus != SentSMSStatus.Failed.ToString()).ToString();
                            }
                            else
                            {
                                //Data not Found msg Sending failed.
                                result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, "Tower ID length should be greater or equal to " + ProsoftCommons.RequestedTowerIDLength + " charaters.", SentSMSStatus.NotFoundAndFailed, System.DateTime.Now);
                                SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = "Tower ID length should be greater or equal to " + ProsoftCommons.RequestedTowerIDLength + " charaters.", SentStatus = SentSMSStatus.NotFoundAndFailed.ToString() });
                            }                            
                        }
                        else
                        {
                            T[] tData = smsDB.GetData<T>(_spName, requestPhoneNo, long.Parse(_sms.SenderNumber));
                            if (tData.Length > 0)
                            {
                                for (int rec = 0; rec < tData.Length; rec++)
                                {
                                    if (tData[rec] != null && AreAnyPropertiesNotNull(tData[rec]))
                                    {
                                        string _msgString = CreateString<T>(tData[rec], _smsRequestType);

                                        status = msgReadWrite.createSMS(c5SerialPort, _sms.SenderNumber, ProsoftCommons.RemoveSpecialCharacters(_msgString));      // ProsoftCommons.RemoveSpecialCharacters(_msgString)

                                        foreach (var items in status)
                                        {
                                            //Data Found msg Sent.
                                            result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, items.PartMsg, items.PartMsgStatus, System.DateTime.Now);
                                            SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = items.PartMsg, SentStatus = items.PartMsgStatus.ToString() });
                                            SentMsgsCount = "Sent Messages : " + SentMessages.Count(x => x.SentStatus != SentSMSStatus.Failed.ToString()).ToString();
                                        }
                                    }
                                    else
                                    {
                                        //Data Not found
                                        status = msgReadWrite.createSMS(c5SerialPort, _sms.SenderNumber, DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"));

                                        if (status.ElementAt(0).PartMsgStatus == SentSMSStatus.Passed)
                                        {
                                            //Data not Found msg sent.
                                            result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"), SentSMSStatus.NotFound, System.DateTime.Now);
                                            SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"), SentStatus = SentSMSStatus.NotFound.ToString() });
                                            SentMsgsCount = "Sent Messages : " + SentMessages.Count(x => x.SentStatus != SentSMSStatus.Failed.ToString()).ToString();
                                        }
                                        else
                                        {
                                            //Data not Found msg Sending failed.
                                            result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"), SentSMSStatus.NotFoundAndFailed, System.DateTime.Now);
                                            SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"), SentStatus = SentSMSStatus.NotFoundAndFailed.ToString() });

                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Data Not found
                                status = msgReadWrite.createSMS(c5SerialPort, _sms.SenderNumber, DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"));

                                if (status.ElementAt(0).PartMsgStatus == SentSMSStatus.Passed)
                                {
                                    //Data not Found msg sent.
                                    result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"), SentSMSStatus.NotFound, System.DateTime.Now);
                                    SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"), SentStatus = SentSMSStatus.NotFound.ToString() });
                                    SentMsgsCount = "Sent Messages : " + SentMessages.Count(x => x.SentStatus != SentSMSStatus.Failed.ToString()).ToString();
                                }
                                else
                                {
                                    //Data not Found msg Sending failed.
                                    result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"), SentSMSStatus.NotFoundAndFailed, System.DateTime.Now);
                                    SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = DataNotFound(_sms.RequestType + " '" + requestPhoneNo + "'"), SentStatus = SentSMSStatus.NotFoundAndFailed.ToString() });

                                }
                            }
                        }
                    }
                    else
                    {
                        //Data Not found
                        status = msgReadWrite.createSMS(c5SerialPort, _sms.SenderNumber, "Your sms request limit is over");

                        if (status.ElementAt(0).PartMsgStatus == SentSMSStatus.Passed)
                        {
                            //Data not Found msg sent.
                            result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, "Your sms request limit is over", SentSMSStatus.NotFound, System.DateTime.Now);
                            SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = "Your sms request limit is over for this month.", SentStatus = SentSMSStatus.NotFound.ToString() });
                            SentMsgsCount = "Sent Messages : " + SentMessages.Count(x => x.SentStatus != SentSMSStatus.Failed.ToString()).ToString();
                        }
                        else
                        {
                            //Data not Found msg Sending failed.
                            result = smsDB.SaveSentLog(_sms.MessageID, requestPhoneNo, "Your sms request limit is over", SentSMSStatus.NotFoundAndFailed, System.DateTime.Now);
                            SentMessages.Add(new SMSLog() { LogID = 0, MessageID = _sms.MessageID, Sender = _sms.Sender, RequestNumber = requestPhoneNo, SentDateTime = System.DateTime.Now, SentMessage = "Your sms request limit is over for this month.", SentStatus = SentSMSStatus.NotFoundAndFailed.ToString() });

                        }
                    }
                }                               

                Settings.Default.isReadSMS = true;

                foreach (var item in status)
                {
                    if (item.PartMsgStatus != SentSMSStatus.Passed)
                    {
                        UpdateMaster = false;
                        break;
                    }
                }
                if (UpdateMaster)
                {
                    // Update Pending Status... in Master.
                    _sms.Status = SMSStatus.Success.ToString();
                    if (smsDB.Update(_sms))
                    {
                        var item = ReceivedMessages.FirstOrDefault(x => x.MessageID == _sms.MessageID);
                        if (item != null)
                            item.Status = _sms.Status;
                    }

                }
                else
                {
                    // Update Pending Status... in Master.
                    _sms.Status = SMSStatus.Failed.ToString();
                    if (smsDB.Update(_sms))
                    {
                        var item = ReceivedMessages.FirstOrDefault(x => x.MessageID == _sms.MessageID);
                        if (item != null)
                            item.Status = _sms.Status;
                    }
                }
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
            }
        }

        public static bool AreAnyPropertiesNotNull(object obj)
        {
            bool result = false;
            foreach (var prop in obj.GetType().GetProperties())
            {
                object propertyValue = prop.GetValue(obj, null);
                if (propertyValue != null)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public static IEnumerable<string> SplitByLength(string str, int maxLength)
        {
            for (int index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }

        private bool CheckSMSValidity(SMSUser _smsUser)
        {
            bool result = false;
            if (_smsUser.SMSValidity == "Unlimited")
            {
                result = true;
            }
            else 
            {
                int smsRequestCount = smsDB.CheckSMSValidity(_smsUser);
                result = smsRequestCount < _smsUser.SMSLimit ? true : false;
            }

            return result;
        }

        private void FilterInbox()
        {
            if (C5SerialPort != null && C5SerialPort.IsOpen)
            {
                ReceivedMessages = smsDB.GetSMSsBetweenDates(InboxFromDate, InboxToDate);
            }
        }

        private void FilterSentbox()
        {
            if (C5SerialPort != null && C5SerialPort.IsOpen)
            {
                SentMessages = smsDB.GetLogsBetweenDates(StboxFromDate, StboxToDate);
                SentMsgsCount = "Sent Messages : " + SentMessages.Count(x => x.SentStatus != SentSMSStatus.Failed.ToString()).ToString();
            }
        }

        public void LoadDateFilters()
        {
            //MaxDate = System.DateTime.Today;
            InboxFromDate = System.DateTime.Today;
            InboxToDate = System.DateTime.Today;
            StboxFromDate = System.DateTime.Today;
            StboxToDate = System.DateTime.Today;
        }
        #endregion
    }
}
