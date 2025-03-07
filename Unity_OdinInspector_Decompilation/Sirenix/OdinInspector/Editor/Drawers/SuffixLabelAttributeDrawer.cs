using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.SuffixLabelAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.LabelTextAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyTooltipAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InlineButtonAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.CustomValueDrawerAttribute" />
	[AllowGUIEnabledForReadonly]
	[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
	public sealed class SuffixLabelAttributeDrawer : OdinAttributeDrawer<SuffixLabelAttribute>
	{
		private ValueResolver<string> labelResolver;

		protected override void Initialize()
		{
			labelResolver = ValueResolver.GetForString(base.Property, base.Attribute.Label);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			if (labelResolver.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(labelResolver.ErrorMessage);
			}
			if (base.Attribute.Overlay)
			{
				CallNextDrawer(label);
				GUIHelper.PushGUIEnabled(enabled: true);
				GUI.Label(GUILayoutUtility.GetLastRect().HorizontalPadding(0f, 8f), labelResolver.GetValue(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
				GUIHelper.PopGUIEnabled();
				return;
			}
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
			CallNextDrawer(label);
			GUILayout.EndVertical();
			GUIHelper.PushGUIEnabled(enabled: true);
			GUILayout.Label(labelResolver.GetValue(), SirenixGUIStyles.RightAlignedGreyMiniLabel, (GUILayoutOption[])GUILayoutOptions.ExpandWidth(expand: false));
			GUIHelper.PopGUIEnabled();
			GUILayout.EndHorizontal();
		}
	}
}
