using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Studiotaiha.Toolkit;
using Studiotaiha.Toolkit.WPF;
using Studiotaiha.TweetPlugin.Models;
using Studiotaiha.TweetPlugin.Models.Tweeters;

namespace Studiotaiha.TweetPlugin.ViewModels
{
	class TweetViewModel : BindableBase, IDisposable
	{
		CompositeDisposable disposables_;
		LinkedList<ImageFile> files_ = new LinkedList<ImageFile>();

		public TweetViewModel(Settings settings)
		{
			if (settings == null) { throw new ArgumentNullException("settings"); }
			Settings = settings;
		}

		public void StartMonitoring()
		{
			if (disposables_ == null) {
				disposables_ = new CompositeDisposable();

				UpdateImageList();

				var idisp = Observable.FromEventPattern<PropertyChangedEventArgs>(Grabacr07.KanColleViewer.Models.StatusService.Current, nameof(Grabacr07.KanColleViewer.Models.StatusService.Current.PropertyChanged))
					.Subscribe(x => Status_PropertyChanged(x.Sender, x.EventArgs));
				disposables_.Add(idisp);

				idisp = Grabacr07.KanColleViewer.Models.Settings.ScreenshotSettings.Destination
					.Subscribe(value => UpdateImageList());
				disposables_.Add(idisp);

				idisp = Observable.FromEventPattern<PropertyChangedEventArgs>(Settings, nameof(Settings.PropertyChanged))
					.Subscribe(x => Settings_PropertyChanged(x.Sender, x.EventArgs));
				disposables_.Add(idisp);
			}
		}

		private void Destination_ValueChanged(object sender, MetroTrilithon.Serialization.ValueChangedEventArgs<string> e)
		{
			UpdateImageList();
		}

		public void StopMonitoring()
		{
			if (disposables_ != null) {
				disposables_.Dispose();
				disposables_ = null;
			}
		}

		void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case nameof(Settings.HashTag):
				case nameof(Settings.AppendPicture):
				case nameof(Settings.AppendHashTag):
				case nameof(Settings.UseMisakuraTweet):
					UpdateTextLength();
					break;
			}
		}

		void Status_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var msg = Grabacr07.KanColleViewer.Models.StatusService.Current.Message;
			if (string.IsNullOrWhiteSpace(msg)) { return; }

			var key = Grabacr07.KanColleViewer.Properties.Resources.Screenshot_Saved;
			if (msg.StartsWith(key)) {
				var dir = Grabacr07.KanColleViewer.Models.Settings.ScreenshotSettings.Destination.Value;
				var filename = msg.Substring(key.Length);
				var path = Path.Combine(dir, filename);
				AddImageFile(path);
				if (Settings.AlwaysUseLatestPicture) {
					TweetImage = path;
				}
			}
		}

		public void UpdateImageList()
		{
			var dir = Grabacr07.KanColleViewer.Models.Settings.ScreenshotSettings.Destination.Value;
			if (string.IsNullOrWhiteSpace(dir)) { return; }
			if (!Directory.Exists(dir)) { return; }

			var files = Directory.GetFiles(dir, "KanColle-*.jpg").Concat(Directory.GetFiles(dir, "KanColle-*.png"));
			var imageFiles = files.Where(x => File.Exists(x))
				.Select(x => new ImageFile { Path = x, WriteTime = File.GetLastWriteTime(x) })
				.OrderBy(x => x.WriteTime);
			files_ = new LinkedList<ImageFile>(imageFiles);
			Dispatch(() => {
				TweetImage = files.LastOrDefault(); // 内部でボタンの確認するからRaise～は不要
			});
		}

		void AddImageFile(string path)
		{
			if (path == null) { throw new ArgumentNullException("path"); }
			files_.AddLast(new ImageFile { Path = path, WriteTime = File.GetLastWriteTime(path) }); ;

			Dispatch(() => {
				LatestCaptureCommand.RaiseCanExecuteChanged();
				NextCaptureCommand.RaiseCanExecuteChanged();
				PreviousCaptureCommand.RaiseCanExecuteChanged();
			});
		}

		void UpdateTextLength()
		{
			if (Tweeter != null) {
				var tweeter = Settings.UseMisakuraTweet ?
					new MisakuraTweeter(Tweeter) : Tweeter;
				Dispatch(() => {
					TextLength = tweeter.CalcurateTweetLength(
									TweetText,
									Settings.AppendHashTag ? Settings.HashTag : null,
									Settings.AppendPicture ? TweetImage : null);
				});
			}
		}

		#region Bindings
		#region Settings
		Settings settings_ = null;
		public Settings Settings
		{
			get
			{
				return settings_;
			}
			set
			{
				SetValue(ref settings_, value);
			}
		}
		#endregion

		#region IsTweeting
		bool isTweeting_ = false;
		public bool IsTweeting
		{
			get
			{
				return isTweeting_;
			}
			set
			{
				if (SetValue(ref isTweeting_, value)) {
					TweetCommand.RaiseCanExecuteChanged();
				}
			}
		}
		#endregion

		#region TweetText
		string tweetText_ = null;
		public string TweetText
		{
			get
			{
				return tweetText_;
			}
			set
			{
				if (SetValue(ref tweetText_, value)) {
					UpdateTextLength();
				}
			}
		}
		#endregion

		#region TextLength
		int textLength_ = 0;
		public int TextLength
		{
			get
			{
				return textLength_;
			}
			set
			{
				SetValue(ref textLength_, value);
			}
		}
		#endregion

		#region ImageFileName
		string imageFileName_ = null;
		public string ImageFileName
		{
			get
			{
				return imageFileName_;
			}
			set
			{
				SetValue(ref imageFileName_, value);
			}
		}
		#endregion

		#region TweetImage
		string tweetImage_ = null;
		public string TweetImage
		{
			get
			{
				return tweetImage_;
			}
			set
			{
				if (SetValue(ref tweetImage_, value)) {
					if (tweetImage_ != null) {
						ImageFileName = Path.GetFileName(tweetImage_);
					}

					LatestCaptureCommand.RaiseCanExecuteChanged();
					NextCaptureCommand.RaiseCanExecuteChanged();
					PreviousCaptureCommand.RaiseCanExecuteChanged();
				}
			}
		}
		#endregion

		#region Tweeter
		ITweeter tweeter_ = null;
		public ITweeter Tweeter
		{
			get
			{
				return tweeter_;
			}
			set
			{
				if (SetValue(ref tweeter_, value)) {
					TweetCommand.RaiseCanExecuteChanged();
				}
			}
		}
		#endregion
		#endregion // Bindings

		#region Commands
		#region TweetCommand
		DelegateCommand tweetCommand_ = null;
		public DelegateCommand TweetCommand
		{
			get
			{
				return tweetCommand_ ?? (tweetCommand_ = new DelegateCommand {
					ExecuteHandler = async param => {
						if (Tweeter == null) { return; }
						var tweeter = Tweeter;
						if (Settings.UseMisakuraTweet) {
							tweeter = new MisakuraTweeter(Tweeter);
						}

						IsTweeting = true;
						try {
							await Task.Run(() => {
								tweeter.Tweet(
									TweetText,
									Settings.AppendHashTag ? Settings.HashTag : null,
									Settings.AppendPicture ? TweetImage : null);
							});
							if (Settings.ClearTextWhenTweeted) {
								TweetText = "";
							}
						}
						catch (FileNotFoundException) {
							MessageBox.Show(
								string.Format("ツイートに失敗しました。\n\n{0}", "画像の読み込みに失敗浸ました。"),
								"ツイート失敗", MessageBoxButton.OK, MessageBoxImage.Stop);
						}
						catch (Exception ex) {
							MessageBox.Show(
								string.Format("ツイートに失敗しました。\n\n【例外情報】\n{0}", ex.ToString()),
								"ツイート失敗", MessageBoxButton.OK, MessageBoxImage.Stop);
						}
						finally {
							IsTweeting = false;
						}
					},
					CanExecuteHandler = param => {
						return (!IsTweeting && Tweeter != null);
					}
				});
			}
		}
		#endregion

		#region PreviousCaptureCommand
		DelegateCommand previousCaptureCommand_ = null;
		public DelegateCommand PreviousCaptureCommand
		{
			get
			{
				return previousCaptureCommand_ ?? (previousCaptureCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						var current = files_.FirstOrDefault(x => x.Path == TweetImage);
						ImageFile file = null;
						if (current == null) {
							file = files_.LastOrDefault();
						}
						else {
							file = files_.LastOrDefault(x => x.WriteTime < current.WriteTime);
						}
						if (file != null) {
							TweetImage = file.Path;
						}
					},
					CanExecuteHandler = param => {
						var file = files_.FirstOrDefault(x => x.Path == TweetImage);
						if (file == null) { return true; }
						else {
							return files_.Any(x => x.WriteTime < file.WriteTime);
						}
					}
				});
			}
		}
		#endregion

		#region NextCaptureCommand
		DelegateCommand nextCaptureCommand_ = null;
		public DelegateCommand NextCaptureCommand
		{
			get
			{
				return nextCaptureCommand_ ?? (nextCaptureCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						var current = files_.FirstOrDefault(x => x.Path == TweetImage);
						ImageFile file = null;
						if (current == null) {
							file = files_.FirstOrDefault();
						}
						else {
							file = files_.FirstOrDefault(x => x.WriteTime > current.WriteTime);
						}
						if (file != null) {
							TweetImage = file.Path;
						}
					},
					CanExecuteHandler = param => {
						var file = files_.FirstOrDefault(x => x.Path == TweetImage);
						if (file == null) { return true; }
						else {
							return files_.Any(x => x.WriteTime > file.WriteTime);
						}
					}
				});
			}
		}
		#endregion

		#region LatestCaptureCommand
		DelegateCommand latestCaptureCommand_ = null;
		public DelegateCommand LatestCaptureCommand
		{
			get
			{
				return latestCaptureCommand_ ?? (latestCaptureCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						var file = files_.LastOrDefault();
						if (file != null) {
							TweetImage = file.Path;
						}
					},
					CanExecuteHandler = param => {
						return (files_.Any());
					}
				});
			}
		}

		public DelegateCommand TweetCommand_
		{
			get
			{
				return tweetCommand_;
			}

			set
			{
				tweetCommand_ = value;
			}
		}
		#endregion
		#endregion // Commands


		#region IDisposable メンバ
		bool isDisposed_ = false;
		virtual protected void Dispose(bool disposing)
		{
			if (isDisposed_) { return; }
			if (disposing) {
				// Write your own disposing code here.
				if (disposables_ != null) {
					disposables_.Dispose();
					disposables_ = null;
				}
			}
			isDisposed_ = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}

	class ImageFile
	{
		public string Path { get; set; }
		public DateTime WriteTime { get; set; }
	}
}
