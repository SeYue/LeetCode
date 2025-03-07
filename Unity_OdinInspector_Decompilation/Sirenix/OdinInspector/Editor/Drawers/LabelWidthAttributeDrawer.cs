using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with the <see cref="T:Sirenix.OdinInspector.LabelWidthAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.LabelTextAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.HideLabelAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.LabelWidthAttribute" />
	/// <seealso cref="T:UnityEngine.TooltipAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.TitleAttribute" />
	/// <seealso cref="T:UnityEngine.HeaderAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.GUIColorAttribute" />
	[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
	public sealed class LabelWidthAttributeDrawer : OdinAttributeDrawer<LabelWidthAttribute>
	{
		/// <summary>
		/// Draws the attribute.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			LabelWidthAttribute labelWidthAttribute = base.Attribute;
			if (labelWidthAttribute.Width < 0f)
			{
				GUIHelper.PushLabelWidth(GUIHelper.BetterLabelWidth + labelWidthAttribute.Width);
			}
			else
			{
				GUIHelper.PushLabelWidth(labelWidthAttribute.Width);
			}
			CallNextDrawer(label);
			GUIHelper.PopLabelWidth();
		}
	}
}
