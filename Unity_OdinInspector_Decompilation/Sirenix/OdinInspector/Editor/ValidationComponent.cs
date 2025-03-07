using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public sealed class ValidationComponent : PropertyComponent, IDisposable
	{
		public readonly IValidatorLocator ValidatorLocator;

		private IList<Validator> validators;

		public ValidationComponent(InspectorProperty property, IValidatorLocator validatorLocator)
			: base(property)
		{
			ValidatorLocator = validatorLocator;
		}

		public void Dispose()
		{
			if (validators == null)
			{
				return;
			}
			for (int i = 0; i < validators.Count; i++)
			{
				IDisposable disposable = validators[i] as IDisposable;
				if (disposable != null)
				{
					try
					{
						disposable.Dispose();
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
					}
				}
			}
			validators = null;
		}

		public IList<Validator> GetValidators()
		{
			if (validators == null)
			{
				if (ValidatorLocator.PotentiallyHasValidatorsFor(Property))
				{
					validators = ValidatorLocator.GetValidators(Property);
				}
				else
				{
					validators = new Validator[0];
				}
			}
			return validators;
		}

		public override void Reset()
		{
			validators = null;
		}

		public void ValidateProperty(ref List<ValidationResult> results)
		{
			if (results == null)
			{
				results = new List<ValidationResult>();
			}
			if (validators == null)
			{
				GetValidators();
			}
			for (int i = 0; i < validators.Count; i++)
			{
				Validator validator = validators[i];
				ValidationResult result = null;
				try
				{
					validator.RunValidation(ref result);
				}
				catch (Exception innerException)
				{
					while (innerException is TargetInvocationException)
					{
						innerException = innerException.InnerException;
					}
					result = new ValidationResult
					{
						Message = "Exception was thrown during validation of property " + Property.NiceName + ": " + innerException.ToString(),
						ResultType = ValidationResultType.Error,
						ResultValue = innerException,
						Setup = new ValidationSetup
						{
							Member = Property.Info.GetMemberInfo(),
							ParentInstance = Property.ParentValues[0],
							Root = Property.SerializationRoot.ValueEntry.WeakValues[0],
							Validator = validator,
							Value = ((Property.ValueEntry == null) ? null : Property.ValueEntry.WeakSmartValue)
						},
						Path = Property.Path
					};
				}
				if (result != null && result.ResultType != ValidationResultType.IgnoreResult)
				{
					results.Add(result);
				}
			}
		}
	}
}
