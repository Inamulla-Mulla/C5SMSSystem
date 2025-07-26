using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace C5SMSSystem
{
    public class ExpHeaderResizeConverter:IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double width = (double)value;
            double diff = -150.0;
            if (parameter != null)
            {
                Double.TryParse(parameter.ToString(), out diff);
            }
            return width + diff;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double width = (double)value;
            double diff = -150.0;
            if (parameter != null)
            {
                Double.TryParse(parameter.ToString(), out diff);
            }
            return width - diff;
        }

        #endregion
    }
}
