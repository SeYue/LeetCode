using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Vector4 property drawer.
	/// </summary>
	public sealed class Vector4Drawer : OdinValueDrawer<Vector4>, IDefinesGenericMenuItems
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
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			Rect labelRect;
			Rect val = SirenixEditorGUI.BeginHorizontalPropertyLayout(label, out labelRect);
			EditorGUI.BeginChangeCheck();
			Vector4 smartValue = SirenixEditorFields.VectorPrefixSlideRect(labelRect, base.ValueEntry.SmartValue);
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = smartValue;
			}
			bool flag = SirenixEditorFields.ResponsiveVectorComponentFields && ((Rect)(ref val)).get_width() >= 185f;
			GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
			base.ValueEntry.Property.Children[0].Draw(flag ? GUIHelper.TempContent("X") : null);
			base.ValueEntry.Property.Children[1].Draw(flag ? GUIHelper.TempContent("Y") : null);
			base.ValueEntry.Property.Children[2].Draw(flag ? GUIHelper.TempContent("Z") : null);
			base.ValueEntry.Property.Children[3].Draw(flag ? GUIHelper.TempContent("w") : null);
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
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Expected O, but got Unknown
			//IL_00ff: Expected O, but got Unknown
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Expected O, but got Unknown
			//IL_0130: Expected O, but got Unknown
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Expected O, but got Unknown
			//IL_0161: Expected O, but got Unknown
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Expected O, but got Unknown
			//IL_0192: Expected O, but got Unknown
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Expected O, but got Unknown
			//IL_01c3: Expected O, but got Unknown
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Expected O, but got Unknown
			//IL_01f4: Expected O, but got Unknown
			Vector4 val = (Vector4)property.ValueEntry.WeakSmartValue;
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			genericMenu.AddItem(new GUIContent("Normalize"), Mathf.Approximately(((Vector4)(ref val)).get_magnitude(), 1f), (MenuFunction)delegate
			{
				NormalizeEntries(property);
			});
			genericMenu.AddItem(new GUIContent("Zero", "Set the vector to (0, 0, 0, 0)"), val == Vector4.get_zero(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector4.op_Implicit(Vector3.get_zero()));
			});
			genericMenu.AddItem(new GUIContent("One", "Set the vector to (1, 1, 1, 1)"), val == Vector4.get_one(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector4.get_one());
			});
			genericMenu.AddSeparator("");
			genericMenu.AddItem(new GUIContent("Right", "Set the vector to (1, 0, 0, 0)"), Vector4.op_Implicit(val) == Vector3.get_right(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector4.op_Implicit(Vector3.get_right()));
			});
			genericMenu.AddItem(new GUIContent("Left", "Set the vector to (-1, 0, 0, 0)"), Vector4.op_Implicit(val) == Vector3.get_left(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector4.op_Implicit(Vector3.get_left()));
			});
			genericMenu.AddItem(new GUIContent("Up", "Set the vector to (0, 1, 0, 0)"), Vector4.op_Implicit(val) == Vector3.get_up(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector4.op_Implicit(Vector3.get_up()));
			});
			genericMenu.AddItem(new GUIContent("Down", "Set the vector to (0, -1, 0, 0)"), Vector4.op_Implicit(val) == Vector3.get_down(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector4.op_Implicit(Vector3.get_down()));
			});
			genericMenu.AddItem(new GUIContent("Forward", "Set the vector property to (0, 0, 1, 0)"), Vector4.op_Implicit(val) == Vector3.get_forward(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector4.op_Implicit(Vector3.get_forward()));
			});
			genericMenu.AddItem(new GUIContent("Back", "Set the vector property to (0, 0, -1, 0)"), Vector4.op_Implicit(val) == Vector3.get_back(), (MenuFunction)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				SetVector(property, Vector4.op_Implicit(Vector3.get_back()));
			});
		}

		private void SetVector(InspectorProperty property, Vector4 value)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			property.Tree.DelayActionUntilRepaint(delegate
			{
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				for (int i = 0; i < property.ValueEntry.ValueCount; i++)
				{
					property.ValueEntry.WeakValues[i] = value;
				}
			});
		}

		private void NormalizeEntries(InspectorProperty property)
		{
			property.Tree.DelayActionUntilRepaint(delegate
			{
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				for (int i = 0; i < property.ValueEntry.ValueCount; i++)
				{
					property.ValueEntry.WeakValues[i] = Vector4.Normalize((Vector4)property.ValueEntry.WeakValues[i]);
				}
			});
		}
	}
}
