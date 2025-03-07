using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class PropertyRangeValidator<T> : AttributeValidator<PropertyRangeAttribute, T> where T : struct
	{
		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		private ValueResolver<double> minValueGetter;

		private ValueResolver<double> maxValueGetter;

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
			minValueGetter = ValueResolver.Get(base.Property, base.Attribute.MinGetter, base.Attribute.Min);
			maxValueGetter = ValueResolver.Get(base.Property, base.Attribute.MaxGetter, base.Attribute.Max);
		}

		protected override void Validate(ValidationResult result)
		{
			if (minValueGetter.HasError || maxValueGetter.HasError)
			{
				result.Message = ValueResolver.GetCombinedErrors(minValueGetter, maxValueGetter);
				result.ResultType = ValidationResultType.Error;
				return;
			}
			double value = minValueGetter.GetValue();
			double value2 = maxValueGetter.GetValue();
			if (!GenericNumberUtility.NumberIsInRange(base.ValueEntry.SmartValue, value, value2))
			{
				result.Message = "Number is not in range.";
				result.ResultType = ValidationResultType.Error;
			}
		}
	}
}
