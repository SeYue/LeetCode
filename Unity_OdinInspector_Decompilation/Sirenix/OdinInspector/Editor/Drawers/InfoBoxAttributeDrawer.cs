using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />.
	/// Draws an info box above the property. Error and warning info boxes can be tracked by Odin Scene Validator.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DetailedInfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.RequiredAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />
	[DrawerPriority(0.0, 10001.0, 0.0)]
	public sealed class InfoBoxAttributeDrawer : OdinAttributeDrawer<InfoBoxAttribute>
	{
		private bool drawMessageBox;

		private ValueResolver<bool> visibleIfResolver;

		private ValueResolver<string> messageResolver;

		private MessageType messageType;

		protected override void Initialize()
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			visibleIfResolver = ValueResolver.Get(base.Property, base.Attribute.VisibleIf, fallbackValue: true);
			messageResolver = ValueResolver.GetForString(base.Property, base.Attribute.Message);
			drawMessageBox = visibleIfResolver.GetValue();
			switch (base.Attribute.InfoMessageType)
			{
			default:
				messageType = (MessageType)0;
				break;
			case InfoMessageType.Info:
				messageType = (MessageType)1;
				break;
			case InfoMessageType.Warning:
				messageType = (MessageType)2;
				break;
			case InfoMessageType.Error:
				messageType = (MessageType)3;
				break;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Invalid comparison between Unknown and I4
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			bool flag = true;
			if (visibleIfResolver.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(visibleIfResolver.ErrorMessage);
				flag = false;
			}
			if (messageResolver.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(messageResolver.ErrorMessage);
				flag = false;
			}
			if (!flag)
			{
				CallNextDrawer(label);
				return;
			}
			if (base.Attribute.GUIAlwaysEnabled)
			{
				GUIHelper.PushGUIEnabled(enabled: true);
			}
			if ((int)Event.get_current().get_type() == 8)
			{
				drawMessageBox = visibleIfResolver.GetValue();
			}
			if (drawMessageBox)
			{
				SirenixEditorGUI.MessageBox(messageResolver.GetValue(), messageType);
			}
			if (base.Attribute.GUIAlwaysEnabled)
			{
				GUIHelper.PopGUIEnabled();
			}
			CallNextDrawer(label);
		}
	}
}
