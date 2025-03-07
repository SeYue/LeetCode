using System;
using System.Reflection;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public abstract class Validator
	{
		public InspectorProperty Property { get; private set; }

		public virtual RevalidationCriteria RevalidationCriteria => RevalidationCriteria.Always;

		public void Initialize(InspectorProperty property)
		{
			Property = property;
			Initialize();
			if (CanValidateMembers())
			{
				if (property.Info.HasBackingMembers)
				{
					if (property.ValueEntry == null)
					{
						throw new NoBackwardsCompatibilityForLegacyValidatorException();
					}
					Initialize(property.Info.GetMemberInfo(), property.ValueEntry.TypeOfValue);
				}
				else
				{
					InspectorProperty inspectorProperty = property.FindParent((InspectorProperty p) => p.Info.GetMemberInfo() != null, includeSelf: true);
					if (inspectorProperty == null)
					{
						throw new NoBackwardsCompatibilityForLegacyValidatorException();
					}
					if (inspectorProperty.ValueEntry == null)
					{
						throw new NoBackwardsCompatibilityForLegacyValidatorException();
					}
					Initialize(inspectorProperty.Info.GetMemberInfo(), inspectorProperty.ValueEntry.TypeOfValue);
				}
			}
			if (CanValidateValues())
			{
				if (property.ValueEntry == null)
				{
					throw new NoBackwardsCompatibilityForLegacyValidatorException();
				}
				Initialize(property.ValueEntry.TypeOfValue);
			}
		}

		public virtual bool CanValidateProperty(InspectorProperty property)
		{
			return true;
		}

		protected virtual void Initialize()
		{
		}

		public virtual void RunValidation(ref ValidationResult result)
		{
		}

		[Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
		public virtual bool CanValidateValues()
		{
			return false;
		}

		[Obsolete("Use CanValidateProperty(InspectorProperty property) instead. There is no longer a distinction between value and member validators; instead properties are validated.", false)]
		public virtual bool CanValidateValue(Type type)
		{
			return true;
		}

		[Obsolete("There is no longer a distinction between value and member validators; instead properties are validated. Override Initialize() and RunValidation(ref ValidationResult result) to implement a validator.", false)]
		public virtual bool CanValidateMembers()
		{
			return false;
		}

		[Obsolete("Use CanValidateProperty(InspectorProperty property) instead. There is no longer a distinction between value and member validators; instead properties are validated.", false)]
		public virtual bool CanValidateMember(MemberInfo member, Type memberValueType)
		{
			return true;
		}

		[Obsolete("Override RunValidation(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
		public virtual void RunValueValidation(object value, Object root, ref ValidationResult result)
		{
		}

		[Obsolete("Override RunValidation(ref ValidationResult result) instead, and use this.Property for context and value information.", false)]
		public virtual void RunMemberValidation(object parentInstance, MemberInfo member, object memberValue, Object root, ref ValidationResult result)
		{
		}

		[Obsolete("Override Initialize() instead, and use this.Property for context and value information.", false)]
		public virtual void Initialize(Type type)
		{
		}

		[Obsolete("Override Initialize() instead, and use this.Property for context and value information.", false)]
		public virtual void Initialize(MemberInfo member, Type memberValueType)
		{
		}
	}
}
