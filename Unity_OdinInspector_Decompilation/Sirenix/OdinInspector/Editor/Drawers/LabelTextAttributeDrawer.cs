using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.LabelTextAttribute" />.
	/// Creates a new GUIContent, with the provided label text, before calling further down in the drawer chain.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.LabelTextAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.HideLabelAttribute" />
	/// <seealso cref="T:UnityEngine.TooltipAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.LabelWidthAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.TitleAttribute" />
	/// <seealso cref="T:UnityEngine.HeaderAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.GUIColorAttribute" />
	[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
	public sealed class LabelTextAttributeDrawer : OdinAttributeDrawer<LabelTextAttribute>
	{
		private ValueResolver<string> textProvider;

		private GUIContent overrideLabel;

		protected override void Initialize()
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			textProvider = ValueResolver.GetForString(base.Property, base.Attribute.Text);
			overrideLabel = new GUIContent();
		}

		/// <summary>
		/// Draws the attribute.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (textProvider.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(textProvider.ErrorMessage);
				CallNextDrawer(label);
				return;
			}
			string text = textProvider.GetValue();
			GUIContent label2;
			if (text == null)
			{
				label2 = null;
			}
			else
			{
				if (base.Attribute.NicifyText)
				{
					text = ObjectNames.NicifyVariableName(text);
				}
				overrideLabel.set_text(text);
				label2 = overrideLabel;
			}
			CallNextDrawer(label2);
		}
	}
}
