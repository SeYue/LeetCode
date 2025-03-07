namespace Sirenix.OdinInspector.Editor
{
	public abstract class ValueStateUpdater<TValue> : StateUpdater
	{
		private IPropertyValueEntry<TValue> valueEntry;

		/// <summary>
		/// Gets the strongly typed ValueEntry of the OdinValueStateUpdater's property.
		/// </summary>
		public IPropertyValueEntry<TValue> ValueEntry
		{
			get
			{
				if (valueEntry == null)
				{
					valueEntry = base.Property.ValueEntry as IPropertyValueEntry<TValue>;
					if (valueEntry == null)
					{
						base.Property.Update(forceUpdate: true);
						valueEntry = base.Property.ValueEntry as IPropertyValueEntry<TValue>;
					}
				}
				return valueEntry;
			}
		}
	}
}
