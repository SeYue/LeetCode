using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Evaluates all strings, enums and primitive types and ensures EditorGUI.showMixedValue is true if there are any value conflicts in the current selection.
	/// </summary>
	[DrawerPriority(0.5, 0.0, 0.0)]
	[AllowGUIEnabledForReadonly]
	public sealed class PrimitiveValueConflictDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Sets the drawer to only be evaluated on primitive types, strings and enums.
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			if (!type.IsPrimitive && type != typeof(string))
			{
				return type.IsEnum;
			}
			return true;
		}

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			return property.Tree.WeakTargets.Count > 1;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			if (propertyValueEntry.ValueState == PropertyValueState.PrimitiveValueConflict)
			{
				GUI.set_changed(false);
				EditorGUI.set_showMixedValue(true);
				CallNextDrawer(label);
				if (GUI.get_changed())
				{
					for (int i = 0; i < propertyValueEntry.ValueCount; i++)
					{
						propertyValueEntry.Values[i] = propertyValueEntry.SmartValue;
					}
				}
				EditorGUI.set_showMixedValue(false);
			}
			else
			{
				EditorGUI.set_showMixedValue(false);
				CallNextDrawer(label);
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Expected O, but got Unknown
			//IL_00f7: Expected O, but got Unknown
			if (property.ValueEntry.ValueState != PropertyValueState.PrimitiveValueConflict)
			{
				return;
			}
			PropertyTree tree = property.Tree;
			if (!typeof(Object).IsAssignableFrom(tree.TargetType))
			{
				return;
			}
			for (int i = 0; i < tree.WeakTargets.Count; i++)
			{
				object value = property.ValueEntry.WeakValues[i];
				string text = ((value == null) ? "null" : value.ToString());
				string text2 = "Resolve value conflict with.../" + ((Object)tree.WeakTargets[i]).get_name() + " (" + text + ")";
				genericMenu.AddItem(new GUIContent(text2), false, (MenuFunction)delegate
				{
					property.Tree.DelayActionUntilRepaint(delegate
					{
						for (int j = 0; j < property.ValueEntry.WeakValues.Count; j++)
						{
							property.ValueEntry.WeakValues[j] = value;
						}
					});
				});
			}
		}
	}
}
