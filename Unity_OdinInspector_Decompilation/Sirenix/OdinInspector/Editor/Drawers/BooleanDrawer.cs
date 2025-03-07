using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Bool property drawer.
	/// </summary>
	public sealed class BooleanDrawer : OdinValueDrawer<bool>
	{
		private GUILayoutOption[] options;

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			bool smartValue = base.ValueEntry.SmartValue;
			EditorGUI.BeginChangeCheck();
			if (label == null)
			{
				options = (GUILayoutOption[])(((object)options) ?? ((object)new GUILayoutOption[1] { GUILayout.ExpandWidth(false) }));
				float currentIndentAmount = GUIHelper.CurrentIndentAmount;
				Rect val = GUILayoutUtility.GetRect(15f + currentIndentAmount, EditorGUIUtility.get_singleLineHeight(), options).AddXMin(currentIndentAmount);
				GUIHelper.PushIndentLevel(0);
				smartValue = EditorGUI.Toggle(val, smartValue);
				GUIHelper.PopIndentLevel();
			}
			else
			{
				smartValue = EditorGUILayout.Toggle(label, smartValue, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = smartValue;
			}
		}
	}
}
