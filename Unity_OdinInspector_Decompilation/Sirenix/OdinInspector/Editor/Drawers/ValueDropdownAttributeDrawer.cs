using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.ValueDropdownAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ValueDropdownAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ValueDropdownItem`1" />
	/// <summary>
	/// Draws the property.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 2002.0)]
	public sealed class ValueDropdownAttributeDrawer : OdinAttributeDrawer<ValueDropdownAttribute>
	{
		private string error;

		private GUIContent label;

		private bool isList;

		private bool isListElement;

		private Func<IEnumerable<ValueDropdownItem>> getValues;

		private Func<IEnumerable<object>> getSelection;

		private IEnumerable<object> result;

		private bool enableMultiSelect;

		private Dictionary<object, string> nameLookup;

		private ValueResolver<object> rawGetter;

		private LocalPersistentContext<bool> isToggled;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			rawGetter = ValueResolver.Get<object>(base.Property, base.Attribute.ValuesGetter);
			isToggled = this.GetPersistentValue("Toggled", SirenixEditorGUI.ExpandFoldoutByDefault);
			error = rawGetter.ErrorMessage;
			isList = base.Property.ChildResolver is ICollectionResolver;
			isListElement = base.Property.Parent != null && base.Property.Parent.ChildResolver is ICollectionResolver;
			getSelection = () => base.Property.ValueEntry.WeakValues.Cast<object>();
			getValues = delegate
			{
				object value = rawGetter.GetValue();
				return (value != null) ? (from object x in value as IEnumerable
					where x != null
					select x).Select(delegate(object x)
				{
					if (x is ValueDropdownItem)
					{
						return (ValueDropdownItem)x;
					}
					if (x is IValueDropdownItem)
					{
						IValueDropdownItem valueDropdownItem = x as IValueDropdownItem;
						return new ValueDropdownItem(valueDropdownItem.GetText(), valueDropdownItem.GetValue());
					}
					return new ValueDropdownItem(null, x);
				}) : null;
			};
			ReloadDropdownCollections();
		}

		private void ReloadDropdownCollections()
		{
			if (error != null)
			{
				return;
			}
			object obj = null;
			object value = rawGetter.GetValue();
			if (value != null)
			{
				obj = (value as IEnumerable).Cast<object>().FirstOrDefault();
			}
			if (obj is IValueDropdownItem)
			{
				IEnumerable<ValueDropdownItem> enumerable = getValues();
				nameLookup = new Dictionary<object, string>(new IValueDropdownEqualityComparer(isTypeLookup: false));
				foreach (ValueDropdownItem item in enumerable)
				{
					nameLookup[item] = item.Text;
				}
			}
			else
			{
				nameLookup = null;
			}
		}

		private static IEnumerable<ValueDropdownItem> ToValueDropdowns(IEnumerable<object> query)
		{
			return query.Select(delegate(object x)
			{
				if (x is ValueDropdownItem)
				{
					return (ValueDropdownItem)x;
				}
				if (x is IValueDropdownItem)
				{
					IValueDropdownItem valueDropdownItem = x as IValueDropdownItem;
					return new ValueDropdownItem(valueDropdownItem.GetText(), valueDropdownItem.GetValue());
				}
				return new ValueDropdownItem(null, x);
			});
		}

		/// <summary>
		/// Draws the property with GUILayout support. This method is called by DrawPropertyImplementation if the GUICallType is set to GUILayout, which is the default.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			this.label = label;
			if (base.Property.ValueEntry == null)
			{
				CallNextDrawer(label);
			}
			else if (error != null)
			{
				SirenixEditorGUI.ErrorMessageBox(error);
				CallNextDrawer(label);
			}
			else if (isList)
			{
				if (base.Attribute.DisableListAddButtonBehaviour)
				{
					CallNextDrawer(label);
					return;
				}
				Action nextCustomAddFunction = CollectionDrawerStaticInfo.NextCustomAddFunction;
				CollectionDrawerStaticInfo.NextCustomAddFunction = OpenSelector;
				CallNextDrawer(label);
				if (result != null)
				{
					AddResult(result);
					result = null;
				}
				CollectionDrawerStaticInfo.NextCustomAddFunction = nextCustomAddFunction;
			}
			else if (base.Attribute.DrawDropdownForListElements || !isListElement)
			{
				DrawDropdown();
			}
			else
			{
				CallNextDrawer(label);
			}
		}

		private void AddResult(IEnumerable<object> query)
		{
			if (isList)
			{
				ICollectionResolver collectionResolver = base.Property.ChildResolver as ICollectionResolver;
				if (enableMultiSelect)
				{
					collectionResolver.QueueClear();
				}
				foreach (object item in query)
				{
					object[] array = new object[base.Property.ParentValues.Count];
					for (int i = 0; i < array.Length; i++)
					{
						if (base.Attribute.CopyValues)
						{
							array[i] = SerializationUtility.CreateCopy(item);
						}
						else
						{
							array[i] = item;
						}
					}
					collectionResolver.QueueAdd(array);
				}
				return;
			}
			object obj = query.FirstOrDefault();
			for (int j = 0; j < base.Property.ValueEntry.WeakValues.Count; j++)
			{
				if (base.Attribute.CopyValues)
				{
					base.Property.ValueEntry.WeakValues[j] = SerializationUtility.CreateCopy(obj);
				}
				else
				{
					base.Property.ValueEntry.WeakValues[j] = obj;
				}
			}
		}

		private void DrawDropdown()
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Invalid comparison between Unknown and I4
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			IEnumerable<object> enumerable = null;
			if (base.Attribute.AppendNextDrawer && !isList)
			{
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				float num = 15f;
				if (label != null)
				{
					num += GUIHelper.BetterLabelWidth;
				}
				enumerable = OdinSelector<object>.DrawSelectorDropdown(label, GUIContent.none, (Func<Rect, OdinSelector<object>>)ShowSelector, GUIStyle.get_none(), (GUILayoutOption[])GUILayoutOptions.Width(num));
				if ((int)Event.get_current().get_type() == 7)
				{
					Rect val = GUILayoutUtility.GetLastRect().AlignRight(15f);
					((Rect)(ref val)).set_y(((Rect)(ref val)).get_y() + 4f);
					SirenixGUIStyles.PaneOptions.Draw(val, GUIContent.none, 0);
				}
				GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
				bool disableGUIInAppendedDrawer = base.Attribute.DisableGUIInAppendedDrawer;
				if (disableGUIInAppendedDrawer)
				{
					GUIHelper.PushGUIEnabled(enabled: false);
				}
				CallNextDrawer(null);
				if (disableGUIInAppendedDrawer)
				{
					GUIHelper.PopGUIEnabled();
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			else
			{
				string currentValueName = GetCurrentValueName();
				if (!base.Attribute.HideChildProperties && base.Property.Children.Count > 0)
				{
					isToggled.Value = SirenixEditorGUI.Foldout(isToggled.Value, label, out var valueRect);
					enumerable = OdinSelector<object>.DrawSelectorDropdown(valueRect, currentValueName, ShowSelector);
					if (SirenixEditorGUI.BeginFadeGroup(this, isToggled.Value))
					{
						EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
						for (int i = 0; i < base.Property.Children.Count; i++)
						{
							InspectorProperty inspectorProperty = base.Property.Children[i];
							inspectorProperty.Draw(inspectorProperty.Label);
						}
						EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
					}
					SirenixEditorGUI.EndFadeGroup();
				}
				else
				{
					enumerable = OdinSelector<object>.DrawSelectorDropdown(label, currentValueName, ShowSelector, null);
				}
			}
			if (enumerable != null && enumerable.Any())
			{
				AddResult(enumerable);
			}
		}

		private void OpenSelector()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			ReloadDropdownCollections();
			Rect rect = default(Rect);
			((Rect)(ref rect))._002Ector(Event.get_current().get_mousePosition(), Vector2.get_zero());
			OdinSelector<object> odinSelector = ShowSelector(rect);
			odinSelector.SelectionConfirmed += delegate(IEnumerable<object> x)
			{
				result = x;
			};
		}

		private OdinSelector<object> ShowSelector(Rect rect)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			GenericSelector<object> genericSelector = CreateSelector();
			((Rect)(ref rect)).set_x((float)(int)((Rect)(ref rect)).get_x());
			((Rect)(ref rect)).set_y((float)(int)((Rect)(ref rect)).get_y());
			((Rect)(ref rect)).set_width((float)(int)((Rect)(ref rect)).get_width());
			((Rect)(ref rect)).set_height((float)(int)((Rect)(ref rect)).get_height());
			if (base.Attribute.AppendNextDrawer && !isList)
			{
				Rect currentLayoutRect = GUIHelper.GetCurrentLayoutRect();
				((Rect)(ref rect)).set_xMax(((Rect)(ref currentLayoutRect)).get_xMax());
			}
			genericSelector.ShowInPopup(rect, new Vector2((float)base.Attribute.DropdownWidth, (float)base.Attribute.DropdownHeight));
			return genericSelector;
		}

		private GenericSelector<object> CreateSelector()
		{
			base.Attribute.IsUniqueList = !(base.Property.ChildResolver is IOrderedCollectionResolver) || base.Attribute.IsUniqueList || base.Attribute.ExcludeExistingValuesInList;
			IEnumerable<ValueDropdownItem> enumerable = getValues() ?? Enumerable.Empty<ValueDropdownItem>();
			if (enumerable.Any())
			{
				if ((isList && base.Attribute.ExcludeExistingValuesInList) || (isListElement && base.Attribute.IsUniqueList))
				{
					List<ValueDropdownItem> list = enumerable.ToList();
					InspectorProperty inspectorProperty = base.Property.FindParent((InspectorProperty x) => x.ChildResolver is ICollectionResolver, includeSelf: true);
					IValueDropdownEqualityComparer comparer = new IValueDropdownEqualityComparer(isTypeLookup: false);
					inspectorProperty.ValueEntry.WeakValues.Cast<IEnumerable>().SelectMany((IEnumerable x) => x.Cast<object>()).ForEach(delegate(object x)
					{
						list.RemoveAll((ValueDropdownItem c) => comparer.Equals(c, x));
					});
					enumerable = list;
				}
				if (nameLookup != null)
				{
					foreach (ValueDropdownItem item in enumerable)
					{
						if (item.Value != null)
						{
							nameLookup[item.Value] = item.Text;
						}
					}
				}
			}
			bool drawSearchToolbar = base.Attribute.NumberOfItemsBeforeEnablingSearch == 0 || (enumerable != null && enumerable.Take(base.Attribute.NumberOfItemsBeforeEnablingSearch).Count() == base.Attribute.NumberOfItemsBeforeEnablingSearch);
			GenericSelector<object> genericSelector = new GenericSelector<object>(base.Attribute.DropdownTitle, supportsMultiSelect: false, enumerable.Select((ValueDropdownItem x) => new GenericSelectorItem<object>(x.Text, x.Value)));
			enableMultiSelect = isList && base.Attribute.IsUniqueList && !base.Attribute.ExcludeExistingValuesInList;
			if (base.Attribute.FlattenTreeView)
			{
				genericSelector.FlattenedTree = true;
			}
			if (isList && !base.Attribute.ExcludeExistingValuesInList && base.Attribute.IsUniqueList)
			{
				genericSelector.CheckboxToggle = true;
			}
			else if (!base.Attribute.DoubleClickToConfirm && !enableMultiSelect)
			{
				genericSelector.EnableSingleClickToSelect();
			}
			if (isList && enableMultiSelect)
			{
				genericSelector.SelectionTree.Selection.SupportsMultiSelect = true;
				genericSelector.DrawConfirmSelectionButton = true;
			}
			genericSelector.SelectionTree.Config.DrawSearchToolbar = drawSearchToolbar;
			IEnumerable<object> selection = Enumerable.Empty<object>();
			if (!isList)
			{
				selection = getSelection();
			}
			else if (enableMultiSelect)
			{
				selection = getSelection().SelectMany((object x) => (x as IEnumerable).Cast<object>());
			}
			genericSelector.SetSelection(selection);
			genericSelector.SelectionTree.EnumerateTree().AddThumbnailIcons(preferAssetPreviewAsIcon: true);
			if (base.Attribute.ExpandAllMenuItems)
			{
				genericSelector.SelectionTree.EnumerateTree(delegate(OdinMenuItem x)
				{
					x.Toggled = true;
				});
			}
			if (base.Attribute.SortDropdownItems)
			{
				genericSelector.SelectionTree.SortMenuItemsByName();
			}
			return genericSelector;
		}

		private string GetCurrentValueName()
		{
			if (!EditorGUI.get_showMixedValue())
			{
				object weakSmartValue = base.Property.ValueEntry.WeakSmartValue;
				string value = null;
				if (nameLookup != null && weakSmartValue != null)
				{
					nameLookup.TryGetValue(weakSmartValue, out value);
				}
				return new GenericSelectorItem<object>(value, weakSmartValue).GetNiceName();
			}
			return "—";
		}
	}
}
