using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Installed Odin Inspector Version Info.
	/// </summary>
	public static class OdinInspectorVersion
	{
		private static string version;

		private static string buildName;

		private static string licensee;

		/// <summary>
		/// Gets the name of the current running version of Odin Inspector.
		/// </summary>
		public static string BuildName
		{
			get
			{
				if (buildName == null)
				{
					SirenixBuildNameAttribute attribute = typeof(InspectorConfig).Assembly.GetAttribute<SirenixBuildNameAttribute>(inherit: true);
					buildName = ((attribute != null) ? attribute.BuildName : "Source Code");
				}
				return buildName;
			}
		}

		public static bool HasLicensee => !string.IsNullOrEmpty(Licensee);

		public static string Licensee
		{
			get
			{
				if (licensee == null && !BakedValues.TryGetBakedValue<string>("Licensee", out licensee))
				{
					licensee = "";
				}
				return licensee;
			}
		}

		/// <summary>
		/// Gets the current running version of Odin Inspector.
		/// </summary>
		public static string Version
		{
			get
			{
				if (version == null)
				{
					SirenixBuildVersionAttribute attribute = typeof(InspectorConfig).Assembly.GetAttribute<SirenixBuildVersionAttribute>(inherit: true);
					version = ((attribute != null) ? attribute.Version : "Source Code Mode");
				}
				return version;
			}
		}

		/// <summary>
		/// Whether the current version of Odin is an enterprise version.
		/// </summary>
		public static bool IsEnterprise => false;
	}
}
