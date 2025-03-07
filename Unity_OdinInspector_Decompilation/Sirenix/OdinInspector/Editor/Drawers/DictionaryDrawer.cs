using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Property drawer for <see cref="T:System.Collections.Generic.IDictionary`2" />.
	/// </summary>
	public class DictionaryDrawer<TDictionary, TKey, TValue> : OdinValueDrawer<TDictionary> where TDictionary : IDictionary<TKey, TValue>
	{
		private const string CHANGE_ID = "DICTIONARY_DRAWER";

		private static readonly bool KeyIsValueType = typeof(TKey).IsValueType;

		private static GUIStyle addKeyPaddingStyle;

		private static GUIStyle listItemStyle;

		private GUIPagingHelper paging = new GUIPagingHelper();

		private GeneralDrawerConfig config;

		private LocalPersistentContext<float> keyWidthOffset;

		private bool showAddKeyGUI;

		private bool? newKeyIsValid;

		private string newKeyErrorMessage;

		private TKey newKey;

		private TValue newValue;

		private StrongDictionaryPropertyResolver<TDictionary, TKey, TValue> dictionaryResolver;

		private DictionaryDrawerSettings attrSettings;

		private bool disableAddKey;

		private GUIContent keyLabel;

		private GUIContent valueLabel;

		private float keyLabelWidth;

		private float valueLabelWidth;

		private TempKeyValuePair<TKey, TValue> tempKeyValue;

		private IPropertyValueEntry<TKey> tempKeyEntry;

		private IPropertyValueEntry<TValue> tempValueEntry;

		private static GUIStyle oneLineMargin;

		private static GUIStyle headerMargin;

		private static GUIStyle AddKeyPaddingStyle
		{
			get
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Expected O, but got Unknown
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				//IL_0040: Unknown result type (might be due to invalid IL or missing references)
				//IL_004a: Expected O, but got Unknown
				//IL_004f: Expected O, but got Unknown
				if (addKeyPaddingStyle == null)
				{
					GUIStyle val = new GUIStyle(GUIStyle.op_Implicit("CN Box"));
					val.set_overflow(new RectOffset(0, 0, 1, 0));
					val.set_fixedHeight(0f);
					val.set_stretchHeight(false);
					val.set_padding(new RectOffset(10, 10, 10, 10));
					addKeyPaddingStyle = val;
				}
				return addKeyPaddingStyle;
			}
		}

		private static GUIStyle OneLineMargin
		{
			get
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Expected O, but got Unknown
				//IL_0020: Expected O, but got Unknown
				if (oneLineMargin == null)
				{
					GUIStyle val = new GUIStyle();
					val.set_margin(new RectOffset(8, 0, 0, 0));
					oneLineMargin = val;
				}
				return oneLineMargin;
			}
		}

		private static GUIStyle HeaderMargin
		{
			get
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Expected O, but got Unknown
				//IL_0021: Expected O, but got Unknown
				if (headerMargin == null)
				{
					GUIStyle val = new GUIStyle();
					val.set_margin(new RectOffset(40, 0, 0, 0));
					headerMargin = val;
				}
				return headerMargin;
			}
		}

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			return property.ChildResolver is StrongDictionaryPropertyResolver<TDictionary, TKey, TValue>;
		}

		protected override void Initialize()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			//IL_001f: Expected O, but got Unknown
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Expected O, but got Unknown
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Expected O, but got Unknown
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			GUIStyle val = new GUIStyle(GUIStyle.get_none());
			val.set_padding(new RectOffset(7, 20, 3, 3));
			listItemStyle = val;
			IPropertyValueEntry<TDictionary> propertyValueEntry = base.ValueEntry;
			attrSettings = propertyValueEntry.Property.GetAttribute<DictionaryDrawerSettings>() ?? new DictionaryDrawerSettings();
			keyWidthOffset = this.GetPersistentValue("KeyColumnWidth", attrSettings.KeyColumnWidth);
			disableAddKey = propertyValueEntry.Property.Tree.PrefabModificationHandler.HasPrefabs && propertyValueEntry.SerializationBackend == SerializationBackend.Odin && !propertyValueEntry.Property.SupportsPrefabModifications;
			keyLabel = new GUIContent(attrSettings.KeyLabel);
			valueLabel = new GUIContent(attrSettings.ValueLabel);
			keyLabelWidth = EditorStyles.get_label().CalcSize(keyLabel).x + 20f;
			valueLabelWidth = EditorStyles.get_label().CalcSize(valueLabel).x + 20f;
			if (!disableAddKey)
			{
				tempKeyValue = new TempKeyValuePair<TKey, TValue>();
				PropertyTree propertyTree = PropertyTree.Create((object)tempKeyValue);
				propertyTree.UpdateTree();
				tempKeyEntry = (IPropertyValueEntry<TKey>)propertyTree.GetPropertyAtPath("Key").ValueEntry;
				tempValueEntry = (IPropertyValueEntry<TValue>)propertyTree.GetPropertyAtPath("Value").ValueEntry;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Invalid comparison between Unknown and I4
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Invalid comparison between Unknown and I4
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0240: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_0287: Invalid comparison between Unknown and I4
			//IL_0289: Unknown result type (might be due to invalid IL or missing references)
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<TDictionary> propertyValueEntry = base.ValueEntry;
			dictionaryResolver = propertyValueEntry.Property.ChildResolver as StrongDictionaryPropertyResolver<TDictionary, TKey, TValue>;
			config = GlobalConfig<GeneralDrawerConfig>.Instance;
			paging.NumberOfItemsPerPage = config.NumberOfItemsPrPage;
			listItemStyle.get_padding().set_right((!propertyValueEntry.IsEditable || attrSettings.IsReadOnly) ? 4 : 20);
			SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
			paging.Update(propertyValueEntry.Property.Children.Count);
			DrawToolbar(propertyValueEntry, label);
			paging.Update(propertyValueEntry.Property.Children.Count);
			if (!disableAddKey && !attrSettings.IsReadOnly)
			{
				DrawAddKey(propertyValueEntry);
			}
			GUIHelper.BeginLayoutMeasuring();
			if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(propertyValueEntry.Property, this), base.Property.State.Expanded, out var t))
			{
				Rect rect = SirenixEditorGUI.BeginVerticalList(false, true);
				if (attrSettings.DisplayMode == DictionaryDisplayOptions.OneLine)
				{
					float num = ((Rect)(ref rect)).get_width() - 90f;
					((Rect)(ref rect)).set_xMin(keyWidthOffset.Value + 22f);
					((Rect)(ref rect)).set_xMax(((Rect)(ref rect)).get_xMin() + 10f);
					GUIHelper.PushGUIEnabled(enabled: true);
					keyWidthOffset.Value += SirenixEditorGUI.SlideRect(rect, (MouseCursor)5).x;
					GUIHelper.PopGUIEnabled();
					if ((int)Event.get_current().get_type() == 7)
					{
						keyWidthOffset.Value = Mathf.Clamp(keyWidthOffset.Value, 30f, num);
					}
					if (paging.ElementCount != 0)
					{
						Rect rect2 = SirenixEditorGUI.BeginListItem(false, null);
						GUILayout.Space(14f);
						if ((int)Event.get_current().get_type() == 7)
						{
							GUI.Label(rect2.SetWidth(keyWidthOffset.Value), keyLabel, SirenixGUIStyles.LabelCentered);
							GUI.Label(rect2.AddXMin(keyWidthOffset.Value), valueLabel, SirenixGUIStyles.LabelCentered);
							SirenixEditorGUI.DrawSolidRect(rect2.AlignBottom(1f), SirenixGUIStyles.BorderColor);
						}
						SirenixEditorGUI.EndListItem();
					}
				}
				GUIHelper.PushHierarchyMode(hierarchyMode: false);
				DrawElements(propertyValueEntry, label);
				GUIHelper.PopHierarchyMode();
				SirenixEditorGUI.EndVerticalList();
			}
			SirenixEditorGUI.EndFadeGroup();
			Rect rect3 = GUIHelper.EndLayoutMeasuring();
			if (t > 0.01f && (int)Event.get_current().get_type() == 7)
			{
				Color borderColor = SirenixGUIStyles.BorderColor;
				((Rect)(ref rect3)).set_yMin(((Rect)(ref rect3)).get_yMin() - 1f);
				SirenixEditorGUI.DrawBorders(rect3, 1, borderColor);
				borderColor.a *= t;
				if (attrSettings.DisplayMode == DictionaryDisplayOptions.OneLine)
				{
					((Rect)(ref rect3)).set_width(1f);
					((Rect)(ref rect3)).set_x(((Rect)(ref rect3)).get_x() + (keyWidthOffset.Value + 13f));
					SirenixEditorGUI.DrawSolidRect(rect3, borderColor);
				}
			}
			SirenixEditorGUI.EndIndentedVertical();
		}

		private void DrawAddKey(IPropertyValueEntry<TDictionary> entry)
		{
			if (!entry.IsEditable || attrSettings.IsReadOnly)
			{
				return;
			}
			if (SirenixEditorGUI.BeginFadeGroup(this, showAddKeyGUI))
			{
				GUILayout.BeginVertical(AddKeyPaddingStyle, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				if (typeof(TKey) == typeof(string) && newKey == null)
				{
					newKey = (TKey)(object)"";
					newKeyIsValid = null;
				}
				if (!newKeyIsValid.HasValue)
				{
					newKeyIsValid = CheckKeyIsValid(entry, newKey, out newKeyErrorMessage);
				}
				tempKeyEntry.Property.Tree.BeginDraw(withUndo: false);
				tempKeyEntry.Property.Update();
				EditorGUI.BeginChangeCheck();
				tempKeyEntry.Property.Draw(keyLabel);
				bool flag = EditorGUI.EndChangeCheck();
				bool flag2 = tempKeyEntry.ApplyChanges();
				if (flag || flag2)
				{
					newKey = tempKeyValue.Key;
					UnityEditorEventUtility.EditorApplication_delayCall += delegate
					{
						newKeyIsValid = null;
					};
					GUIHelper.RequestRepaint();
				}
				tempValueEntry.Property.Update();
				tempValueEntry.Property.Draw(valueLabel);
				tempValueEntry.ApplyChanges();
				newValue = tempKeyValue.Value;
				tempKeyEntry.Property.Tree.InvokeDelayedActions();
				if (tempKeyEntry.Property.Tree.ApplyChanges())
				{
					newKey = tempKeyValue.Key;
					UnityEditorEventUtility.EditorApplication_delayCall += delegate
					{
						newKeyIsValid = null;
					};
					GUIHelper.RequestRepaint();
				}
				tempKeyEntry.Property.Tree.EndDraw();
				GUIHelper.PushGUIEnabled(GUI.get_enabled() && newKeyIsValid.Value);
				if (GUILayout.Button(newKeyIsValid.Value ? "Add" : newKeyErrorMessage, (GUILayoutOption[])(object)new GUILayoutOption[0]))
				{
					object[] array = new object[entry.ValueCount];
					object[] array2 = new object[entry.ValueCount];
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = SerializationUtility.CreateCopy(newKey);
					}
					for (int j = 0; j < array2.Length; j++)
					{
						array2[j] = SerializationUtility.CreateCopy(newValue);
					}
					dictionaryResolver.QueueSet(array, array2);
					UnityEditorEventUtility.EditorApplication_delayCall += delegate
					{
						newKeyIsValid = null;
					};
					GUIHelper.RequestRepaint();
					entry.Property.Tree.DelayActionUntilRepaint(delegate
					{
						newValue = default(TValue);
						tempKeyValue.Value = default(TValue);
						tempValueEntry.Update();
					});
				}
				GUIHelper.PopGUIEnabled();
				GUILayout.EndVertical();
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		private void DrawToolbar(IPropertyValueEntry<TDictionary> entry, GUIContent label)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			SirenixEditorGUI.BeginHorizontalToolbar();
			if (entry.ListLengthChangedFromPrefab)
			{
				GUIHelper.PushIsBoldLabel(isBold: true);
			}
			if (config.HideFoldoutWhileEmpty && paging.ElementCount == 0)
			{
				if (label != null)
				{
					GUILayout.Label(label, (GUILayoutOption[])GUILayoutOptions.ExpandWidth(expand: false));
				}
			}
			else
			{
				bool flag = ((label != null) ? SirenixEditorGUI.Foldout(base.Property.State.Expanded, label) : SirenixEditorGUI.Foldout(base.Property.State.Expanded, ""));
				if (!flag && base.Property.State.Expanded)
				{
					showAddKeyGUI = false;
				}
				base.Property.State.Expanded = flag;
			}
			if (entry.ListLengthChangedFromPrefab)
			{
				GUIHelper.PopIsBoldLabel();
			}
			GUILayout.FlexibleSpace();
			if (config.ShowItemCount)
			{
				if (entry.ValueState == PropertyValueState.CollectionLengthConflict)
				{
					int num = entry.Values.Min((TDictionary x) => x.Count);
					int num2 = entry.Values.Max((TDictionary x) => x.Count);
					GUILayout.Label(num + " / " + num2 + " items", EditorStyles.get_centeredGreyMiniLabel(), (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
				else
				{
					GUILayout.Label((paging.ElementCount == 0) ? "Empty" : (paging.ElementCount + " items"), EditorStyles.get_centeredGreyMiniLabel(), (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
			}
			if ((!config.HidePagingWhileCollapsed || base.Property.State.Expanded) && (!config.HidePagingWhileOnlyOnePage || paging.PageCount != 1))
			{
				bool enabled = GUI.get_enabled();
				bool flag2 = paging.IsEnabled && paging.PageCount != 1;
				GUI.set_enabled(enabled && flag2 && !paging.IsOnFirstPage);
				if (SirenixEditorGUI.ToolbarButton(EditorIcons.ArrowLeft, ignoreGUIEnabled: true))
				{
					if (Event.get_current().get_button() == 0)
					{
						paging.CurrentPage--;
					}
					else
					{
						paging.CurrentPage = 0;
					}
				}
				GUI.set_enabled(enabled && flag2);
				GUILayoutOptions.GUILayoutOptionsInstance gUILayoutOptionsInstance = GUILayoutOptions.Width(10 + paging.PageCount.ToString().Length * 10);
				paging.CurrentPage = EditorGUILayout.IntField(paging.CurrentPage + 1, (GUILayoutOption[])gUILayoutOptionsInstance) - 1;
				GUILayout.Label(GUIHelper.TempContent("/ " + paging.PageCount), (GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUI.set_enabled(enabled && flag2 && !paging.IsOnLastPage);
				if (SirenixEditorGUI.ToolbarButton(EditorIcons.ArrowRight, ignoreGUIEnabled: true))
				{
					if (Event.get_current().get_button() == 0)
					{
						paging.CurrentPage++;
					}
					else
					{
						paging.CurrentPage = paging.PageCount - 1;
					}
				}
				GUI.set_enabled(enabled && paging.PageCount != 1);
				if (config.ShowExpandButton && SirenixEditorGUI.ToolbarButton(paging.IsEnabled ? EditorIcons.ArrowDown : EditorIcons.ArrowUp, ignoreGUIEnabled: true))
				{
					paging.IsEnabled = !paging.IsEnabled;
				}
				GUI.set_enabled(enabled);
			}
			if (!disableAddKey && !attrSettings.IsReadOnly && SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
			{
				showAddKeyGUI = !showAddKeyGUI;
				if (showAddKeyGUI)
				{
					base.Property.State.Expanded = true;
				}
			}
			SirenixEditorGUI.EndHorizontalToolbar();
		}

		private void DrawElements(IPropertyValueEntry<TDictionary> entry, GUIContent label)
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0300: Unknown result type (might be due to invalid IL or missing references)
			//IL_039f: Unknown result type (might be due to invalid IL or missing references)
			int i;
			for (i = paging.StartIndex; i < paging.EndIndex; i++)
			{
				InspectorProperty inspectorProperty = entry.Property.Children[i];
				EditableKeyValuePair<TKey, TValue> smartValue = (inspectorProperty.ValueEntry as IPropertyValueEntry<EditableKeyValuePair<TKey, TValue>>).SmartValue;
				Rect val = SirenixEditorGUI.BeginListItem(false, listItemStyle);
				if (attrSettings.DisplayMode != 0)
				{
					bool defaultValue = attrSettings.DisplayMode switch
					{
						DictionaryDisplayOptions.CollapsedFoldout => false, 
						DictionaryDisplayOptions.ExpandedFoldout => true, 
						_ => SirenixEditorGUI.ExpandFoldoutByDefault, 
					};
					LocalPersistentContext<bool> persistent = inspectorProperty.Context.GetPersistent(this, "Expanded", defaultValue);
					SirenixEditorGUI.BeginBox();
					SirenixEditorGUI.BeginToolbarBoxHeader();
					if (smartValue.IsInvalidKey)
					{
						GUIHelper.PushColor(Color.get_red());
					}
					Rect rect = GUIHelper.GetCurrentLayoutRect().AlignLeft(HeaderMargin.get_margin().get_left());
					((Rect)(ref rect)).set_y(((Rect)(ref rect)).get_y() + 1f);
					GUILayout.BeginVertical(HeaderMargin, (GUILayoutOption[])(object)new GUILayoutOption[0]);
					GUIHelper.PushIsDrawingDictionaryKey(enabled: true);
					GUIHelper.PushLabelWidth(keyLabelWidth);
					InspectorProperty keyProperty = inspectorProperty.Children[0];
					GUIContent val2 = GUIHelper.TempContent(" ");
					DrawKeyProperty(keyProperty, val2);
					GUIHelper.PopLabelWidth();
					GUIHelper.PopIsDrawingDictionaryKey();
					GUILayout.EndVertical();
					if (smartValue.IsInvalidKey)
					{
						GUIHelper.PopColor();
					}
					persistent.Value = SirenixEditorGUI.Foldout(rect, persistent.Value, keyLabel);
					SirenixEditorGUI.EndToolbarBoxHeader();
					if (SirenixEditorGUI.BeginFadeGroup(persistent, persistent.Value))
					{
						inspectorProperty.Children[1].Draw(null);
					}
					SirenixEditorGUI.EndFadeGroup();
					SirenixEditorGUI.EndToolbarBox();
				}
				else
				{
					GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
					GUILayout.BeginVertical((GUILayoutOption[])GUILayoutOptions.Width(keyWidthOffset.Value));
					InspectorProperty keyProperty2 = inspectorProperty.Children[0];
					if (smartValue.IsInvalidKey)
					{
						GUIHelper.PushColor(Color.get_red());
					}
					if (attrSettings.IsReadOnly)
					{
						GUIHelper.PushGUIEnabled(enabled: false);
					}
					GUIHelper.PushIsDrawingDictionaryKey(enabled: true);
					GUIHelper.PushLabelWidth(10f);
					DrawKeyProperty(keyProperty2, null);
					GUIHelper.PopLabelWidth();
					GUIHelper.PopIsDrawingDictionaryKey();
					if (attrSettings.IsReadOnly)
					{
						GUIHelper.PopGUIEnabled();
					}
					if (smartValue.IsInvalidKey)
					{
						GUIHelper.PopColor();
					}
					GUILayout.EndVertical();
					GUILayout.BeginVertical(OneLineMargin, (GUILayoutOption[])(object)new GUILayoutOption[0]);
					GUIHelper.PushHierarchyMode(hierarchyMode: false);
					InspectorProperty inspectorProperty2 = inspectorProperty.Children[1];
					float actualLabelWidth = GUIHelper.ActualLabelWidth;
					GUIHelper.BetterLabelWidth = 150f;
					inspectorProperty2.Draw(null);
					GUIHelper.BetterLabelWidth = actualLabelWidth;
					GUIHelper.PopHierarchyMode();
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
				}
				if (entry.IsEditable && !attrSettings.IsReadOnly && SirenixEditorGUI.IconButton(new Rect(((Rect)(ref val)).get_xMax() - 24f + 5f, ((Rect)(ref val)).get_y() + 4f + (float)(((int)((Rect)(ref val)).get_height() - 23) / 2), 14f, 14f), EditorIcons.X))
				{
					dictionaryResolver.QueueRemoveKey((from n in Enumerable.Range(0, entry.ValueCount)
						select dictionaryResolver.GetKey(n, i)).ToArray());
					UnityEditorEventUtility.EditorApplication_delayCall += delegate
					{
						newKeyIsValid = null;
					};
					GUIHelper.RequestRepaint();
				}
				SirenixEditorGUI.EndListItem();
			}
			if (paging.IsOnLastPage && entry.ValueState == PropertyValueState.CollectionLengthConflict)
			{
				SirenixEditorGUI.BeginListItem(false, null);
				GUILayout.Label(GUIHelper.TempContent("------"), EditorStyles.get_centeredGreyMiniLabel(), (GUILayoutOption[])(object)new GUILayoutOption[0]);
				SirenixEditorGUI.EndListItem();
			}
		}

		private void DrawKeyProperty(InspectorProperty keyProperty, GUIContent keyLabel)
		{
			EditorGUI.BeginChangeCheck();
			keyProperty.Draw(keyLabel);
			bool flag = EditorGUI.EndChangeCheck();
			bool flag2 = ValuesAreDirty(keyProperty);
			if (!flag && flag2)
			{
				dictionaryResolver.ValueApplyIsTemporary = true;
				ApplyChangesToProperty(keyProperty);
				dictionaryResolver.ValueApplyIsTemporary = false;
			}
			else if (flag && !flag2)
			{
				MarkPropertyDirty(keyProperty);
			}
		}

		private static void MarkPropertyDirty(InspectorProperty keyProperty)
		{
			keyProperty.ValueEntry.WeakValues.ForceMarkDirty();
			if (KeyIsValueType)
			{
				for (int i = 0; i < keyProperty.Children.Count; i++)
				{
					MarkPropertyDirty(keyProperty.Children[i]);
				}
			}
		}

		private static void ApplyChangesToProperty(InspectorProperty keyProperty)
		{
			if (keyProperty.ValueEntry != null && keyProperty.ValueEntry.WeakValues.AreDirty)
			{
				keyProperty.ValueEntry.ApplyChanges();
			}
			if (KeyIsValueType)
			{
				for (int i = 0; i < keyProperty.Children.Count; i++)
				{
					ApplyChangesToProperty(keyProperty.Children[i]);
				}
			}
		}

		private static bool ValuesAreDirty(InspectorProperty keyProperty)
		{
			if (keyProperty.ValueEntry != null && keyProperty.ValueEntry.WeakValues.AreDirty)
			{
				return true;
			}
			if (KeyIsValueType)
			{
				for (int i = 0; i < keyProperty.Children.Count; i++)
				{
					if (ValuesAreDirty(keyProperty.Children[i]))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool CheckKeyIsValid(IPropertyValueEntry<TDictionary> entry, TKey key, out string errorMessage)
		{
			if (!KeyIsValueType && key == null)
			{
				errorMessage = "Key cannot be null.";
				return false;
			}
			string dictionaryKeyString = DictionaryKeyUtility.GetDictionaryKeyString(key);
			if (entry.Property.Children[dictionaryKeyString] == null)
			{
				errorMessage = "";
				return true;
			}
			errorMessage = "An item with the same key already exists.";
			return false;
		}
	}
}
