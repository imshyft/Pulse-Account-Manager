using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Studio.Helpers.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    [ValueConversion(typeof(Range), typeof(bool))]
    public class RangeToBooleanConverter : IMultiValueConverter
    {
        #region IValueConverter Members


        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(x => x == DependencyProperty.UnsetValue))
                return DependencyProperty.UnsetValue;

            if (values[0] is not Range range)
            {
                Debug.WriteLine(values[0].GetType());
                return DependencyProperty.UnsetValue;
            }
                //throw new InvalidOperationException("First parameter must be a range");
                
                
            if (values[1] is not int value)
                throw new InvalidOperationException("Second parameter must be an int");

            return value >= range.Start.Value && value <= range.End.Value;
        }


            
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
