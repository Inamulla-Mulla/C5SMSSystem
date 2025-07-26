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
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Editors;

namespace C5SMSSystem
{
    /// <summary>
    /// Interaction logic for UserManagement.xaml
    /// </summary>
    public partial class UserManagement : UserControl
    {
        private UserManagementViewModel viewModel;
        public UserManagement()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(UserManagement_Loaded);
        }

        void UserManagement_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = new UserManagementViewModel();
        }

        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text.Trim());
        }

        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);

        }

        private void grid_CustomUnboundColumnData(object sender, GridColumnDataEventArgs e)
        {
            if (e.Column.FieldName == "Selected")
            {
                int key = (int)e.GetListSourceFieldValue("userID");

                if (e.IsGetData)
                    e.Value = GetIsSelected(key);

                if (e.IsSetData)
                    SetIsSelected(key, (bool)e.Value);
            }
        }

        private bool GetIsSelected(int key)
        {
            bool isSelected;

            viewModel = (UserManagementViewModel)this.DataContext;
            if (viewModel.SelectedValues.TryGetValue(key, out isSelected))
                return isSelected;

            return false;
        }

        private void SetIsSelected(int key, bool value)
        {
            if (value)
                viewModel.SelectedValues[key] = value;
            else
                viewModel.SelectedValues.Remove(key);

            viewModel.RaisePropertyChanged("SelectedValues");
        }

        private void CheckEdit_EditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            bool isChecked = (bool)e.NewValue;
            if (!isChecked)
            {
                for (int i = 0; i < grdSMSDeleteUser.VisibleRowCount; i++)
                {
                    int rowHandle = grdSMSDeleteUser.GetRowHandleByVisibleIndex(i);
                    grdSMSDeleteUser.SetCellValue(rowHandle, "Selected", false);
                    grdSMSDeleteUser.RefreshRow(rowHandle);
                }
            }
            else
            {
                for (int i = 0; i < grdSMSDeleteUser.VisibleRowCount; i++)
                {
                    int rowHandle = grdSMSDeleteUser.GetRowHandleByVisibleIndex(i);
                    grdSMSDeleteUser.SetCellValue(rowHandle, "Selected", true);
                    grdSMSDeleteUser.RefreshRow(rowHandle);
                }
            }

        }

        private void view_CellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "Selected")
                ((TableView)sender).PostEditor();
        }
    }
}
