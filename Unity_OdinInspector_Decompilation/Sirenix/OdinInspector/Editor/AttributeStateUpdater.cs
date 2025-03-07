using System;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class AttributeStateUpdater<TAttribute> : StateUpdater where TAttribute : Attribute
	{
		private TAttribute attribute;

		/// <summary>
		/// Gets the attribute that the OdinAttributeStateUpdater applies to.
		/// </summary>
		public TAttribute Attribute
		{
			get
			{
				if (this.attribute == null)
				{
					StateUpdater[] stateUpdaters = base.Property.StateUpdaters;
					int num = 0;
					foreach (StateUpdater stateUpdater in stateUpdaters)
					{
						if (stateUpdater.GetType() == GetType())
						{
							if (this == stateUpdater)
							{
								break;
							}
							num++;
						}
					}
					int num2 = num;
					Type typeFromHandle = typeof(TAttribute);
					for (int j = 0; j < base.Property.Attributes.Count; j++)
					{
						Attribute attribute = base.Property.Attributes[j];
						if (attribute.GetType() == typeFromHandle)
						{
							if (num == 0)
							{
								this.attribute = (TAttribute)attribute;
								break;
							}
							num--;
						}
					}
					if (this.attribute == null)
					{
						Debug.LogError((object)("Could not find attribute '" + typeof(TAttribute).GetNiceName() + "' number " + num2 + " for the state updater '" + GetType().GetNiceName() + "' number " + num2 + "; not enough attributes of the required type on the property - why are there more drawers for the attribute than there are attributes?"));
						this.attribute = base.Property.GetAttribute<TAttribute>();
					}
				}
				return this.attribute;
			}
		}
	}
	public abstract class AttributeStateUpdater<TAttribute, TValue> : AttributeStateUpdater<TAttribute> where TAttribute : Attribute
	{
		private IPropertyValueEntry<TValue> valueEntry;

		/// <summary>
		/// Gets the strongly typed ValueEntry of the OdinAttributeStateUpdater's property.
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
