using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties with a <see cref="F:Sirenix.OdinInspector.Editor.PropertyValueState.ReferencePathConflict" /> set.
	/// </summary>
	[DrawerPriority(0.5, 0.0, 0.0)]
	[AllowGUIEnabledForReadonly]
	public sealed class ReferencePathConflictDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : class
	{
		private static readonly bool IsUnityObject = typeof(Object).IsAssignableFrom(typeof(T));

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
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected O, but got Unknown
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			if (propertyValueEntry.ValueState == PropertyValueState.ReferencePathConflict)
			{
				if (IsUnityObject)
				{
					bool showMixedValue = EditorGUI.get_showMixedValue();
					EditorGUI.set_showMixedValue(true);
					CallNextDrawer(label);
					EditorGUI.set_showMixedValue(showMixedValue);
				}
				else
				{
					EditorGUILayout.LabelField(label, new GUIContent("Reference path conflict... (right-click to resolve)"), (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
			}
			else
			{
				CallNextDrawer(label);
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Expected O, but got Unknown
			//IL_0118: Expected O, but got Unknown
			if (property.ValueEntry.ValueState != PropertyValueState.ReferencePathConflict)
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
				tree.ObjectIsReferenced(value, out var referencePath);
				string text2 = "Resolve reference path conflict with.../" + ((Object)tree.WeakTargets[i]).get_name() + " -> " + referencePath + " (" + text + ")";
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
