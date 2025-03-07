using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class RequiredValidator<T> : AttributeValidator<RequiredAttribute, T> where T : class
	{
		private ValueResolver<string> errorMessageGetter;

		protected override void Initialize()
		{
			if (base.Attribute.ErrorMessage != null)
			{
				errorMessageGetter = ValueResolver.GetForString(base.Property, base.Attribute.ErrorMessage);
			}
		}

		protected override void Validate(ValidationResult result)
		{
			if (!IsValid(base.ValueEntry.SmartValue))
			{
				result.ResultType = base.Attribute.MessageType.ToValidationResultType();
				result.Message = ((errorMessageGetter != null) ? errorMessageGetter.GetValue() : (base.Property.NiceName + " is required"));
			}
		}

		private bool IsValid(T memberValue)
		{
			if (memberValue == null)
			{
				return false;
			}
			if (memberValue is string && string.IsNullOrEmpty(memberValue as string))
			{
				return false;
			}
			if (memberValue is Object && memberValue as Object == (Object)null)
			{
				return false;
			}
			return true;
		}
	}
}
