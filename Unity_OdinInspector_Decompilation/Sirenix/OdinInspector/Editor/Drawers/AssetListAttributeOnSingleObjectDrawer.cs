using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 3001.0)]
	public class AssetListAttributeOnSingleObjectDrawer<TElement> : OdinAttributeDrawer<AssetListAttribute, TElement> where TElement : Object
	{
		private static readonly NamedValue[] customFilterMethodArgs = new NamedValue[1]
		{
			new NamedValue("asset", typeof(TElement))
		};

		private ValueResolver<bool> customFilterMethod;

		private List<Object> availableAssets = new List<Object>();

		private string[] tags;

		private string[] layerNames;

		private DirectoryInfo assetsFolderLocation;

		private string prettyPath;

		private bool isPopulated;

		private double maxSearchDurationPrFrameInMS = 1.0;

		private int numberOfResultsToSearch;

		private int totalSearchCount;

		private int currentSearchingIndex;

		private IEnumerator populateListRoutine;

		private static GUIStyle padding;

		private static GUIStyle Padding
		{
			get
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Expected O, but got Unknown
				//IL_0020: Expected O, but got Unknown
				if (padding == null)
				{
					GUIStyle val = new GUIStyle();
					val.set_padding(new RectOffset(5, 5, 3, 3));
					padding = val;
				}
				return padding;
			}
		}

		protected override void Initialize()
		{
			IPropertyValueEntry<TElement> propertyValueEntry = base.ValueEntry;
			AssetListAttribute assetListAttribute = base.Attribute;
			tags = ((assetListAttribute.Tags != null) ? (from i in assetListAttribute.Tags.Trim().Split(',')
				select i.Trim()).ToArray() : null);
			layerNames = ((assetListAttribute.LayerNames != null) ? (from i in assetListAttribute.LayerNames.Trim().Split(',')
				select i.Trim()).ToArray() : null);
			if (assetListAttribute.Path != null)
			{
				string text = assetListAttribute.Path.Trim('/', ' ');
				text = "Assets/" + text + "/";
				text = Application.get_dataPath() + "/" + text;
				assetsFolderLocation = new DirectoryInfo(text);
				text = assetListAttribute.Path.TrimStart('/').TrimEnd('/');
				prettyPath = "/" + text.TrimStart('/');
			}
			if (assetListAttribute.CustomFilterMethod != null)
			{
				customFilterMethod = ValueResolver.Get<bool>(base.Property, assetListAttribute.CustomFilterMethod, customFilterMethodArgs);
			}
			if (Event.get_current() != null)
			{
				maxSearchDurationPrFrameInMS = 20.0;
				EnsureListPopulation();
			}
			maxSearchDurationPrFrameInMS = 1.0;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			//IL_0228: Expected O, but got Unknown
			//IL_0363: Unknown result type (might be due to invalid IL or missing references)
			//IL_0383: Unknown result type (might be due to invalid IL or missing references)
			//IL_038d: Expected O, but got Unknown
			//IL_038d: Expected O, but got Unknown
			//IL_0402: Unknown result type (might be due to invalid IL or missing references)
			//IL_0414: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<TElement> propertyValueEntry = base.ValueEntry;
			AssetListAttribute assetListAttribute = base.Attribute;
			Object val = (Object)propertyValueEntry.WeakSmartValue;
			if (customFilterMethod != null && customFilterMethod.HasError)
			{
				customFilterMethod.DrawError();
			}
			else
			{
				EnsureListPopulation();
			}
			SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
			SirenixEditorGUI.BeginHorizontalToolbar();
			if (label != null)
			{
				GUILayout.Label(label, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			GUILayout.FlexibleSpace();
			if (prettyPath != null)
			{
				GUILayout.Label(prettyPath, SirenixGUIStyles.RightAlignedGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				SirenixEditorGUI.VerticalLineSeparator();
			}
			if (isPopulated)
			{
				GUILayout.Label(availableAssets.Count + " items", SirenixGUIStyles.RightAlignedGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUIHelper.PushGUIEnabled(GUI.get_enabled() && availableAssets.Count > 0 && (customFilterMethod == null || !customFilterMethod.HasError));
			}
			else
			{
				GUILayout.Label("Scanning " + currentSearchingIndex + " / " + numberOfResultsToSearch, SirenixGUIStyles.RightAlignedGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUIHelper.PushGUIEnabled(enabled: false);
			}
			SirenixEditorGUI.VerticalLineSeparator();
			bool flag = propertyValueEntry.Property.ParentValues.Count > 1;
			if (!flag)
			{
				int num = availableAssets.IndexOf(val) + 1;
				if (num > 0)
				{
					GUILayout.Label(num.ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				GUILayout.Label("-", SirenixGUIStyles.RightAlignedGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleLeft) && isPopulated)
			{
				int num2 = availableAssets.IndexOf(val) - 1;
				num2 = ((num2 < 0) ? (availableAssets.Count - 1) : num2);
				propertyValueEntry.WeakSmartValue = availableAssets[num2];
			}
			if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleDown) && isPopulated)
			{
				GenericMenu val2 = new GenericMenu();
				Object val3 = val;
				int num3 = 40;
				bool flag2 = availableAssets.Count > 50;
				string text = "";
				List<Object> list = availableAssets;
				object weakSmartValue = propertyValueEntry.WeakSmartValue;
				int num4 = list.IndexOf(weakSmartValue as Object) / num3;
				for (int i = 0; i < availableAssets.Count; i++)
				{
					Object obj = availableAssets[i];
					if (!(obj != (Object)null))
					{
						continue;
					}
					string assetPath = AssetDatabase.GetAssetPath(obj);
					string text2 = (string.IsNullOrEmpty(assetPath) ? obj.get_name() : assetPath.Substring(7).Replace("/", "\\"));
					IPropertyValueEntry<TElement> localEntry = propertyValueEntry;
					if (flag2)
					{
						int num5 = i / num3;
						text = num5 * num3 + " - " + Mathf.Min((num5 + 1) * num3, availableAssets.Count - 1);
						if (num4 == num5)
						{
							text += " (contains selected)";
						}
						text += "/";
					}
					val2.AddItem(new GUIContent(text + text2), obj == val3, (MenuFunction)delegate
					{
						localEntry.Property.Tree.DelayActionUntilRepaint(delegate
						{
							localEntry.WeakSmartValue = obj;
						});
					});
				}
				val2.ShowAsContext();
			}
			if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleRight) && isPopulated)
			{
				int num6 = availableAssets.IndexOf(val) + 1;
				propertyValueEntry.WeakSmartValue = availableAssets[num6 % availableAssets.Count];
			}
			GUIHelper.PopGUIEnabled();
			SirenixEditorGUI.EndHorizontalToolbar();
			SirenixEditorGUI.BeginVerticalList(true, true);
			SirenixEditorGUI.BeginListItem(false, padding);
			CallNextDrawer(null);
			SirenixEditorGUI.EndListItem();
			SirenixEditorGUI.EndVerticalList();
			SirenixEditorGUI.EndIndentedVertical();
		}

		private IEnumerator PopulateListRoutine()
		{
			while (true)
			{
				if (isPopulated)
				{
					yield return null;
					continue;
				}
				HashSet<Object> seenObjects = new HashSet<Object>();
				int[] layers = ((layerNames != null) ? layerNames.Select((string l) => LayerMask.NameToLayer(l)).ToArray() : null);
				availableAssets.Clear();
				IEnumerable<AssetUtilities.AssetSearchResult> enumerable = ((prettyPath != null) ? AssetUtilities.GetAllAssetsOfTypeWithProgress(base.Property.ValueEntry.BaseValueType, "Assets/" + prettyPath.TrimStart('/')) : AssetUtilities.GetAllAssetsOfTypeWithProgress(base.Property.ValueEntry.BaseValueType));
				Stopwatch sw = new Stopwatch();
				sw.Start();
				foreach (AssetUtilities.AssetSearchResult p in enumerable)
				{
					if (sw.Elapsed.TotalMilliseconds > maxSearchDurationPrFrameInMS)
					{
						numberOfResultsToSearch = p.NumberOfResults;
						currentSearchingIndex = p.CurrentIndex;
						GUIHelper.RequestRepaint();
						yield return null;
						sw.Reset();
						sw.Start();
					}
					Object asset = p.Asset;
					if (!(asset != (Object)null) || !seenObjects.Add(asset))
					{
						continue;
					}
					GameObject val = (GameObject)(((Object)(asset as Component) != (Object)null) ? ((object)(asset as Component).get_gameObject()) : ((object)(((Object)(asset as GameObject) == (Object)null) ? ((GameObject)null) : (asset as GameObject))));
					string text = (((Object)(object)val == (Object)null) ? asset.get_name() : ((Object)val).get_name());
					if (base.Attribute.AssetNamePrefix != null && !text.StartsWith(base.Attribute.AssetNamePrefix, StringComparison.InvariantCultureIgnoreCase))
					{
						continue;
					}
					if (assetsFolderLocation != null)
					{
						DirectoryInfo subDir = new DirectoryInfo(Path.GetDirectoryName(Application.get_dataPath() + "/" + AssetDatabase.GetAssetPath(asset)));
						if (!assetsFolderLocation.HasSubDirectory(subDir))
						{
							continue;
						}
					}
					if ((layerNames != null && (Object)(object)val == (Object)null) || (tags != null && (Object)(object)val == (Object)null) || ((Object)(object)val != (Object)null && tags != null && !tags.Contains(val.get_tag())) || ((Object)(object)val != (Object)null && layerNames != null && !layers.Contains(val.get_layer())))
					{
						continue;
					}
					if (customFilterMethod != null)
					{
						customFilterMethod.Context.NamedValues.Set("asset", asset);
						if (!customFilterMethod.GetValue())
						{
							continue;
						}
					}
					availableAssets.Add(asset);
				}
				isPopulated = true;
				GUIHelper.RequestRepaint();
				yield return null;
			}
		}

		public void EnsureListPopulation()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			if ((int)Event.get_current().get_type() == 8)
			{
				if (populateListRoutine == null)
				{
					populateListRoutine = PopulateListRoutine();
				}
				populateListRoutine.MoveNext();
			}
		}
	}
}
