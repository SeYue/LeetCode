using System;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(0.3, 0.0, 0.0)]
	public sealed class MinValueAttributeDrawer<T> : OdinAttributeDrawer<MinValueAttribute, T> where T : struct
	{
		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		private ValueResolver<double> minValueGetter;

		public override bool CanDrawTypeFilter(Type type)
		{
			if (!IsNumber)
			{
				return IsVector;
			}
			return true;
		}

		protected override void Initialize()
		{
			minValueGetter = ValueResolver.Get(base.Property, base.Attribute.Expression, base.Attribute.MinValue);
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (minValueGetter.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(minValueGetter.ErrorMessage);
			}
			CallNextDrawer(label);
			if (!minValueGetter.HasError)
			{
				T smartValue = base.ValueEntry.SmartValue;
				double value = minValueGetter.GetValue();
				if (!GenericNumberUtility.NumberIsInRange(smartValue, value, double.MaxValue))
				{
					base.ValueEntry.SmartValue = GenericNumberUtility.Clamp(smartValue, value, double.MaxValue);
				}
			}
		}
	}
}
