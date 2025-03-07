using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public abstract class AttributeValidator<TAttribute> : Validator, IAttributeValidator where TAttribute : Attribute
	{
		private static readonly Dictionary<Type, bool> ValidatorIsLegacyMap = new Dictionary<Type, bool>(FastTypeComparer.Instance);

		private bool? isValueValidator_backing;

		private bool? isLegacy_backing;

		private bool IsValueValidator
		{
			get
			{
				if (!isValueValidator_backing.HasValue)
				{
					isValueValidator_backing = this is IAttributeValueValidator;
				}
				return isValueValidator_backing.Value;
			}
		}

		private bool IsLegacy
		{
			get
			{
				if (!isLegacy_backing.HasValue)
				{
					if (IsValueValidator)
					{
						isLegacy_backing = (this as IAttributeValueValidator).IsLegacy;
					}
					else
					{
						Type type = GetType();
						if (!ValidatorIsLegacyMap.TryGetValue(type, out var value))
						{
							MethodInfo method = type.GetMethod("Validate", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[4]
							{
								typeof(object),
								typeof(object),
								typeof(MemberInfo),
								typeof(ValidationResult)
							}, null);
							value = method != null;
							ValidatorIsLegacyMap.Add(type, value);
						}
						isLegacy_backing = value;
					}
				}
				return isLegacy_backing.Value;
			}
		}

		public TAttribute Attribute { get; private set; }

		[Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
		public sealed override bool CanValidateMembers()
		{
			return IsLegacy;
		}

		[Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
		public sealed override bool CanValidateValues()
		{
			return false;
		}

		[Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
		public sealed override bool CanValidateValue(Type type)
		{
			return false;
		}

		[Obsolete("Override Initialize() instead, and use this.Property for context and value information.", false)]
		public sealed override void Initialize(Type type)
		{
			throw new NotSupportedException("Attribute validators cannot validate values without members");
		}

		[Obsolete("Override RunValidation(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
		public sealed override void RunValueValidation(object value, Object root, ref ValidationResult result)
		{
			throw new NotSupportedException("Attribute validators cannot validate values without members");
		}

		[Obsolete("Override RunValidation(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
		public sealed override void RunMemberValidation(object parentInstance, MemberInfo member, object memberValue, Object root, ref ValidationResult result)
		{
			RunValidation(ref result);
		}

		public sealed override void RunValidation(ref ValidationResult result)
		{
			if (result == null)
			{
				result = new ValidationResult();
			}
			result.Setup = new ValidationSetup
			{
				Kind = ValidationKind.Member,
				Validator = this,
				Member = base.Property.Info.GetMemberInfo(),
				ParentInstance = base.Property.ParentValues[0],
				Value = ((base.Property.ValueEntry == null) ? null : base.Property.ValueEntry.WeakSmartValue),
				Root = base.Property.SerializationRoot.ValueEntry.WeakValues[0]
			};
			result.Path = base.Property.Path;
			if (result.Setup.Member == null)
			{
				InspectorProperty inspectorProperty = base.Property.FindParent((InspectorProperty p) => p.Info.GetMemberInfo() != null, includeSelf: false);
				if (inspectorProperty != null)
				{
					result.Setup.Member = inspectorProperty.Info.GetMemberInfo();
				}
			}
			bool isValueValidator = IsValueValidator;
			if (isValueValidator)
			{
				IPropertyValueEntry propertyValueEntry = (this as IAttributeValueValidator).RefreshValueEntry();
				if (propertyValueEntry == null)
				{
					result.ResultValue = null;
					result.ResultType = ValidationResultType.Error;
					result.Message = "Property " + base.Property.NiceName + " did not have validator " + GetType().GetNiceName() + "'s expected value entry of type '" + (this as IAttributeValueValidator).GetValueType().GetNiceName() + "' on it, but instead a value entry of type '" + base.Property.ValueEntry.TypeOfValue.GetNiceName() + "'!";
				}
			}
			result.ResultValue = null;
			result.ResultType = ValidationResultType.Valid;
			result.Message = "";
			try
			{
				if (isValueValidator || !IsLegacy)
				{
					Validate(result);
				}
				else
				{
					Validate(result.Setup.ParentInstance, result.Setup.Value, result.Setup.Member, result);
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

		protected virtual void Validate(ValidationResult result)
		{
			result.ResultType = ValidationResultType.Warning;
			result.Message = "Validation logic for " + GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
		}

		[Obsolete("Override Validate(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
		protected virtual void Validate(object parentInstance, object memberValue, MemberInfo member, ValidationResult result)
		{
			Validate(result);
		}

		void IAttributeValidator.SetAttributeInstance(Attribute attribute)
		{
			Attribute = (TAttribute)attribute;
		}
	}
	public abstract class AttributeValidator<TAttribute, TValue> : AttributeValidator<TAttribute>, IAttributeValueValidator where TAttribute : Attribute
	{
		private static readonly Dictionary<Type, bool> ValidatorIsLegacyMap = new Dictionary<Type, bool>(FastTypeComparer.Instance);

		private bool? isLegacy_backing;

		private bool IsLegacy
		{
			get
			{
				if (!isLegacy_backing.HasValue)
				{
					Type type = GetType();
					if (!ValidatorIsLegacyMap.TryGetValue(type, out var value))
					{
						MethodInfo method = type.GetMethod("Validate", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[4]
						{
							typeof(object),
							typeof(TValue),
							typeof(MemberInfo),
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

		bool IAttributeValueValidator.IsLegacy => IsLegacy;

		Type IAttributeValueValidator.GetValueType()
		{
			return typeof(TValue);
		}

		IPropertyValueEntry IAttributeValueValidator.RefreshValueEntry()
		{
			return ValueEntry = base.Property.ValueEntry as IPropertyValueEntry<TValue>;
		}

		protected override void Validate(ValidationResult result)
		{
			if (IsLegacy)
			{
				Validate(result.Setup.ParentInstance, result.Setup.Value, result.Setup.Member, result);
				return;
			}
			result.ResultType = ValidationResultType.Warning;
			result.Message = "Validation logic for " + GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
		}

		[Obsolete("Override Validate(ref ValidationResult result) instead, and use this.Property and this.ValueEntry for context and value information.", false)]
		protected sealed override void Validate(object parentInstance, object memberValue, MemberInfo member, ValidationResult result)
		{
			Validate(parentInstance, (TValue)memberValue, member, result);
		}

		[Obsolete("Override Validate(ref ValidationResult result) instead, and use this.Property and this.ValueEntry for context and value information.", false)]
		protected virtual void Validate(object parentInstance, TValue memberValue, MemberInfo member, ValidationResult result)
		{
			result.ResultType = ValidationResultType.Warning;
			result.Message = "Validation logic for " + GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
		}
	}
}
