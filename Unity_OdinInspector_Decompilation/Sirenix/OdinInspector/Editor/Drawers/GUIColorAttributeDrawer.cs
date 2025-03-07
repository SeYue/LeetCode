using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.GUIColorAttribute" />.
	/// This drawer sets the current GUI color, before calling the next drawer in the chain.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.GUIColorAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.LabelTextAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.TitleAttribute" />
	/// <seealso cref="T:UnityEngine.HeaderAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ColorPaletteAttribute" />
	[DrawerPriority(0.5, 0.0, 0.0)]
	public sealed class GUIColorAttributeDrawer : OdinAttributeDrawer<GUIColorAttribute>
	{
		internal static Color CurrentOuterColor = Color.get_white();

		private ValueResolver<Color> colorResolver;

		protected override void Initialize()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			colorResolver = ValueResolver.Get<Color>(base.Property, base.Attribute.GetColor, base.Attribute.Color);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (colorResolver.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(colorResolver.ErrorMessage);
				CallNextDrawer(label);
			}
			else
			{
				GUIHelper.PushColor(colorResolver.GetValue());
				CallNextDrawer(label);
				GUIHelper.PopColor();
			}
		}
	}
}
