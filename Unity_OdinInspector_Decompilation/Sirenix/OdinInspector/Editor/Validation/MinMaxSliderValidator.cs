using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class MinMaxSliderValidator<T> : AttributeValidator<MinMaxSliderAttribute, T> where T : struct
	{
		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		private ValueResolver<double> minValueGetter;

		private ValueResolver<double> maxValueGetter;

		private ValueResolver<Vector2> rangeGetter;

		public override bool CanValidateProperty(InspectorProperty property)
		{
			if (!IsNumber)
			{
				return IsVector;
			}
			return true;
		}

		protected override void Initialize()
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			if (base.Attribute.MinMaxValueGetter != null)
			{
				rangeGetter = ValueResolver.Get<Vector2>(base.Property, base.Attribute.MinMaxValueGetter, new Vector2(base.Attribute.MinValue, base.Attribute.MaxValue));
				return;
			}
			minValueGetter = ValueResolver.Get(base.Property, base.Attribute.MinValueGetter, (double)base.Attribute.MinValue);
			maxValueGetter = ValueResolver.Get(base.Property, base.Attribute.MaxValueGetter, (double)base.Attribute.MaxValue);
		}

		protected override void Validate(ValidationResult result)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			double min;
			double max;
			if (rangeGetter != null)
			{
				if (rangeGetter.HasError)
				{
					result.Message = rangeGetter.ErrorMessage;
					result.ResultType = ValidationResultType.Error;
					return;
				}
				Vector2 value = rangeGetter.GetValue();
				min = value.x;
				max = value.y;
			}
			else
			{
				if (minValueGetter.HasError || maxValueGetter.HasError)
				{
					result.Message = ValueResolver.GetCombinedErrors(minValueGetter, maxValueGetter);
					result.ResultType = ValidationResultType.Error;
					return;
				}
				min = minValueGetter.GetValue();
				max = maxValueGetter.GetValue();
			}
			if (!GenericNumberUtility.NumberIsInRange(base.ValueEntry.SmartValue, min, max))
			{
				result.Message = "Number is not in range.";
				result.ResultType = ValidationResultType.Error;
			}
		}
	}
}
