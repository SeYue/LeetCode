using System;
using System.IO;
using System.Net;
using System.Threading;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class CheckForUpdatesWindow : EditorWindow
	{
		[Serializable]
		private struct SirenixVersion
		{
			[Serializable]
			public struct BetaVersion
			{
				public bool available;

				public string version;

				public int releaseDateTicks;

				public string patchNotesUrl;
			}

			public string name;

			public string version;

			public string patchNotesUrl;

			public int releaseDateTicks;

			public BetaVersion beta;

			public string downloadUrl;
		}

		private enum FetchVersionResult : byte
		{
			None,
			Fetching,
			Failed,
			Success
		}

		private const string CheckForUpdatesDailyPref = "CheckForUpdates.CheckDaily";

		private const string CheckBetaVersionsEnablePref = "CheckForUpdates.EnableBeta";

		private const string LastCheckDatePref = "CheckForUpdates.LastCheckDate";

		private static bool? checkDaily;

		private static bool? checkForBeta;

		private static FetchVersionResult state;

		private static SirenixVersion latestVersion;

		internal static bool CheckDailyEnabled
		{
			get
			{
				if (!checkDaily.HasValue)
				{
					checkDaily = EditorPrefs.GetBool("CheckForUpdates.CheckDaily", false);
				}
				return checkDaily.Value;
			}
			set
			{
				if (!checkDaily.HasValue || checkDaily.Value != value)
				{
					checkDaily = value;
					EditorPrefs.SetBool("CheckForUpdates.CheckDaily", value);
				}
			}
		}

		internal static bool IncludeBetasEnabled
		{
			get
			{
				if (!checkForBeta.HasValue)
				{
					checkForBeta = EditorPrefs.GetBool("CheckForUpdates.EnableBeta", false);
				}
				return checkForBeta.Value;
			}
			set
			{
				if (!checkForBeta.HasValue || checkForBeta.Value != value)
				{
					checkForBeta = value;
					EditorPrefs.SetBool("CheckForUpdates.EnableBeta", value);
				}
			}
		}

		[InitializeOnLoadMethod]
		private static void DailyCheckForUpdates()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			if (CheckDailyEnabled)
			{
				EditorApplication.update = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(WaitForVersionFetch));
				EditorApplication.update = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(WaitForVersionFetch));
				int @int = EditorPrefs.GetInt("CheckForUpdates.LastCheckDate", 0);
				int num = (int)(DateTime.Now.Date.Ticks / 864000000000L);
				if (@int < num)
				{
					EditorPrefs.SetInt("CheckForUpdates.LastCheckDate", num);
					StartTryRefreshLatestOdinVersion();
				}
			}
		}

		private static void WaitForVersionFetch()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Expected O, but got Unknown
			if (state == FetchVersionResult.Success || state == FetchVersionResult.Failed)
			{
				EditorApplication.update = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(WaitForVersionFetch));
				if (state == FetchVersionResult.Success && (IsVersionHigher(latestVersion.version, OdinInspectorVersion.Version) || (IncludeBetasEnabled && IsVersionHigher(latestVersion.beta.version, OdinInspectorVersion.Version))))
				{
					OpenWindow();
				}
			}
		}

		public static void OpenWindow()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			CheckForUpdatesWindow window = EditorWindow.GetWindow<CheckForUpdatesWindow>("Odin Updates");
			((EditorWindow)window).set_position(GUIHelper.GetEditorWindowRect().AlignCenter(350f, 150f));
			((EditorWindow)window).set_minSize(new Vector2(350f, 150f));
			((EditorWindow)window).set_maxSize(new Vector2(350f, 150f));
		}

		private void OnEnable()
		{
			if (state == FetchVersionResult.None && state != FetchVersionResult.Fetching)
			{
				StartTryRefreshLatestOdinVersion();
			}
		}

		private void OnGUI()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			//IL_0267: Unknown result type (might be due to invalid IL or missing references)
			//IL_0271: Unknown result type (might be due to invalid IL or missing references)
			//IL_0285: Unknown result type (might be due to invalid IL or missing references)
			//IL_028f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0296: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02da: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_030c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0337: Unknown result type (might be due to invalid IL or missing references)
			//IL_033d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0344: Unknown result type (might be due to invalid IL or missing references)
			//IL_035d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0363: Unknown result type (might be due to invalid IL or missing references)
			//IL_036a: Unknown result type (might be due to invalid IL or missing references)
			Rect position = ((EditorWindow)this).get_position();
			float width = ((Rect)(ref position)).get_width();
			position = ((EditorWindow)this).get_position();
			Rect rect = RectExtensions.Padding(new Rect(0f, 0f, width, ((Rect)(ref position)).get_height()), 4f);
			GUI.set_enabled(state != FetchVersionResult.Fetching);
			if (SirenixEditorGUI.IconButton(rect.AlignRight(20f).AlignTop(20f), EditorIcons.Refresh, "Refresh"))
			{
				StartTryRefreshLatestOdinVersion();
			}
			GUI.set_enabled(true);
			Rect rect2 = rect.AlignTop(((Rect)(ref rect)).get_height() - 48f);
			GUI.DrawTexture(rect2.AlignLeft(64f), (Texture)(object)EditorIcons.OdinInspectorLogo, (ScaleMode)2);
			rect2 = rect2.AddXMin(64f);
			if (state == FetchVersionResult.Fetching)
			{
				Rect rect3 = rect2.AlignCenterY(20f);
				GUI.DrawTexture(rect3.AlignLeft(20f).SubY(2f), (Texture)(object)EditorIcons.Refresh.Raw);
				GUI.Label(rect3.AddXMin(24f), GUIHelper.TempContent("Getting latest version..."));
				GUIHelper.RequestRepaint();
			}
			else if (state == FetchVersionResult.Failed)
			{
				GUI.Label(rect2, GUIHelper.TempContent("Failed to fetch latest Odin Inspector version.\nPlease try again later."), SirenixGUIStyles.MultiLineCenteredLabel);
			}
			else if (state == FetchVersionResult.Success)
			{
				Rect rect4 = rect2.AlignCenterY(20f);
				if (latestVersion.beta.available && IsVersionHigher(latestVersion.beta.version, OdinInspectorVersion.Version))
				{
					((Rect)(ref rect4)).set_y(((Rect)(ref rect4)).get_y() + 15f);
					GUI.DrawTexture(rect4.AlignLeft(20f).SubY(2f), (Texture)(object)EditorIcons.Bell.Raw);
					GUI.Label(rect4.AddXMin(24f), "Beta " + latestVersion.beta.version + " is available for download!");
					((Rect)(ref rect4)).set_y(((Rect)(ref rect4)).get_y() - 25f);
				}
				if (IsVersionHigher(latestVersion.version, OdinInspectorVersion.Version))
				{
					GUI.DrawTexture(rect4.AlignLeft(20f).SubY(2f), (Texture)(object)EditorIcons.Bell.Raw);
					GUI.Label(rect4.AddXMin(24f), "Patch " + latestVersion.version + " is available for download!");
				}
				else
				{
					GUI.set_color(Color.get_green());
					GUI.DrawTexture(rect4.AlignLeft(20f).SubY(2f), (Texture)(object)EditorIcons.Checkmark.Raw);
					GUI.set_color(Color.get_white());
					GUI.Label(rect4.AddXMin(24f), "Latest stable Odin version " + OdinInspectorVersion.Version + " installed");
				}
			}
			GUI.set_enabled(state == FetchVersionResult.Success);
			Rect rect5 = rect.AlignBottom(44f);
			if (GUI.Button(rect5.AlignTop(20f).Split(0, 2), GUIHelper.TempContent("See patch notes")))
			{
				Application.OpenURL(latestVersion.patchNotesUrl);
			}
			if (GUI.Button(rect5.AlignTop(20f).Split(1, 2), GUIHelper.TempContent("Download here")))
			{
				Application.OpenURL(latestVersion.downloadUrl);
			}
			GUI.set_enabled(true);
			CheckDailyEnabled = EditorGUI.ToggleLeft(rect5.AlignBottom(20f).Split(0, 2), "Daily check for updates", CheckDailyEnabled);
			IncludeBetasEnabled = EditorGUI.ToggleLeft(rect5.AlignBottom(20f).Split(1, 2), "Include betas", IncludeBetasEnabled);
			((EditorWindow)(object)this).RepaintIfRequested();
		}

		private static string GetUpdateAvailableMessage(SirenixVersion version)
		{
			if (version.beta.available)
			{
				if (IsVersionHigher(version.beta.version, OdinInspectorVersion.Version))
				{
					return "Beta version " + version.version + " is available for download at " + version.downloadUrl;
				}
			}
			else if (IsVersionHigher(version.version, OdinInspectorVersion.Version))
			{
				return "Update " + version.version + " is available for download at " + version.downloadUrl;
			}
			return "Looks like you're up to date with the latest version. You get a cookie for that.";
		}

		private static bool IsVersionHigher(string a, string b)
		{
			try
			{
				Version version = new Version(a);
				Version version2 = new Version(b);
				return Math.Max(version.Major, 0) > Math.Max(version2.Major, 0) || Math.Max(version.Minor, 0) > Math.Max(version2.Minor, 0) || Math.Max(version.Build, 0) > Math.Max(version2.Build, 0) || Math.Max(version.Revision, 0) > Math.Max(version2.Revision, 0);
			}
			catch
			{
				return false;
			}
		}

		private static void StartTryRefreshLatestOdinVersion()
		{
			if (state != FetchVersionResult.Fetching)
			{
				state = FetchVersionResult.Fetching;
				Thread thread = new Thread((ThreadStart)delegate
				{
					TryRefreshLatestOdinVersion();
				});
				thread.IsBackground = true;
				thread.Start();
			}
		}

		private static void TryRefreshLatestOdinVersion()
		{
			try
			{
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://odininspector.com/latest-version/odin-inspector");
				httpWebRequest.Method = "GET";
				using HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				using Stream stream = httpWebResponse.GetResponseStream();
				using StreamReader streamReader = new StreamReader(stream);
				if (httpWebResponse.ContentType == "application/json; charset=utf-8")
				{
					latestVersion = JsonUtility.FromJson<SirenixVersion>(streamReader.ReadToEnd());
					state = FetchVersionResult.Success;
				}
				else
				{
					latestVersion = default(SirenixVersion);
					state = FetchVersionResult.Failed;
				}
			}
			catch
			{
				latestVersion = default(SirenixVersion);
				state = FetchVersionResult.Failed;
			}
		}

		public CheckForUpdatesWindow()
			: this()
		{
		}
	}
}
