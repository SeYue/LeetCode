using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// <para>
	/// When multiple objects are selected and inspected, this his drawer ensures UnityEditor.EditorGUI.showMixedValue
	/// gets set to true if there are any conflicts in the selection for any given property.
	/// Otherwise the next drawer is called.
	/// </para>
	/// <para>This drawer also implements <see cref="T:Sirenix.OdinInspector.Editor.IDefinesGenericMenuItems" /> and provides a right-click context menu item for resolving conflicts if any.</para>
	/// </summary>
	[DrawerPriority(0.5, 0.0, 0.0)]
	[AllowGUIEnabledForReadonly]
	public sealed class ReferenceValueConflictDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : class
	{
		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			if (!property.IsTreeRoot)
			{
				return property.Tree.WeakTargets.Count > 1;
			}
			return false;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			if (propertyValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
			{
				GUIHelper.PushGUIEnabled(GUI.get_enabled() && propertyValueEntry.IsEditable);
				if (typeof(Object).IsAssignableFrom(propertyValueEntry.TypeOfValue))
				{
					bool showMixedValue = EditorGUI.get_showMixedValue();
					EditorGUI.set_showMixedValue(true);
					CallNextDrawer(label);
					EditorGUI.set_showMixedValue(showMixedValue);
				}
				else
				{
					bool showMixedValue2 = EditorGUI.get_showMixedValue();
					EditorGUI.set_showMixedValue(true);
					propertyValueEntry.SmartValue = SirenixEditorFields.PolymorphicObjectField(label, propertyValueEntry.SmartValue, propertyValueEntry.BaseValueType, propertyValueEntry.Property.GetAttribute<AssetsOnlyAttribute>() == null) as T;
					EditorGUI.set_showMixedValue(showMixedValue2);
				}
				GUIHelper.PopGUIEnabled();
			}
			else
			{
				CallNextDrawer(label);
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Expected O, but got Unknown
			//IL_00fc: Expected O, but got Unknown
			if (property.ValueEntry.ValueState != PropertyValueState.ReferenceValueConflict)
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
				string text = ((value == null) ? "null" : value.GetType().GetNiceName());
				string text2 = "Resolve type conflict with.../" + ((Object)tree.WeakTargets[i]).get_name() + " (" + text + ")";
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
