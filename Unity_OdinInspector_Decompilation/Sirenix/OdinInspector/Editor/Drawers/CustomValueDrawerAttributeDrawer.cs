using System;
using System.Collections;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />
	[DrawerPriority(0.0, 0.0, double.MaxValue)]
	public class CustomValueDrawerAttributeDrawer<T> : OdinAttributeDrawer<CustomValueDrawerAttribute, T>
	{
		private ValueResolver customDrawer;

		private static readonly NamedValue[] customDrawerArgs = new NamedValue[2]
		{
			new NamedValue("label", typeof(GUIContent)),
			new NamedValue("callNextDrawer", typeof(Func<GUIContent, bool>))
		};

		public override bool CanDrawTypeFilter(Type type)
		{
			return !typeof(IList).IsAssignableFrom(type);
		}

		protected override void Initialize()
		{
			customDrawer = ValueResolver.Get(base.ValueEntry.BaseValueType, base.Property, base.Attribute.Action, customDrawerArgs);
			if (!customDrawer.HasError)
			{
				customDrawer.Context.NamedValues.Set("callNextDrawer", new Func<GUIContent, bool>(base.CallNextDrawer));
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (customDrawer.ErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(customDrawer.ErrorMessage);
				CallNextDrawer(label);
			}
			else
			{
				customDrawer.Context.NamedValues.Set("label", label);
				base.ValueEntry.SmartValue = (T)customDrawer.GetWeakValue();
			}
		}
	}
}
