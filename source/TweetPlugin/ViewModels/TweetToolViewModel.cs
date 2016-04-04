using System;
using System.Windows;
using Studiotaiha.Toolkit;
using Studiotaiha.Toolkit.WPF;
using Studiotaiha.TweetPlugin.Models;

namespace Studiotaiha.TweetPlugin.ViewModels
{
	internal class TweetToolViewModel : BindableBase
	{
		Settings settings_ = null;

		public TweetToolViewModel(Settings settings)
		{
			if (settings == null) { throw new ArgumentNullException(); }
			settings_ = settings;
		}

		#region Bindings
		#region TweetVM
		TweetViewModel tweetVM_ = null;
		public TweetViewModel TweetVM
		{
			get
			{
				return tweetVM_;
			}
			set
			{
				SetValue(ref tweetVM_, value);
			}
		}
		#endregion // TweetVM

		#region SettingsVM
		SettingsViewModel settingsVM_ = null;
		public SettingsViewModel SettingsVM
		{
			get
			{
				return settingsVM_;
			}
			set
			{
				SetValue(ref settingsVM_, value);
			}
		}
		#endregion // SettingsVM

		#region Version
		public string Version
		{
			get
			{
				var version = System.Reflection.Assembly.GetExecutingAssembly()
					.GetName().Version;
				return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
			}
		}
		#endregion

		object windowLockObj_ = new object();
		Views.TweetWindow window_ = null;
		public void CloseWindow()
		{
			lock (windowLockObj_) {
				if (window_ != null) {
					if (window_.IsVisible) {
						window_.Close();
					}
					window_ = null;
				}
				IsWindowOpen = false;
			}
		}

		public void OpenWindow()
		{
			lock (windowLockObj_) {
				if (window_ == null) {
					window_ = new Views.TweetWindow();
					window_.Owner = Application.Current.MainWindow;
					window_.DataContext = TweetVM;
					window_.Show();
					IsWindowOpen = true;
				}
			}
		}

		#region IsWindowOpen
		bool isWindowOpen_ = false;
		public bool IsWindowOpen
		{
			get
			{
				return isWindowOpen_;
			}
			set
			{
				if (SetValue(ref isWindowOpen_, value)) {
					settings_.IsWindowOpen = value;
					if (isWindowOpen_) {
						OpenWindow();
					}
					else {
						CloseWindow();
					}
				}
			}
		}
		#endregion
		#endregion // Bindings

		#region Commands
		#region OpenLinkCommand
		DelegateCommand openLinkCommand_ = null;
		public DelegateCommand OpenLinkCommand
		{
			get
			{
				if (openLinkCommand_ == null) {
					openLinkCommand_ = new DelegateCommand {
						ExecuteHandler = param => {
							var str = param as string;
							if (!string.IsNullOrWhiteSpace(str)) {
								System.Diagnostics.Process.Start(str);
							}
						}
					};
				}
				return openLinkCommand_;
			}
		}
		#endregion

		#region OpenWindowCommand
		#endregion
		#endregion // Bindings

		#region エラーメッセージ
		void ErrMsg(string msg)
		{
			MessageBox.Show(msg, "エラー - 艦ぶらたいまープラグイン", MessageBoxButton.OK, MessageBoxImage.Stop);
		}

		void ErrMsg(string msg, Exception ex)
		{
			ErrMsg(
				string.Format("{0}\n\n【例外情報】\n{1}",
					msg, ex.ToString())
				);
		}
		#endregion
	}
}
