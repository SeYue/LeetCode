using System;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(0.3, 0.0, 0.0)]
	public sealed class MaxValueAttributeDrawer<T> : OdinAttributeDrawer<MaxValueAttribute, T> where T : struct
	{
		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		private ValueResolver<double> maxValueGetter;

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
			maxValueGetter = ValueResolver.Get(base.Property, base.Attribute.Expression, base.Attribute.MaxValue);
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (maxValueGetter.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(maxValueGetter.ErrorMessage);
			}
			CallNextDrawer(label);
			if (!maxValueGetter.HasError)
			{
				T smartValue = base.ValueEntry.SmartValue;
				double value = maxValueGetter.GetValue();
				if (!GenericNumberUtility.NumberIsInRange(smartValue, double.MinValue, value))
				{
					base.ValueEntry.SmartValue = GenericNumberUtility.Clamp(smartValue, double.MinValue, value);
				}
			}
		}
	}
}
