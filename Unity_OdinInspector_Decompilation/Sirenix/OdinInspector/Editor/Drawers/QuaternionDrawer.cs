using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Quaternion property drawer.
	/// </summary>
	public sealed class QuaternionDrawer : OdinValueDrawer<Quaternion>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<Quaternion> propertyValueEntry = base.ValueEntry;
			propertyValueEntry.SmartValue = SirenixEditorFields.RotationField(label, propertyValueEntry.SmartValue, GlobalConfig<GeneralDrawerConfig>.Instance.QuaternionDrawMode);
		}

		/// <summary>
		/// Populates the generic menu for the property.
		/// </summary>
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Expected O, but got Unknown
			//IL_0080: Expected O, but got Unknown
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Expected O, but got Unknown
			//IL_00a1: Expected O, but got Unknown
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Expected O, but got Unknown
			//IL_00c2: Expected O, but got Unknown
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Expected O, but got Unknown
			//IL_00f4: Expected O, but got Unknown
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Expected O, but got Unknown
			//IL_013b: Expected O, but got Unknown
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Expected O, but got Unknown
			//IL_0167: Expected O, but got Unknown
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Expected O, but got Unknown
			//IL_0193: Expected O, but got Unknown
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Expected O, but got Unknown
			//IL_01bf: Expected O, but got Unknown
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Expected O, but got Unknown
			//IL_01eb: Expected O, but got Unknown
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Expected O, but got Unknown
			//IL_0217: Expected O, but got Unknown
			if (property.ValueEntry.WeakSmartValue == null)
			{
				return;
			}
			Quaternion val = (Quaternion)property.ValueEntry.WeakSmartValue;
			float num = default(float);
			Vector3 val2 = default(Vector3);
			((Quaternion)(ref val)).ToAngleAxis(ref num, ref val2);
			genericMenu.AddSeparator("");
			QuaternionDrawMode quaternionDrawMode = GlobalConfig<GeneralDrawerConfig>.Instance.QuaternionDrawMode;
			genericMenu.AddItem(new GUIContent("Euler"), quaternionDrawMode == QuaternionDrawMode.Eulers, (MenuFunction)delegate
			{
				SetDrawMode(property, QuaternionDrawMode.Eulers);
			});
			genericMenu.AddItem(new GUIContent("Angle axis"), quaternionDrawMode == QuaternionDrawMode.AngleAxis, (MenuFunction)delegate
			{
				SetDrawMode(property, QuaternionDrawMode.AngleAxis);
			});
			genericMenu.AddItem(new GUIContent("Raw"), quaternionDrawMode == QuaternionDrawMode.Raw, (MenuFunction)delegate
			{
				SetDrawMode(property, QuaternionDrawMode.Raw);
			});
			genericMenu.AddSeparator("");
			genericMenu.AddItem(new GUIContent("Zero"), val == Quaternion.get_identity(), (MenuFunction)delegate
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				for (int i = 0; i < property.ValueEntry.ValueCount; i++)
				{
					property.ValueEntry.WeakValues[i] = Quaternion.get_identity();
				}
			});
			genericMenu.AddSeparator("");
			genericMenu.AddItem(new GUIContent("Right", "Set the axis to (1, 0, 0)"), val2 == Vector3.get_right() && num != 0f, (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetAxis(property, Vector3.get_right());
			});
			genericMenu.AddItem(new GUIContent("Left", "Set the axis to (-1, 0, 0)"), val2 == Vector3.get_left(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetAxis(property, Vector3.get_left());
			});
			genericMenu.AddItem(new GUIContent("Up", "Set the axis to (0, 1, 0)"), val2 == Vector3.get_up(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetAxis(property, Vector3.get_up());
			});
			genericMenu.AddItem(new GUIContent("Down", "Set the axis to (0, -1, 0)"), val2 == Vector3.get_down(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetAxis(property, Vector3.get_down());
			});
			genericMenu.AddItem(new GUIContent("Forward", "Set the axis property to (0, 0, 1)"), val2 == Vector3.get_forward(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetAxis(property, Vector3.get_forward());
			});
			genericMenu.AddItem(new GUIContent("Back", "Set the axis property to (0, 0, -1)"), val2 == Vector3.get_back(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetAxis(property, Vector3.get_back());
			});
		}

		private void SetAxis(InspectorProperty property, Vector3 axis)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			property.Tree.DelayActionUntilRepaint(delegate
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				Quaternion val = (Quaternion)property.ValueEntry.WeakSmartValue;
				float num = default(float);
				Vector3 val2 = default(Vector3);
				((Quaternion)(ref val)).ToAngleAxis(ref num, ref val2);
				Quaternion val3 = Quaternion.AngleAxis(num, axis);
				for (int i = 0; i < property.ValueEntry.ValueCount; i++)
				{
					property.ValueEntry.WeakValues[i] = val3;
				}
			});
		}

		private void SetDrawMode(InspectorProperty property, QuaternionDrawMode mode)
		{
			if (GlobalConfig<GeneralDrawerConfig>.Instance.QuaternionDrawMode != mode)
			{
				GlobalConfig<GeneralDrawerConfig>.Instance.QuaternionDrawMode = mode;
			}
		}
	}
}
