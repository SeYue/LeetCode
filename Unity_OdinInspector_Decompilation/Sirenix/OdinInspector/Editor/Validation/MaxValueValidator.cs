using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class MaxValueValidator<T> : AttributeValidator<MaxValueAttribute, T> where T : struct
	{
		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

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
			maxValueGetter = ValueResolver.Get(base.Property, base.Attribute.Expression, base.Attribute.MaxValue);
		}

		protected override void Validate(ValidationResult result)
		{
			if (maxValueGetter.HasError)
			{
				result.Message = maxValueGetter.ErrorMessage;
				result.ResultType = ValidationResultType.Error;
				return;
			}
			double value = maxValueGetter.GetValue();
			if (!GenericNumberUtility.NumberIsInRange(base.ValueEntry.SmartValue, double.MinValue, value))
			{
				result.Message = "Number is larger than " + value + ".";
				result.ResultType = ValidationResultType.Error;
			}
		}
	}
}
