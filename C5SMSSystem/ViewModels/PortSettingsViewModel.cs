using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Commands;
using System.Windows;
using C5SMSSystem.Properties;

namespace C5SMSSystem.ViewModels
{
    public class PortSettingsViewModel : ViewModelBase
    {
        #region Fields
        #endregion

        #region Private members & Public properties
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        private string _baudRate = "9600";  // 115200
        public string BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; RaisePropertyChanged("BaudRate"); }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (value == _isOpen) return;
                _isOpen = value;
                RaisePropertyChanged("IsOpen");
            }
        }
        #endregion

        #region Constructors
        public PortSettingsViewModel()
        {
            OkCommand = new DelegateCommand(OkCommandClicked);
        }
        #endregion

        #region Commands
        public DelegateCommand OkCommand { get; set; }
        #endregion

        #region Private methods and command methods
        public void OkCommandClicked()
        {
            if (!string.IsNullOrEmpty(BaudRate))
            {
                int baudRate = 9600;
                bool isConverted = int.TryParse(BaudRate, out baudRate);
                if (!isConverted)
                {
                    MessageBox.Show("BaudRate should be numerical value.", "Invalid BaudRate", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Settings.Default.BaudRate = baudRate.ToString();
                    Settings.Default.Save();

                    IsOpen = false;
                }

                //if (Password.Equals("Pro@123$"))
                //{
                //    IsOpen = false;
                //}
                //else if (Password == ProsoftCommons.DecryptFrom128Bits(Properties.Settings.Default.SMSAppPassword))
                //{
                //    IsOpen = false;
                //}
            }
        }
        #endregion
    }
}
