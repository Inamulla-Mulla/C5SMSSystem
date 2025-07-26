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
    /// Interaction logic for PrintFormat.xaml
    /// </summary>
    public partial class PrintFormat : ResourceDictionary
    {
        public PrintFormat()
        {
            InitializeComponent();
        }

        static readonly PrintFormat instance = new PrintFormat();

        public static DataTemplate PageHeader
        {
            get
            {
                return (DataTemplate)instance["PageHeader"];
            }
        }

        public static DataTemplate PageFooter
        {
            get
            {
                return (DataTemplate)instance["PageFooter"];
            }
        }
    }
}
