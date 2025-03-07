using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(DelayedAttribute))]
	[AttributeExample(typeof(DelayedPropertyAttribute))]
	internal class DelayedPropertyExample
	{
		[Delayed]
		[OnValueChanged("OnValueChanged", false)]
		public int DelayedField;

		[ShowInInspector]
		[DelayedProperty]
		[OnValueChanged("OnValueChanged", false)]
		public string DelayedProperty { get; set; }

		private void OnValueChanged()
		{
			Debug.Log((object)"Value changed!");
		}
	}
}
