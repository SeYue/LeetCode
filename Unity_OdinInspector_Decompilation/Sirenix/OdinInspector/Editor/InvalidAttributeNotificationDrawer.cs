using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[DrawerPriority(-1.0, -1.0, -1.0)]
	[OdinDontRegister]
	public class InvalidAttributeNotificationDrawer<TInvalidAttribute> : OdinDrawer
	{
		private class Context
		{
			public string ErrorMessage;

			public string ValidTypeMessage;

			public bool IsFolded = true;
		}

		private Context context;

		protected override void Initialize()
		{
			context = new Context();
			StringBuilder stringBuilder = new StringBuilder("Attribute '").Append(typeof(TInvalidAttribute).GetNiceName()).Append("' cannot be put on property '").Append(base.Property.Name)
				.Append("'");
			if (base.Property.ValueEntry != null)
			{
				stringBuilder.Append(" of base type '").Append(base.Property.ValueEntry.BaseValueType.GetNiceName()).Append("'");
			}
			stringBuilder.Append('.');
			context.ErrorMessage = stringBuilder.ToString();
			stringBuilder.Length = 0;
			List<Type> validTargets = DrawerUtilities.InvalidAttributeTargetUtility.GetValidTargets(typeof(TInvalidAttribute));
			stringBuilder.AppendLine("The following types are valid:");
			stringBuilder.AppendLine();
			for (int i = 0; i < validTargets.Count; i++)
			{
				Type type = validTargets[i];
				stringBuilder.Append(type.GetNiceName());
				if (type.IsGenericParameter)
				{
					stringBuilder.Append(" ").Append(type.GetGenericParameterConstraintsString(useFullTypeNames: true));
				}
				stringBuilder.AppendLine();
			}
			stringBuilder.Append("Supported collections where the element type is any of the above types");
			context.ValidTypeMessage = stringBuilder.ToString();
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			context.IsFolded = SirenixEditorGUI.DetailedMessageBox(context.ErrorMessage, context.ValidTypeMessage, (MessageType)3, context.IsFolded);
			CallNextDrawer(label);
		}
	}
}
