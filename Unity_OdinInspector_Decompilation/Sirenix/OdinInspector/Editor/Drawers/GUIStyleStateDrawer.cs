using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// The GUIStyleState Drawer
	/// </summary>
	/// <seealso cref="!:Sirenix.OdinInspector.Editor.OdinValueDrawer&lt;UnityEngine.GUIStyleState&gt;" />
	public class GUIStyleStateDrawer : OdinValueDrawer<GUIStyleState>
	{
		private bool isVisible;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			isVisible = SirenixEditorGUI.ExpandFoldoutByDefault;
		}

		/// <summary>
		/// Draws the property with GUILayout support.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Expected O, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Expected O, but got Unknown
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<GUIStyleState> propertyValueEntry = base.ValueEntry;
			InspectorProperty inspectorProperty = propertyValueEntry.Property;
			if (label != null)
			{
				isVisible = SirenixEditorGUI.Foldout(isVisible, label);
				if (SirenixEditorGUI.BeginFadeGroup(isVisible, isVisible))
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
					propertyValueEntry.SmartValue.set_background((Texture2D)SirenixEditorFields.UnityObjectField(label, (Object)(object)propertyValueEntry.SmartValue.get_background(), typeof(Texture2D), true));
					propertyValueEntry.SmartValue.set_textColor(EditorGUILayout.ColorField(label ?? GUIContent.none, propertyValueEntry.SmartValue.get_textColor(), (GUILayoutOption[])(object)new GUILayoutOption[0]));
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				}
				SirenixEditorGUI.EndFadeGroup();
			}
			else
			{
				propertyValueEntry.SmartValue.set_background((Texture2D)SirenixEditorFields.UnityObjectField(label, (Object)(object)propertyValueEntry.SmartValue.get_background(), typeof(Texture2D), true));
				propertyValueEntry.SmartValue.set_textColor(EditorGUILayout.ColorField(label ?? GUIContent.none, propertyValueEntry.SmartValue.get_textColor(), (GUILayoutOption[])(object)new GUILayoutOption[0]));
			}
		}
	}
}
