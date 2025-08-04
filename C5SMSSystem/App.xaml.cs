using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace C5SMSSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void APP_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show(e.Exception.Message + Environment.NewLine +
                "Stack = " + e.Exception.StackTrace + Environment.NewLine +
                "Inner Exception = " + e.Exception.InnerException.Message +
                "Stack 2= " + e.Exception.StackTrace + Environment.NewLine +
                "Inner Exception 2 = " + e.Exception.InnerException.Message +
                "Stack 3= " + e.Exception.StackTrace + Environment.NewLine, "Error");
            Application.Current.Shutdown();
        }
    }
}
