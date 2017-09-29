using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace PDTestSerial.UCT
{
    public class TestResultToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if((string)value == "PASS")
            {
                return new SolidColorBrush(Colors.LightGreen);
            }
            if ((string)value == "FAIL")
            {
                return new SolidColorBrush(Colors.Red);
            }
            if ((string)value == "TEST...")
            {
                return new SolidColorBrush(Colors.LightYellow);
            }
            if ((string)value == "NO_TEST")
            {
                return new SolidColorBrush(Colors.White);
            }
            if ((string)value == "IGNORED")
            {
                return new SolidColorBrush(Colors.LightGray);
            }
            else return new SolidColorBrush(Colors.DarkGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
