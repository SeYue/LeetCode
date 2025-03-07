using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public abstract class ModuleDefinition
	{
		private static Rect lastEnumButtonRectWhyUnity;

		private bool isActivatedCached;

		private bool supportsCurrentEnvironmentCached;

		private ModuleManifest installedManifestCached;

		private string statusStringCached;

		private static GUIStyle private_titleStyle;

		private static GUIStyle private_statusStyle;

		public ModuleManager ModuleManager;

		public static GUIStyle TitleStyle
		{
			get
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_0029: Expected O, but got Unknown
				if (private_titleStyle == null)
				{
					GUIStyle val = new GUIStyle(EditorStyles.get_largeLabel());
					val.set_fontSize(14);
					val.set_font(EditorStyles.get_boldFont());
					private_titleStyle = val;
				}
				return private_titleStyle;
			}
		}

		public static GUIStyle StatusStyle
		{
			get
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Expected O, but got Unknown
				if (private_statusStyle == null)
				{
					private_statusStyle = new GUIStyle(SirenixGUIStyles.SubtitleRight);
					RectOffset margin = private_statusStyle.get_margin();
					margin.set_top(margin.get_top() + 7);
				}
				return private_statusStyle;
			}
		}

		public abstract string ID { get; }

		public abstract string NiceName { get; }

		public abstract Version LatestVersion { get; }

		public abstract string Description { get; }

		public abstract string BuildFromPath { get; }

		public virtual string DocumentationLink => null;

		public virtual string DependenciesDescription => null;

		public virtual bool UnstableExperimental => false;

		public virtual void OnSelectedInInspector()
		{
			isActivatedCached = CheckIsActivated();
			supportsCurrentEnvironmentCached = CheckSupportsCurrentEnvironment();
			if (isActivatedCached)
			{
				installedManifestCached = LoadManifest();
			}
			statusStringCached = (isActivatedCached ? string.Concat("Installed (", installedManifestCached.Version, ")") : ("Inactive (available: " + (LatestVersion ?? new Version(0, 0, 0, 0)).ToString() + ")"));
		}

		[OnInspectorGUI]
		[PropertyOrder(-10f)]
		protected virtual void DrawTitle()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			GUILayout.Label(NiceName, TitleStyle, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			Rect lastRect = GUILayoutUtility.GetLastRect();
			GUI.Label(lastRect.AlignCenterY(16f).AddY(2f).SetXMin(((Rect)(ref lastRect)).get_center().x), statusStringCached, StatusStyle);
			SirenixEditorGUI.HorizontalLineSeparator(SirenixGUIStyles.BorderColor);
		}

		[OnInspectorGUI]
		[PropertyOrder(-5f)]
		protected virtual void DrawDescription()
		{
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			GUILayout.Label(Description, SirenixGUIStyles.MultiLineLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			if (!string.IsNullOrEmpty(DependenciesDescription))
			{
				GUILayout.Space(6f);
				GUILayout.Label("Dependencies: " + DependenciesDescription, SirenixGUIStyles.MultiLineLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			if (!string.IsNullOrEmpty(DocumentationLink))
			{
				GUILayout.Space(6f);
				GUIHelper.PushColor(new Color(0.3f, 0.6f, 0.7f, 1f));
				if (GUILayout.Button("Go to documentation.", EditorStyles.get_label(), (GUILayoutOption[])(object)new GUILayoutOption[0]))
				{
					Application.OpenURL(DocumentationLink);
				}
				Rect lastRect = GUILayoutUtility.GetLastRect();
				SirenixEditorGUI.DrawSolidRect(lastRect.SetWidth(EditorStyles.get_label().CalcSize(GUIHelper.TempContent("Go to documentation.")).x).Expand(-2f).AlignBottom(1f)
					.AddY(0f), Color.get_white() * 0.7f);
				GUIHelper.PopColor();
			}
		}

		[OnInspectorGUI]
		[PropertyOrder(5f)]
		protected virtual void DrawActivationButtons()
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_0275: Unknown result type (might be due to invalid IL or missing references)
			GUILayout.FlexibleSpace();
			if (!supportsCurrentEnvironmentCached)
			{
				GUIHelper.PushColor(new Color(64f / 85f, 0.2235294f, 0.1686275f, 1f) * 1.3f);
				GUILayout.Label("Dependencies are missing for this module" + ((DependenciesDescription != null) ? (": " + DependenciesDescription) : ""), SirenixGUIStyles.MultiLineLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUIHelper.PopColor();
			}
			ModuleConfiguration config = GlobalConfig<OdinModuleConfig>.Instance.GetConfig(this);
			bool enabled = false;
			if (config != null)
			{
				if (config.ActivationSettings == ActivationSettings.Manual)
				{
					enabled = true;
				}
				else if (config.ActivationSettings == ActivationSettings.GlobalSettings && GlobalConfig<OdinModuleConfig>.Instance.ModuleTogglingSettings != OdinModuleConfig.ModuleAutomationSettings.Automatic)
				{
					enabled = true;
				}
			}
			else
			{
				enabled = true;
			}
			EditorGUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			if (!isActivatedCached)
			{
				GUIHelper.PushGUIEnabled(enabled);
				if (GUILayout.Button("Activate", SirenixGUIStyles.ButtonLeft, (GUILayoutOption[])(object)new GUILayoutOption[0]))
				{
					Activate();
					isActivatedCached = true;
					AssetDatabase.Refresh();
					GUIUtility.ExitGUI();
				}
				GUIHelper.PopGUIEnabled();
			}
			else
			{
				GUIHelper.PushGUIEnabled(enabled);
				if (GUILayout.Button("Deactivate", SirenixGUIStyles.ButtonLeft, (GUILayoutOption[])(object)new GUILayoutOption[0]))
				{
					Deactivate();
					isActivatedCached = false;
					AssetDatabase.Refresh();
					GUIUtility.ExitGUI();
				}
				GUIHelper.PopGUIEnabled();
				if (installedManifestCached != null && installedManifestCached.Version < LatestVersion && GUILayout.Button("Update to " + LatestVersion, SirenixGUIStyles.ButtonMid, (GUILayoutOption[])(object)new GUILayoutOption[0]))
				{
					Deactivate();
					Activate();
					AssetDatabase.Refresh();
					GUIUtility.ExitGUI();
				}
			}
			string t;
			if (config == null)
			{
				GUIHelper.PushGUIEnabled(enabled: false);
				t = "Activation Rule: Config Broken";
			}
			else
			{
				ActivationSettings activationSettings = config.ActivationSettings;
				t = ((activationSettings == ActivationSettings.GlobalSettings || activationSettings != ActivationSettings.Manual) ? "Activation Rule: Global Settings" : "Activation Rule: Manual");
			}
			bool flag = GUILayout.Button(GUIHelper.TempContent(t), SirenixGUIStyles.ButtonRight, (GUILayoutOption[])GUILayoutOptions.Width(250f));
			Rect lastRect = GUILayoutUtility.GetLastRect();
			if (((Rect)(ref lastRect)).get_x() != 0f && ((Rect)(ref lastRect)).get_y() != 0f)
			{
				lastEnumButtonRectWhyUnity = lastRect;
			}
			if (flag)
			{
				EnumSelector<ActivationSettings> enumSelector = new EnumSelector<ActivationSettings>();
				enumSelector.SelectionChanged += delegate(IEnumerable<ActivationSettings> values)
				{
					ActivationSettings activationSettings2 = values.FirstOrDefault();
					if (activationSettings2 != config.ActivationSettings)
					{
						config.ActivationSettings = activationSettings2;
						GlobalConfig<OdinModuleConfig>.Instance.SaveAssetChanges();
						OdinModuleConfig.RefreshModuleSetup();
					}
				};
				((OdinSelector<ActivationSettings>)enumSelector).ShowInPopup(new Vector2(((Rect)(ref lastEnumButtonRectWhyUnity)).get_xMin(), ((Rect)(ref lastEnumButtonRectWhyUnity)).get_yMax()));
				SirenixEditorGUI.DrawSolidRect(lastRect, Color.get_green());
			}
			if (config == null)
			{
				GUIHelper.PopGUIEnabled();
			}
			EditorGUILayout.EndHorizontal();
		}

		public abstract bool CheckSupportsCurrentEnvironment();

		public virtual bool CheckIsActivated()
		{
			string path = ModuleManager.DataManager.InstallPath + "/" + ID + "/manifest.txt";
			return File.Exists(path);
		}

		protected virtual void OnBeforeActivate()
		{
		}

		protected virtual void OnAfterActivate()
		{
		}

		protected virtual void OnBeforeDeactivate()
		{
		}

		protected virtual void OnAfterDeactivate()
		{
		}

		public virtual void Activate()
		{
			if (CheckIsActivated())
			{
				return;
			}
			OnBeforeActivate();
			byte[] array = ModuleManager.DataManager.LoadData(ID);
			if (array == null)
			{
				throw new Exception("Could not load module data for module '" + ID + "': data could not be found.");
			}
			ModuleData moduleData = ModuleData.Deserialize(array);
			if (array == null)
			{
				throw new Exception("Could not load module data for module '" + ID + "': data could not be parsed.");
			}
			string text = ModuleManager.DataManager.InstallPath + "/" + ID;
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			foreach (ModuleData.ModuleFile file in moduleData.Files)
			{
				string path = text + "/" + file.Path;
				File.WriteAllBytes(path, file.Data);
			}
			string path2 = text + "/manifest.txt";
			ModuleManifest manifest = moduleData.ToManifest();
			ModuleManifest.Save(path2, manifest);
			OnAfterActivate();
		}

		public virtual void Deactivate()
		{
			if (!CheckIsActivated())
			{
				return;
			}
			string text = ModuleManager.DataManager.InstallPath + "/" + ID;
			string text2 = text + "/manifest.txt";
			ModuleManifest moduleManifest = ModuleManifest.Load(text2);
			if (moduleManifest == null)
			{
				throw new Exception("Could not load module manifest.");
			}
			OnBeforeDeactivate();
			foreach (string file in moduleManifest.Files)
			{
				string path = text + "/" + file;
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			File.Delete(text2);
			File.Delete(text2 + ".meta");
			DeleteIfEmpty(new DirectoryInfo(text));
			OnAfterDeactivate();
		}

		public virtual ModuleData GetModuleDataForPackaging()
		{
			string buildFromPath = BuildFromPath;
			DirectoryInfo dir = new DirectoryInfo(buildFromPath);
			if (!dir.Exists)
			{
				throw new InvalidOperationException("Directory '" + dir.FullName + "' does not exist to build module package from.");
			}
			return new ModuleData
			{
				ID = ID,
				Version = LatestVersion,
				Files = (from n in dir.GetFiles("*", SearchOption.AllDirectories).Select(delegate(FileInfo file)
					{
						switch (file.Name.ToLower())
						{
						case "thumbs.db":
						case "manifest.txt":
						case "manifest.txt.meta":
							return null;
						default:
						{
							string path = PathUtilities.MakeRelative(dir.FullName, file.FullName);
							byte[] data = File.ReadAllBytes(file.FullName);
							return new ModuleData.ModuleFile
							{
								Path = path,
								Data = data
							};
						}
						}
					})
					where n != null
					select n).ToList()
			};
		}

		public virtual ModuleManifest LoadManifest()
		{
			if (!CheckIsActivated())
			{
				return null;
			}
			string text = ModuleManager.DataManager.InstallPath + "/" + ID;
			string path = text + "/manifest.txt";
			return ModuleManifest.Load(path);
		}

		protected static void DeleteIfEmpty(DirectoryInfo dir)
		{
			if (!dir.Exists)
			{
				return;
			}
			if (dir.Name.ToLower() == "__macosx")
			{
				if (dir.Parent != null)
				{
					string path = dir.Parent.FullName + "/" + dir.Name + ".meta";
					if (File.Exists(path))
					{
						File.Delete(path);
					}
				}
				dir.Delete(recursive: true);
				return;
			}
			DirectoryInfo[] directories = dir.GetDirectories();
			foreach (DirectoryInfo dir2 in directories)
			{
				DeleteIfEmpty(dir2);
			}
			if (dir.GetDirectories().Length != 0)
			{
				return;
			}
			FileInfo[] files = dir.GetFiles();
			if (files.Length > 2)
			{
				return;
			}
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				string text = fileInfo.Name.ToLower();
				if (text != "thumbs.db" && text != ".ds_store")
				{
					return;
				}
			}
			if (dir.Parent != null)
			{
				string path2 = dir.Parent.FullName + "/" + dir.Name + ".meta";
				if (File.Exists(path2))
				{
					File.Delete(path2);
				}
			}
			dir.Delete(recursive: true);
		}
	}
}
