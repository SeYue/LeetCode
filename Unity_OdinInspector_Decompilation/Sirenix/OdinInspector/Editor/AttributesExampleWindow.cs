using System;
using System.Linq;
using Sirenix.OdinInspector.Editor.Examples;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class AttributesExampleWindow : OdinMenuEditorWindow
	{
		private OdinAttributeExampleItem example;

		private Vector2 scrollPosition;

		public static void OpenWindow()
		{
			OpenWindow(null);
		}

		public static void OpenWindow(Type attributeType)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			bool flag = Resources.FindObjectsOfTypeAll<AttributesExampleWindow>().Length == 0;
			AttributesExampleWindow window = EditorWindow.GetWindow<AttributesExampleWindow>();
			if (flag)
			{
				window.MenuWidth = 250f;
				((EditorWindow)window).set_position(GUIHelper.GetEditorWindowRect().AlignCenterXY(850f, 700f));
			}
			if (attributeType != null)
			{
				window.ForceMenuTreeRebuild();
				OdinMenuItem odinMenuItem = window.MenuTree.EnumerateTree().FirstOrDefault((OdinMenuItem x) => x.Value == attributeType);
				if (odinMenuItem != null)
				{
					window.MenuTree.Selection.Clear();
					window.MenuTree.Selection.Add(odinMenuItem);
				}
			}
		}

		protected override OdinMenuTree BuildMenuTree()
		{
			OdinMenuTree odinMenuTree = new OdinMenuTree();
			odinMenuTree.Selection.SupportsMultiSelect = false;
			odinMenuTree.Selection.SelectionChanged += SelectionChanged;
			odinMenuTree.Config.DrawSearchToolbar = true;
			odinMenuTree.Config.DefaultMenuStyle.Height = 22;
			AttributeExampleUtilities.BuildMenuTree(odinMenuTree);
			return odinMenuTree;
		}

		private void SelectionChanged(SelectionChangedType obj)
		{
			if (example != null)
			{
				example.OnDeselected();
				example = null;
			}
			Type type = base.MenuTree.Selection.Select((OdinMenuItem i) => i.Value).FilterCast<Type>().FirstOrDefault();
			if (type != null)
			{
				example = AttributeExampleUtilities.GetExample(type);
			}
		}

		protected override void DrawEditors()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			Rect position = ((EditorWindow)this).get_position();
			float num = Mathf.Max(300f, ((Rect)(ref position)).get_width() - MenuWidth - 4f);
			position = ((EditorWindow)this).get_position();
			GUILayout.BeginArea(new Rect(4f, 0f, num, ((Rect)(ref position)).get_height()));
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, (GUILayoutOption[])GUILayoutOptions.ExpandWidth(expand: false));
			GUILayout.Space(4f);
			if (example != null)
			{
				example.Draw();
			}
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		private void OnDisable()
		{
			if (example != null)
			{
				example.OnDeselected();
				example = null;
			}
		}

		protected override void OnDestroy()
		{
			if (example != null)
			{
				example.OnDeselected();
				example = null;
			}
		}
	}
}
