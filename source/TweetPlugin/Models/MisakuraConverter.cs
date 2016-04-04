using System.Text.RegularExpressions;

namespace Studiotaiha.TweetPlugin.Models
{
	public static class MisakuraConverter
	{
		public static string Convert(string input)
		{
			string result = input;
			foreach (var replace in replaces) {
				result = Regex.Replace(result, replace.Pattern, replace.Replacement);
			}
			return result;
		}

		struct ReplaceInfo
		{
			public string Pattern { get; set; }
			public string Replacement { get; set; }
		}

		static ReplaceInfo[] replaces = new ReplaceInfo[]{
				new ReplaceInfo{Pattern = "(気持|きも)ちいい", Replacement = "ぎも゛ぢい゛い゛ぃ"},
				new ReplaceInfo{Pattern = "(大好|だいす)き", Replacement = "らいしゅきいぃっ"},
				new ReplaceInfo{Pattern = "(ミルク|みるく|牛乳)", Replacement = "ちんぽミルク"},
				new ReplaceInfo{Pattern = "お(願|ねが)い", Replacement = "お願いぃぃぃっっっﾞ"},
				new ReplaceInfo{Pattern = "ぁ", Replacement = "ぁぁ゛ぁ゛"},
				new ReplaceInfo{Pattern = "あ", Replacement = "ぁああああぉ"},
				new ReplaceInfo{Pattern = "お", Replacement = "おﾞぉおォおん"},
				new ReplaceInfo{Pattern = "ごきげんよう", Replacement = "ごきげんよぉおお！んおっ！んおおーーっ！ "},
				new ReplaceInfo{Pattern = "ごきげんよう", Replacement = "ごきげんみゃぁあ゛あ゛ぁ゛ぁぁあ！！"},
				new ReplaceInfo{Pattern = "こん(にち|ばん)[はわ]", Replacement = "こん$1みゃぁあ゛あ゛ぁ゛ぁぁあ！！"},
				new ReplaceInfo{Pattern = "えて", Replacement = "えてへぇええぇﾞ"},
				new ReplaceInfo{Pattern = "する", Replacement = "するの"},
				new ReplaceInfo{Pattern = "します", Replacement = "するの"},
				new ReplaceInfo{Pattern = "精液", Replacement = "せーしっせーし でりゅぅ でちゃいましゅ みるく ちんぽみるく ふたなりみるく"},
				new ReplaceInfo{Pattern = "射精", Replacement = "でちゃうっ れちゃうよぉおおﾞ"},
				new ReplaceInfo{Pattern = "(馬鹿|バカ|ばか)", Replacement = "バカ！バカ！まんこ!!"},
				new ReplaceInfo{Pattern = "いい", Replacement = "いぃぃっよぉおおﾞ"},
				new ReplaceInfo{Pattern = "[好す]き", Replacement = "ちゅき"},
				new ReplaceInfo{Pattern = "して", Replacement = "してぇぇぇぇ゛"},
				new ReplaceInfo{Pattern = "行く", Replacement = "んはっ イっぐぅぅぅふうぅ"},
				new ReplaceInfo{Pattern = "いく", Replacement = "イっくぅぅふぅん"},
				new ReplaceInfo{Pattern = "イク", Replacement = "イッちゃううぅん"},
				new ReplaceInfo{Pattern = "駄目", Replacement = "らめにゃのぉぉぉ゛"},
				new ReplaceInfo{Pattern = "ダメ", Replacement = "んぉほぉぉォォ　らめぇ"},
				new ReplaceInfo{Pattern = "だめ", Replacement = "らめぇぇ"},
				new ReplaceInfo{Pattern = "ちゃん", Replacement = "ちゃぁん"},
				new ReplaceInfo{Pattern = "(おい|美味)しい", Replacement = "$1ひぃ…れしゅぅ"},
				new ReplaceInfo{Pattern = "(た|る|ない)([。、　 ・…!?！？」\n\r\x00])", Replacement = "$1の$2"},
				new ReplaceInfo{Pattern = "さい([。、　 ・…!?！？」\n\r\x00])", Replacement = "さいなの$1"},
				new ReplaceInfo{Pattern = "よ([。、　 ・…!?！？」\n\r\x00])", Replacement = "よお゛お゛お゛ぉ$1"},
				new ReplaceInfo{Pattern = "もう", Replacement = "んもぉ゛お゛お゛ぉぉ"},
				new ReplaceInfo{Pattern = "(い|入)れて", Replacement = "いれてえぇぇぇえ"},
				new ReplaceInfo{Pattern = "(気持|きも)ちいい", Replacement = "きも゛ぢい゛～っ"},
				new ReplaceInfo{Pattern = "(がんば|頑張)る", Replacement = "尻穴ちんぽしごき$1るぅ!!!"},
				new ReplaceInfo{Pattern = "出る", Replacement = "でちゃうっ れちゃうよぉおおﾞ"},
				new ReplaceInfo{Pattern = "でる", Replacement = "でっ…でるぅでるうぅうぅ"},
				new ReplaceInfo{Pattern = "です", Replacement = "れしゅぅぅぅ"},
				new ReplaceInfo{Pattern = "ます", Replacement = "ましゅぅぅぅ"},
				new ReplaceInfo{Pattern = "はい", Replacement = "はひぃ"},
				new ReplaceInfo{Pattern = "スゴイ", Replacement = "スゴぉッ!!"},
				new ReplaceInfo{Pattern = "(すご|凄)い", Replacement = "しゅごいのょぉぉぅ"},
				new ReplaceInfo{Pattern = "だ", Replacement = "ら"},
				new ReplaceInfo{Pattern = "さ", Replacement = "しゃ"},
				new ReplaceInfo{Pattern = "な", Replacement = "にゃ"},
				new ReplaceInfo{Pattern = "つ", Replacement = "ちゅ"},
				new ReplaceInfo{Pattern = "ちゃ", Replacement = "ひゃ"},
				new ReplaceInfo{Pattern = "じゃ", Replacement = "に゛ゃ"},
				new ReplaceInfo{Pattern = "ほ", Replacement = "ほお゛お゛っ"},
				new ReplaceInfo{Pattern = "で", Replacement = "れ"},
				new ReplaceInfo{Pattern = "す", Replacement = "しゅ"},
				new ReplaceInfo{Pattern = "ざ", Replacement = "じゃ"},
				new ReplaceInfo{Pattern = "い", Replacement = "いぃ"},
				new ReplaceInfo{Pattern = "の", Replacement = "のぉおお"}
			};
	}
}
