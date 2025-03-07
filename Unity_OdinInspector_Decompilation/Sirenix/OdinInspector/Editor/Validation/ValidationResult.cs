using System;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class ValidationResult
	{
		public string Message;

		public ValidationResultType ResultType;

		public object ResultValue;

		public ValidationSetup Setup;

		public string Path;

		[Obsolete("Use Path instead.", false)]
		public string GetFullPath()
		{
			return Path;
		}

		public void RerunValidation()
		{
			if (Setup.Validator != null)
			{
				ValidationResult result = this;
				ValidationSetup setup = Setup;
				try
				{
					Setup.Validator.RunValidation(ref result);
				}
				catch (Exception ex)
				{
					Setup = setup;
					Message = "An exception was thrown during validation of property " + Setup.Validator.Property.Path + ": " + ex.ToString();
					ResultType = ValidationResultType.Error;
				}
			}
		}

		public ValidationResult CreateCopy()
		{
			ValidationResult validationResult = new ValidationResult();
			validationResult.Path = Path;
			validationResult.Message = Message;
			validationResult.ResultType = ResultType;
			validationResult.ResultValue = ResultValue;
			validationResult.Setup = Setup;
			return validationResult;
		}
	}
}
