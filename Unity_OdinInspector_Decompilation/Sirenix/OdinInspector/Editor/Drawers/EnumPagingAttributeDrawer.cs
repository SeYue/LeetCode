using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Odin drawer for the <see cref="T:Sirenix.OdinInspector.EnumPagingAttribute" />.
	/// </summary>
	public class EnumPagingAttributeDrawer<T> : OdinAttributeDrawer<EnumPagingAttribute, T>
	{
		/// <summary>
		/// Returns <c>true</c> if the drawer can draw the type.
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			return type.IsEnum;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			Rect val = EditorGUILayout.GetControlRect(label != null, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			if (label != null)
			{
				val = EditorGUI.PrefixLabel(val, label);
			}
			Rect val2 = val.AlignRight(20f);
			Rect val3 = val2;
			((Rect)(ref val3)).set_x(((Rect)(ref val3)).get_x() - ((Rect)(ref val3)).get_width());
			((Rect)(ref val3)).set_height(((Rect)(ref val3)).get_height() - 1f);
			((Rect)(ref val2)).set_height(((Rect)(ref val2)).get_height() - 1f);
			if (GUI.Button(val3, GUIContent.none))
			{
				string[] names = Enum.GetNames(typeof(T));
				string name = Enum.GetName(typeof(T), propertyValueEntry.SmartValue);
				int num = ((IList<string>)names).IndexOf(name);
				num = MathUtilities.Wrap(num - 1, 0, names.Length);
				propertyValueEntry.SmartValue = (T)Enum.Parse(typeof(T), names[num]);
			}
			if (GUI.Button(val2, GUIContent.none))
			{
				string[] names2 = Enum.GetNames(typeof(T));
				string name2 = Enum.GetName(typeof(T), propertyValueEntry.SmartValue);
				int num2 = ((IList<string>)names2).IndexOf(name2);
				num2 = MathUtilities.Wrap(num2 + 1, 0, names2.Length);
				propertyValueEntry.SmartValue = (T)Enum.Parse(typeof(T), names2[num2]);
			}
			EditorIcons.TriangleLeft.Draw(val3.AlignCenter(16f, 16f));
			EditorIcons.TriangleRight.Draw(val2.AlignCenter(16f, 16f));
			((Rect)(ref val)).set_xMax(((Rect)(ref val)).get_xMax() - ((Rect)(ref val2)).get_width() * 2f);
			propertyValueEntry.WeakSmartValue = SirenixEditorFields.EnumDropdown(val, (Enum)propertyValueEntry.WeakSmartValue);
		}
	}
}
