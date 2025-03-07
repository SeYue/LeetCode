using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class OdinPropertyProcessor
	{
		public InspectorProperty Property { get; private set; }

		public abstract void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos);

		public virtual bool CanProcessForProperty(InspectorProperty property)
		{
			return true;
		}

		protected virtual void Initialize()
		{
		}

		public static OdinPropertyProcessor Create(Type processorType, InspectorProperty property)
		{
			if (processorType == null)
			{
				throw new ArgumentNullException("processorType");
			}
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			if (!typeof(OdinPropertyProcessor).IsAssignableFrom(processorType))
			{
				throw new ArgumentException("Type is not a MemberPropertyProcessor");
			}
			OdinPropertyProcessor odinPropertyProcessor = (OdinPropertyProcessor)Activator.CreateInstance(processorType);
			odinPropertyProcessor.Property = property;
			odinPropertyProcessor.Initialize();
			return odinPropertyProcessor;
		}

		public static T Create<T>(InspectorProperty property) where T : OdinPropertyProcessor, new()
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			T val = new T();
			val.Property = property;
			val.Initialize();
			return val;
		}
	}
	public abstract class OdinPropertyProcessor<TValue> : OdinPropertyProcessor
	{
	}
	public abstract class OdinPropertyProcessor<TValue, TAttribute> : OdinPropertyProcessor<TValue> where TAttribute : Attribute
	{
	}
}
