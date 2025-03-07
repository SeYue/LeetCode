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
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.AssetListAttribute" />.
	/// Displays a configurable list of assets, where each item can be enabled or disabled.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.AssetListAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.AssetsOnlyAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.SceneObjectsOnlyAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.RequiredAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />
	[DrawerPriority(DrawerPriorityLevel.AttributePriority)]
	public sealed class AssetListAttributeDrawer<TList, TElement> : OdinAttributeDrawer<AssetListAttribute, TList>, IDefinesGenericMenuItems where TList : IList<TElement> where TElement : Object
	{
		[Serializable]
		[ShowOdinSerializedPropertiesInInspector]
		private class AssetList
		{
			[HideInInspector]
			public bool AutoPopulate;

			[HideInInspector]
			public string AssetNamePrefix;

			[HideInInspector]
			public string[] LayerNames;

			[HideInInspector]
			public string[] Tags;

			[HideInInspector]
			public IPropertyValueEntry<TList> List;

			[HideInInspector]
			public IOrderedCollectionResolver CollectionResolver;

			[HideInInspector]
			public DirectoryInfo AssetsFolderLocation;

			[HideInInspector]
			public string PrettyPath;

			[HideInInspector]
			public ValueResolver<bool> CustomFilterMethod;

			[HideInInspector]
			public InspectorProperty Property;

			[SerializeField]
			[ListDrawerSettings(IsReadOnly = true, DraggableItems = false, OnTitleBarGUI = "OnListTitlebarGUI", ShowItemCount = false)]
			[DisableContextMenu(true, true)]
			[HideReferenceObjectPicker]
			private List<ToggleableAsset> toggleableAssets = new List<ToggleableAsset>();

			[SerializeField]
			[HideInInspector]
			private HashSet<TElement> toggledAssets = new HashSet<TElement>();

			[SerializeField]
			[HideInInspector]
			private Dictionary<TElement, ToggleableAsset> toggleableAssetLookup = new Dictionary<TElement, ToggleableAsset>();

			[NonSerialized]
			public bool IsPopulated;

			[NonSerialized]
			public double MaxSearchDurationPrFrameInMS = 1.0;

			[NonSerialized]
			public int NumberOfResultsToSearch;

			[NonSerialized]
			public int TotalSearchCount;

			[NonSerialized]
			public int CurrentSearchingIndex;

			[NonSerialized]
			private IEnumerator populateListRoutine;

			public List<ToggleableAsset> ToggleableAssets => toggleableAssets;

			private IEnumerator PopulateListRoutine()
			{
				while (true)
				{
					if (IsPopulated)
					{
						yield return null;
						continue;
					}
					HashSet<Object> seenObjects = new HashSet<Object>();
					toggleableAssets.Clear();
					toggleableAssetLookup.Clear();
					IEnumerable<AssetUtilities.AssetSearchResult> enumerable = ((PrettyPath != null) ? AssetUtilities.GetAllAssetsOfTypeWithProgress(typeof(TElement), "Assets/" + PrettyPath.TrimStart('/')) : AssetUtilities.GetAllAssetsOfTypeWithProgress(typeof(TElement)));
					int[] layers = ((LayerNames != null) ? LayerNames.Select((string l) => LayerMask.NameToLayer(l)).ToArray() : null);
					Stopwatch sw = new Stopwatch();
					sw.Start();
					foreach (AssetUtilities.AssetSearchResult p in enumerable)
					{
						if (sw.Elapsed.TotalMilliseconds > MaxSearchDurationPrFrameInMS)
						{
							NumberOfResultsToSearch = p.NumberOfResults;
							CurrentSearchingIndex = p.CurrentIndex;
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
						if (AssetNamePrefix != null && !text.StartsWith(AssetNamePrefix, StringComparison.InvariantCultureIgnoreCase))
						{
							continue;
						}
						if (AssetsFolderLocation != null)
						{
							DirectoryInfo subDir = new DirectoryInfo(Path.GetDirectoryName(Application.get_dataPath() + "/" + AssetDatabase.GetAssetPath(asset)));
							if (!AssetsFolderLocation.HasSubDirectory(subDir))
							{
								continue;
							}
						}
						if ((LayerNames != null && (Object)(object)val == (Object)null) || (Tags != null && (Object)(object)val == (Object)null) || ((Object)(object)val != (Object)null && Tags != null && !Tags.Contains(val.get_tag())) || ((Object)(object)val != (Object)null && LayerNames != null && !layers.Contains(val.get_layer())) || toggleableAssetLookup.ContainsKey((TElement)(asset as TElement)))
						{
							continue;
						}
						if (CustomFilterMethod != null)
						{
							CustomFilterMethod.Context.NamedValues.Set("asset", asset);
							if (!CustomFilterMethod.GetValue())
							{
								continue;
							}
						}
						ToggleableAsset toggleableAsset = new ToggleableAsset((TElement)(asset as TElement), AutoPopulate);
						toggleableAssets.Add(toggleableAsset);
						toggleableAssetLookup.Add((TElement)(asset as TElement), toggleableAsset);
					}
					SetToggleValues();
					IsPopulated = true;
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

			public void SetToggleValues(int startIndex = 0)
			{
				if (List.SmartValue == null)
				{
					return;
				}
				for (int i = startIndex; i < toggleableAssets.Count; i++)
				{
					if (toggleableAssets[i] == null || (Object)(object)toggleableAssets[i].Object == (Object)null)
					{
						Rescan();
						break;
					}
					toggleableAssets[i].Toggled = false;
				}
				for (int num = List.SmartValue.Count - 1; num >= startIndex; num--)
				{
					TElement val = List.SmartValue[num];
					ToggleableAsset value;
					if ((Object)(object)val == (Object)null)
					{
						CollectionResolver.QueueRemoveAt(num);
					}
					else if (toggleableAssetLookup.TryGetValue(val, out value))
					{
						value.Toggled = true;
					}
					else if (IsPopulated)
					{
						CollectionResolver.QueueRemoveAt(num);
					}
				}
			}

			public void Rescan()
			{
				IsPopulated = false;
			}

			private void OnListTitlebarGUI()
			{
				if (PrettyPath != null)
				{
					GUILayout.Label(PrettyPath, SirenixGUIStyles.RightAlignedGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
					SirenixEditorGUI.VerticalLineSeparator();
				}
				if (IsPopulated)
				{
					GUILayout.Label(List.SmartValue.Count + " / " + toggleableAssets.Count, SirenixGUIStyles.CenteredGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
				else
				{
					GUILayout.Label("Scanning " + CurrentSearchingIndex + " / " + NumberOfResultsToSearch, SirenixGUIStyles.RightAlignedGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
				bool flag = !IsPopulated;
				if (flag)
				{
					GUIHelper.PushGUIEnabled(enabled: false);
				}
				if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh) && IsPopulated)
				{
					Rescan();
				}
				if (AssetUtilities.CanCreateNewAsset<TElement>() && SirenixEditorGUI.ToolbarButton(EditorIcons.Plus) && IsPopulated)
				{
					string text = PrettyPath;
					if (text == null)
					{
						TElement val = ((List.SmartValue.Count > 0) ? List.SmartValue[List.SmartValue.Count - 1] : default(TElement));
						if ((Object)(object)val == (Object)null)
						{
							ToggleableAsset toggleableAsset = toggleableAssets.LastOrDefault();
							if (toggleableAsset != null)
							{
								val = toggleableAsset.Object;
							}
						}
						if ((Object)(object)val != (Object)null)
						{
							text = AssetUtilities.GetAssetLocation((Object)(object)val);
						}
					}
					AssetUtilities.CreateNewAsset<TElement>(text, null);
					Rescan();
				}
				if (flag)
				{
					GUIHelper.PopGUIEnabled();
				}
			}

			public void UpdateList()
			{
				UpdateList(includeAll: false);
			}

			public void UpdateList(bool includeAll)
			{
				if (List.SmartValue == null)
				{
					return;
				}
				toggledAssets.Clear();
				for (int i = 0; i < toggleableAssets.Count; i++)
				{
					if (includeAll || AutoPopulate || toggleableAssets[i].Toggled)
					{
						toggledAssets.Add(toggleableAssets[i].Object);
					}
				}
				for (int num = List.SmartValue.Count - 1; num >= 0; num--)
				{
					if ((Object)(object)List.SmartValue[num] == (Object)null)
					{
						CollectionResolver.QueueRemoveAt(num);
						Rescan();
					}
					else if (!toggledAssets.Contains(List.SmartValue[num]))
					{
						if (IsPopulated)
						{
							CollectionResolver.QueueRemoveAt(num);
						}
					}
					else
					{
						toggledAssets.Remove(List.SmartValue[num]);
					}
				}
				foreach (TElement item in toggledAssets.GFIterator())
				{
					CollectionResolver.QueueAdd((object[])(object)Enumerable.Repeat(item, List.ValueCount).ToArray());
				}
				toggledAssets.Clear();
			}
		}

		[Serializable]
		private class ToggleableAsset
		{
			[HideInInspector]
			public bool AutoToggle;

			public bool Toggled;

			public TElement Object;

			public ToggleableAsset(TElement obj, bool autoToggle)
			{
				AutoToggle = autoToggle;
				Object = obj;
			}
		}

		private sealed class AssetInstanceDrawer : OdinValueDrawer<ToggleableAsset>
		{
			protected override void DrawPropertyLayout(GUIContent label)
			{
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				//IL_005d: Unknown result type (might be due to invalid IL or missing references)
				//IL_00af: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b5: Invalid comparison between Unknown and I4
				//IL_0110: Unknown result type (might be due to invalid IL or missing references)
				//IL_014b: Unknown result type (might be due to invalid IL or missing references)
				IPropertyValueEntry<ToggleableAsset> propertyValueEntry = base.ValueEntry;
				if (propertyValueEntry.SmartValue.AutoToggle)
				{
					SirenixEditorGUI.ObjectField(null, (Object)(object)propertyValueEntry.SmartValue.Object, ((object)propertyValueEntry.SmartValue.Object).GetType(), allowSceneObjects: false, isReadOnly: true);
					return;
				}
				Rect rect = GUILayoutUtility.GetRect(16f, 16f, (GUILayoutOption[])GUILayoutOptions.ExpandWidth());
				Rect val = default(Rect);
				((Rect)(ref val))._002Ector(((Rect)(ref rect)).get_x(), ((Rect)(ref rect)).get_y(), 16f, 16f);
				Rect rect2 = default(Rect);
				((Rect)(ref rect2))._002Ector(((Rect)(ref rect)).get_x() + 20f, ((Rect)(ref rect)).get_y(), ((Rect)(ref rect)).get_width() - 20f, 16f);
				if ((int)Event.get_current().get_type() != 7)
				{
					((Rect)(ref val)).set_x(((Rect)(ref val)).get_x() - 5f);
					((Rect)(ref val)).set_y(((Rect)(ref val)).get_y() - 5f);
					((Rect)(ref val)).set_width(((Rect)(ref val)).get_width() + 10f);
					((Rect)(ref val)).set_height(((Rect)(ref val)).get_height() + 10f);
				}
				bool changed = GUI.get_changed();
				propertyValueEntry.SmartValue.Toggled = GUI.Toggle(val, propertyValueEntry.SmartValue.Toggled, "");
				if (changed != GUI.get_changed())
				{
					propertyValueEntry.ApplyChanges();
				}
				GUIHelper.PushGUIEnabled(propertyValueEntry.SmartValue.Toggled);
				SirenixEditorGUI.ObjectField(rect2, null, (Object)(object)propertyValueEntry.SmartValue.Object, ((object)propertyValueEntry.SmartValue.Object).GetType(), allowSceneObjects: false, isReadOnly: true);
				GUIHelper.PopGUIEnabled();
			}
		}

		private static readonly NamedValue[] customFilterMethodArgs = new NamedValue[1]
		{
			new NamedValue("asset", typeof(TElement))
		};

		private AssetList assetList;

		private PropertyTree propertyTree;

		private InspectorProperty listProperty;

		protected override void Initialize()
		{
			InspectorProperty inspectorProperty = base.Property;
			IPropertyValueEntry<TList> propertyValueEntry = base.ValueEntry;
			AssetListAttribute assetListAttribute = base.Attribute;
			assetList = new AssetList();
			assetList.AutoPopulate = assetListAttribute.AutoPopulate;
			assetList.AssetNamePrefix = assetListAttribute.AssetNamePrefix;
			assetList.Tags = ((assetListAttribute.Tags != null) ? (from i in assetListAttribute.Tags.Trim().Split(',')
				select i.Trim()).ToArray() : null);
			assetList.LayerNames = ((assetListAttribute.LayerNames != null) ? (from i in assetListAttribute.LayerNames.Trim().Split(',')
				select i.Trim()).ToArray() : null);
			assetList.List = propertyValueEntry;
			assetList.CollectionResolver = inspectorProperty.ChildResolver as IOrderedCollectionResolver;
			assetList.Property = propertyValueEntry.Property;
			if (assetListAttribute.Path != null)
			{
				string text = assetListAttribute.Path.TrimStart('/', ' ').TrimEnd('/', ' ');
				text = assetListAttribute.Path.Trim('/', ' ');
				text = "Assets/" + text + "/";
				text = Application.get_dataPath() + "/" + text;
				assetList.AssetsFolderLocation = new DirectoryInfo(text);
				text = assetListAttribute.Path.Trim('/', ' ');
				assetList.PrettyPath = "/" + text.TrimStart('/');
			}
			if (assetListAttribute.CustomFilterMethod != null)
			{
				assetList.CustomFilterMethod = ValueResolver.Get<bool>(base.Property, assetListAttribute.CustomFilterMethod, customFilterMethodArgs);
			}
			if (Event.get_current() != null)
			{
				assetList.MaxSearchDurationPrFrameInMS = 20.0;
				assetList.EnsureListPopulation();
			}
			assetList.MaxSearchDurationPrFrameInMS = 1.0;
			propertyTree = PropertyTree.Create((object)assetList);
			propertyTree.UpdateTree();
			listProperty = propertyTree.GetPropertyAtPath("toggleableAssets");
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Invalid comparison between Unknown and I4
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Invalid comparison between Unknown and I4
			InspectorProperty inspectorProperty = base.Property;
			IPropertyValueEntry<TList> propertyValueEntry = base.ValueEntry;
			AssetListAttribute assetListAttribute = base.Attribute;
			if (inspectorProperty.ValueEntry.WeakSmartValue != null)
			{
				propertyTree.GetRootProperty(0).Label = label;
				listProperty.State.Enabled = base.Property.State.Enabled;
				listProperty.State.Expanded = base.Property.State.Expanded;
				if ((int)Event.get_current().get_type() == 8)
				{
					assetList.Property = propertyValueEntry.Property;
					assetList.EnsureListPopulation();
					assetList.SetToggleValues();
				}
				if (assetList.CustomFilterMethod != null && assetList.CustomFilterMethod.HasError)
				{
					assetList.CustomFilterMethod.DrawError();
				}
				assetList.Property = propertyValueEntry.Property;
				propertyTree.Draw(applyUndo: false);
				base.Property.State.Enabled = listProperty.State.Enabled;
				base.Property.State.Expanded = listProperty.State.Expanded;
				if ((int)Event.get_current().get_type() == 12)
				{
					assetList.UpdateList();
				}
			}
		}

		/// <summary>
		/// Populates the generic menu for the property.
		/// </summary>
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Expected O, but got Unknown
			//IL_0056: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			if (assetList == null)
			{
				return;
			}
			if (assetList.List.SmartValue.Count != assetList.ToggleableAssets.Count)
			{
				genericMenu.AddItem(new GUIContent("Include All"), false, (MenuFunction)delegate
				{
					assetList.UpdateList(includeAll: true);
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Include All"));
			}
		}
	}
}
