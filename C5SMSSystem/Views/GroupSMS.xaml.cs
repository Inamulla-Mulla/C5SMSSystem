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

namespace C5SMSSystem
{
    /// <summary>
    /// Interaction logic for GroupSMS.xaml
    /// </summary>
    public partial class GroupSMS : UserControl
    {
        public GroupSMS()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(GroupSMS_Loaded);
        }

        void GroupSMS_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new GroupSMSViewModel();
        }
    }
}
