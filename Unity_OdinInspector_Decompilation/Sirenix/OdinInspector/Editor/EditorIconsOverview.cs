using System;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Opens a window which displays a list of all icons available from <see cref="T:Sirenix.Utilities.Editor.EditorIcons" />.
	/// </summary>
	public class EditorIconsOverview : OdinSelector<object>
	{
		[ShowInInspector]
		[PropertyOrder(30f)]
		[PropertyRange(10.0, 34.0)]
		[LabelWidth(50f)]
		[InfoBox("This is an overview of all available icons in the Sirenix.Utilities.Editor.EditorIcons utility class.", InfoMessageType.Info, null)]
		private float Size
		{
			get
			{
				return base.SelectionTree.DefaultMenuStyle.IconSize;
			}
			set
			{
				base.SelectionTree.DefaultMenuStyle.IconSize = value;
				base.SelectionTree.DefaultMenuStyle.Height = (int)value + 9;
			}
		}

		/// <summary>
		/// Opens a window which displays a list of all icons available from <see cref="T:Sirenix.Utilities.Editor.EditorIcons" />.
		/// </summary>
		public static void OpenEditorIconsOverview()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			OdinEditorWindow odinEditorWindow = OdinEditorWindow.InspectObject(new EditorIconsOverview());
			((EditorWindow)odinEditorWindow).ShowUtility();
			odinEditorWindow.WindowPadding = default(Vector4);
		}

		/// <summary>
		/// Builds the selection tree.
		/// </summary>
		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Expected O, but got Unknown
			DrawConfirmSelectionButton = false;
			tree.Config.DrawSearchToolbar = true;
			tree.DefaultMenuStyle.Height = 25;
			foreach (PropertyInfo item in from x in typeof(EditorIcons).GetProperties(BindingFlags.Static | BindingFlags.Public)
				orderby x.Name
				select x)
			{
				Type returnType = item.GetReturnType();
				if (typeof(Texture).IsAssignableFrom(returnType))
				{
					tree.Add(item.Name, (object)item.Name, (Texture)item.GetGetMethod().Invoke(null, null));
				}
				else if (typeof(EditorIcon).IsAssignableFrom(returnType))
				{
					tree.Add(item.Name, item.Name, (EditorIcon)item.GetGetMethod().Invoke(null, null));
				}
			}
		}
	}
}
