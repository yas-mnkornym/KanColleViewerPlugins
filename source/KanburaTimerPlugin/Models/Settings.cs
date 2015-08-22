using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Studiotaiha.Toolkit;

namespace Studiotaiha.KanburaTimerPlugin.Models
{
	public class Settings : SettingsBase
	{
		public Settings(
			ISettings settings,
			IDispatcher dispatcher)
			: base(settings, dispatcher)
		{ }

		#region Settings
		/// <summary>
		/// the value representing if the user authorized with KanburaTimer
		/// </summary>
		public bool IsAuthorized
		{
			get
			{
				return GetMe(false);
			}
			set
			{
				SetMe(value);
			}
		}

		/// <summary>
		/// Admiral ID of KanColle
		/// </summary>
		public long AdmiralID
		{
			get
			{
				return GetMe<long>(0L);
			}
			set
			{
				SetMe(value);
			}
		}

		/// <summary>
		/// Nick name of Kancolle
		/// </summary>
		public string UserName
		{
			get
			{
				return GetMe<string>(null);
			}
			set
			{
				SetMe(value);
			}
		}

		/// <summary>
		/// The password of KanburaTimer
		/// </summary>
		public string Password
		{
			get
			{
				return GetMe<string>(null);
			}
			set
			{
				SetMe(value);
			}
		}

		/// <summary>
		/// The delay of sending timer
		/// </summary>
		public int TimerSendDelay
		{
			get
			{
				return GetMe(10000);
			}
			set
			{
				SetMe(value);
			}
		}

		#endregion
	}
}
