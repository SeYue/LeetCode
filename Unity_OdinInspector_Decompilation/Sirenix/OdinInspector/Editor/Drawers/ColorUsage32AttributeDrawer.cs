using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws Color properties marked with <see cref="T:UnityEngine.ColorUsageAttribute" />.
	/// </summary>
	public sealed class ColorUsage32AttributeDrawer : OdinAttributeDrawer<ColorUsageAttribute, Color32>, IDefinesGenericMenuItems
	{
		private ColorPickerHDRConfig pickerConfig;

		protected override void Initialize()
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Expected O, but got Unknown
			pickerConfig = new ColorPickerHDRConfig(base.Attribute.minBrightness, base.Attribute.maxBrightness, base.Attribute.minExposureValue, base.Attribute.maxExposureValue);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			Rect controlRect = EditorGUILayout.GetControlRect(label != null, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			bool flag = false;
			if (Event.get_current().OnMouseDown(controlRect, 1, useEvent: false))
			{
				GUIHelper.PushEventType((EventType)12);
				flag = true;
			}
			base.ValueEntry.SmartValue = Color32.op_Implicit(EditorGUI.ColorField(controlRect, label ?? GUIContent.none, Color32.op_Implicit(base.ValueEntry.SmartValue), true, base.Attribute.showAlpha, base.Attribute.hdr, pickerConfig));
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
