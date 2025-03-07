using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public abstract class ValueValidator<TValue> : Validator, DefaultValidatorLocator.IValueValidator_InternalTemporaryHack
	{
		private static readonly Dictionary<Type, bool> ValidatorIsLegacyMap = new Dictionary<Type, bool>(FastTypeComparer.Instance);

		private bool? isLegacy_backing;

		Type DefaultValidatorLocator.IValueValidator_InternalTemporaryHack.ValidatedType => typeof(TValue);

		private bool IsLegacy
		{
			get
			{
				if (!isLegacy_backing.HasValue)
				{
					Type type = GetType();
					if (!ValidatorIsLegacyMap.TryGetValue(type, out var value))
					{
						MethodInfo method = type.GetMethod("Validate", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[2]
						{
							typeof(TValue),
							typeof(ValidationResult)
						}, null);
						value = method != null;
						ValidatorIsLegacyMap.Add(type, value);
					}
					isLegacy_backing = value;
				}
				return isLegacy_backing.Value;
			}
		}

		public IPropertyValueEntry<TValue> ValueEntry { get; private set; }

		[Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
		public sealed override bool CanValidateValues()
		{
			return IsLegacy;
		}

		[Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
		public sealed override bool CanValidateMembers()
		{
			return false;
		}

		[Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
		public sealed override bool CanValidateMember(MemberInfo member, Type memberValueType)
		{
			return false;
		}

		[Obsolete("Override Initialize() instead, and use this.Property for context and value information.", false)]
		public sealed override void Initialize(MemberInfo member, Type memberValueType)
		{
			throw new NotSupportedException("Value validators cannot validate members");
		}

		[Obsolete("Override RunValidation(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
		public sealed override void RunMemberValidation(object parentInstance, MemberInfo member, object memberValue, Object root, ref ValidationResult result)
		{
			throw new NotSupportedException("Value validators cannot validate members");
		}

		[Obsolete("Override RunValidation(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
		public sealed override void RunValueValidation(object value, Object root, ref ValidationResult result)
		{
			RunValidation(ref result);
		}

		[Obsolete("Override Validate(ref ValidationResult result) instead, and use this.Property and this.ValueEntry for context and value information.", false)]
		protected virtual void Validate(TValue value, ValidationResult result)
		{
			Validate(result);
		}

		protected virtual void Validate(ValidationResult result)
		{
			result.ResultType = ValidationResultType.Warning;
			result.Message = "Validation logic for " + GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
		}

		public sealed override void RunValidation(ref ValidationResult result)
		{
			if (result == null)
			{
				result = new ValidationResult();
			}
			result.Setup = new ValidationSetup
			{
				Kind = ValidationKind.Value,
				Validator = this,
				Value = base.Property.ValueEntry.WeakSmartValue,
				Root = (object)(base.Property.SerializationRoot.ValueEntry.WeakValues[0] as Object)
			};
			result.Path = base.Property.Path;
			ValueEntry = base.Property.ValueEntry as IPropertyValueEntry<TValue>;
			if (ValueEntry == null)
			{
				result.ResultValue = null;
				result.ResultType = ValidationResultType.Error;
				result.Message = "Property " + base.Property.NiceName + " did not have validator " + GetType().GetNiceName() + "'s expected value entry of type '" + typeof(TValue).GetNiceName() + "' on it, but instead a value entry of type '" + base.Property.ValueEntry.TypeOfValue.GetNiceName() + "'!";
				return;
			}
			result.ResultValue = null;
			result.ResultType = ValidationResultType.Valid;
			result.Message = "";
			try
			{
				if (IsLegacy)
				{
					Validate((TValue)base.Property.ValueEntry.WeakSmartValue, result);
				}
				else
				{
					Validate(result);
				}
			}
			catch (Exception innerException)
			{
				while (innerException is TargetInvocationException)
				{
					innerException = innerException.InnerException;
				}
				result.ResultType = ValidationResultType.Error;
				result.Message = "An exception was thrown during validation: " + innerException.ToString();
			}
		}
	}
}
