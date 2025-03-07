using System;

namespace 导表工具
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ColTypeAttribute : Attribute
	{
		public bool isUsing;

		public ColTypeAttribute(bool isUsing)
		{
			this.isUsing = isUsing;
		}
	}
}
