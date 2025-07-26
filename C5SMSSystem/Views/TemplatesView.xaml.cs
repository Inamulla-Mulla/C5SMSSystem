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

namespace C5SMSSystem.Views
{
    /// <summary>
    /// Interaction logic for TemplatesView.xaml
    /// </summary>
    public partial class TemplatesView : UserControl
    {
        public TemplatesView()
        {
            InitializeComponent();
           // this.Loaded += new RoutedEventHandler(TemplatesView_Loaded);
        }

        void TemplatesView_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new TemplatesViewModel();
        }
    }
}
