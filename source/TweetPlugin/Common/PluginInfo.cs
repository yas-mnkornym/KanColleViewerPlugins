using System.IO;
using System.Reflection;

namespace Studiotaiha.TweetPlugin
{
	public static class PluginInfo
	{
		static string assemblyPath = null;
		public static string AssemblyPath
		{
			get
			{
				if (assemblyPath == null) {
					var asm = Assembly.GetEntryAssembly();
					assemblyPath = asm.Location;
				}
				return assemblyPath;
			}
		}

		public static string AssemblyDirectory
		{
			get
			{
				return Path.GetDirectoryName(AssemblyPath);
			}
		}

		public static string AssemblyName
		{
			get
			{
				return Path.GetFileName(AssemblyPath);
			}
		}

		public static string SettingsFilePath
		{
			get
			{
				return Path.Combine(AssemblyDirectory, Constants.SettingsFileName);
			}
		}
	}
}
