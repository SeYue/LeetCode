using System;

namespace Sirenix.OdinInspector.Editor.Validation
{
	/// <summary>
	/// Apply this to an assembly to register validators for the validation system.
	/// This enables locating of all relevant validator types very quickly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class RegisterValidatorAttribute : Attribute
	{
		public readonly Type ValidatorType;

		public int Priority;

		public RegisterValidatorAttribute(Type validatorType)
		{
			ValidatorType = validatorType;
		}
	}
}
