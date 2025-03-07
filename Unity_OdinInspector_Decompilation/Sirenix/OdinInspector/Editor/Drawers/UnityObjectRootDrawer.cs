using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(0.0, 100.0, 0.0)]
	public sealed class UnityObjectRootDrawer<T> : OdinValueDrawer<T> where T : Object
	{
		public static readonly bool IsGameObject = typeof(T) == typeof(GameObject);

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			return property.IsTreeRoot;
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (IsGameObject)
			{
				SirenixEditorGUI.MessageBox("Odin does not currently have a full GameObject inspector window substitute implemented, so a GameObject cannot be directly inspected inline in the editor.");
				SirenixEditorFields.UnityObjectField((Object)(object)base.ValueEntry.SmartValue, typeof(GameObject), true);
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUIHelper.PushGUIEnabled((Object)(object)base.ValueEntry.SmartValue != (Object)null);
				string t = (((Object)(object)base.ValueEntry.SmartValue != (Object)null) ? ("Open Inspector window for " + ((Object)base.ValueEntry.SmartValue).get_name()) : "Open Inspector window (null)");
				if (GUILayout.Button(GUIHelper.TempContent(t), (GUILayoutOption[])(object)new GUILayoutOption[0]))
				{
					GUIHelper.OpenInspectorWindow((Object)(object)base.ValueEntry.SmartValue);
					GUIUtility.ExitGUI();
				}
				t = (((Object)(object)base.ValueEntry.SmartValue != (Object)null) ? ("Select " + ((Object)base.ValueEntry.SmartValue).get_name()) : "Select GO (null)");
				if (GUILayout.Button(GUIHelper.TempContent(t), (GUILayoutOption[])(object)new GUILayoutOption[0]))
				{
					Selection.set_activeObject((Object)(object)base.ValueEntry.SmartValue);
					GUIUtility.ExitGUI();
				}
				GUIHelper.PopGUIEnabled();
				GUILayout.EndHorizontal();
			}
			else
			{
				int count = base.Property.Children.Count;
				for (int i = 0; i < count; i++)
				{
					base.Property.Children[i].Draw();
				}
			}
		}
	}
}
