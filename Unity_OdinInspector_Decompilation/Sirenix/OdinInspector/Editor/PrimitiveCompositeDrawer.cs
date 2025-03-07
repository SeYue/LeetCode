using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Property drawer for primitive composite properties.
	/// </summary>
	public abstract class PrimitiveCompositeDrawer<T> : OdinValueDrawer<T>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			bool flag = false;
			int count = propertyValueEntry.Property.Children.Count;
			for (int i = 0; i < count; i++)
			{
				InspectorProperty inspectorProperty = propertyValueEntry.Property.Children[i];
				if (inspectorProperty.ValueEntry != null && inspectorProperty.ValueEntry.ValueState == PropertyValueState.PrimitiveValueConflict)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				EditorGUI.set_showMixedValue(true);
				GUI.set_changed(false);
			}
			DrawPropertyField(propertyValueEntry, label);
			if (!flag)
			{
				return;
			}
			EditorGUI.set_showMixedValue(false);
			if (GUI.get_changed())
			{
				T smartValue = propertyValueEntry.SmartValue;
				for (int j = 0; j < propertyValueEntry.ValueCount; j++)
				{
					propertyValueEntry.Values[j] = smartValue;
				}
			}
		}

		/// <summary>
		/// Draws the property field.
		/// </summary>
		protected abstract void DrawPropertyField(IPropertyValueEntry<T> entry, GUIContent label);
	}
}
