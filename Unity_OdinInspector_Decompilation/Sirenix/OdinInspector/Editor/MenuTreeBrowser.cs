using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[HideReferenceObjectPicker]
	[HideLabel]
	public abstract class MenuTreeBrowser
	{
		private OdinMenuTree tree;

		private ResizableColumn[] columns = new ResizableColumn[2]
		{
			ResizableColumn.FlexibleColumn(280f, 80f),
			ResizableColumn.DynamicColumn()
		};

		/// <summary>
		/// The content padding
		/// </summary>
		[HideInInspector]
		public Vector2 ContentPadding = Vector4.op_Implicit(new Vector4(10f, 0f));

		public ResizableColumn MenuColumn => columns[0];

		/// <summary>
		/// Gets the value selected value.
		/// </summary>
		[EnableGUI]
		[HideLabel]
		[ShowInInspector]
		[DisableContextMenu(true, false)]
		[VerticalGroup(0f)]
		[InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
		[SuppressInvalidAttributeError]
		public object Value
		{
			get
			{
				if (tree == null)
				{
					return null;
				}
				return tree.Selection.FirstOrDefault()?.Value;
			}
		}

		/// <summary>
		/// Draws the menu tree.
		/// </summary>
		[OnInspectorGUI]
		[PropertyOrder(-1f)]
		protected virtual void DrawMenuTree()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			Rect currentLayoutRect = GUIHelper.GetCurrentLayoutRect();
			GUITableUtilities.ResizeColumns(currentLayoutRect, columns);
			currentLayoutRect = EditorGUILayout.BeginVertical((GUILayoutOption[])GUILayoutOptions.Width(MenuColumn.ColWidth));
			EditorGUI.DrawRect(currentLayoutRect, SirenixGUIStyles.DarkEditorBackground);
			tree = tree ?? BuildMenuTree();
			if (tree != null)
			{
				tree.Config.AutoHandleKeyboardNavigation = true;
				tree.DrawMenuTree();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndVertical();
			GUILayout.Space(ContentPadding.x);
			SirenixEditorGUI.DrawBorders(currentLayoutRect, 1);
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.Space(ContentPadding.y);
		}

		[OnInspectorGUI]
		[PropertyOrder(-1000f)]
		private void BeginDrawEditor()
		{
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
		}

		[OnInspectorGUI]
		[PropertyOrder(1000f)]
		private void EndDrawEditor()
		{
			GUILayout.Space(ContentPadding.y);
			GUILayout.EndVertical();
			GUILayout.Space(ContentPadding.x);
			GUILayout.EndHorizontal();
		}

		/// <summary>
		/// Invokes BuildMenuTree.
		/// </summary>
		public void ForceRebuildMenuTree()
		{
			tree = BuildMenuTree();
		}

		public abstract OdinMenuTree BuildMenuTree();
	}
}
