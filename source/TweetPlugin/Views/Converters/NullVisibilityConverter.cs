using System;
using System.Windows;
using System.Windows.Data;

namespace Studiotaiha.TweetPlugin.Views.Converters
{
	internal sealed class NullVisibilityConverter : IValueConverter
	{
		public NullVisibilityConverter()
		{
			NullVisibility = Visibility.Collapsed;
			NotNullVisibility = Visibility.Visible;
		}

		public Visibility NullVisibility { get; set; }

		public Visibility NotNullVisibility { get; set; }

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return (value == null ? NullVisibility : NotNullVisibility);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
