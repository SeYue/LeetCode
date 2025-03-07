using System;
using System.IO;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>
	/// Tell Odin which types should be drawn or should not be drawn by Odin.
	/// </para>
	/// <para>
	/// You can modify which types should be drawn by Odin in the Preferences window found in 'Tools -&gt; Odin Inspector -&gt; Preferences -&gt; Editor Types',
	/// or by locating the configuration file stored as a serialized object in the Sirenix folder under 'Odin Inspector/Config/Editor/InspectorConfig'.
	/// </para>
	/// </summary>
	[SirenixEditorConfig]
	public class InspectorConfig : GlobalConfig<InspectorConfig>, ISerializationCallbackReceiver
	{
		private static bool hasUpdatedEditorsOnce;

		[Space(5f)]
		[SerializeField]
		[HorizontalGroup(0f, 0, 0, 0f)]
		[OnValueChanged("UpdateOdinEditors", false)]
		[ToggleLeft]
		[LabelText(" Enable Odin In Inspector")]
		[Tooltip("Whether Odin is enabled in the inspector or not.")]
		private bool enableOdinInInspector = true;

		[SerializeField]
		[HideInInspector]
		private InspectorDefaultEditors defaultEditorBehaviour = InspectorDefaultEditors.UserTypes | InspectorDefaultEditors.PluginTypes | InspectorDefaultEditors.OtherTypes;

		[SerializeField]
		[HideInInspector]
		private bool processMouseMoveInInspector = true;

		[SerializeField]
		[DisableContextMenu(true, true)]
		private InspectorTypeDrawingConfig drawingConfig = new InspectorTypeDrawingConfig();

		private static bool IsHeadlessMode => (int)SystemInfo.get_graphicsDeviceType() == 4;

		/// <summary>
		/// Whether Odin is enabled in the inspector or not.
		/// </summary>
		public bool EnableOdinInInspector
		{
			get
			{
				return enableOdinInInspector;
			}
			set
			{
				if (value != enableOdinInInspector)
				{
					enableOdinInInspector = value;
					UpdateOdinEditors();
				}
			}
		}

		/// <summary>
		/// InspectorDefaultEditors is a bitmask used to tell which types should have an Odin Editor generated.
		/// </summary>
		public InspectorDefaultEditors DefaultEditorBehaviour
		{
			get
			{
				return defaultEditorBehaviour;
			}
			set
			{
				defaultEditorBehaviour = value;
			}
		}

		/// <summary>
		/// The config which contains configuration data for which types Odin should draw in the inspector.
		/// </summary>
		public InspectorTypeDrawingConfig DrawingConfig => drawingConfig;

		internal bool ProcessMouseMoveInInspector
		{
			get
			{
				return processMouseMoveInInspector;
			}
			set
			{
				processMouseMoveInInspector = value;
			}
		}

		[InitializeOnLoadMethod]
		private static void RemoveObsoleteGeneratedOdinEditorsDLL()
		{
			UnityEditorEventUtility.EditorApplication_delayCall += delegate
			{
				UnityEditorEventUtility.EditorApplication_delayCall += delegate
				{
					if (!EditorPrefs.HasKey("PREVENT_SIRENIX_FILE_GENERATION"))
					{
						string text = SirenixAssetPaths.SirenixAssembliesPath + "Editor";
						string text2 = text + "/GeneratedOdinEditors.dll";
						if (File.Exists(text2))
						{
							AssetDatabase.DeleteAsset(text2);
							if (File.Exists(text2 + ".mdb"))
							{
								AssetDatabase.DeleteAsset(text2 + ".mdb");
							}
							AssetDatabase.Refresh();
						}
					}
				};
			};
		}

		private void SuppressMissingEditorTypeErrorsMessage()
		{
			if (UnityVersion.Major == 2017 && UnityVersion.Minor == 1)
			{
				SirenixEditorGUI.ErrorMessageBox("Suppressing these error messages may cause crashes on Unity 2017.1 (see Unity issue 920772). A fix is being backported from 2017.2 - meanwhile, you may want to disable this option, and live with the constant error messages about missing editor types.");
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			drawingConfig.UpdateCaches();
			UnityEditorEventUtility.DelayAction(delegate
			{
				UpdateOdinEditors();
			});
		}

		/// <summary>
		/// Updates Unity with the current Odin editor configuration.
		/// </summary>
		[Button("Update Editors", 22)]
		[HorizontalGroup(100f, 0, 0, 0f)]
		public void UpdateOdinEditors()
		{
			if (IsHeadlessMode || InternalEditorUtility.get_inBatchMode())
			{
				return;
			}
			CustomEditorUtility.ResetCustomEditors();
			if (enableOdinInInspector)
			{
				TypeDrawerPair[] editors = InspectorTypeDrawingConfigDrawer.GetEditors();
				for (int i = 0; i < editors.Length; i++)
				{
					TypeDrawerPair typeDrawerPair = editors[i];
					Type type = TwoWaySerializationBinder.Default.BindToType(typeDrawerPair.DrawnTypeName);
					Type type2 = TwoWaySerializationBinder.Default.BindToType(typeDrawerPair.EditorTypeName);
					if (type != null && type2 != null)
					{
						CustomEditorUtility.SetCustomEditor(type, type2, isFallbackEditor: false, isEditorForChildClasses: false);
					}
				}
			}
			Type type3 = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
			Type type4 = typeof(EditorWindow).Assembly.GetType("UnityEditor.ActiveEditorTracker");
			if (type3 != null && type4 != null)
			{
				MethodInfo method = type3.GetMethod("CreateTracker", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				FieldInfo field = type3.GetField("m_Tracker", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				MethodInfo method2 = type4.GetMethod("ForceRebuild", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (method != null && field != null && method2 != null)
				{
					Object[] array = Resources.FindObjectsOfTypeAll(type3);
					Object[] array2 = array;
					foreach (Object obj in array2)
					{
						method.Invoke(obj, null);
						object value = field.GetValue(obj);
						method2.Invoke(value, null);
					}
				}
			}
			hasUpdatedEditorsOnce = true;
		}

		internal void EnsureEditorsHaveBeenUpdated()
		{
			if (!hasUpdatedEditorsOnce)
			{
				UpdateOdinEditors();
			}
		}

		[OnInspectorGUI]
		private void BottomSpace()
		{
			GUILayout.Space(10f);
		}
	}
}
