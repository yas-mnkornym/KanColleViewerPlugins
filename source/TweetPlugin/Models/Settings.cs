using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Studiotaiha.Toolkit;

namespace Studiotaiha.TweetPlugin.Models
{
	class Settings : SettingsBase
	{
		public Settings(
			ISettings settings,
			IDispatcher dispatcher)
			: base(settings, dispatcher)
		{ }

		#region 設定
		public bool AppendHashTag
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

		public string HashTag
		{
			get
			{
				return GetMe("#艦これ");
			}
			set
			{
				SetMe(value);
			}
		}

		public bool AppendPicture
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

		public bool UseMisakuraTweet
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

		public bool AlwaysUseLatestPicture
		{
			get
			{
				return GetMe(true);
			}
			set
			{
				SetMe(value);
			}
		}

		public string TwitterAccessToken
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

		public string TwitterAccessTokenSecret
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

		public bool IsTwitterAuthorized
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

		public bool ToolTipEnabled
		{
			get
			{
				return GetMe(true);
			}
			set
			{
				SetMe(value);
			}
		}

		public bool IsWindowOpen
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

		public bool ClearTextWhenTweeted
		{
			get
			{
				return GetMe(true);
			}
			set
			{
				SetMe(value);
			}
		}
		#endregion
	}
}
