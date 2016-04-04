using System;
using System.Collections.Generic;
using System.IO;
using TweetSharp;

namespace Studiotaiha.TweetPlugin.Models.Tweeters
{
	class TweetSharpTweeter : ITweeter
	{
		string consumerKey_;
		string consumerSecret_;
		string accessToken_;
		string accessTokenSecret_;
		TwitterService twitterService_ = null;

		public TwitterService TwitterService
		{
			get
			{
				if (twitterService_ == null) {
					twitterService_ = new TwitterService(consumerKey_, consumerSecret_);
					twitterService_.AuthenticateWith(accessToken_, accessTokenSecret_);
				}
				return twitterService_;
			}
		}

		public TweetSharpTweeter(
			string consumerKey,
			string consumerSecret,
			string accessToken,
			string accessTokenSecret)
		{
			if (consumerKey == null) { throw new ArgumentNullException("consumerKey"); }
			if (consumerSecret == null) { throw new ArgumentNullException("consumerSecret"); }
			if (accessToken == null) { throw new ArgumentNullException("accessToken"); }
			if (accessTokenSecret == null) { throw new ArgumentNullException("accessTokenSecret"); }
			consumerKey_ = consumerKey;
			consumerSecret_ = consumerSecret;
			accessToken_ = accessToken;
			accessTokenSecret_ = accessTokenSecret;
		}

		public TweetSharpTweeter(TwitterService twitterService)
		{
			if (twitterService == null) { throw new ArgumentNullException("twitterService"); }
			twitterService_ = twitterService;
		}

		static readonly int PictureLength = 35;
		public void Tweet(string message, string hashTag, string picture)
		{
			if (CalcurateTweetLength(message, hashTag, picture) > 140) {
				throw new ArgumentException("文字数が多すぎます。");
			}

			if (twitterService_ == null) {
				twitterService_ = new TwitterService(consumerKey_, consumerSecret_);
				twitterService_.AuthenticateWith(accessToken_, accessTokenSecret_);
			}

			var str = CreateTweetText(message, hashTag);
			if (picture != null) {
				SendTweetWithMediaOptions opt = new SendTweetWithMediaOptions();
				using (var fs = new FileStream(picture, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					opt.Images = new Dictionary<string, Stream> { { "image", fs } };
					opt.Status = str;
					twitterService_.SendTweetWithMedia(opt);
				}
			}
			else {
				var opt = new SendTweetOptions();
				opt.Status = str;
				twitterService_.SendTweet(opt);
			}
		}

		string CreateTweetText(string message, string hashTag)
		{
			if (hashTag == null) {
				return message;
			}
			else {
				return (message + " " + hashTag);
			}
		}

		public int CalcurateTweetLength(string message, string hashTag, string picture)
		{
			int length = 0;

			if (message != null) {
				length += message.Length;
			}

			if (picture != null) {
				length += PictureLength;
			}

			if (hashTag != null) {
				length += hashTag.Length + 1;
			}

			return length;
		}
	}
}
