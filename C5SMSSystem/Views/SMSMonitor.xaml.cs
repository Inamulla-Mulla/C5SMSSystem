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
using DevExpress.Xpf.Editors;

namespace C5SMSSystem
{
    /// <summary>
    /// Interaction logic for SMSMonitor.xaml
    /// </summary>
    public partial class SMSMonitor : UserControl
    {
        public SMSMonitor()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SMSMonitor_Loaded);
        }

        void SMSMonitor_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = SMSMonitorViewModel.Instance;
        }

        private void inFrmDate_Validate(object sender, RoutedEventArgs e)
        {
            DateTime from = ((DateEdit)sender).DateTime;
            if (inToDate.DateTime < from)
                inToDate.EditValue = from;
        }

        private void inToDate_Validate(object sender, RoutedEventArgs e)
        {
            DateTime to = ((DateEdit)sender).DateTime;
            if (to < inFrmDate.DateTime)
                ((DateEdit)sender).DateTime = inFrmDate.DateTime;
        }

        private void stFrmDate_LostFocus(object sender, RoutedEventArgs e)
        {
            DateTime from = ((DateEdit)sender).DateTime;
            if (stToDate.DateTime < from)
                stToDate.EditValue = from;
        }

        private void stToDate_LostFocus(object sender, RoutedEventArgs e)
        {
            DateTime to = ((DateEdit)sender).DateTime;
            if (to < stFrmDate.DateTime)
                ((DateEdit)sender).DateTime = stFrmDate.DateTime;
        }
    }
}
