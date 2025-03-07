using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.TitleAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.TitleAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.TitleGroupAttribute" />
	[DrawerPriority(1.0, 0.0, 0.0)]
	public sealed class TitleAttributeDrawer : OdinAttributeDrawer<TitleAttribute>
	{
		private ValueResolver<string> titleResolver;

		private ValueResolver<string> subtitleResolver;

		protected override void Initialize()
		{
			titleResolver = ValueResolver.GetForString(base.Property, base.Attribute.Title);
			subtitleResolver = ValueResolver.GetForString(base.Property, base.Attribute.Subtitle);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (base.Property != base.Property.Tree.GetRootProperty(0))
			{
				EditorGUILayout.Space();
			}
			bool flag = true;
			if (titleResolver.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(titleResolver.ErrorMessage);
				flag = false;
			}
			if (subtitleResolver.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(subtitleResolver.ErrorMessage);
				flag = false;
			}
			if (flag)
			{
				SirenixEditorGUI.Title(titleResolver.GetValue(), subtitleResolver.GetValue(), (TextAlignment)base.Attribute.TitleAlignment, base.Attribute.HorizontalLine, base.Attribute.Bold);
			}
			CallNextDrawer(label);
		}
	}
}
