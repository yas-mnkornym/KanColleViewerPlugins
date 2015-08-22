using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Grabacr07.KanColleWrapper;
using Kanbura.KanburaTimerClient;

namespace Studiotaiha.KanburaTimerPlugin.Models
{
	public class KanburaTimerSender : IDisposable
	{
		KanburaTimerClient client_ = null;
		KanColleClient proxy_ = null;
		Timer timer_ = null;
		bool sending_ = false;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="kancolleData">艦これデータ</param>
		/// <param name="kanburaTimerUri">艦ぶらたいまーのサーバホスト名</param>
		/// <param name="userId">提督iD</param>
		/// <param name="password">パスワード</param>
		public KanburaTimerSender(
			KanColleClient kancolleClient,
			KanburaTimerClient client,
			int delay = 10000)
		{
			if (kancolleClient == null) { throw new ArgumentNullException("proxy"); }
			if (client == null) { throw new ArgumentNullException("client"); }
			if (delay < 0) { throw new ArgumentOutOfRangeException("delay", "delay must be > 0"); }
			proxy_ = kancolleClient;
			client_ = client;
			
			// 送信用タイマを作成
			timer_ = new Timer(delay);
			timer_.AutoReset = false;
			timer_.Elapsed += SendTimerElapsed;
		}

		Grabacr07.KanColleViewer.ViewModels.Contents.Fleets.FleetsViewModel fleets_ = null;

		void RegisterFleetChanges()
		{
			foreach (var fleet in fleets_.Fleets) {
				fleet.Expedition.PropertyChanged += (s, e) => {
					if (e.PropertyName == "ReturnTime" || e.PropertyName == "Mission" || e.PropertyName == "IsInExecution") {
						EnqueTimers();
					}
				};
			}
		}

		/// <summary>
		/// 送信を開始する
		/// </summary>
		public void StartSending()
		{
			fleets_ = new Grabacr07.KanColleViewer.ViewModels.Contents.Fleets.FleetsViewModel();
			fleets_.PropertyChanged += (s, e) => {
				if (e.PropertyName == "Fleets") {
					RegisterFleetChanges();
				}
			};
			RegisterFleetChanges();


			proxy_.Homeport.Repairyard.PropertyChanged += Repairyard_PropertyChanged;
			proxy_.Homeport.Organization.PropertyChanged += Organization_PropertyChanged;
			proxy_.Homeport.Dockyard.PropertyChanged += Dockyard_PropertyChanged;
			foreach (var dock in proxy_.Homeport.Repairyard.Docks.Values) {
				dock.PropertyChanged += (s, e) => {
					if (e.PropertyName != "Remaining") {
						EnqueTimers();
					}
				};
			}
			foreach (var dock in proxy_.Homeport.Dockyard.Docks.Values) {
				dock.PropertyChanged += (s, e) => {
					if (e.PropertyName != "Remaining") {
						EnqueTimers();
					}
				};
			}
			foreach (var fleet in proxy_.Homeport.Organization.Fleets.Values) {
				fleet.PropertyChanged += (s, e) => {
					if (e.PropertyName == "State") {
						EnqueTimers();
					}
				};
			}

		}

		/// <summary>
		/// 送信を停止する
		/// </summary>
		public void StopSending()
		{
			proxy_.Homeport.Repairyard.PropertyChanged -= Repairyard_PropertyChanged;
			proxy_.Homeport.Organization.PropertyChanged -= Organization_PropertyChanged;
			proxy_.Homeport.Dockyard.PropertyChanged -= Dockyard_PropertyChanged;
		}

		void Dockyard_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Docks") {
				EnqueTimers();
				foreach (var dock in proxy_.Homeport.Dockyard.Docks.Values) {
					dock.PropertyChanged += (s, ee) => {
						if (e.PropertyName != "Remaining") {
							EnqueTimers();
						}
					};
				}
			}
		}

		void Organization_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Fleets") {
				EnqueTimers();
				foreach (var fleet in proxy_.Homeport.Organization.Fleets.Values) {
					fleet.PropertyChanged += (s, ee) => {
						if (e.PropertyName == "State") {
							EnqueTimers();
						}
					};
				}
			}
		}

		void Repairyard_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Docks") {
				EnqueTimers();
				foreach (var dock in proxy_.Homeport.Repairyard.Docks.Values) {
					dock.PropertyChanged += (s, ee) => {
						if (e.PropertyName != "Remaining") {
							EnqueTimers();
						}
					};
				}
			}
		}

		List<TimerInfo> prevTimers_ = new List<TimerInfo>();
		/// <summary>
		/// 全てのタイマ情報をキューに追加する
		/// </summary>
		void EnqueTimers()
		{
			var expeditionTimers = proxy_.Homeport.Organization.Fleets.Values
				.Select(x => CreateExpeditionTimerInfo(x));
			var repairTimers = proxy_.Homeport.Repairyard.Docks.Values
				.Select(x => CreateRepairDockTimerInfo(x));
			var shipbuildingTimers = proxy_.Homeport.Dockyard.Docks.Values
				.Select(x => CreateShipbuildingTiemrInfo(x));

			var timers = expeditionTimers.Concat(repairTimers).Concat(shipbuildingTimers)
				.Where(x => x != null);
			var sendTimers = timers.Where(x => !prevTimers_.Any(y =>
					x.Tag == y.Tag &&
					x.Type == y.Type &&
					x.CompleteTime == y.CompleteTime &&
					x.IsTitleEnabled == y.IsTitleEnabled &&
					x.TimerTitle == y.TimerTitle));
			foreach (var timer in sendTimers) {
				EnqueTimer(timer, true);
			}
			prevTimers_.Clear();
			prevTimers_.AddRange(timers);
		}


		/// <summary>
		/// 即座にタイマー情報を送信する。
		/// </summary>
		public void SendImmediately()
		{
			Task.Factory.StartNew(() => {
				try {
					// タイマをすべて追加
					EnqueTimers();

					// 送信
					try {
						SendTimerImpl();
					}
					catch (Exception ex) {
						throw new Exception("送信処理に失敗しました。", ex);
					}
				}
				catch (Exception ex) {
					OnSendFailed("タイマーの強制送信に失敗しました。", ex);
				}
			});
		}

		#region 内部実装

		#region タイマー情報生成
		TimerInfo CreateShipbuildingTiemrInfo(Grabacr07.KanColleWrapper.Models.BuildingDock dock)
		{
			if (dock.State == Grabacr07.KanColleWrapper.Models.BuildingDockState.Locked) { return null; }
			TimerInfo timer = new TimerInfo(dock.Id) {
				Type = TimerType.Shipbuilding,
				IsTitleEnabled = true
			};
			if (dock.Ship != null) {
				timer.TimerTitle = string.Format("{0} {1}",
					dock.Ship.ShipType.Name,
					dock.Ship.Name);
			}
			if (dock.CompleteTime.HasValue) {
				timer.CompleteDate = dock.CompleteTime.Value.LocalDateTime;
			}
			return timer;
		}

		TimerInfo CreateRepairDockTimerInfo(Grabacr07.KanColleWrapper.Models.RepairingDock dock)
		{
			if (dock.State == Grabacr07.KanColleWrapper.Models.RepairingDockState.Locked) { return null; }
			var timer = new TimerInfo(dock.Id) {
				Type = TimerType.Repair,
				IsTitleEnabled = true,
			};

			if (dock.Ship != null) {
				timer.TimerTitle = string.Format("{0} Lv.{1}",
					dock.Ship.Info.Name,
					dock.Ship.Level);
			}
			if (dock.CompleteTime.HasValue) {
				timer.CompleteDate = dock.CompleteTime.Value.LocalDateTime;
			}
			return timer;
		}

		TimerInfo CreateExpeditionTimerInfo(Grabacr07.KanColleWrapper.Models.Fleet fleet)
		{
			if (fleet.Id == 1) { return null; }
			TimerInfo timer = new TimerInfo(
				TimerType.Expedition,
				fleet.Id);

			var expedition = fleet.Expedition;
			if (expedition != null && expedition.Mission != null) {
				timer.TimerTitle = expedition.Mission.Title;
				timer.IsTitleEnabled = true;
				if (expedition.ReturnTime.HasValue) {
					timer.CompleteDate = expedition.ReturnTime.Value.LocalDateTime;
				}
			}
			return timer;
		}
		#endregion

		#region 非同期送信処理
		LinkedList<TimerInfo> timerQueue_ = new LinkedList<TimerInfo>();
		object lockObj_ = new object();

		/// <summary>
		/// 送信キューに加える
		/// </summary>
		/// <param name="info"></param>
		void EnqueTimer(TimerInfo info, bool resetTimer = true)
		{
			if (info == null) { throw new ArgumentNullException("info"); }

			// タイマを停止
			if (resetTimer) {
				timer_.Stop();
			}

			// キューに追加
			lock (lockObj_) {
				// 同じのあったら消す
				var item = timerQueue_.FirstOrDefault(x => x.Tag == info.Tag && x.Type == info.Type);
				if (item != null) {
					timerQueue_.Remove(item);
				}

				// 追加
				timerQueue_.AddLast(info);
			}

			// タイマを再開
			if (resetTimer) {
				timer_.Start();
			}
		}

		/// <summary>
		/// 送信キューを空にする
		/// </summary>
		void ClearQueue()
		{
			lock (lockObj_) {
				timerQueue_.Clear();
			}
		}

		/// <summary>
		/// 送信タイマ経過
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void SendTimerElapsed(object sender, ElapsedEventArgs e)
		{
			try {
				SendTimerImpl();
			}
			catch (Exception ex) {
				OnSendFailed("タイマーの自動送信に失敗しました。", ex);
			}
		}

		/// <summary>
		/// 送信実装
		/// </summary>
		void SendTimerImpl()
		{
			// まずタイマを停止
			timer_.Stop();

			lock (lockObj_) {
				if (sending_) { return; }
				else {
					sending_ = true;
				}
			}

			// キューの中身を掘り出す
			TimerInfo[] timers = null;
			lock (lockObj_) {
				timers = timerQueue_.ToArray();
				timerQueue_.Clear();
			}

			// クライアントで送信
			try {
				client_.SendTimers(timers.ToArray());
				OnTimerSent();
				if (timerQueue_.Count > 0) {
					timer_.Start();
				}
			}
			catch (Exception ex) {
				// 送信失敗したらキューにもどす
				lock (lockObj_) {
					foreach (var timer in timers) {
						EnqueTimer(timer, false);
					}
				}
				throw ex;
			}
			finally {
				lock (lockObj_) {
					sending_ = false;
				}
			}
		}
		#endregion

		#endregion

		#region events
		public event EventHandler TimerSent;
		public event EventHandler<SendFailedEventArgs> SendFailed;

		void OnTimerSent()
		{
			if (TimerSent != null) {
				TimerSent(this, new EventArgs());
			}
		}

		void OnSendFailed(string message, Exception ex)
		{
			if (SendFailed != null) {
				SendFailed(this, new SendFailedEventArgs(message, ex));
			}
		}
		#endregion

		#region IDisposable
		bool isDisposed_ = false;
		virtual protected void Dispose(bool disposing)
		{
			if (isDisposed_) { return; }
			if (disposing) {
				if (timer_ != null) {
					timer_.Dispose();
					timer_ = null;
				}

				try {
					StopSending();
				}
				catch { }
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

	public class SendFailedEventArgs : EventArgs
	{
		public SendFailedEventArgs(
			string message,
			Exception ex)
		{
			Message = message;
			Exception = ex;
		}


		public string Message { get; private set; }
		public Exception Exception { get; private set; }
	}
}
