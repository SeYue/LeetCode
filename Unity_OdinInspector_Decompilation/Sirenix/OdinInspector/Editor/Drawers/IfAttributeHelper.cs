using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class IfAttributeHelper
	{
		private readonly ValueResolver<object> valueResolver;

		private bool result;

		public bool DefaultResult;

		public string ErrorMessage { get; private set; }

		public IfAttributeHelper(InspectorProperty property, string memberName, bool defaultResult = false)
		{
			valueResolver = ValueResolver.Get<object>(property, memberName);
			ErrorMessage = valueResolver.ErrorMessage;
			DefaultResult = defaultResult;
		}

		public bool GetValue(object value)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			if (ErrorMessage == null)
			{
				result = false;
				object value2 = valueResolver.GetValue();
				if (value2 is Object)
				{
					result = (Object)value2 != (Object)null;
				}
				else if (value2 is bool)
				{
					result = (bool)value2;
				}
				else if (value2 is string)
				{
					result = !string.IsNullOrEmpty((string)value2);
				}
				else if (value == null)
				{
					if (value2 != null)
					{
						result = true;
					}
				}
				else if (object.Equals(value2, value))
				{
					result = true;
				}
				return result;
			}
			return DefaultResult;
		}
	}
}
