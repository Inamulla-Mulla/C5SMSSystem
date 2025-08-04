using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Commands;
using System.Windows;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System.Windows.Controls;
using System.IO.Ports;
using C5SMSSystem.Properties;

namespace C5SMSSystem
{
    public class UserManagementViewModel : ViewModelBase
    {
        #region Private & public data Members (Fields)
         private ISMSdb smsDB;
         private ISMSReadWrite msgReadWrite;
        #endregion

        #region Private data members & public properties
         private SMSUser singleSMSUser;
         public SMSUser SingleSMSUser
         {
             get { return singleSMSUser; }
             set
             {
                 if (value != null && value != singleSMSUser)
                 {
                     singleSMSUser = value;
                     RaisePropertyChanged("SingleSMSUser");
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
        // For Check Box..
         public Dictionary<int, bool> SelectedValues { get; set; }

         private List<object> selectedPrivileges;
         public List<object> SelectedPrivileges
         {
             get { return selectedPrivileges; }
             set
             {
                 if (value != null && value != selectedPrivileges)
                 {
                     selectedPrivileges = value;
                     RaisePropertyChanged("SelectedPrivileges");
                 }
             }
         }

         private SerialPort c5SerialPort;
         public SerialPort C5SerialPort
         {
             get { return c5SerialPort; }
             set { c5SerialPort = value; }
         }

         private string smsLimit;
         public string SMSLimit
         {
             get { return smsLimit; }
             set { smsLimit = value; RaisePropertyChanged("SMSLimit"); }
         }
         

         private string selectedSMSValidity;
         public string SelectedSMSValidity
         {
             get { return selectedSMSValidity; }
             set 
             {
                 selectedSMSValidity = value;
                 RaisePropertyChanged("SelectedSMSValidity");
                 if (SelectedSMSValidity == "Unlimited") SMSLimit = string.Empty;
             }
         }
         
        #endregion

        #region Command properties
         public DelegateCommand CreateCommand { get; set; }
         public DelegateCommand UpdateCommand { get; set; }
         public DelegateCommand DeleteCommand { get; set; }
         public DelegateCommand<object> PrintExportCommand { get; set; }
        #endregion

        #region Constructors
        public UserManagementViewModel()
        {
            smsDB = new SMSdb();
            SingleSMSUser = new SMSUser();
            PrintExportCommand = new DelegateCommand<object>(PrintExport);
            CreateCommand = new DelegateCommand(Create);
            UpdateCommand = new DelegateCommand(Update);
            DeleteCommand = new DelegateCommand(Delete);
            SelectedValues = new Dictionary<int, bool>();
            SMSUsers = GetUsers();
            msgReadWrite = new SMSReadWrite(SMSMonitorViewModel.Instance.C5AutoResetEvent);
            this.C5SerialPort = SMSMonitorViewModel.Instance.C5SerialPort;
            SMSLimit = string.Empty;
            SelectedSMSValidity = "-1";
        }
        #endregion

        #region Public & private methods
        public ObservableCollection<SMSUser> GetUsers()
        {
            return smsDB.GetSMSUsers();
        }

        private bool delay()
        {
            long i;
            for (i = 1; i <= 90000000; i++)
            {
            }
            return true;
        }

        private void Create()
        {
            foreach (string item in SelectedPrivileges)
            {
                SingleSMSUser.Privileges = SingleSMSUser.Privileges + item + ",";
            }
            SingleSMSUser.Privileges = SingleSMSUser.Privileges.Substring(0, SingleSMSUser.Privileges.Length - 1);

            SingleSMSUser.UserPhoneNumber = SingleSMSUser.UserPhoneNumber.Length == 10 ? "91" + SingleSMSUser.UserPhoneNumber : SingleSMSUser.UserPhoneNumber;
            SingleSMSUser.CreatedDateTime = System.DateTime.Now;
            SingleSMSUser.SMSValidity = SelectedSMSValidity;
            if (!string.IsNullOrEmpty(SMSLimit)) { SingleSMSUser.SMSLimit = Convert.ToInt32(SMSLimit); } else { SingleSMSUser.SMSLimit = null; }

            List<PartSMSs> status = new List<PartSMSs>();

            if (!smsDB.SMSUserValidation(SingleSMSUser.UserPhoneNumber))
            {
                if (smsDB.SaveUser(SingleSMSUser))
                {
                    if (Settings.Default.isSendSmsOnUserCreation == true)
                    {
                        if (C5SerialPort != null && C5SerialPort.IsOpen)
                        {
                            Settings.Default.isSendSMS = false;
                            while (Settings.Default.isReadSMS == false) { }
                            Settings.Default.isReadSMS = false;

                            delay();

                            status = msgReadWrite.createSMS(c5SerialPort, SingleSMSUser.UserPhoneNumber, CreateString(XMLTemplates.GetTemplateWelcomMsg(), SingleSMSUser));

                            Settings.Default.isReadSMS = true;
                            Settings.Default.isSendSMS = true;
                        }

                    }
                    MessageBox.Show("User created successfully.", "C5 SMS System");
                    SingleSMSUser = new SMSUser();
                    SelectedPrivileges = new List<object>();
                    SMSUsers = GetUsers();
                    SMSMonitorViewModel.Instance.SMSUsers = SMSUsers;
                }
            }
            else
            {
                MessageBox.Show("User already exists.", "C5 SMS System");
                SingleSMSUser = new SMSUser();
                SelectedPrivileges = new List<object>();
            }
        }

        private void Update()
        {
            bool result = true;
            bool isValid = true;
            foreach (var item in SMSUsers)
            {
                if (item.SMSValidity != "Unlimited" && (item.SMSLimit == null || item.SMSLimit == 0))
                {
                    isValid = false;
                    MessageBox.Show("Please enter SMS Limit for " + item.UserName, "C5 SMS System");
                    break;

                }

                //if (smsDB.SMSUserValidation(SingleSMSUser.UserPhoneNumber))
                //{
                //    isValid = false;
                //    MessageBox.Show("User already exists.", "C5 SMS System");
                //    break;
                //}
            }
            if (isValid)
            {
                foreach (var item in SMSUsers)
                {
                    if (!smsDB.UpdateUsersDetails(item))
                    {
                        result = false;
                        break;
                    }
                }
                if (result)
                {
                    MessageBox.Show("Updated Successfully.", "C5 SMS System");
                    SMSUsers = GetUsers();
                    SMSMonitorViewModel.Instance.SMSUsers = SMSUsers;
                }
                else
                {
                    MessageBox.Show("Updated Failed.", "C5 SMS System");
                }
            }
        }

        private void Delete()
        {
            string userIDs = string.Empty;
            foreach (var item in SelectedValues.Keys)
            {
                userIDs = userIDs + item + ",";
            }
            if (!string.IsNullOrEmpty(userIDs))
            {
                userIDs = userIDs.Substring(0, userIDs.Length - 1);
                if (smsDB.DeleteMultiUsers(userIDs))
                {
                    SMSUsers = GetUsers();
                    SMSMonitorViewModel.Instance.SMSUsers = SMSUsers;
                    SelectedValues = new Dictionary<int, bool>();
                }
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

        public void userManageTabControlSelectionChanged(object sender, DevExpress.Xpf.Core.TabControlSelectionChangedEventArgs e)
        {
            SMSUsers = GetUsers();
        }

        private string CreateString(string _welcomeMsg, SMSUser _smsUser)
        {
            string msgString = string.Empty;
            msgString = _welcomeMsg.Replace("{uName}", _smsUser.UserName).Replace("{pNo}", _smsUser.UserPhoneNumber).Replace("{sName}", _smsUser.State).Replace("{pvlg}", _smsUser.Privileges);
            return msgString;
        }

        #endregion
    }
}
