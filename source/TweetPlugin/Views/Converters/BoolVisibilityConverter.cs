using System;
using System.Windows;
using System.Windows.Data;

namespace Studiotaiha.TweetPlugin.Views.Converters
{
	internal sealed class BoolVisibilityConverter : IValueConverter
	{
		public BoolVisibilityConverter()
		{
			TrueVisibility = Visibility.Visible;
			FalseVisibility = Visibility.Collapsed;
		}

		public Visibility TrueVisibility { get; set; }
		public Visibility FalseVisibility { get; set; }
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var b = (bool)value;
			return (b ? TrueVisibility : FalseVisibility);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
