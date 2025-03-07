using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class AssetsOnlyValidator<T> : AttributeValidator<AssetsOnlyAttribute, T> where T : Object
	{
		protected override void Validate(ValidationResult result)
		{
			T smartValue = base.ValueEntry.SmartValue;
			if ((Object)(object)smartValue != (Object)null && !AssetDatabase.Contains((Object)(object)smartValue))
			{
				string text = ((Object)smartValue).get_name();
				object obj = smartValue;
				Component val = obj as Component;
				if ((Object)(object)val != (Object)null)
				{
					text = "from " + ((Object)val.get_gameObject()).get_name();
				}
				result.ResultType = ValidationResultType.Error;
				result.Message = ((object)smartValue).GetType().GetNiceName() + " " + text + " is not an asset.";
			}
		}
	}
}
