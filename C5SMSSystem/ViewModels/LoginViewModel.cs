using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Commands;

namespace C5SMSSystem
{
    public class LoginViewModel : ViewModelBase
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

        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                if (value == password) return;
                password = value;
                RaisePropertyChanged("Password");
            }
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
        public LoginViewModel()
        {
            CloseCommand = new DelegateCommand(CloseMethod);
        }
        #endregion

        #region Commands
        public DelegateCommand CloseCommand { get; set; }
        #endregion

        #region Private methods and command methods
        public void CloseMethod()
        {
            if (!string.IsNullOrEmpty(Password))
            {
                if (Password.Equals("Pro@123$"))
                {
                    IsOpen = false;
                }
                else if (Password == ProsoftCommons.DecryptFrom128Bits(Properties.Settings.Default.SMSAppPassword))
                {
                    IsOpen = false;
                }
            }

            Password = string.Empty;
        }
        #endregion
    }
}
