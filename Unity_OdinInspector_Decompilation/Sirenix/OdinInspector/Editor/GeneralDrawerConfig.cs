using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>Contains general configuration for all Odin drawers.</para>
	/// <para>
	/// You can modify the configuration in the Odin Preferences window found in 'Tools -&gt; Odin Inspector -&gt; Preferences -&gt; Drawers -&gt; General',
	/// or by locating the configuration file stored as a serialized object in the Sirenix folder under 'Odin Inspector/Config/Editor/GeneralDrawerConfig'.
	/// </para>
	/// </summary>
	[InitializeOnLoad]
	[SirenixEditorConfig]
	[Searchable]
	public class GeneralDrawerConfig : GlobalConfig<GeneralDrawerConfig>
	{
		private class ColorPref
		{
			private Color? color;

			private Color defaultColor;

			private string key;

			public Color Value
			{
				get
				{
					//IL_0014: Unknown result type (might be due to invalid IL or missing references)
					//IL_002f: Unknown result type (might be due to invalid IL or missing references)
					//IL_003e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0053: Unknown result type (might be due to invalid IL or missing references)
					if (!color.HasValue)
					{
						string @string = EditorPrefs.GetString(key, ColorToString(defaultColor));
						if (TryStringToColor(@string, out var value))
						{
							color = value;
						}
						else
						{
							color = defaultColor;
						}
					}
					return color.Value;
				}
				set
				{
					//IL_0013: Unknown result type (might be due to invalid IL or missing references)
					//IL_0018: Unknown result type (might be due to invalid IL or missing references)
					//IL_0026: Unknown result type (might be due to invalid IL or missing references)
					//IL_0032: Unknown result type (might be due to invalid IL or missing references)
					if (!color.HasValue || color.Value != value)
					{
						EditorPrefs.SetString(key, ColorToString(value));
					}
					color = value;
				}
			}

			public ColorPref(string key, Color defaultValue)
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				this.key = key;
				defaultColor = defaultValue;
			}

			public void Reset()
			{
				if (EditorPrefs.HasKey(key))
				{
					EditorPrefs.DeleteKey(key);
					color = null;
				}
			}

			private unsafe static string ColorToString(Color color)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				Color32 val = Color32.op_Implicit(color);
				byte[] array = new byte[4];
				fixed (byte* ptr = array)
				{
					*ptr = (byte)val;
				}
				return ProperBitConverter.BytesToHexString(array);
			}

			private unsafe static bool TryStringToColor(string data, out Color color)
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_0050: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					byte[] array = ProperBitConverter.HexStringToBytes(data);
					if (array.Length != 4)
					{
						color = default(Color);
						return false;
					}
					fixed (byte* ptr = array)
					{
						Color32 val = (Color32)(*ptr);
						color = Color32.op_Implicit(val);
						return true;
					}
				}
				catch
				{
					color = default(Color);
					return false;
				}
			}
		}

		[Flags]
		public enum UnityObjectType
		{
			Textures = 0x2,
			Sprites = 0x4,
			Materials = 0x8,
			GameObjects = 0x10,
			Components = 0x20,
			Others = 0x40
		}

		private Sirenix.Utilities.Editor.ObjectFieldAlignment? squareUnityObjectAlignment;

		private UnityObjectType? squareUnityObjectEnableFor;

		private QuaternionDrawMode? quaternionDrawMode;

		private float? squareUnityObjectFieldHeight;

		private bool? showPrefabModificationsDisabledMessage;

		private bool? hidePagingWhileOnlyOnePage;

		private bool? useNewImprovedEnumDropdown;

		private bool? hidePagingWhileCollapsed;

		private bool? showMonoScriptInEditor;

		private bool? hideFoldoutWhileEmpty;

		private bool? showPagingInTables;

		private bool? openListsByDefault;

		private bool? drawEnumTypeTitle;

		private bool? showExpandButton;

		private bool? showIndexLabels;

		private bool? showItemCount;

		private int? maxRecursiveDrawDepth;

		private int? numberOfItemsPerPage;

		private bool? precomputeTypeMatching;

		private bool? showPrefabModifiedValueBar;

		private static readonly ColorPref listItemColorEvenDarkSkinPref = new ColorPref("GeneralDrawerConfig.ListItemColorEvenDarkSkin", new Color(0.235f, 0.235f, 0.235f, 1f));

		private static readonly ColorPref listItemColorEvenLightSkinPref = new ColorPref("GeneralDrawerConfig.ListItemColorEvenLightSkin", new Color(0.838f, 0.838f, 0.838f, 1f));

		private static readonly ColorPref listItemColorOddDarkSkinPref = new ColorPref("GeneralDrawerConfig.ListItemColorOddDarkSkin", new Color(0.2f, 0.2f, 0.2f, 1f));

		private static readonly ColorPref listItemColorOddLightSkinPref = new ColorPref("GeneralDrawerConfig.ListItemColorOddLightSkin", new Color(0.788f, 0.788f, 0.788f, 1f));

		[NonSerialized]
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyOrder(20f)]
		private List<int> exampleList = new List<int>();

		/// <summary>
		/// Specify whether or not the script selector above components should be drawn.
		/// </summary>
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify whether or not the script selector above components should be drawn")]
		public bool ShowMonoScriptInEditor
		{
			get
			{
				if (!showMonoScriptInEditor.HasValue)
				{
					showMonoScriptInEditor = EditorPrefs.GetBool("GeneralDrawerConfig.ShowMonoScriptInEditor", true);
				}
				return showMonoScriptInEditor.Value;
			}
			set
			{
				showMonoScriptInEditor = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowMonoScriptInEditor", value);
			}
		}

		/// <summary>
		/// Specify whether or not the warning for properties that do not support prefab modifications should be shown in the inspector.
		/// </summary>
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify whether or not the warning for properties that do not support prefab modifications should be shown in the inspector")]
		public bool ShowPrefabModificationsDisabledMessage
		{
			get
			{
				if (!showPrefabModificationsDisabledMessage.HasValue)
				{
					showPrefabModificationsDisabledMessage = EditorPrefs.GetBool("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage", true);
				}
				return showPrefabModificationsDisabledMessage.Value;
			}
			set
			{
				showPrefabModificationsDisabledMessage = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage", value);
			}
		}

		/// <summary>
		/// Specify whether or not the warning for properties that do not support prefab modifications should be shown in the inspector.
		/// </summary>
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify whether or not a blue bar should be drawn next to modified prefab values")]
		[LabelText("Show Blue Prefab Value Modified Bar")]
		public bool ShowPrefabModifiedValueBar
		{
			get
			{
				if (!showPrefabModifiedValueBar.HasValue)
				{
					showPrefabModifiedValueBar = EditorPrefs.GetBool("GeneralDrawerConfig.ShowPrefabModifiedValueBar", true);
				}
				return showPrefabModifiedValueBar.Value;
			}
			set
			{
				showPrefabModifiedValueBar = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowPrefabModifiedValueBar", value);
			}
		}

		/// <summary>
		/// Specifies the maximum depth to which a property can draw itself recursively before the system refuses to draw it any deeper.
		/// </summary>
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specifies the maximum depth to which a property can draw itself recursively before the system refuses to draw it any deeper.")]
		[MinValue(1.0)]
		[MaxValue(100.0)]
		public int MaxRecursiveDrawDepth
		{
			get
			{
				if (!maxRecursiveDrawDepth.HasValue)
				{
					maxRecursiveDrawDepth = EditorPrefs.GetInt("GeneralDrawerConfig.MaxRecursiveDrawDepth", 10);
				}
				return Mathf.Clamp(maxRecursiveDrawDepth.Value, 1, 100);
			}
			set
			{
				value = Mathf.Clamp(value, 1, 100);
				maxRecursiveDrawDepth = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.MaxRecursiveDrawDepth", value);
			}
		}

		/// <summary>
		/// If set to true, most foldouts throughout the inspector will be expanded by default.
		/// </summary>
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("If set to true, most foldouts throughout the inspector will be expanded by default.")]
		public bool ExpandFoldoutByDefault
		{
			get
			{
				return SirenixEditorGUI.ExpandFoldoutByDefault;
			}
			set
			{
				SirenixEditorGUI.ExpandFoldoutByDefault = value;
			}
		}

		/// <summary>
		/// If set to true, buttons will show the result values from invoking them in the inspector by default.
		/// </summary>
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("If set to true, buttons will show the result values from invoking them in the inspector by default.")]
		public bool ShowButtonResultsByDefault
		{
			get
			{
				return SirenixEditorGUI.ShowButtonResultsByDefault;
			}
			set
			{
				SirenixEditorGUI.ShowButtonResultsByDefault = value;
			}
		}

		/// <summary>
		/// If set to true, type matching for things such as drawers will be precomputed in a separate thread based on a cache from the previously loaded AppDomain that is stored on disk in the Project's Temp folder, resulting in some overall speedups in some cases when doing first-time type matching. Requires a recompile/project reload before it takes any effect.
		/// </summary>
		[TabGroup("General", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("If set to true, type matching for things such as drawers will be precomputed in a separate thread based on a cache from the previously loaded AppDomain that is stored on disk in the Project's Temp folder, resulting in some overall speedups in some cases when doing first-time type matching. Requires a recompile/project reload before it takes any effect.")]
		[SuffixLabel("EXPERIMENTAL", false)]
		public bool PrecomputeTypeMatching
		{
			get
			{
				if (!precomputeTypeMatching.HasValue)
				{
					precomputeTypeMatching = EditorPrefs.GetBool("GeneralDrawerConfig.PrecomputeTypeMatching", false);
				}
				return precomputeTypeMatching.Value;
			}
			set
			{
				precomputeTypeMatching = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.PrecomputeTypeMatching", value);
			}
		}

		/// <summary>
		/// Specify the animation speed for most foldouts throughout the inspector.
		/// </summary>
		[TabGroup("Animations", false, 0f)]
		[ShowInInspector]
		[PropertyRange(0.0010000000474974513, 4.0)]
		[PropertyTooltip("Specify the animation speed for most foldouts throughout the inspector.")]
		public float GUIFoldoutAnimationDuration
		{
			get
			{
				return SirenixEditorGUI.DefaultFadeGroupDuration;
			}
			set
			{
				SirenixEditorGUI.DefaultFadeGroupDuration = value;
			}
		}

		/// <summary>
		/// Specify the shaking duration for most shaking animations throughout the inspector.
		/// </summary>
		[TabGroup("Animations", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify the shaking duration for most shaking animations throughout the inspector.")]
		[PropertyRange(0.0, 4.0)]
		public float ShakingAnimationDuration
		{
			get
			{
				return SirenixEditorGUI.ShakingAnimationDuration;
			}
			set
			{
				SirenixEditorGUI.ShakingAnimationDuration = value;
			}
		}

		/// <summary>
		/// Specify the animation speed for <see cref="T:Sirenix.OdinInspector.TabGroupAttribute" />
		/// </summary>
		[TabGroup("Animations", false, 0f)]
		[ShowInInspector]
		[PropertyRange(0.0010000000474974513, 4.0)]
		public float TabPageSlideAnimationDuration
		{
			get
			{
				return SirenixEditorGUI.TabPageSlideAnimationDuration;
			}
			set
			{
				SirenixEditorGUI.TabPageSlideAnimationDuration = value;
			}
		}

		/// <summary>
		/// When <c>true</c> the component labels, for vector fields, will be hidden when the field is too narrow.
		/// </summary>
		[TabGroup("Structs", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("When on the component labels, for vector fields, will be hidden when the field is too narrow.\nThis allows more space for the actual component fields themselves.")]
		public bool ResponsiveVectorComponentFields
		{
			get
			{
				return SirenixEditorFields.ResponsiveVectorComponentFields;
			}
			set
			{
				SirenixEditorFields.ResponsiveVectorComponentFields = value;
			}
		}

		/// <summary>
		/// Specify how the Quaternion struct should be shown in the inspector.
		/// </summary>
		[TabGroup("Structs", false, 0f)]
		[ShowInInspector]
		[EnumToggleButtons]
		[PropertyTooltip("Current mode for how quaternions are edited in the inspector.\n\nEuler: Rotations as yaw, pitch and roll.\n\nAngle axis: Rotations as a axis of rotation, and an angle of rotation around that axis.\n\nRaw: Directly edit the x, y, z and w components of a quaternion.")]
		public QuaternionDrawMode QuaternionDrawMode
		{
			get
			{
				if (!quaternionDrawMode.HasValue)
				{
					quaternionDrawMode = (QuaternionDrawMode)EditorPrefs.GetInt("GeneralDrawerConfig.QuaternionDrawMode", 0);
				}
				return quaternionDrawMode.Value;
			}
			set
			{
				quaternionDrawMode = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.QuaternionDrawMode", (int)value);
			}
		}

		[TabGroup("Structs", false, 0f)]
		[ShowInInspector]
		private Quaternion ExampleQuaternion { get; set; }

		[TabGroup("Structs", false, 0f)]
		[ShowInInspector]
		private Vector3 ExampleVector { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [use improved enum drop down].
		/// </summary>
		[TabGroup("Enums", false, 0f)]
		[ShowInInspector]
		public bool UseImprovedEnumDropDown
		{
			get
			{
				if (!useNewImprovedEnumDropdown.HasValue)
				{
					useNewImprovedEnumDropdown = EditorPrefs.GetBool("GeneralDrawerConfig.UseImprovedEnumDropDown", true);
				}
				return useNewImprovedEnumDropdown.Value;
			}
			set
			{
				useNewImprovedEnumDropdown = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.UseImprovedEnumDropDown", value);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether [use improved enum drop down].
		/// </summary>
		[TabGroup("Enums", false, 0f)]
		[ShowInInspector]
		[EnableIf("UseImprovedEnumDropDown")]
		public bool DrawEnumTypeTitle
		{
			get
			{
				if (!drawEnumTypeTitle.HasValue)
				{
					drawEnumTypeTitle = EditorPrefs.GetBool("GeneralDrawerConfig.DrawEnumTypeTitle", false);
				}
				return drawEnumTypeTitle.Value;
			}
			set
			{
				drawEnumTypeTitle = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.DrawEnumTypeTitle", value);
			}
		}

		[TabGroup("Enums", false, 0f, Paddingless = true)]
		[ShowInInspector]
		private KeyCode ExampleEnum { get; set; }

		[TabGroup("Enums", false, 0f)]
		[ShowInInspector]
		private AssemblyTypeFlags ExampleFlagEnum { get; set; }

		/// <summary>
		/// Specify whether or not a list should hide the foldout triangle when the list is empty.
		/// </summary>
		[InfoBox("All collection settings - and more - can be overridden for individual collections using the ListDrawerSettings attribute.", InfoMessageType.Info, null)]
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specifies whether all tables should include paging, or ")]
		public bool ShowPagingInTables
		{
			get
			{
				if (!showPagingInTables.HasValue)
				{
					showPagingInTables = EditorPrefs.GetBool("GeneralDrawerConfig.ShowPagingInTables", false);
				}
				return showPagingInTables.Value;
			}
			set
			{
				showPagingInTables = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowPagingInTables", value);
			}
		}

		/// <summary>
		/// Specifies whether a list should hide the foldout triangle when the list is empty.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specifies whether a list should hide the foldout triangle when the list is empty.")]
		public bool HideFoldoutWhileEmpty
		{
			get
			{
				if (!hideFoldoutWhileEmpty.HasValue)
				{
					hideFoldoutWhileEmpty = EditorPrefs.GetBool("GeneralDrawerConfig.HideFoldoutWhileEmpty", true);
				}
				return hideFoldoutWhileEmpty.Value;
			}
			set
			{
				hideFoldoutWhileEmpty = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.HideFoldoutWhileEmpty", value);
			}
		}

		/// <summary>
		/// Specify whether or not lists should hide the paging buttons when the list is collapsed.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify whether or not lists should hide the paging buttons when the list is collapsed.")]
		public bool HidePagingWhileCollapsed
		{
			get
			{
				if (!hidePagingWhileCollapsed.HasValue)
				{
					hidePagingWhileCollapsed = EditorPrefs.GetBool("GeneralDrawerConfig.HidePagingWhileCollapsed", true);
				}
				return hidePagingWhileCollapsed.Value;
			}
			set
			{
				hidePagingWhileCollapsed = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.HidePagingWhileCollapsed", value);
			}
		}

		/// <summary>
		/// Specify whether or not lists should hide the paging buttons when there is only one page.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		public bool HidePagingWhileOnlyOnePage
		{
			get
			{
				if (!hidePagingWhileOnlyOnePage.HasValue)
				{
					hidePagingWhileOnlyOnePage = EditorPrefs.GetBool("GeneralDrawerConfig.HidePagingWhileOnlyOnePage", true);
				}
				return hidePagingWhileOnlyOnePage.Value;
			}
			set
			{
				hidePagingWhileOnlyOnePage = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.HidePagingWhileOnlyOnePage", value);
			}
		}

		/// <summary>
		/// Specify the number of elements drawn per page.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[OnValueChanged("ResizeExampleList", false)]
		[MaxValue(500.0)]
		[MinValue(2.0)]
		[PropertyTooltip("Specify the number of elements drawn per page.")]
		[LabelText("Number Of Items Per Page")]
		public int NumberOfItemsPrPage
		{
			get
			{
				if (!numberOfItemsPerPage.HasValue)
				{
					numberOfItemsPerPage = EditorPrefs.GetInt("GeneralDrawerConfig.NumberOfItemsPrPage", 15);
				}
				return numberOfItemsPerPage.Value;
			}
			set
			{
				numberOfItemsPerPage = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.NumberOfItemsPrPage", value);
			}
		}

		/// <summary>
		/// Specify whether or not lists should be expanded or collapsed by default.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify whether or not lists should be expanded or collapsed by default.")]
		public bool OpenListsByDefault
		{
			get
			{
				if (!openListsByDefault.HasValue)
				{
					openListsByDefault = EditorPrefs.GetBool("GeneralDrawerConfig.OpenListsByDefault", false);
				}
				return openListsByDefault.Value;
			}
			set
			{
				openListsByDefault = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.OpenListsByDefault", value);
			}
		}

		/// <summary>
		/// Specify whether or not to include a button which expands the list, showing all pages at once.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify whether or not to include a button which expands the list, showing all pages at once")]
		public bool ShowExpandButton
		{
			get
			{
				if (!showExpandButton.HasValue)
				{
					showExpandButton = EditorPrefs.GetBool("GeneralDrawerConfig.ShowExpandButton", true);
				}
				return showExpandButton.Value;
			}
			set
			{
				showExpandButton = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowExpandButton", value);
			}
		}

		/// <summary>
		/// Specify whether or not lists should show item count.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify whether or not lists should show item count.")]
		public bool ShowItemCount
		{
			get
			{
				if (!showItemCount.HasValue)
				{
					showItemCount = EditorPrefs.GetBool("GeneralDrawerConfig.ShowItemCount", true);
				}
				return showItemCount.Value;
			}
			set
			{
				showItemCount = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowItemCount", value);
			}
		}

		/// <summary>
		/// Specify whether or not lists should show item count.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify whether or not lists should show item count.")]
		public bool ShowIndexLabels
		{
			get
			{
				if (!showIndexLabels.HasValue)
				{
					showIndexLabels = EditorPrefs.GetBool("GeneralDrawerConfig.ShowIndexLabels", false);
				}
				return showIndexLabels.Value;
			}
			set
			{
				showIndexLabels = value;
				EditorPrefs.SetBool("GeneralDrawerConfig.ShowIndexLabels", value);
			}
		}

		/// <summary>
		/// Specify the color of even list elements when in the dark skin.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify the color of even list elements when in the dark skin.")]
		public Color ListItemColorEvenDarkSkin
		{
			get
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				return listItemColorEvenDarkSkinPref.Value;
			}
			set
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				if (listItemColorEvenDarkSkinPref.Value != value)
				{
					listItemColorEvenDarkSkinPref.Value = value;
					if (EditorGUIUtility.get_isProSkin())
					{
						SirenixGUIStyles.ListItemColorEven = value;
					}
				}
			}
		}

		/// <summary>
		/// Specify the color of odd list elements when in the dark skin.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify the color of odd list elements when in the dark skin.")]
		public Color ListItemColorOddDarkSkin
		{
			get
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				return listItemColorOddDarkSkinPref.Value;
			}
			set
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				if (listItemColorOddDarkSkinPref.Value != value)
				{
					listItemColorOddDarkSkinPref.Value = value;
					if (EditorGUIUtility.get_isProSkin())
					{
						SirenixGUIStyles.ListItemColorOdd = value;
					}
				}
			}
		}

		/// <summary>
		/// Specify the color of even list elements when in the light skin.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify the color of even list elements when in the light skin.")]
		public Color ListItemColorEvenLightSkin
		{
			get
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				return listItemColorEvenLightSkinPref.Value;
			}
			set
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				if (listItemColorEvenLightSkinPref.Value != value)
				{
					listItemColorEvenLightSkinPref.Value = value;
					if (!EditorGUIUtility.get_isProSkin())
					{
						SirenixGUIStyles.ListItemColorEven = value;
					}
				}
			}
		}

		/// <summary>
		/// Specify the color of odd list elements when in the light skin.
		/// </summary>
		[TabGroup("Collections", false, 0f)]
		[ShowInInspector]
		[PropertyTooltip("Specify the color of odd list elements when in the light skin.")]
		public Color ListItemColorOddLightSkin
		{
			get
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				return listItemColorOddLightSkinPref.Value;
			}
			set
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				if (listItemColorOddLightSkinPref.Value != value)
				{
					listItemColorOddLightSkinPref.Value = value;
					if (!EditorGUIUtility.get_isProSkin())
					{
						SirenixGUIStyles.ListItemColorOdd = value;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the default size of the preview object field.
		/// </summary>
		[TabGroup("Object Fields", false, 0f)]
		[ShowInInspector]
		public float SquareUnityObjectFieldHeight
		{
			get
			{
				if (!squareUnityObjectFieldHeight.HasValue)
				{
					squareUnityObjectFieldHeight = EditorPrefs.GetFloat("GeneralDrawerConfig.squareUnityObjectFieldHeight", 50f);
				}
				return squareUnityObjectFieldHeight.Value;
			}
			set
			{
				squareUnityObjectFieldHeight = value;
				EditorPrefs.SetFloat("GeneralDrawerConfig.squareUnityObjectFieldHeight", value);
			}
		}

		/// <summary>
		/// Gets or sets the default alignment of the preview object field.
		/// </summary>
		[TabGroup("Object Fields", false, 0f)]
		[ShowInInspector]
		[EnumToggleButtons]
		public Sirenix.Utilities.Editor.ObjectFieldAlignment SquareUnityObjectAlignment
		{
			get
			{
				if (!squareUnityObjectAlignment.HasValue)
				{
					squareUnityObjectAlignment = (Sirenix.Utilities.Editor.ObjectFieldAlignment)EditorPrefs.GetInt("GeneralDrawerConfig.squareUnityObjectAlignment", 2);
				}
				return squareUnityObjectAlignment.Value;
			}
			set
			{
				squareUnityObjectAlignment = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.squareUnityObjectAlignment", (int)value);
			}
		}

		/// <summary>
		/// Gets or sets which types should be drawn by default by the preview object field.
		/// </summary>
		[LabelText("Enable Globally For")]
		[TabGroup("Object Fields", false, 0f)]
		[ShowInInspector]
		public UnityObjectType SquareUnityObjectEnableFor
		{
			get
			{
				if (!squareUnityObjectEnableFor.HasValue)
				{
					squareUnityObjectEnableFor = (UnityObjectType)EditorPrefs.GetInt("GeneralDrawerConfig.squareUnityObjectEnableFor", 0);
				}
				return squareUnityObjectEnableFor.Value;
			}
			set
			{
				squareUnityObjectEnableFor = value;
				EditorPrefs.SetInt("GeneralDrawerConfig.squareUnityObjectEnableFor", (int)value);
			}
		}

		[PreviewField]
		[TabGroup("Object Fields", false, 0f)]
		[ShowInInspector]
		private Object ExampleObject { get; set; }

		private void ResizeExampleList()
		{
			exampleList = Enumerable.Range(0, Math.Max(10, (int)((float)NumberOfItemsPrPage * (float)Math.PI))).ToList();
		}

		[OnInspectorGUI]
		[PropertyOrder(999f)]
		private void DrawFlexibleSpace()
		{
			GUILayout.FlexibleSpace();
		}

		/// <summary>
		/// Resets all settings to default.
		/// </summary>
		[Button(ButtonSizes.Large)]
		[PropertyOrder(1000f)]
		public void ResetToDefault()
		{
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowMonoScriptInEditor"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowMonoScriptInEditor");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.MaxRecursiveDrawDepth"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.MaxRecursiveDrawDepth");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.MarkObjectsDirtyOnButtonClick"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.MarkObjectsDirtyOnButtonClick");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.QuaternionDrawMode"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.QuaternionDrawMode");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.UseImprovedEnumDropDown"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.UseImprovedEnumDropDown");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.DrawEnumTypeTitle"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.DrawEnumTypeTitle");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.HideFoldoutWhileEmpty"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.HideFoldoutWhileEmpty");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.HidePagingWhileCollapsed"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.HidePagingWhileCollapsed");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.HidePagingWhileOnlyOnePage"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.HidePagingWhileOnlyOnePage");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.NumberOfItemsPrPage"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.NumberOfItemsPrPage");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.OpenListsByDefault"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.OpenListsByDefault");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowExpandButton"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowExpandButton");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowItemCount"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowItemCount");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowIndexLabels"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowIndexLabels");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.squareUnityObjectFieldHeight"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.squareUnityObjectFieldHeight");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.squareUnityObjectAlignment"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.squareUnityObjectAlignment");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.squareUnityObjectEnableFor"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.squareUnityObjectEnableFor");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowPagingInTables"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowPagingInTables");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.PrecomputeTypeMatching"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.PrecomputeTypeMatching");
			}
			if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowPrefabModifiedValueBar"))
			{
				EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowPrefabModifiedValueBar");
			}
			listItemColorEvenDarkSkinPref.Reset();
			listItemColorEvenLightSkinPref.Reset();
			listItemColorOddDarkSkinPref.Reset();
			listItemColorOddLightSkinPref.Reset();
			if (EditorGUIUtility.get_isProSkin())
			{
				SirenixGUIStyles.ListItemColorEven = ListItemColorEvenDarkSkin;
				SirenixGUIStyles.ListItemColorOdd = ListItemColorOddDarkSkin;
			}
			else
			{
				SirenixGUIStyles.ListItemColorEven = ListItemColorEvenLightSkin;
				SirenixGUIStyles.ListItemColorOdd = ListItemColorOddLightSkin;
			}
			showMonoScriptInEditor = null;
			hideFoldoutWhileEmpty = null;
			openListsByDefault = null;
			showItemCount = null;
			numberOfItemsPerPage = null;
			hidePagingWhileCollapsed = null;
			hidePagingWhileOnlyOnePage = null;
			showExpandButton = null;
			quaternionDrawMode = null;
			showPrefabModificationsDisabledMessage = null;
			maxRecursiveDrawDepth = null;
			squareUnityObjectFieldHeight = null;
			squareUnityObjectAlignment = null;
			showIndexLabels = null;
			useNewImprovedEnumDropdown = null;
			drawEnumTypeTitle = null;
			showPagingInTables = null;
			precomputeTypeMatching = null;
			showPrefabModifiedValueBar = null;
			EditorPrefs.DeleteKey("SirenixEditorGUI.DefaultFadeGroupDuration");
			EditorPrefs.DeleteKey("SirenixEditorGUI.TabPageSlideAnimationDuration");
			EditorPrefs.DeleteKey("SirenixEditorGUI.ShakingAnimationDuration");
			EditorPrefs.DeleteKey("SirenixEditorGUI.ExpandFoldoutByDefault");
			SirenixEditorGUI.DefaultFadeGroupDuration = 0.13f;
			SirenixEditorGUI.TabPageSlideAnimationDuration = 0.13f;
			SirenixEditorGUI.ShakingAnimationDuration = 0.5f;
			SirenixEditorGUI.ExpandFoldoutByDefault = false;
		}
	}
}
