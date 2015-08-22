using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Grabacr07.KanColleWrapper;
using Kanbura.KanburaTimerClient;
using Studiotaiha.Toolkit;
using Studiotaiha.Toolkit.WPF;

namespace Studiotaiha.KanburaTimerPlugin.ViewModels
{
	class KanburaTimerToolViewModel : BindableBase
	{
		static ILogger Logger { get; } = LoggingService.Current.GetLogger(typeof(KanburaTimerToolViewModel));
		KanColleClient kanColleClient_ = null;
		KanburaTimerClient timerClient_ = null;
		Models.KanburaTimerSender sender_ = null;

		public KanburaTimerToolViewModel(
			KanColleClient kancolleClient,
			Models.Settings settings,
			IDispatcher dispatcher)
			: base(dispatcher)
		{
			if (kancolleClient == null) { throw new ArgumentNullException("kancolleClient"); }
			if (settings == null) { throw new ArgumentNullException("settings"); }
			kanColleClient_ = kancolleClient;
			Settings = settings;

			Settings.PropertyChanged += Settings_PropertyChanged;
			UpdateAdmiralInfo();
			if (Settings.IsAuthorized) {
				StartSending();
			}
		}

		void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == GetMemberName(() => Settings.AdmiralID)) {
				Settings.IsAuthorized = false;
				Settings.Password = null;
				StopSending();
			}
			RegisterCommand.RaiseCanExecuteChanged();
		}

		#region Monitoring
		/// <summary>
		/// Starts monitoring data changing
		/// </summary>
		public void StartMonitoring()
		{

			kanColleClient_.Homeport.Admiral.PropertyChanged += Admiral_PropertyChanged;
			kanColleClient_.Homeport.PropertyChanged += Admiral_PropertyChanged;
		}

		/// <summary>
		/// Stops monitoring data changing
		/// </summary>
		public void StopMonitoring()
		{
			kanColleClient_.Homeport.Admiral.PropertyChanged -= Admiral_PropertyChanged;
			kanColleClient_.Homeport.PropertyChanged -= Admiral_PropertyChanged;
		}
		#endregion

		#region Sending
		public bool IsSendingTimer
		{
			get
			{
				return (sender_ != null);
			}
		}

		void StartSending(bool doRegistration = false)
		{
			if (IsSendingTimer) { return; }
			if (!Settings.IsAuthorized && !doRegistration) { return; }
			timerClient_ = new KanburaTimerClient(
				Constants.KanburaTimerUri,
				Settings.AdmiralID,
				doRegistration ? TimerSharingPassword : Settings.Password,
				Constants.KanburaTimerAgent);
			if (doRegistration) {
				timerClient_.Register(
					Settings.UserName);
				Settings.IsAuthorized = true;
			}
			sender_ = new Models.KanburaTimerSender(
				kanColleClient_,
				timerClient_,
				Settings.TimerSendDelay);
			sender_.SendFailed += sender__SendFailed;
			sender_.TimerSent += sender__TimerSent;
			sender_.StartSending();
			if (doRegistration) {
				sender_.SendImmediately();
			}
		}

		void StopSending()
		{
			if (!IsSendingTimer) { return; }
			timerClient_.Dispose();
			timerClient_ = null;
			sender_.Dispose();
			sender_ = null;
		}
		#endregion

		#region タイマーセンダーのイベントハンドラ
		void sender__TimerSent(object sender, EventArgs e)
		{
			SetMessage("艦ぶらたいまーに情報を送信しました。");
		}

		void sender__SendFailed(object sender, Models.SendFailedEventArgs e)
		{
			SetMessage(
				string.Format("艦ぶらタイマーへの情報送信に失敗しました。({0})", e.Message)
				);
		}
		#endregion // タイマーセンダーのイベントハンドラ

		#region 艦これデータのイベントハンドラ
		void Admiral_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			UpdateAdmiralInfo();
		}
		#endregion

		/// <summary>
		/// Update admiral info (Admiral ID & User Name)
		/// </summary>
		void UpdateAdmiralInfo()
		{
			Settings.AdmiralID = long.Parse(kanColleClient_.Homeport.Admiral.MemberId);
			Settings.UserName = kanColleClient_.Homeport.Admiral.Nickname;
		}

		#region ログ
		public void SetMessage(string str)
		{
			var now = DateTime.Now;
			LogMessage = string.Format("{0}/{1}/{2} {3:D2}:{4:D2}:{5:D2}.{6:D3}  {7}",
				now.Year, now.Month, now.Day,
				now.Hour, now.Minute, now.Second, now.Millisecond,
				str);
		}
		#endregion

		#region Bindings
		#region Settings
		Models.Settings settings_ = null;
		public Models.Settings Settings
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


		#region TimerSharingPassword
		string timerSharingPassword_ = null;
		public string TimerSharingPassword
		{
			get
			{
				return timerSharingPassword_;
			}
			set
			{
				if (SetValue(ref timerSharingPassword_, value)) {
					RegisterCommand.RaiseCanExecuteChanged();
				}
			}
		}
		#endregion

		#region LogMessage
		string logMessage_ = null;
		public string LogMessage
		{
			get
			{
				return logMessage_;
			}
			set
			{
				SetValue(ref logMessage_, value);
			}
		}
		#endregion
		#endregion

		#region Commands
		#region SendImmediatelyCommand
		DelegateCommand sendImmediatelyCommand_ = null;
		public DelegateCommand SendImmediatelyCommand
		{
			get
			{
				if (sendImmediatelyCommand_ == null) {
					sendImmediatelyCommand_ = new DelegateCommand {
						ExecuteHandler = param => {
							if (sender_ != null) {
								try {
									sender_.SendImmediately();
								}
								catch (Exception ex) {
									Logger.Error("Failed sending timer information.", ex);
									if (ex.IsCritical()) { throw; }

									ErrMsg("タイマー情報の即時送信に失敗しました。", ex);
								}
							}
						}
					};
				}
				return sendImmediatelyCommand_;
			}
		}
		#endregion

		#region RegisterCommand
		DelegateCommand registerCommand_ = null;
		public DelegateCommand RegisterCommand
		{
			get
			{
				if (registerCommand_ == null) {
					registerCommand_ = new DelegateCommand {
						ExecuteHandler = param => {
							if (Settings.IsAuthorized) {
								var ret = MessageBox.Show("すでに艦ぶらたいまーに登録されています。\n再登録しますか？",
									"確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
								if (ret != MessageBoxResult.Yes) {
									return;
								}
							}

							try {
								StartSending(true);
								Settings.Password = TimerSharingPassword;
								SetMessage("艦ぶらたいまーへの登録に成功しました。");
								MessageBox.Show(
									string.Format("艦ぶらたいまーの登録に成功しました！\n以下の情報を用いてAndroidアプリ「艦ぶらたいまー」を認証してください。\n提督ID: {0}\nパスワード: {1}",
									Settings.AdmiralID, Settings.Password)
									);
								TimerSharingPassword = null;
							}
							catch (Exception ex) {
								Logger.Error("Failed sending timer info to server.", ex);
								if (ex.IsCritical()) { throw; }

								SetMessage("艦ぶらタイマーへの登録に失敗しました。");
								ErrMsg("艦ぶらたいまーへの登録に失敗しました。", ex);
							}
						},
						CanExecuteHandler = param => {
							return (
								Settings != null &&
								Settings.AdmiralID != 0 &&
								!string.IsNullOrWhiteSpace(timerSharingPassword_) &&
								!string.IsNullOrWhiteSpace(settings_.UserName)
								);
						}
					};
				}
				return registerCommand_;
			}
		}
		#endregion
		#endregion // Commands

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
