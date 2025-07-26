using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Commands;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;
using System.Collections.ObjectModel;
using System.Windows.Input;
using C5SMSSystem.Properties;

namespace C5SMSSystem
{
    public class SMSTestViewModel : ViewModelBase
    {
        #region Private & public data Members (Fields)
         private ISMSdb smsDB;
         private ISMSReadWrite msgReadWrite;
        #endregion

        #region Private data members & public properties
         private SerialPort c5SerialPort;
         public SerialPort C5SerialPort
         {
             get { return c5SerialPort; }
             set { c5SerialPort = value; }
         }

         private string sendToNumber;
         public string SendToNumber
         {
             get { return sendToNumber; }
             set 
             {
                 if (value != null && value != sendToNumber)
                 {
                     sendToNumber = value;
                     RaisePropertyChanged("SendToNumber");
                 }  
             }
         }

         private string sendMessage;
         public string SendMessage
         {
             get { return sendMessage; }
             set
             {
                 if (value != null && value != sendMessage)
                 {
                     sendMessage = value;
                     RaisePropertyChanged("SendMessage");
                 }
             }
         }

         private ObservableCollection<SMSLog> sentTestMessages;
         public ObservableCollection<SMSLog> SentTestMessages
         {
             get { return sentTestMessages; }
             set
             {
                 if (value != null && value != sentTestMessages)
                 {
                     sentTestMessages = value;
                     RaisePropertyChanged("SentTestMessages");
                 }
             }
         }

         private TemplatesViewModel templatesDialog;
         public TemplatesViewModel TemplatesDialog
         {
             get { return templatesDialog; }
             set
             {
                 templatesDialog = value;
                 RaisePropertyChanged("TemplatesDialog");
             }
         }

         private bool templateCount;
         public bool TemplateCount
         {
             get { return templateCount; }
             set
             {
                 if (value != null && value != templateCount)
                 {
                     templateCount = value;
                     RaisePropertyChanged("TemplateCount");
                 }
             }
         }
        #endregion

        #region Command properties
        public DelegateCommand SendCommand { get; set; }
        public DelegateCommand<object> PrintExportCommand { get; set; }
        #endregion
    
        #region Constructors
        public SMSTestViewModel()
        {
            TemplatesDialog = new TemplatesViewModel();
            SendCommand = new DelegateCommand(SendTestMsg);
            PrintExportCommand = new DelegateCommand<object>(PrintExport);
            smsDB = new SMSdb();
            msgReadWrite = new SMSReadWrite(SMSMonitorViewModel.Instance.C5AutoResetEvent);
            SentTestMessages = smsDB.GetTestLogs();
            this.C5SerialPort = SMSMonitorViewModel.Instance.C5SerialPort;
            LoadTemplatesList();
        }
        #endregion

        #region Public & private methods

        private bool delay()
        {
            long i;
            for (i = 1; i <= 90000000; i++)
            {
            }
            return true;
        }

        private void SendTestMsg()
        {
            if (C5SerialPort != null && C5SerialPort.IsOpen)
            {
                bool result = false;

                Settings.Default.isSendSMS = false;
                while (Settings.Default.isReadSMS == false) { }
                Settings.Default.isReadSMS = false;

                delay();

                List<PartSMSs> status = new List<PartSMSs>();
                status = msgReadWrite.createSMS(c5SerialPort, SendToNumber=SendToNumber.Length == 10 ? "91" + SendToNumber : SendToNumber, SendMessage.Trim());

                foreach (var items in status)
                {
                    if (items.PartMsgStatus == SentSMSStatus.Passed)
                    {
                        //Data Found msg Sent.
                        result = smsDB.SaveSentLog(0, SendToNumber, items.PartMsg, SentSMSStatus.Test, System.DateTime.Now);
                        SentTestMessages.Add(new SMSLog() { LogID = 0, MessageID = 0, Sender = string.Empty, RequestNumber = SendToNumber, SentDateTime = System.DateTime.Now, SentMessage = items.PartMsg, SentStatus = SentSMSStatus.Test.ToString() });
                    }
                    else
                    {
                        //Data Found msg Sending failed.
                        result = smsDB.SaveSentLog(0, SendToNumber, items.PartMsg, SentSMSStatus.TestFailed, System.DateTime.Now);
                        SentTestMessages.Add(new SMSLog() { LogID = 0, MessageID = 0, Sender = string.Empty, RequestNumber = SendToNumber, SentDateTime = System.DateTime.Now, SentMessage = items.PartMsg, SentStatus = SentSMSStatus.TestFailed.ToString() });
                    }
                }

                Settings.Default.isReadSMS = true;
                Settings.Default.isSendSMS = true;

                MessageBox.Show("Message sending completed.", "C5 SMS System");
            }
            else
            {
                MessageBox.Show("Com port is not connected.","C5 SMS System");
            }
        }

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
                Title = "C5 SMS Preview"
            };
            link.CreateDocument(true);
            preview.Show();
        }

        public void addconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TemplatesDialog.IsOpen = true;
                if (!TemplatesDialog.IsOpen)
                {
                    if (TemplatesDialog.LocalSelectedTemplate != null)
                    {
                        SendMessage = TemplatesDialog.LocalSelectedTemplate.Text;
                    }
                }
            }
        }

        private void LoadTemplatesList()
        {
            TemplatesDialog.TemplateCollection = XMLTemplates.GetTemplatesWithoutWelcomMsg();
            TemplateCount = TemplatesDialog.TemplateCollection.Count > 0 ? true : false;
        }

        public void smsTestTabControlSelectionChanged(object sender, DevExpress.Xpf.Core.TabControlSelectionChangedEventArgs e)
        {
            LoadTemplatesList();
        }
        #endregion
    }
}
