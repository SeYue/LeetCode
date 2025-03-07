using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class RangeValidator<T> : AttributeValidator<RangeAttribute, T> where T : struct
	{
		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		public override bool CanValidateProperty(InspectorProperty property)
		{
			if (!IsNumber)
			{
				return IsVector;
			}
			return true;
		}

		protected override void Validate(ValidationResult result)
		{
			if (!GenericNumberUtility.NumberIsInRange(base.ValueEntry.SmartValue, base.Attribute.min, base.Attribute.max))
			{
				result.Message = "Number is not in range.";
				result.ResultType = ValidationResultType.Error;
			}
		}
	}
}
