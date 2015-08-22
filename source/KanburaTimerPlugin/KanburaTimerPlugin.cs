using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using Studiotaiha.KanburaTimerPlugin.Models;
using Studiotaiha.KanburaTimerPlugin.ViewModels;
using Studiotaiha.KanburaTimerPlugin.Views;
using Studiotaiha.Toolkit;
using Studiotaiha.Toolkit.WPF;
using Studiotaiha.Toolkit.WPF.Settings;

namespace Studiotaiha.KanburaTimerPlugin
{
	[Export(typeof(IPlugin))]
	[Export(typeof(ITool))]
	[ExportMetadata("Guid", "8DCBE702-5181-4D66-94EA-D8BB7FFC19F1")]
	[ExportMetadata("Title", "艦ぶらたいまープラグイン for 提督業も忙しい！")]
	[ExportMetadata("Description", "Androidアプリ艦ぶらたいまーに情報を送信するプラグインです。")]
	[ExportMetadata("Version", "0.0.3")]
	[ExportMetadata("Author", "まなよめ / スタジオ大破")]
	public class KanburaTimerPlugin
		: IPlugin, ITool, IDisposable
	{
		static ILogger Logger { get; } = LoggingService.Current.GetLogger(typeof(KanburaTimerPlugin));
		Settings settings_;
		SettingsAutoExpoter expoter_;
		KanburaTimerToolViewModel viewModel_;

		public string Name { get; } = Constants.PluginName;

		public object View
		{
			get
			{
				if (viewModel_ == null) {
					Logger.Fatal("View{get;} called before ViewModel is initialized.");
					throw new InvalidOperationException("ViewModel hasn't been initialized yet.");
				}

				return new KanburaTimerToolView {
					DataContext = viewModel_
				};
			}
		}

		public void Initialize()
		{
			KanColleClient.Current.PropertyChanged += KanColleClient_PropertyChanged;
		}

		private void KanColleClient_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (KanColleClient.Current.Homeport != null) {
				KanColleClient.Current.PropertyChanged -= KanColleClient_PropertyChanged;

				var admiral = KanColleClient.Current.Homeport.Admiral;
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

				viewModel_ = new ViewModels.KanburaTimerToolViewModel(
					KanColleClient.Current,
					settings_,
					dispatcher);
			}
		}


		bool isDisposed_ = false;

		virtual protected void Dispose(bool disposing) {
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
