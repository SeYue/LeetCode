using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class MinValueValidator<T> : AttributeValidator<MinValueAttribute, T> where T : struct
	{
		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		private ValueResolver<double> minValueGetter;

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
			minValueGetter = ValueResolver.Get(base.Property, base.Attribute.Expression, base.Attribute.MinValue);
		}

		protected override void Validate(ValidationResult result)
		{
			if (minValueGetter.HasError)
			{
				result.Message = minValueGetter.ErrorMessage;
				result.ResultType = ValidationResultType.Error;
				return;
			}
			double value = minValueGetter.GetValue();
			if (!GenericNumberUtility.NumberIsInRange(base.ValueEntry.SmartValue, value, double.MaxValue))
			{
				result.Message = "Number is smaller than " + value + ".";
				result.ResultType = ValidationResultType.Error;
			}
		}
	}
}
