using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studiotaiha.KanburaTimerPlugin
{
	public class Constants
	{
		public static string PluginName { get; } = "艦ぶらたいまー";

		/// <summary>
		/// プラグインの設定ファイル名
		/// </summary>
		public const string SettingsFileName = "KanburaTimerPlugin.xml";

		/// <summary>
		/// 艦ぶらタイマーのURI
		/// </summary>
		public const string KanburaTimerUri = "http://kanburatimer.studio-taiha.net/";

		/// <summary>
		/// 艦ぶらタイマーのユーザエージェント
		/// </summary>
		public const string KanburaTimerAgent = "KancolleViewer.KanburaTimerPlugin";
	}
}
