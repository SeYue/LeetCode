using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.TitleGroupAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.TitleGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.TitleAttribute" />
	public sealed class TitleGroupAttributeDrawer : OdinGroupDrawer<TitleGroupAttribute>
	{
		public ValueResolver<string> TitleHelper;

		public ValueResolver<string> SubtitleHelper;

		protected override void Initialize()
		{
			TitleHelper = ValueResolver.GetForString(base.Property, base.Attribute.GroupName);
			SubtitleHelper = ValueResolver.GetForString(base.Property, base.Attribute.Subtitle);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			InspectorProperty inspectorProperty = base.Property;
			TitleGroupAttribute titleGroupAttribute = base.Attribute;
			if (inspectorProperty != inspectorProperty.Tree.GetRootProperty(0))
			{
				EditorGUILayout.Space();
			}
			SirenixEditorGUI.Title(TitleHelper.GetValue(), SubtitleHelper.GetValue(), (TextAlignment)titleGroupAttribute.Alignment, titleGroupAttribute.HorizontalLine, titleGroupAttribute.BoldTitle);
			GUIHelper.PushIndentLevel(EditorGUI.get_indentLevel() + (titleGroupAttribute.Indent ? 1 : 0));
			for (int i = 0; i < inspectorProperty.Children.Count; i++)
			{
				InspectorProperty inspectorProperty2 = inspectorProperty.Children[i];
				inspectorProperty2.Draw(inspectorProperty2.Label);
			}
			GUIHelper.PopIndentLevel();
		}
	}
}
