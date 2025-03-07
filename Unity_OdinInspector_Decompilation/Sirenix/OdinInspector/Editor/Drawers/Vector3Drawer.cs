using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Vector3 property drawer.
	/// </summary>
	public sealed class Vector3Drawer : OdinValueDrawer<Vector3>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			Rect labelRect;
			Rect val = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
			EditorGUI.BeginChangeCheck();
			Vector4 val2 = SirenixEditorFields.VectorPrefixSlideRect(labelRect, Vector4.op_Implicit(base.ValueEntry.SmartValue));
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = Vector4.op_Implicit(val2);
			}
			bool flag = SirenixEditorFields.ResponsiveVectorComponentFields && ((Rect)(ref val)).get_width() >= 185f;
			GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
			base.ValueEntry.Property.Children[0].Draw(flag ? GUIHelper.TempContent("X") : null);
			base.ValueEntry.Property.Children[1].Draw(flag ? GUIHelper.TempContent("Y") : null);
			base.ValueEntry.Property.Children[2].Draw(flag ? GUIHelper.TempContent("Z") : null);
			GUIHelper.PopLabelWidth();
			SirenixEditorGUI.EndHorizontalPropertyLayout();
		}

		/// <summary>
		/// Populates the generic menu for the property.
		/// </summary>
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Expected O, but got Unknown
			//IL_006b: Expected O, but got Unknown
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Expected O, but got Unknown
			//IL_0097: Expected O, but got Unknown
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Expected O, but got Unknown
			//IL_00c3: Expected O, but got Unknown
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Expected O, but got Unknown
			//IL_00fa: Expected O, but got Unknown
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Expected O, but got Unknown
			//IL_0126: Expected O, but got Unknown
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Expected O, but got Unknown
			//IL_0152: Expected O, but got Unknown
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Expected O, but got Unknown
			//IL_017e: Expected O, but got Unknown
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Expected O, but got Unknown
			//IL_01aa: Expected O, but got Unknown
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Expected O, but got Unknown
			//IL_01d6: Expected O, but got Unknown
			Vector3 val = (Vector3)property.ValueEntry.WeakSmartValue;
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			genericMenu.AddItem(new GUIContent("Normalize"), Mathf.Approximately(((Vector3)(ref val)).get_magnitude(), 1f), (MenuFunction)delegate
			{
				NormalizeEntries(property);
			});
			genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0, 0)"), val == Vector3.get_zero(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector3.get_zero());
			});
			genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1, 1)"), val == Vector3.get_one(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector3.get_one());
			});
			genericMenu.AddSeparator("");
			genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0, 0)"), val == Vector3.get_right(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector3.get_right());
			});
			genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0, 0)"), val == Vector3.get_left(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector3.get_left());
			});
			genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1, 0)"), val == Vector3.get_up(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector3.get_up());
			});
			genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1, 0)"), val == Vector3.get_down(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector3.get_down());
			});
			genericMenu.AddItem(new GUIContent("Forward", "Set the vector property to (0, 0, 1)"), val == Vector3.get_forward(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector3.get_forward());
			});
			genericMenu.AddItem(new GUIContent("Back", "Set the vector property to (0, 0, -1)"), val == Vector3.get_back(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector3.get_back());
			});
		}

		private void SetVector(InspectorProperty property, Vector3 value)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			property.Tree.DelayActionUntilRepaint(delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				property.ValueEntry.WeakSmartValue = value;
			});
		}

		private void NormalizeEntries(InspectorProperty property)
		{
			property.Tree.DelayActionUntilRepaint(delegate
			{
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				IPropertyValueEntry propertyValueEntry = property.ValueEntry;
				Vector3 val = (Vector3)property.ValueEntry.WeakSmartValue;
				propertyValueEntry.WeakSmartValue = ((Vector3)(ref val)).get_normalized();
			});
		}
	}
}
