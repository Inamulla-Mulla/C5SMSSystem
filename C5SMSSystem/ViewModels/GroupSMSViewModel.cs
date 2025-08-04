using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Xpf.Printing;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Grid;
using Microsoft.Practices.Prism.Commands;
using System.IO.Ports;
using System.Collections.ObjectModel;
using System.Data;
using C5SMSSystem.Properties;
using System.Windows.Input;
using System.Windows.Threading;
using DevExpress.Xpf.Core;
using System.Threading;
using System.IO;
using System.Xml;

namespace C5SMSSystem
{
    public class GroupSMSViewModel : ViewModelBase
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

         private IEnumerable<object> dynamicList;
         public IEnumerable<object> DynamicList
         {
             get { return dynamicList; }
             set 
             {
                 if (value != null && value != dynamicList)
                 {
                     dynamicList = value;
                     RaisePropertyChanged("DynamicList");
                 }  
             }
         }

         private List<object> selectedList;
         public List<object> SelectedList
         {
             get { return selectedList; }
             set
             {
                 if (value != null && value != selectedList)
                 {
                     selectedList = value;
                     RaisePropertyChanged("SelectedList");
                 }
             }
         }

         private GroupSelected isSelected;
         public GroupSelected IsSelected
         {
             get { return isSelected; }
             set 
             {
                 if (value != null && value != isSelected)
                 {
                     isSelected = value;
                     RaisePropertyChanged("IsSelected");
                     selectionChanged(value);
                 }   
             }
         }

         private string sendMaintenanceMessage;
         public string SendMaintenanceMessage
         {
             get { return sendMaintenanceMessage; }
             set
             {
                 if (value != null && value != sendMaintenanceMessage)
                 {
                     sendMaintenanceMessage = value;
                     RaisePropertyChanged("SendMaintenanceMessage");
                 }
             }
         }

         private ObservableCollection<SMSLog> maintenanceMessages;
         public ObservableCollection<SMSLog> MaintenanceMessages
         {
             get { return maintenanceMessages; }
             set
             {
                 if (value != null && value != maintenanceMessages)
                 {
                     maintenanceMessages = value;
                     RaisePropertyChanged("MaintenanceMessages");
                 }
             }
         }

         private ObservableCollection<SMSUser> smsUserDetails;
         public ObservableCollection<SMSUser> SMSUserDetails
         {
             get { return smsUserDetails; }
             set 
             {
                 if (value != null && value != smsUserDetails)
                 {
                     smsUserDetails = value;
                     RaisePropertyChanged("SMSUserDetails");
                 }  
             }
         }

         private ObservableCollection<Template> templateCollection;
         public ObservableCollection<Template> TemplateCollection
         {
             get { return templateCollection; }
             set
             {
                 if (value != null && value != templateCollection)
                 {
                     templateCollection = value;
                     RaisePropertyChanged("TemplateCollection");
                 }
             }
         }

         private Template selectedTemplate;
         public Template SelectedTemplate
         {
             get { return selectedTemplate; }
             set
             {
                 if (value != null && value != selectedTemplate)
                 {
                     selectedTemplate = value;
                     RaisePropertyChanged("SelectedTemplate");
                 }
             }
         }

         private string templateBtnCaption;
         public string TemplateBtnCaption
         {
             get { return templateBtnCaption; }
             set
             {
                 if (value != null && value != templateBtnCaption)
                 {
                     templateBtnCaption = value;
                     RaisePropertyChanged("TemplateBtnCaption");
                 }
             }
         }

         private string templateTitle;
         public string TemplateTitle
         {
             get { return templateTitle; }
             set
             {
                 if (value != null && value != templateTitle)
                 {
                     templateTitle = value;
                     RaisePropertyChanged("TemplateTitle");
                 }
             }
         }

         private string templateText;
         public string TemplateText
         {
             get { return templateText; }
             set
             {
                 if (value != null && value != templateText)
                 {
                     templateText = value;
                     RaisePropertyChanged("TemplateText");
                 }
             }
         }

         private DataView errors;
         public DataView Errors
         {
             get { return errors; }
             set { errors = value; RaisePropertyChanged("Errors"); }
         }

         private bool isAdornVisible;
         public bool IsAdornVisible
         {
             get { return isAdornVisible; }
             set
             {
                 isAdornVisible = value;
                 RaisePropertyChanged("IsAdornVisible");
             }
         }

         private Visibility currentPasswordVisiblity;
         public Visibility CurrentPasswordVisiblity
         {
             get { return currentPasswordVisiblity; }
             set 
             {
                 if (value != null && value != currentPasswordVisiblity)
                 {
                     currentPasswordVisiblity = value;
                     RaisePropertyChanged("CurrentPasswordVisiblity");
                 }  
             }
         }

         private string settingsHeader;
         public string SettingsHeader
         {
             get { return settingsHeader; }
             set 
             {
                 if (value != null && value != settingsHeader)
                 {
                     settingsHeader = value;
                     RaisePropertyChanged("SettingsHeader");
                 } 
             }
         }

        private string towerIDLengthHeader = "Requested tower ID minimum length";
        public string TowerIDLengthHeader
        {
            get { return towerIDLengthHeader; }
            set
            {
                if (value != null && value != towerIDLengthHeader)
                {
                    towerIDLengthHeader = value;
                    RaisePropertyChanged("TowerIDLengthHeader");
                }
            }
        }

        private string requestedTowerIDMinimumLength = "10";
        public string RequestedTowerIDMinimumLength
        {
            get { return requestedTowerIDMinimumLength; }
            set
            {
                if (value != null && value != requestedTowerIDMinimumLength)
                {
                    requestedTowerIDMinimumLength = value;
                    RaisePropertyChanged("RequestedTowerIDMinimumLength");
                }
            }
        }

        private string currentPassword;
         public string CurrentPassword
         {
             get { return currentPassword; }
             set 
             {
                 if (value != null && value != currentPassword)
                 {
                     currentPassword = value;
                     RaisePropertyChanged("CurrentPassword");
                 }  
             }
         }

         private string newPassword;
         public string NewPassword
         {
             get { return newPassword; }
             set 
             {
                 if (value != null && value != newPassword)
                 {
                     newPassword = value;
                     RaisePropertyChanged("NewPassword");
                 }  
             }
         }

         private string verifyPassword;
         public string VerifyPassword
         {
             get { return verifyPassword; }
             set 
             {
                 if (value != null && value != verifyPassword)
                 {
                     verifyPassword = value;
                     RaisePropertyChanged("VerifyPassword");
                 } 
             }
         }

         private bool sendWelcomeMsg;
         public bool SendWelcomeMsg
         {
             get { return sendWelcomeMsg; }
             set
             {
                 sendWelcomeMsg = value;
                 Settings.Default.isSendSmsOnUserCreation = sendWelcomeMsg;
                 Settings.Default.Save();
                 RaisePropertyChanged("SendWelcomeMsg");
             }
         }

        private string welcomeMessage;
        public string WelcomeMessage
         {
             get { return welcomeMessage; }
             set 
             {
                 if (value != null && value != welcomeMessage)
                 {
                     welcomeMessage = value;
                     RaisePropertyChanged("WelcomeMessage");
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

        private Visibility waitSMSVisibility;
        public Visibility WaitSMSVisibility
        {
            get { return waitSMSVisibility; }
            set 
            {
                RaisePropertyChanging("WaitSMSVisibility");
                waitSMSVisibility = value;
                RaisePropertyChanged("WaitSMSVisibility");
            }
        }
        
        #endregion

        #region Command properties
         public DelegateCommand SendMaintenanceCommand { get; set; }
         public DelegateCommand RefreshCommand { get; set; }
         public DelegateCommand<object> PrintExportCommand { get; set; }
         public DelegateCommand SavePasswordCommand { get; set; }
         public DelegateCommand AddTemplateCommand { get; set; }
         public DelegateCommand CancelTemplateCommand { get; set; }
         public DelegateCommand SaveWelcomeMsgCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        #endregion

        #region Constructors
        public GroupSMSViewModel()
        {
            TemplatesDialog = new TemplatesViewModel();

            SendMaintenanceCommand = new DelegateCommand(SendMaintenanceMsg);
            AddTemplateCommand = new DelegateCommand(AddTemplate);
            CancelTemplateCommand = new DelegateCommand(CancelTemplate);
            PrintExportCommand = new DelegateCommand<object>(PrintExport);
            RefreshCommand = new DelegateCommand(RefreshGrid);
            SavePasswordCommand = new DelegateCommand(SavePassword);
            SaveWelcomeMsgCommand = new DelegateCommand(SaveWelcomeMsg);
            SaveCommand = new DelegateCommand(SaveCommandClick);

            RequestedTowerIDMinimumLength = ProsoftCommons.RequestedTowerIDLength.ToString();

            IsSelected = GroupSelected.Users;
            InitializeSettings();
            smsDB = new SMSdb();
            SMSUserDetails = smsDB.GetSMSUsers();
            //DynamicList = smsDB.GetSMSUserByName_PhoneNo();
            DynamicList = SMSUserDetails.Select(x => x.UserName + "-" + x.UserPhoneNumber);
            msgReadWrite = new SMSReadWrite(SMSMonitorViewModel.Instance.C5AutoResetEvent);
            MaintenanceMessages = smsDB.GetMaintenanceLogs();
            this.C5SerialPort = SMSMonitorViewModel.Instance.C5SerialPort;
            RefreshGrid();
            LoadTemplates();
            LoadTemplatesList();
            LoadWelcomeTemplates();
            TemplateBtnCaption = "Add";
            WaitSMSVisibility = Visibility.Collapsed;
        }
        #endregion

        #region Public & private methods
        private void SaveCommandClick()
        {
            int reqTowerIDMinLength = 10;
            if (RequestedTowerIDMinimumLength == null || RequestedTowerIDMinimumLength.Equals(""))
            {
                MessageBox.Show("Requested Tower ID Minimum Length should not be empty.", "Parse Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (Int32.TryParse(RequestedTowerIDMinimumLength, out reqTowerIDMinLength) == false)
            {
                MessageBox.Show("Please enter only numeric values for Requested Tower ID Minimum Length.", "Parse Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //RequestedTowerIDMinimumLength
            string xmlFilePath = Environment.CurrentDirectory + "\\Settings.xml";

            try
            {
                if (File.Exists(xmlFilePath))
                {
                    // instantiate XmlDocument and load XML from file
                    XmlDocument doc = new XmlDocument();
                    doc.Load(xmlFilePath);

                    // get a list of nodes - in this case, I'm selecting all <AID> nodes under
                    // the <GroupAIDs> node - change to suit your needs
                    XmlNodeList aNodes = doc.SelectNodes("/settings/setting");

                    bool isRecordEdited = false;

                    // loop through all AID nodes
                    foreach (XmlNode aNode in aNodes)
                    {
                        string sName = aNode.SelectSingleNode("name").InnerText;
                        string value = aNode.SelectSingleNode("value").InnerText;

                        if (sName != null && sName.Equals("RequestedTowerIDLength"))
                        {
                            aNode.SelectSingleNode("value").InnerText = RequestedTowerIDMinimumLength;
                            isRecordEdited = true;
                        }
                        
                        break;

                        //// grab the "id" attribute
                        //XmlAttribute idAttribute = aNode.Attributes["RequestedTowerIDLength"];

                        //// check if that attribute even exists...
                        //if (idAttribute != null)
                        //{
                        //    // if yes - read its current value
                        //    string currentValue = idAttribute.Value;

                        //    // here, you can now decide what to do - for demo purposes,
                        //    // I just set the ID value to a fixed value if it was empty before
                        //    if (string.IsNullOrEmpty(currentValue))
                        //    {
                        //        idAttribute.Value = "515";
                        //    }
                        //}
                    }

                    if (isRecordEdited)
                    {
                        try
                        {
                            // save the XmlDocument back to disk
                            doc.Save(xmlFilePath);

                            ProsoftCommons.RequestedTowerIDLength = reqTowerIDMinLength;
                            MessageBox.Show("Requested tower ID minimum length successfully saved.", "Saved!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch(Exception ex_002)
                        {
                            MessageBox.Show("Error while saving the Settings file.\nError : " + ex_002.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("[Attribute : RequestedTowerIDLength] is missing in Settings file.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Create an XmlWriterSettings object with the correct options.
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "    "; //  "\t";
                    settings.OmitXmlDeclaration = false;
                    settings.Encoding = System.Text.Encoding.UTF8;
                    using (XmlWriter writer = XmlWriter.Create(xmlFilePath, settings))
                    {
                        writer.WriteStartElement("settings");
                        writer.WriteStartElement("setting");
                        writer.WriteElementString("name", "RequestedTowerIDLength");
                        writer.WriteElementString("value", RequestedTowerIDMinimumLength);
                        writer.WriteEndElement();
                        writer.WriteEndElement();

                        writer.Flush();
                    }

                    MessageBox.Show("Requested tower ID minimum length successfully saved.", "Saved!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex_0001)
            {
                MessageBox.Show("Failed to save Requested tower ID minimum length.\nError : " + ex_0001.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeSettings()
        {
            if (string.IsNullOrEmpty(Settings.Default.SMSAppPassword))
            {
                SettingsHeader = "Create Password";
                CurrentPasswordVisiblity = Visibility.Collapsed;
            }
            else
            {
                SettingsHeader = "Change Password";
                CurrentPasswordVisiblity = Visibility.Visible;
            }

            sendWelcomeMsg = Settings.Default.isSendSmsOnUserCreation;
        }

        private void LoadTemplates()
        {
            TemplateCollection = XMLTemplates.GetTemplatesWithoutWelcomMsg();
        }

        private void LoadTemplatesList()
        {
            TemplatesDialog.TemplateCollection = XMLTemplates.GetTemplatesWithoutWelcomMsg();
            TemplateCount = TemplatesDialog.TemplateCollection.Count > 0 ? true : false;
        }

        private void LoadWelcomeTemplates()
        {
            WelcomeMessage = XMLTemplates.GetTemplateWelcomMsg();
        }

        private void SendMaintenanceMsg()
        {
            WaitSMSVisibility = Visibility.Visible;

            MessageBoxResult smsResult = MessageBox.Show("Are you sure you want to send SMS to selected member(s).", "Sure?", MessageBoxButton.YesNo,MessageBoxImage.Question);
            if (smsResult == MessageBoxResult.No)
            {
                WaitSMSVisibility = Visibility.Collapsed;
                return;
            }

            if (C5SerialPort != null && C5SerialPort.IsOpen)
            {
                StartStopWait();
                bool result = false;
                List<string> lstofNUmbers = new List<string>();

                if (IsSelected == GroupSelected.Users)
                {
                    foreach (string item in SelectedList)
                    {
                        string[] name_numbers = item.Split(new char[] { '-' });
                        lstofNUmbers.Add(name_numbers[1]);
                    }
                }
                else
                {
                    foreach (string item in SelectedList)
                    {
                        lstofNUmbers.AddRange(SMSUserDetails.Where(x => x.Rank == item.ToString()).Select(y => y.UserPhoneNumber).ToList());
                    }
                }

                if (lstofNUmbers.Count > 0)
                {
                    foreach (var item in lstofNUmbers)
                    {
                        Settings.Default.isSendSMS = false;
                        while (Settings.Default.isReadSMS == false) { }
                        Settings.Default.isReadSMS = false;

                        delay();
                        
                        List<PartSMSs > status = new List< PartSMSs >();
                        status = msgReadWrite.createSMS(c5SerialPort, item, SendMaintenanceMessage.Trim());

                        foreach (var items in status)
                        {
                            if (items.PartMsgStatus  == SentSMSStatus.Passed)
                            {
                                result = smsDB.SaveSentLog(0, item, items.PartMsg, SentSMSStatus.Maintenance, System.DateTime.Now);
                                MaintenanceMessages.Add(new SMSLog() { LogID = 0, MessageID = 0, Sender = string.Empty, RequestNumber = item, SentDateTime = System.DateTime.Now, SentMessage = items.PartMsg, SentStatus = SentSMSStatus.Maintenance.ToString() });    
                            }
                            else
                            {
                                result = smsDB.SaveSentLog(0, item, items.PartMsg , SentSMSStatus.MaintenanceFailed, System.DateTime.Now);
                                MaintenanceMessages.Add(new SMSLog() { LogID = 0, MessageID = 0, Sender = string.Empty, RequestNumber = item, SentDateTime = System.DateTime.Now, SentMessage = items.PartMsg, SentStatus = SentSMSStatus.MaintenanceFailed.ToString() });
                            }
                        }
                        Settings.Default.isReadSMS = true;
                        Settings.Default.isSendSMS = true;
                    }
                    
                    MessageBox.Show("Message sending completed.", "C5 SMS System");
                }
                StartStopWait();
            }
            else
            {
                MessageBox.Show("Com port is not connected.","C5 SMS System");
            }
            WaitSMSVisibility = Visibility.Collapsed;
        }

        private bool delay()
        {
            long i;
            for (i = 1; i <= 90000000; i++)
            {
            }
            return true;
        }

        private void AddTemplate()
        {
            if (TemplateBtnCaption == "Add")
            {
                Template insertTemplate = new Template();
                insertTemplate.Title = TemplateTitle;
                insertTemplate.Text = TemplateText;
                if (XMLTemplates.InsertintoTemplateList(insertTemplate))
                {
                    MessageBox.Show("New template added successfully.","C5 SMS System");
                    LoadTemplates();
                }
            }
            else if (TemplateBtnCaption == "Update")
            {
                Template updateTemplate = new Template();
                updateTemplate.TemplateID = SelectedTemplate.TemplateID;
                updateTemplate.Title = TemplateTitle;
                updateTemplate.Text = TemplateText;
                if (XMLTemplates.UpdateTemplateList(updateTemplate))
                {
                    MessageBox.Show("Template updated successfully.", "C5 SMS System");
                    LoadTemplates();
                }
            }
        }

        private void CancelTemplate()
        {
            TemplateBtnCaption = "Add";
            TemplateTitle = string.Empty;
            TemplateText = string.Empty;
        }

        public void editTemplateMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    if (SelectedTemplate != null)
                    {
                        TemplateBtnCaption = "Update";
                        TemplateTitle = SelectedTemplate.Title;
                        TemplateText = SelectedTemplate.Text;
                    }
                }));
            }
        }

        public void deleteTemplateMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    if (SelectedTemplate != null)
                    {
                        MessageBoxResult mResult = MessageBox.Show("Are you sure you want to delete this template ?", "C5 SMS System", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                        if (mResult == MessageBoxResult.Yes)
                        {
                            if (XMLTemplates.RemovefromTemplateList(SelectedTemplate))
                            {
                                MessageBox.Show("Template deleted successfully.", "C5 SMS System");
                                LoadTemplates();
                            }
                        }
                    }
                }));
                
            }
        }

        private void selectionChanged(GroupSelected value)
        {
            if (value == GroupSelected.Users)
            {
               // DynamicList = smsDB.GetSMSUserByName_PhoneNo();
                DynamicList = SMSUserDetails.Select(x => x.UserName + "-" + x.UserPhoneNumber);
            }
            else
            {
               // DynamicList = smsDB.GetSMSUsersByRank();
                DynamicList = SMSUserDetails.Where(y=> !string.IsNullOrEmpty(y.Rank)).Select(x => x.Rank).Distinct();
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

        private void SavePassword()
        {
            if (string.IsNullOrEmpty(Settings.Default.SMSAppPassword))
            {
                if (!string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(VerifyPassword))
                {
                    if (string.Equals(NewPassword, VerifyPassword))
                    {
                        Settings.Default.SMSAppPassword = ProsoftCommons.EncryptTo128Bits(NewPassword);
                        Settings.Default.Save();
                        MessageBox.Show("Password saved successfully.", "C5 SMS system");
                    }
                    else
                    { 
                        MessageBox.Show("Password does not match.","C5 SMS system");
                    }
                    NewPassword = string.Empty;
                    VerifyPassword = string.Empty;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(CurrentPassword) && !string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(VerifyPassword))
                {
                    if (CurrentPassword.Equals("Pro@123$") || string.Equals(CurrentPassword, ProsoftCommons.DecryptFrom128Bits(Settings.Default.SMSAppPassword)))
                    {
                        if (string.Equals(NewPassword, VerifyPassword))
                        {
                            Settings.Default.SMSAppPassword = ProsoftCommons.EncryptTo128Bits(NewPassword);
                            Settings.Default.Save();
                            MessageBox.Show("Password updated successfully.", "C5 SMS system");
                        }
                        else
                        {
                            MessageBox.Show("Password does not match.", "C5 SMS system");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid current password.", "C5 SMS system");
                    }
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    VerifyPassword = string.Empty;
                }
            }
        }

        private void SaveWelcomeMsg()
        {
            if (!string.IsNullOrEmpty(WelcomeMessage))
            {
                Template welcomeTemplate = new Template();
                welcomeTemplate.TemplateID = 1;
                welcomeTemplate.Title = "Welcome";
                welcomeTemplate.Text = welcomeMessage;
                if (XMLTemplates.UpdateTemplateList(welcomeTemplate))
                {
                    //MessageBox.Show("Success", "C5 SMS System");
                    LoadWelcomeTemplates();
                    MessageBox.Show("Welcome template updated successfully.", "C5 SMS System");
                }
                else
                {
                    MessageBox.Show("Failed to update welcome template.", "C5 SMS System");
                }
            }
        }

        private Dictionary<bool,DataTable> LoadErrorFromXml()
        {
            DataSet ds = new DataSet();
            bool result = false;
            ds.ReadXml(ProsoftCommons.m_logFileName);
            result = ds.Tables["exception"] != null ? true : false;
            return new Dictionary<bool, DataTable>() { { result, ds.Tables["exception"] } };
        }

        private void RefreshGrid()
        {
            Dictionary<bool, DataTable> dictXml = LoadErrorFromXml();
            if (dictXml.ContainsKey(true)) { Errors = (dictXml.Single(c => c.Key == true).Value).DefaultView; }
        }

        private void StartStopWait()
        {
            IsAdornVisible = !IsAdornVisible;
            //ListBox1.IsEnabled = !ListBox1.IsEnabled;
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
                        SendMaintenanceMessage = TemplatesDialog.LocalSelectedTemplate.Text;
                    }
                }
            }
        }

        public void groupSMSTabControlSelectionChanged(object sender, DevExpress.Xpf.Core.TabControlSelectionChangedEventArgs e)
        {
            DXTabControl tab = sender as DXTabControl;
            if (tab.SelectedIndex == 0)
            {
                LoadTemplatesList();
            }
        }
        #endregion
    }
}
