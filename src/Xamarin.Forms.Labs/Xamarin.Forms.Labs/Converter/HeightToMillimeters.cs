using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLabs.Ioc;

namespace Xamarin.Forms.Labs.Converter
{
    public class HeightToMillimeters : IValueConverter
    {
        private static IDisplay display;
        private const double MillimetersInInch = 25.4;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ToMillimeters((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        private static IDisplay Display
        {
            get
            {
                if ((display ?? (display = Resolver.Resolve<IDevice>().Display)) == null)
                {
                    throw new InvalidOperationException("Unable to resolve display. Please set the IDevice implementation on your IoC container.");
                }

                return display;
            }
        }

        private static double ToMillimeters(double inches)
        {
            return Display.HeightRequestInInches(inches) / MillimetersInInch;
        }
    }
}
