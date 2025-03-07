using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws ulong properties marked with <see cref="T:UnityEngine.DelayedAttribute" />.
	/// </summary>
	public sealed class DelayedAttributeUInt64Drawer : OdinAttributeDrawer<DelayedAttribute, ulong>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			ulong result = base.ValueEntry.SmartValue;
			string text = result.ToString();
			text = ((label == null) ? EditorGUILayout.DelayedTextField(text, (GUILayoutOption[])GUILayoutOptions.MinWidth(0f)) : EditorGUILayout.DelayedTextField(label, text, (GUILayoutOption[])GUILayoutOptions.MinWidth(0f)));
			if (GUI.get_changed() && ulong.TryParse(text, out result))
			{
				base.ValueEntry.SmartValue = result;
			}
		}
	}
}
