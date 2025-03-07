using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	[NoValidationInInspector]
	public class ChildGameObjectsOnlyValidator<T> : AttributeValidator<ChildGameObjectsOnlyAttribute, T> where T : Object
	{
		protected override void Validate(ValidationResult result)
		{
			object root = result.Setup.Root;
			GameObject val = root as GameObject;
			if ((Object)(object)val == (Object)null)
			{
				object root2 = result.Setup.Root;
				Component val2 = root2 as Component;
				if ((Object)(object)val2 != (Object)null)
				{
					val = val2.get_gameObject();
				}
			}
			object obj = base.ValueEntry.SmartValue;
			GameObject val3 = obj as GameObject;
			if ((Object)(object)val3 == (Object)null)
			{
				object obj2 = base.ValueEntry.SmartValue;
				Component val4 = obj2 as Component;
				if ((Object)(object)val4 != (Object)null)
				{
					val3 = val4.get_gameObject();
				}
			}
			if ((Object)(object)val == (Object)null || (Object)(object)val3 == (Object)null)
			{
				result.ResultType = ValidationResultType.IgnoreResult;
				return;
			}
			if (base.Attribute.IncludeSelf && (Object)(object)val == (Object)(object)val3)
			{
				result.ResultType = ValidationResultType.Valid;
				return;
			}
			Transform val5 = val3.get_transform();
			while (true)
			{
				val5 = val5.get_parent();
				if ((Object)(object)val5 == (Object)null)
				{
					break;
				}
				if ((Object)(object)((Component)val5).get_gameObject() == (Object)(object)val)
				{
					result.ResultType = ValidationResultType.Valid;
					return;
				}
			}
			result.ResultType = ValidationResultType.Error;
			result.Message = ((Object)val3).get_name() + " must be a child of " + ((Object)val).get_name();
		}
	}
}
