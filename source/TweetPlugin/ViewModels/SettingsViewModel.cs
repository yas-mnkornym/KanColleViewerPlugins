using System;
using System.Windows;
using Studiotaiha.Toolkit;
using Studiotaiha.Toolkit.WPF;
using Studiotaiha.TweetPlugin.Models;
using Studiotaiha.TweetPlugin.Models.Tweeters;
using TweetSharp;

namespace Studiotaiha.TweetPlugin.ViewModels
{
	internal sealed class SettingsViewModel : BindableBase
	{

		public SettingsViewModel(Settings settings)
		{
			if (settings == null) { throw new ArgumentNullException("settings"); }
			Settings = settings;
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

		#region TwitterAuthPin
		string twitterAuthPin_ = null;
		public string TwitterAuthPin
		{
			get
			{
				return twitterAuthPin_;
			}
			set
			{
				if (SetValue(ref twitterAuthPin_, value)) {
					AuthorizeTwitterCommand.RaiseCanExecuteChanged();
				}
			}
		}
		#endregion

		#region Tweeter
		TweetSharpTweeter tweeter_ = null;
		public TweetSharpTweeter Tweeter
		{
			get
			{
				return tweeter_;
			}
			set
			{
				if (SetValue(ref tweeter_, value)) {
					OpenTwitterAuthPageCommand.RaiseCanExecuteChanged();
					AuthorizeTwitterCommand.RaiseCanExecuteChanged();
				}
			}
		}
		#endregion
		#endregion // Bindings


		OAuthRequestToken twitterReqToken_ = null;
		#region Commands
		#region OpenTwitterAuthPageCommand
		DelegateCommand openTwitterAuthPageCommand_ = null;
		public DelegateCommand OpenTwitterAuthPageCommand
		{
			get
			{
				return openTwitterAuthPageCommand_ ?? (openTwitterAuthPageCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						if (Tweeter == null) { return; }
						try {
							var ts = Tweeter.TwitterService;
							twitterReqToken_ = ts.GetRequestToken();
							Uri uri = ts.GetAuthorizationUri(twitterReqToken_);
							System.Diagnostics.Process.Start(uri.ToString());

							AuthorizeTwitterCommand.RaiseCanExecuteChanged();
						}
						catch (Exception ex) {
							ErrMsg("Twitterの認証処理に失敗しました。", ex);
						}
					},
					CanExecuteHandler = param => {
						return (Tweeter != null);
					}
				});
			}
		}
		#endregion

		#region AuthorizeTwitterCommand
		DelegateCommand authorizeTwitterCommand_ = null;
		public DelegateCommand AuthorizeTwitterCommand
		{
			get
			{
				return authorizeTwitterCommand_ ?? (authorizeTwitterCommand_ = new DelegateCommand {
					ExecuteHandler = param => {
						if (Tweeter == null) { return; }
						var ts = Tweeter.TwitterService;
						if (ts != null) {
							var pin = TwitterAuthPin;
							if (!string.IsNullOrWhiteSpace(pin)) {
								pin.Trim();
								var accessToken = ts.GetAccessToken(twitterReqToken_, pin);
								if (accessToken != null && accessToken.Token != "?") {
									ts.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);

									Settings.TwitterAccessToken = accessToken.Token;
									Settings.TwitterAccessTokenSecret = accessToken.TokenSecret;
									RaisePropertyChanged("CanAuthorizeTwitter");

									if (TwitterAuthorized != null) {
										TwitterAuthorized(this, new EventArgs());
									}
									MessageBox.Show("ツイッターの認証が完了しました!", "認証完了", MessageBoxButton.OK, MessageBoxImage.Information);
								}
								else {
									ErrMsg("Twitterの認証に失敗しました。\nPinを正しく入力してください。");
								}
							}
						}
					},
					CanExecuteHandler = param => {
						return (Tweeter != null && twitterReqToken_ != null && !string.IsNullOrWhiteSpace(TwitterAuthPin));
					}
				});
			}
		}
		#endregion
		#endregion // Bindings

		#region events
		public event EventHandler TwitterAuthorized;
		#endregion

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
