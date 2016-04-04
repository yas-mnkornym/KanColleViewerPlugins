namespace Studiotaiha.TweetPlugin.Models
{
	public interface ITweeter
	{
		void Tweet(string message, string hashTag, string picture);

		int CalcurateTweetLength(string message, string hashTag, string picture);
	}
}
