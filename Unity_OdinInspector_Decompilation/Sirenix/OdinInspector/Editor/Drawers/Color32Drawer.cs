using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Color32 property drawer.
	/// </summary>
	public sealed class Color32Drawer : PrimitiveCompositeDrawer<Color32>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyField(IPropertyValueEntry<Color32> entry, GUIContent label)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			Rect val = EditorGUILayout.GetControlRect(label != null, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			if (label != null)
			{
				val = EditorGUI.PrefixLabel(val, label);
			}
			bool flag = false;
			if (Event.get_current().OnMouseDown(val, 1, useEvent: false))
			{
				GUIHelper.PushEventType((EventType)12);
				flag = true;
			}
			entry.SmartValue = Color32.op_Implicit(EditorGUI.ColorField(val, Color32.op_Implicit(entry.SmartValue)));
			if (flag)
			{
				GUIHelper.PopEventType();
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			ColorDrawer.PopulateGenericMenu((IPropertyValueEntry<Color32>)property.ValueEntry, genericMenu);
		}
	}
}
