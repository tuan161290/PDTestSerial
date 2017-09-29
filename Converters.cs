using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace PDTestSerial
{
    public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PinValue)
            {
                if ((PinValue)value == PinValue.ON)
                    return new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.DarkGray);
            //throw new NotImplementedException();

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                int temp;
                if (int.TryParse(value.ToString(), out temp))
                    return temp;
                else return 0;
            }
            return 0;
        }
    }
}
