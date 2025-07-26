using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;
using System.Globalization;

namespace C5SMSSystem
{
    public class CollectionToIsCheckedConverter : IMultiValueConverter
    {
        private UserManagementViewModel dataContext;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ICollection collection = values[0] as ICollection;
            dataContext = values[1] as UserManagementViewModel;

            if (collection == null || dataContext == null)
                throw new NotSupportedException();

            if (collection.Count == 0)
                return false;

            if (collection.Count == dataContext.SMSUsers.Count)
                return true;

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return null;
            //throw new NotSupportedException();
        }
    }
}
