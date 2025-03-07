using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public interface IValidatorLocator
	{
		bool PotentiallyHasValidatorsFor(InspectorProperty property);

		[Obsolete("Use PotentiallyHasValidatorsFor(InspectorProperty property) instead.", true)]
		bool PotentiallyHasValidatorsFor(Type valueType);

		[Obsolete("Use PotentiallyHasValidatorsFor(InspectorProperty property) instead.", true)]
		bool PotentiallyHasValidatorsFor(MemberInfo member, Type memberValueType, bool isCollectionElement);

		IList<Validator> GetValidators(InspectorProperty property);

		[Obsolete("Use GetValidators(InspectorProperty property) instead.", true)]
		IList<Validator> GetValidators(Type valueType);

		[Obsolete("Use GetValidators(InspectorProperty property) instead.", true)]
		IList<Validator> GetValidators(MemberInfo member, Type memberValueType, bool isCollectionElement);
	}
}
