using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Sirenix.OdinInspector.Editor.TypeSearch;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class DefaultValidatorLocator : IValidatorLocator
	{
		internal interface IValueValidator_InternalTemporaryHack
		{
			Type ValidatedType { get; }
		}

		public class BrokenAttributeValidator : Validator
		{
			private Type brokenValidatorType;

			private string message;

			public BrokenAttributeValidator(Type brokenValidatorType, string message)
			{
				this.brokenValidatorType = brokenValidatorType;
				this.message = message;
			}

			public override void RunValidation(ref ValidationResult result)
			{
				if (result == null)
				{
					result = new ValidationResult();
				}
				result.Setup = new ValidationSetup
				{
					Validator = this,
					Member = base.Property.Info.GetMemberInfo(),
					ParentInstance = base.Property.ParentValues[0],
					Value = ((base.Property.ValueEntry == null) ? null : base.Property.ValueEntry.WeakSmartValue),
					Root = base.Property.SerializationRoot.ValueEntry.WeakValues[0]
				};
				result.ResultType = ValidationResultType.Error;
				result.Message = message;
				result.Path = base.Property.Path;
			}
		}

		private static readonly Dictionary<Type, Validator> EmptyInstances;

		public static readonly TypeSearchIndex ValidatorSearchIndex;

		public Func<Type, bool> CustomValidatorFilter;

		protected readonly List<TypeSearchResult> ResultList = new List<TypeSearchResult>();

		protected readonly List<TypeSearchResult[]> SearchResultList = new List<TypeSearchResult[]>();

		protected readonly Dictionary<Type, int> AttributeNumberMap = new Dictionary<Type, int>(FastTypeComparer.Instance);

		static DefaultValidatorLocator()
		{
			EmptyInstances = new Dictionary<Type, Validator>(FastTypeComparer.Instance);
			ValidatorSearchIndex = new TypeSearchIndex
			{
				MatchedTypeLogName = "validator"
			};
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly member in assemblies)
			{
				foreach (RegisterValidatorAttribute attribute in member.GetAttributes<RegisterValidatorAttribute>())
				{
					Type validatorType = attribute.ValidatorType;
					TypeSearchInfo typeToIndex;
					if (!validatorType.InheritsFrom<Validator>())
					{
						Debug.LogError((object)("The registered validator type " + attribute.ValidatorType.GetNiceFullName() + " is not derived from " + typeof(Validator).GetNiceFullName()));
					}
					else if (validatorType.ImplementsOpenGenericClass(typeof(AttributeValidator<, >)))
					{
						TypeSearchIndex validatorSearchIndex = ValidatorSearchIndex;
						typeToIndex = new TypeSearchInfo
						{
							MatchType = validatorType,
							Targets = validatorType.GetArgumentsOfInheritedOpenGenericClass(typeof(AttributeValidator<, >)),
							Priority = attribute.Priority
						};
						validatorSearchIndex.AddIndexedType(typeToIndex);
					}
					else if (validatorType.ImplementsOpenGenericClass(typeof(AttributeValidator<>)))
					{
						TypeSearchIndex validatorSearchIndex2 = ValidatorSearchIndex;
						typeToIndex = new TypeSearchInfo
						{
							MatchType = validatorType,
							Targets = validatorType.GetArgumentsOfInheritedOpenGenericClass(typeof(AttributeValidator<>)),
							Priority = attribute.Priority
						};
						validatorSearchIndex2.AddIndexedType(typeToIndex);
					}
					else if (validatorType.ImplementsOpenGenericClass(typeof(ValueValidator<>)))
					{
						TypeSearchIndex validatorSearchIndex3 = ValidatorSearchIndex;
						typeToIndex = new TypeSearchInfo
						{
							MatchType = validatorType,
							Targets = validatorType.GetArgumentsOfInheritedOpenGenericClass(typeof(ValueValidator<>)),
							Priority = attribute.Priority
						};
						validatorSearchIndex3.AddIndexedType(typeToIndex);
					}
					else
					{
						TypeSearchIndex validatorSearchIndex4 = ValidatorSearchIndex;
						typeToIndex = new TypeSearchInfo
						{
							MatchType = validatorType,
							Targets = Type.EmptyTypes,
							Priority = attribute.Priority
						};
						validatorSearchIndex4.AddIndexedType(typeToIndex);
					}
				}
			}
		}

		public bool PotentiallyHasValidatorsFor(InspectorProperty property)
		{
			List<TypeSearchResult[]> searchResults = GetSearchResults(property);
			for (int i = 0; i < searchResults.Count; i++)
			{
				if (searchResults[i].Length != 0)
				{
					return true;
				}
			}
			return false;
		}

		[Obsolete("Use PotentiallyHasValidatorsFor(InspectorProperty property) instead.", true)]
		public bool PotentiallyHasValidatorsFor(Type valueType)
		{
			throw new NotSupportedException("Use PotentiallyHasValidatorsFor(InspectorProperty property) instead.");
		}

		[Obsolete("Use PotentiallyHasValidatorsFor(InspectorProperty property) instead.", true)]
		public bool PotentiallyHasValidatorsFor(MemberInfo member, Type memberValueType, bool isCollectionElement)
		{
			throw new NotSupportedException("Use PotentiallyHasValidatorsFor(InspectorProperty property) instead.");
		}

		public virtual IList<Validator> GetValidators(InspectorProperty property)
		{
			List<TypeSearchResult> mergedSearchResults = GetMergedSearchResults(property);
			List<Validator> list = new List<Validator>(mergedSearchResults.Count);
			AttributeNumberMap.Clear();
			ImmutableList<Attribute> attributes = property.Attributes;
			for (int i = 0; i < mergedSearchResults.Count; i++)
			{
				TypeSearchResult typeSearchResult = mergedSearchResults[i];
				if (CustomValidatorFilter != null && !CustomValidatorFilter(typeSearchResult.MatchedType))
				{
					continue;
				}
				Validator emptyInstance = GetEmptyInstance(typeSearchResult.MatchedType);
				if (!emptyInstance.CanValidateProperty(property))
				{
					continue;
				}
				IValueValidator_InternalTemporaryHack valueValidator_InternalTemporaryHack = emptyInstance as IValueValidator_InternalTemporaryHack;
				if (valueValidator_InternalTemporaryHack != null && property.ValueEntry != null && property.ValueEntry.TypeOfValue != valueValidator_InternalTemporaryHack.ValidatedType)
				{
					continue;
				}
				try
				{
					Validator validator = (Validator)Activator.CreateInstance(typeSearchResult.MatchedType);
					if (validator is IAttributeValidator)
					{
						Attribute attribute = null;
						Type type = typeSearchResult.MatchedTargets[0];
						if (!type.InheritsFrom<Attribute>())
						{
							throw new NotSupportedException("Please don't manually implement the IAttributeValidator interface on any types; it's a special snowflake.");
						}
						int num = 0;
						AttributeNumberMap.TryGetValue(type, out var value);
						for (int j = 0; j < attributes.Count; j++)
						{
							Attribute attribute2 = attributes[j];
							if (attribute2.GetType() == type)
							{
								if (num == value)
								{
									attribute = attribute2;
									break;
								}
								num++;
							}
						}
						if (attribute == null)
						{
							throw new Exception("Could not find the correctly numbered attribute of type '" + type.GetNiceFullName() + "' on property " + property.Path + "; found " + num + " attributes of that type, but needed number " + value + ".");
						}
						(validator as IAttributeValidator).SetAttributeInstance(attribute);
						value++;
						AttributeNumberMap[type] = value;
					}
					validator.Initialize(property);
					list.Add(validator);
				}
				catch (NoBackwardsCompatibilityForLegacyValidatorException)
				{
				}
				catch (Exception ex2)
				{
					list.Add(new BrokenAttributeValidator(typeSearchResult.MatchedType, "Creating instance of validator '" + typeSearchResult.MatchedType.GetNiceName() + "' failed with exception: " + ex2.ToString()));
				}
			}
			return list;
		}

		[Obsolete("Use GetValidators(InspectorProperty property) instead.", true)]
		public virtual IList<Validator> GetValidators(Type valueType)
		{
			throw new NotSupportedException("Use GetValidators(InspectorProperty property) instead.");
		}

		[Obsolete("Use GetValidators(InspectorProperty property) instead.", true)]
		public virtual IList<Validator> GetValidators(MemberInfo member, Type memberValueType, bool isCollectionElement)
		{
			throw new NotSupportedException("Use GetValidators(InspectorProperty property) instead.");
		}

		protected List<TypeSearchResult[]> GetSearchResults(InspectorProperty property)
		{
			SearchResultList.Clear();
			SearchResultList.Add(ValidatorSearchIndex.GetMatches(Type.EmptyTypes));
			Type type = ((property.ValueEntry == null) ? null : property.ValueEntry.TypeOfValue);
			if (type != null)
			{
				SearchResultList.Add(ValidatorSearchIndex.GetMatches(type));
			}
			ImmutableList<Attribute> attributes = property.Attributes;
			for (int i = 0; i < attributes.Count; i++)
			{
				Type type2 = attributes[i].GetType();
				SearchResultList.Add(ValidatorSearchIndex.GetMatches(type2));
				if (type != null)
				{
					SearchResultList.Add(ValidatorSearchIndex.GetMatches(type2, type));
				}
			}
			return SearchResultList;
		}

		protected List<TypeSearchResult> GetMergedSearchResults(InspectorProperty property)
		{
			List<TypeSearchResult[]> searchResults = GetSearchResults(property);
			ResultList.Clear();
			TypeSearchIndex.MergeQueryResultsIntoList(searchResults, ResultList);
			return ResultList;
		}

		private static Validator GetEmptyInstance(Type type)
		{
			if (!EmptyInstances.TryGetValue(type, out var value))
			{
				value = (Validator)FormatterServices.GetUninitializedObject(type);
				EmptyInstances[type] = value;
			}
			return value;
		}
	}
}
