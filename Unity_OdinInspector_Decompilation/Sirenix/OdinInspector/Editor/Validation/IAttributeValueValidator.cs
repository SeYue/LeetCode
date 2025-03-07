using System;

namespace Sirenix.OdinInspector.Editor.Validation
{
	internal interface IAttributeValueValidator
	{
		bool IsLegacy { get; }

		Type GetValueType();

		IPropertyValueEntry RefreshValueEntry();
	}
}
