using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.DetailedInfoBoxAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.DetailedInfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.RequiredAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnInspectorGUIAttribute" />
	[DrawerPriority(0.0, 100.0, 0.0)]
	public sealed class DetailedInfoBoxAttributeDrawer : OdinAttributeDrawer<DetailedInfoBoxAttribute>
	{
		private bool drawMessageBox;

		private MessageType messageType;

		private bool valid;

		private ValueResolver<bool> visibleIfGetter;

		private ValueResolver<string> messageGetter;

		private ValueResolver<string> detailsGetter;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			visibleIfGetter = ValueResolver.Get(base.Property, base.Attribute.VisibleIf, fallbackValue: true);
			messageGetter = ValueResolver.GetForString(base.Property, base.Attribute.Message);
			detailsGetter = ValueResolver.GetForString(base.Property, base.Attribute.Details);
			valid = !visibleIfGetter.HasError && !messageGetter.HasError && !detailsGetter.HasError;
			base.Property.State.Create("ShowDetailedMessage", persistent: false, defaultValue: false);
			switch (base.Attribute.InfoMessageType)
			{
			case InfoMessageType.None:
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
			default:
				Debug.LogError((object)("Unknown InfoBoxType: " + base.Attribute.InfoMessageType));
				break;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Invalid comparison between Unknown and I4
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			ValueResolver.DrawErrors(visibleIfGetter, messageGetter, detailsGetter);
			if (valid)
			{
				if ((int)Event.get_current().get_type() == 8)
				{
					drawMessageBox = visibleIfGetter.GetValue();
				}
				if (drawMessageBox)
				{
					base.Property.State.Set("ShowDetailedMessage", !SirenixEditorGUI.DetailedMessageBox(messageGetter.GetValue(), detailsGetter.GetValue(), messageType, !base.Property.State.Get<bool>("ShowDetailedMessage")));
				}
			}
			CallNextDrawer(label);
		}
	}
}
