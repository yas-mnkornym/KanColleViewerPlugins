using System;
using System.Windows.Data;

namespace Studiotaiha.KanburaTimerPlugin.Views.Converters
{
	internal class BoolValueConverter : IValueConverter
	{
		public object TrueValue { get; set; }
		public object FalseValue { get; set; }

		#region IValueConverter メンバー

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var b = (bool)value;
			return (b ? TrueValue : FalseValue);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
