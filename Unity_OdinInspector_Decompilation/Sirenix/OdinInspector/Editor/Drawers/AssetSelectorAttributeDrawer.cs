using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(0.0, 0.0, 2002.0)]
	public sealed class AssetSelectorAttributeDrawer<T> : OdinAttributeDrawer<AssetSelectorAttribute, T> where T : Object
	{
		private GUIContent label;

		private bool isList;

		private bool isListElement;

		private Func<IEnumerable<ValueDropdownItem>> getValues;

		private Func<IEnumerable<object>> getSelection;

		private Type elementOrBaseType;

		private bool isString;

		private IEnumerable<object> result;

		private bool enableMultiSelect;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			isList = base.Property.ChildResolver is IOrderedCollectionResolver;
			isListElement = base.Property.Parent != null && base.Property.Parent.ChildResolver is IOrderedCollectionResolver;
			getSelection = () => base.Property.ValueEntry.WeakValues.Cast<object>();
			elementOrBaseType = (isList ? (base.Property.ChildResolver as IOrderedCollectionResolver).ElementType : base.Property.ValueEntry.BaseValueType);
			isString = elementOrBaseType == typeof(string);
			getValues = delegate
			{
				string text = base.Attribute.Filter ?? "";
				if (string.IsNullOrEmpty(text) && !typeof(Component).IsAssignableFrom(elementOrBaseType) && !elementOrBaseType.IsInterface)
				{
					text = "t:" + elementOrBaseType.Name;
				}
				string[] source = AssetDatabase.FindAssets(text, base.Attribute.SearchInFolders ?? new string[0]);
				return source.Select((string x) => AssetDatabase.GUIDToAssetPath(x)).Distinct().SelectMany(delegate(string x)
				{
					IEnumerable<Object> source2 = ((!x.EndsWith(".unity", StringComparison.InvariantCultureIgnoreCase)) ? AssetDatabase.LoadAllAssetsAtPath(x) : Enumerable.Repeat<Object>(AssetDatabase.LoadAssetAtPath(x, typeof(Object)), 1));
					return from obj in source2
						where obj != (Object)null && elementOrBaseType.IsAssignableFrom(((object)obj).GetType())
						select new
						{
							o = obj,
							p = x
						};
				})
					.Select(x =>
					{
						ValueDropdownItem valueDropdownItem = default(ValueDropdownItem);
						valueDropdownItem.Text = x.p + (AssetDatabase.IsMainAsset(x.o) ? "" : ("/" + x.o.get_name()));
						valueDropdownItem.Value = (isString ? x.p : ((string)(object)x.o));
						return valueDropdownItem;
					});
			};
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
			else if (isList)
			{
				if (base.Attribute.DisableListAddButtonBehaviour)
				{
					CallNextDrawer(label);
					return;
				}
				CollectionDrawerStaticInfo.NextCustomAddFunction = OpenSelector;
				CallNextDrawer(label);
				if (result != null)
				{
					AddResult(result);
					result = null;
				}
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
				IOrderedCollectionResolver orderedCollectionResolver = base.Property.ChildResolver as IOrderedCollectionResolver;
				if (enableMultiSelect)
				{
					orderedCollectionResolver.QueueClear();
				}
				foreach (object item in query)
				{
					object[] array = new object[base.Property.ParentValues.Count];
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = item;
					}
					orderedCollectionResolver.QueueAdd(array);
				}
			}
			else
			{
				object value = query.FirstOrDefault();
				for (int j = 0; j < base.Property.ValueEntry.WeakValues.Count; j++)
				{
					base.Property.ValueEntry.WeakValues[j] = value;
				}
			}
		}

		private void DrawDropdown()
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Invalid comparison between Unknown and I4
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			IEnumerable<object> enumerable = null;
			if (!isList)
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
				CallNextDrawer(null);
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			else
			{
				string currentValueName = GetCurrentValueName();
				enumerable = OdinSelector<object>.DrawSelectorDropdown(label, currentValueName, ShowSelector, null);
			}
			if (enumerable != null && enumerable.Any())
			{
				AddResult(enumerable);
			}
		}

		private void OpenSelector()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
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
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			GenericSelector<object> genericSelector = CreateSelector();
			((Rect)(ref rect)).set_x((float)(int)((Rect)(ref rect)).get_x());
			((Rect)(ref rect)).set_y((float)(int)((Rect)(ref rect)).get_y());
			((Rect)(ref rect)).set_width((float)(int)((Rect)(ref rect)).get_width());
			((Rect)(ref rect)).set_height((float)(int)((Rect)(ref rect)).get_height());
			if (!isList)
			{
				Rect currentLayoutRect = GUIHelper.GetCurrentLayoutRect();
				((Rect)(ref rect)).set_xMax(((Rect)(ref currentLayoutRect)).get_xMax());
			}
			genericSelector.ShowInPopup(rect, new Vector2((float)base.Attribute.DropdownWidth, (float)base.Attribute.DropdownHeight));
			return genericSelector;
		}

		private GenericSelector<object> CreateSelector()
		{
			base.Attribute.IsUniqueList = base.Attribute.IsUniqueList || base.Attribute.ExcludeExistingValuesInList;
			IEnumerable<ValueDropdownItem> source = getValues() ?? Enumerable.Empty<ValueDropdownItem>();
			if (source.Any() && ((isList && base.Attribute.ExcludeExistingValuesInList) || (isListElement && base.Attribute.IsUniqueList)))
			{
				List<ValueDropdownItem> list = source.ToList();
				InspectorProperty inspectorProperty = base.Property.FindParent((InspectorProperty x) => x.ChildResolver is IOrderedCollectionResolver, includeSelf: true);
				IValueDropdownEqualityComparer comparer = new IValueDropdownEqualityComparer(isTypeLookup: false);
				inspectorProperty.ValueEntry.WeakValues.Cast<IEnumerable>().SelectMany((IEnumerable x) => x.Cast<object>()).ForEach(delegate(object x)
				{
					list.RemoveAll((ValueDropdownItem c) => comparer.Equals(c, x));
				});
				source = list;
			}
			GenericSelector<object> genericSelector = new GenericSelector<object>(base.Attribute.DropdownTitle, supportsMultiSelect: false, source.Select((ValueDropdownItem x) => new GenericSelectorItem<object>(x.Text, x.Value)));
			enableMultiSelect = isList && base.Attribute.IsUniqueList && !base.Attribute.ExcludeExistingValuesInList;
			if (base.Attribute.FlattenTreeView)
			{
				genericSelector.FlattenedTree = true;
			}
			if (isList && !base.Attribute.ExcludeExistingValuesInList && base.Attribute.IsUniqueList)
			{
				genericSelector.CheckboxToggle = true;
			}
			else if (!enableMultiSelect)
			{
				genericSelector.EnableSingleClickToSelect();
			}
			if (isList && enableMultiSelect)
			{
				genericSelector.SelectionTree.Selection.SupportsMultiSelect = true;
				genericSelector.DrawConfirmSelectionButton = true;
			}
			genericSelector.SelectionTree.Config.DrawSearchToolbar = true;
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
			return genericSelector;
		}

		private string GetCurrentValueName()
		{
			if (!EditorGUI.get_showMixedValue())
			{
				object weakSmartValue = base.Property.ValueEntry.WeakSmartValue;
				return string.Concat(weakSmartValue);
			}
			return "—";
		}
	}
}
