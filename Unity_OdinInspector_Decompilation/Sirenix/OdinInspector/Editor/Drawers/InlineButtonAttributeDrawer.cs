using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.InlineButtonAttribute" />
	/// </summary>
	[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
	public sealed class InlineButtonAttributeDrawer<T> : OdinAttributeDrawer<InlineButtonAttribute, T>
	{
		private ValueResolver<string> labelGetter;

		private ActionResolver clickAction;

		private ValueResolver<bool> showIfGetter;

		private bool show = true;

		protected override void Initialize()
		{
			if (base.Attribute.Label != null)
			{
				labelGetter = ValueResolver.GetForString(base.Property, base.Attribute.Label);
			}
			else
			{
				labelGetter = ValueResolver.Get(base.Property, null, base.Attribute.Action.SplitPascalCase());
			}
			clickAction = ActionResolver.Get(base.Property, base.Attribute.Action);
			showIfGetter = ValueResolver.Get(base.Property, base.Attribute.ShowIf, fallbackValue: true);
			show = showIfGetter.GetValue();
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Invalid comparison between Unknown and I4
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			if (labelGetter.HasError || clickAction.HasError)
			{
				labelGetter.DrawError();
				clickAction.DrawError();
				CallNextDrawer(label);
				return;
			}
			if ((int)Event.get_current().get_type() == 8)
			{
				show = showIfGetter.GetValue();
			}
			if (show)
			{
				EditorGUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				EditorGUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
				CallNextDrawer(label);
				EditorGUILayout.EndVertical();
				string value = labelGetter.GetValue();
				if (GUILayout.Button(value, EditorStyles.get_miniButton(), (GUILayoutOption[])GUILayoutOptions.ExpandWidth(expand: false).MinWidth(20f)))
				{
					base.Property.RecordForUndo("Click " + value);
					clickAction.DoActionForAllSelectionIndices();
				}
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				CallNextDrawer(label);
			}
		}
	}
}
