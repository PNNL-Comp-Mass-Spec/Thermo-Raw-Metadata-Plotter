using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace ThermoRawMetadataPlotting
{
    public class DescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value);
        }

        public string Convert(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var inherit = true;
            var flags = BindingFlags.FlattenHierarchy | BindingFlags.Instance;

            if (value.GetType().IsEnum)
            {
                inherit = false;
                flags = BindingFlags.Default;
            }

            DescriptionAttribute desc = null;
            if (value is PropertyInfo pi)
            {
                desc = pi.GetCustomAttributes(inherit).OfType<DescriptionAttribute>().FirstOrDefault();
            }
            else
            {
                var attrib = value.GetType().GetField(value.ToString(), flags)?.GetCustomAttributes(inherit);
                desc = attrib?.OfType<DescriptionAttribute>().FirstOrDefault();

                if (desc == null)
                {
                    attrib = value.GetType().GetProperty(value.ToString(), flags)?.GetCustomAttributes(inherit);
                    desc = attrib?.OfType<DescriptionAttribute>().FirstOrDefault();
                }
            }

            if (desc == null)
            {
                return value.ToString();
            }
            else
            {
                return desc.Description;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
