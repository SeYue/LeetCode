using System;

namespace Sirenix.OdinInspector.Editor
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class StaticInitializeBeforeDrawingAttribute : Attribute
	{
		public Type[] Types;

		public StaticInitializeBeforeDrawingAttribute(params Type[] types)
		{
			Types = types;
		}
	}
}
