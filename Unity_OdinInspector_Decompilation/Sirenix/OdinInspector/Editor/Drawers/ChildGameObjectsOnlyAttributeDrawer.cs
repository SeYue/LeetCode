using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class ChildGameObjectsOnlyAttributeDrawer<T> : OdinAttributeDrawer<ChildGameObjectsOnlyAttribute, T> where T : class
	{
		private bool isValidValues;

		private bool rootIsComponent;

		private int rootCount;

		private bool isList;

		protected override void Initialize()
		{
			Transform root = GetRoot(0);
			rootIsComponent = root != null;
			rootCount = base.Property.SerializationRoot.BaseValueEntry.WeakValues.Count;
			base.Property.ValueEntry.OnValueChanged += delegate
			{
				ValidateCurrentValue();
			};
			isList = base.Property.ChildResolver is ICollectionResolver;
			if (rootIsComponent)
			{
				ValidateCurrentValue();
			}
		}

		private Transform GetRoot(int index)
		{
			IPropertyValueCollection weakValues = base.Property.SerializationRoot.BaseValueEntry.WeakValues;
			object obj = weakValues[index];
			Component val = obj as Component;
			if (Object.op_Implicit((Object)(object)val))
			{
				return val.get_transform();
			}
			return null;
		}

		private void ValidateCurrentValue()
		{
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			isValidValues = true;
			T smartValue = propertyValueEntry.SmartValue;
			if (!Object.op_Implicit(smartValue as Object))
			{
				return;
			}
			for (int i = 0; i < rootCount; i++)
			{
				Transform root = GetRoot(i);
				T val = base.ValueEntry.Values[i];
				Object val2 = val as Object;
				if (Object.op_Implicit(val2))
				{
					Component val3 = val2 as Component;
					GameObject val4 = val2 as GameObject;
					if (Object.op_Implicit((Object)(object)val4))
					{
						val3 = (Component)(object)val4.get_transform();
					}
					if (!Object.op_Implicit((Object)(object)val3))
					{
						isValidValues = false;
						break;
					}
					Transform transform = val3.get_transform();
					if (!base.Attribute.IncludeSelf && (Object)(object)transform == (Object)(object)root)
					{
						isValidValues = false;
						break;
					}
					if (!IsRootOf(root, transform))
					{
						isValidValues = false;
						break;
					}
				}
			}
		}

		private string GetGameObjectPath(Transform root, Transform child)
		{
			if ((Object)(object)root == (Object)(object)child)
			{
				return ((Object)root).get_name();
			}
			string text = "";
			Transform val = child;
			while (Object.op_Implicit((Object)(object)val))
			{
				if (!base.Attribute.IncludeSelf && (Object)(object)val == (Object)(object)root)
				{
					return text.Trim('/');
				}
				text = ((Object)val).get_name() + "/" + text;
				if (base.Attribute.IncludeSelf && (Object)(object)val == (Object)(object)root)
				{
					return text.Trim('/');
				}
				val = val.get_parent();
			}
			return null;
		}

		private static bool IsRootOf(Transform root, Transform child)
		{
			Transform val = child;
			while (Object.op_Implicit((Object)(object)val))
			{
				if ((Object)(object)val == (Object)(object)root)
				{
					return true;
				}
				val = val.get_parent();
			}
			return false;
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Invalid comparison between Unknown and I4
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			if (!rootIsComponent)
			{
				CallNextDrawer(label);
				return;
			}
			if (rootCount > 1)
			{
				CallNextDrawer(label);
				return;
			}
			if (isList)
			{
				Action nextCustomAddFunction = CollectionDrawerStaticInfo.NextCustomAddFunction;
				CollectionDrawerStaticInfo.NextCustomAddFunction = ListAddButton;
				CallNextDrawer(label);
				CollectionDrawerStaticInfo.NextCustomAddFunction = nextCustomAddFunction;
				return;
			}
			if (!isValidValues)
			{
				SirenixEditorGUI.ErrorMessageBox("The object must be a child of the selected GameObject.");
			}
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			float num = 15f;
			if (label != null)
			{
				num += GUIHelper.BetterLabelWidth;
			}
			IEnumerable<Object> enumerable = OdinSelector<Object>.DrawSelectorDropdown(label, GUIContent.none, (Func<Rect, OdinSelector<Object>>)ShowSelector, GUIStyle.get_none(), (GUILayoutOption[])GUILayoutOptions.Width(num));
			if (enumerable != null && enumerable.Any())
			{
				base.ValueEntry.SmartValue = enumerable.FirstOrDefault() as T;
			}
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

		private void ListAddButton()
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			OdinSelector<Object> odinSelector = ShowSelector(default(Rect));
			odinSelector.SelectionConfirmed += delegate(IEnumerable<Object> x)
			{
				ICollectionResolver collectionResolver = base.Property.ChildResolver as ICollectionResolver;
				collectionResolver.QueueAdd(new object[1] { x.FirstOrDefault() });
			};
		}

		private OdinSelector<Object> ShowSelector(Rect rect)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			GenericSelector<Object> genericSelector = CreateSelector();
			Rect val = rect;
			Rect val2 = default(Rect);
			if (val == val2)
			{
				((Rect)(ref rect))._002Ector(Event.get_current().get_mousePosition(), Vector2.get_zero());
				((Rect)(ref rect)).set_x((float)(int)((Rect)(ref rect)).get_x());
				((Rect)(ref rect)).set_y((float)(int)((Rect)(ref rect)).get_y());
				((Rect)(ref rect)).set_width((float)(int)((Rect)(ref rect)).get_width());
				((Rect)(ref rect)).set_height((float)(int)((Rect)(ref rect)).get_height());
				if (!isList)
				{
					val2 = GUIHelper.GetCurrentLayoutRect();
					((Rect)(ref rect)).set_xMax(((Rect)(ref val2)).get_xMax());
				}
				genericSelector.ShowInPopup(rect, new Vector2(0f, 0f));
			}
			else
			{
				((Rect)(ref rect)).set_x((float)(int)((Rect)(ref rect)).get_x());
				((Rect)(ref rect)).set_y((float)(int)((Rect)(ref rect)).get_y());
				((Rect)(ref rect)).set_width((float)(int)((Rect)(ref rect)).get_width());
				((Rect)(ref rect)).set_height((float)(int)((Rect)(ref rect)).get_height());
				val2 = GUIHelper.GetCurrentLayoutRect();
				((Rect)(ref rect)).set_xMax(((Rect)(ref val2)).get_xMax());
				genericSelector.ShowInPopup(rect);
			}
			return genericSelector;
		}

		private GenericSelector<Object> CreateSelector()
		{
			Type t = (isList ? (base.Property.ChildResolver as ICollectionResolver).ElementType : typeof(T));
			bool flag = t == typeof(GameObject);
			Transform root = GetRoot(0);
			IEnumerable<Object> source = (from x in ((Component)root).GetComponentsInChildren(flag ? typeof(Transform) : t, base.Attribute.IncludeInactive)
				where base.Attribute.IncludeSelf || (Object)(object)x.get_transform() != (Object)(object)root
				select x).OfType<Object>();
			if (flag)
			{
				source = (from x in source.OfType<Component>()
					select x.get_gameObject()).OfType<Object>();
			}
			Func<Object, string> getMenuItemName = delegate(Object x)
			{
				Component val = x as Component;
				GameObject val2 = x as GameObject;
				Transform child = (Object.op_Implicit((Object)(object)val) ? val.get_transform() : val2.get_transform());
				return GetGameObjectPath(root, child);
			};
			GenericSelector<Object> genericSelector = new GenericSelector<Object>(null, supportsMultiSelect: false, getMenuItemName, source.Where((Object x) => ((object)x).GetType().InheritsFrom(t)));
			genericSelector.SelectionTree.Config.DrawSearchToolbar = true;
			T smartValue = base.ValueEntry.SmartValue;
			genericSelector.SetSelection(smartValue as Object);
			genericSelector.SelectionTree.EnumerateTree().AddThumbnailIcons(preferAssetPreviewAsIcon: true);
			(from x in genericSelector.SelectionTree.EnumerateTree()
				where (Object)(object)x.Icon == (Object)null
				select x).ForEach(delegate(OdinMenuItem x)
			{
				x.Icon = (Texture)(object)EditorIcons.UnityGameObjectIcon;
			});
			genericSelector.SelectionTree.EnumerateTree().ForEach(delegate(OdinMenuItem x)
			{
				x.Toggled = true;
			});
			genericSelector.EnableSingleClickToSelect();
			return genericSelector;
		}
	}
}
