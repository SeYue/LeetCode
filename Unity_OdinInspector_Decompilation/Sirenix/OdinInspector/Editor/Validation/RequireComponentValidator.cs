using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class RequireComponentValidator<T> : AttributeValidator<RequireComponent, T> where T : Component
	{
		public override bool CanValidateProperty(InspectorProperty property)
		{
			return property == property.Tree.RootProperty;
		}

		protected override void Validate(ValidationResult result)
		{
			T smartValue = base.ValueEntry.SmartValue;
			bool flag = false;
			if ((Object)(object)smartValue == (Object)null)
			{
				flag = true;
			}
			if (flag)
			{
				result.ResultType = ValidationResultType.IgnoreResult;
				return;
			}
			if (base.Attribute.m_Type0 != null && typeof(Component).IsAssignableFrom(base.Attribute.m_Type0) && (Object)(object)((Component)smartValue).get_gameObject().GetComponent(base.Attribute.m_Type0) == (Object)null)
			{
				result.Message = "GameObject is missing required component of type '" + base.Attribute.m_Type0.GetNiceName() + "'";
				result.ResultType = ValidationResultType.Error;
			}
			if (base.Attribute.m_Type1 != null && typeof(Component).IsAssignableFrom(base.Attribute.m_Type1) && (Object)(object)((Component)smartValue).get_gameObject().GetComponent(base.Attribute.m_Type1) == (Object)null)
			{
				result.Message = result.Message + "\n\nGameObject is missing required component of type '" + base.Attribute.m_Type1.GetNiceName() + "'";
				result.ResultType = ValidationResultType.Error;
			}
			if (base.Attribute.m_Type2 != null && typeof(Component).IsAssignableFrom(base.Attribute.m_Type2) && (Object)(object)((Component)smartValue).get_gameObject().GetComponent(base.Attribute.m_Type2) == (Object)null)
			{
				result.Message = result.Message + "\n\nGameObject is missing required component of type '" + base.Attribute.m_Type2.GetNiceName() + "'";
				result.ResultType = ValidationResultType.Error;
			}
		}
	}
}
