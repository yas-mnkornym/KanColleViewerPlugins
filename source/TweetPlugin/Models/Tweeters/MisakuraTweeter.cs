using System;

namespace Studiotaiha.TweetPlugin.Models.Tweeters
{
	class MisakuraTweeter : ITweeter
	{
		ITweeter tweeter_ = null;

		public MisakuraTweeter(ITweeter tweeter)
		{
			if (tweeter == null) { throw new ArgumentNullException("tweeter_"); }
			tweeter_ = tweeter;
		}

		public void Tweet(string message, string hashTag, string picture)
		{
			tweeter_.Tweet(MisakuraConverter.Convert(message), hashTag, picture);
		}

		public int CalcurateTweetLength(string message, string hashTag, string picture)
		{
			return tweeter_.CalcurateTweetLength(MisakuraConverter.Convert(message), hashTag, picture);
		}
	}
}
