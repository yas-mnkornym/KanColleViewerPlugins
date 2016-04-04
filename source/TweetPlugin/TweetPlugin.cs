using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Grabacr07.KanColleViewer.Composition;
using Studiotaiha.Toolkit;
using Studiotaiha.Toolkit.WPF;
using Studiotaiha.Toolkit.WPF.Settings;
using Studiotaiha.TweetPlugin.Models;
using Studiotaiha.TweetPlugin.Models.Tweeters;
using Studiotaiha.TweetPlugin.ViewModels;
using Studiotaiha.TweetPlugin.Views;

namespace Studiotaiha.TweetPlugin
{
	[Export(typeof(IPlugin))]
	[Export(typeof(ITool))]
	[ExportMetadata("Guid", "3E245A04-9EF4-44DF-9880-D85C5D964F78")]
	[ExportMetadata("Title", "Tweetプラグイン for 提督業も忙しい！")]
	[ExportMetadata("Description", "艦ぶらたいまーにタイマー情報を送信します。")]
	[ExportMetadata("Version", "1.0.3")]
	[ExportMetadata("Author", "まなよめ / スタジオ大破")]
	public class TweetPlugin
		: IPlugin, ITool
	{
		static ILogger Logger { get; } = LoggingService.Current.GetLogger(typeof(TweetPlugin));
		Settings settings_;
		SettingsAutoExpoter expoter_;
		TweetToolViewModel toolViewModel_;


		public string Name => Constants.ToolPluginTitle;

		public object View => new TweetToolView { DataContext = ToolViewModel };

		TweetToolViewModel ToolViewModel
		{
			get
			{
				if (toolViewModel_ == null) {
					if (settings_ == null) { throw new InvalidOperationException("settings is not initialized."); }

					// initialize tool view model
					var tweeter = settings_.IsTwitterAuthorized ?
						new TweetSharpTweeter(Constants.TwitterConsumerKey, Constants.TwitterConsumerSecret, settings_.TwitterAccessToken, settings_.TwitterAccessTokenSecret)
						:
						new TweetSharpTweeter(new TweetSharp.TwitterService(Constants.TwitterConsumerKey, Constants.TwitterConsumerSecret));

					var tweetVm = new TweetViewModel(settings_);
					if (settings_.IsTwitterAuthorized) { tweetVm.Tweeter = tweeter; }

					var settingsVm = new SettingsViewModel(settings_) { Tweeter = tweeter };
					settingsVm.TwitterAuthorized += (_, __) => {
						settings_.IsTwitterAuthorized = true;
						tweetVm.Tweeter = tweeter;
					};

					toolViewModel_ = new TweetToolViewModel(settings_) {
						TweetVM = tweetVm,
						SettingsVM = settingsVm,
						IsWindowOpen = settings_.IsWindowOpen
					};
					toolViewModel_.TweetVM.StartMonitoring();
				}
				return toolViewModel_;
			}
		}

		public void Initialize()
		{
			var dispatcher = new WPFDispatcher(Application.Current.Dispatcher);

			// Initialize Settings
			var settingsImpl = new SettingsImpl((string)null);
			settings_ = new Settings(settingsImpl, dispatcher);

			// Load settings from file if it exists
			if (File.Exists(PluginInfo.SettingsFilePath)) {
				try {
					using (var fs = new FileStream(PluginInfo.SettingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
						var serializer = new DataContractSettingsSerializer();
						serializer.Deserialize(fs, settingsImpl);
					}
				}
				catch (Exception ex) {
					Logger.Error("Failed to load settings from file.", ex);
					if (ex.IsCritical()) { throw; }
				}
			}

			// Initialize settings auto expoter
			expoter_ = new SettingsAutoExpoter(
				PluginInfo.SettingsFilePath, PluginInfo.SettingsFilePath + ".temp",
				settingsImpl, new DataContractSettingsSerializer());

		}


		bool isDisposed_ = false;

		virtual protected void Dispose(bool disposing)
		{
			if (isDisposed_) { return; }
			if (disposing) {
				if (expoter_ != null) {
					expoter_.Dispose();
					expoter_ = null;
				}
			}
			isDisposed_ = true;
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(true);
		}
	}
}
