using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace C5SMSSystem.Controls
{
    public class DialogManager : Control
    {
        Window _window;

        static DialogManager()
        {
            //this control really is a blank template - it really just contains a UI presence so it can be declaratively 
            //be added to a page.  

            //the DataContext of this control must be set to the ViewModel that you wish to display in the dialog.  Also you must 
            //configure a DataTemplate that associates the ViewModel to the View that will be shown inside this dialog
            // e.g
            //         <DataTemplate DataType="{x:Type vm:MessageWindowViewModel}">
            //              <v:MessageWindow/>
            //          </DataTemplate>
            //Usually these datatemplates are defined in a global resource library such as App.xaml
            //If this is not configured propertly instead of seeing your control - you will just see the classname in the resulting dialog
        }

        /// <summary>
        /// This is invoked when the red X is clicked or a keypress closes the window - 
        /// </summary>
        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }
        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(DialogManager), new UIPropertyMetadata(null));

        /// <summary>
        /// This should be bound to IsOpen (or similar) in the ViewModel associated with DialogManager
        /// </summary>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(DialogManager), new UIPropertyMetadata(false, IsOpenChanged));

        public static void IsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DialogManager m = d as DialogManager;
            bool newVal = (bool)e.NewValue;
            if (newVal)
                m.Show();
            else
                m.Close();
        }

        void Show()
        {
            if (_window != null) Close();

            Window w = new Window();
            _window = w;
            w.Closing += w_Closing;
            w.Owner = GetParentWindow(this);

            w.DataContext = this.DataContext;
            w.SetBinding(Window.ContentProperty, "");
            w.ShowInTaskbar = false;
            w.Title = Title;
            w.Icon = Icon;
            w.Height = DialogHeight == 0.0 ? w.Owner.ActualHeight : DialogHeight;
            w.Width = DialogWidth == 0.0 ? w.Owner.ActualWidth : DialogWidth;
            w.ResizeMode = DialogResizeMode;
            w.WindowStyle = DialogWindowStyle;
            w.WindowStartupLocation = DialogWindowStartupLocation;
            w.WindowState = DialogWindowState;
            w.ShowDialog();
        }

        void w_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_internalClose)
            {
                _externalClose = true;
                if (CloseCommand != null) CloseCommand.Execute(null);
                _externalClose = false;
            }
        }

        bool _internalClose = false;
        bool _externalClose = false;

        void Close()
        {
            _internalClose = true;

            if (!_externalClose) _window.Close();
            _window = null;
            _internalClose = false;
        }

        Window GetParentWindow(FrameworkElement current)
        {
            //if (current is Window)
            //    return current as Window;
            //else if (current.Parent is FrameworkElement)
            //    return GetParentWindow(current.Parent as FrameworkElement);
            //else
            //    return null;
            return GetVisualParent<Window>(current);
        }



        public T GetVisualParent<T>(object childObject) where T : Visual
        {
            DependencyObject child = childObject as DependencyObject;
            while ((child != null) && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }

        #region DependencyProperties that control the look of the shown dialog

        public double DialogHeight
        {
            get { return (double)GetValue(DialogHeightProperty); }
            set { SetValue(DialogHeightProperty, value); }
        }
        public static readonly DependencyProperty DialogHeightProperty =
            DependencyProperty.Register("DialogHeight", typeof(double), typeof(DialogManager));

        public double DialogWidth
        {
            get { return (double)GetValue(DialogWidthProperty); }
            set { SetValue(DialogWidthProperty, value); }
        }
        public static readonly DependencyProperty DialogWidthProperty =
            DependencyProperty.Register("DialogWidth", typeof(double), typeof(DialogManager));

        public ResizeMode DialogResizeMode
        {
            get { return (ResizeMode)GetValue(DialogResizeModeProperty); }
            set { SetValue(DialogResizeModeProperty, value); }
        }
        public static readonly DependencyProperty DialogResizeModeProperty =
            DependencyProperty.Register("DialogResizeMode", typeof(ResizeMode), typeof(DialogManager));

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(DialogManager), new UIPropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(DialogManager), new UIPropertyMetadata(null));

        public WindowStyle DialogWindowStyle
        {
            get { return (WindowStyle)GetValue(WindowStyleProperty); }
            set { SetValue(WindowStyleProperty, value); }
        }
        public static readonly DependencyProperty WindowStyleProperty =
            DependencyProperty.Register("WindowStyle", typeof(WindowStyle), typeof(DialogManager));

        public WindowStartupLocation DialogWindowStartupLocation
        {
            get { return (WindowStartupLocation)GetValue(DialogWindowStartupLocationProperty); }
            set { SetValue(DialogWindowStartupLocationProperty, value); }
        }
        public static readonly DependencyProperty DialogWindowStartupLocationProperty =
            DependencyProperty.Register("DialogWindowStartupLocation", typeof(WindowStartupLocation), typeof(DialogManager));

        public WindowState DialogWindowState
        {
            get { return (WindowState)GetValue(DialogWindowStateProperty); }
            set { SetValue(DialogWindowStateProperty, value); }
        }
        public static readonly DependencyProperty DialogWindowStateProperty =
            DependencyProperty.Register("DialogWindowState", typeof(WindowState), typeof(DialogManager));

        #endregion
    }
}
