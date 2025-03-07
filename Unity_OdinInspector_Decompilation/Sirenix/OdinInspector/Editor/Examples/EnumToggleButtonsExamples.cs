using System;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(EnumToggleButtonsAttribute))]
	internal class EnumToggleButtonsExamples
	{
		public enum SomeEnum
		{
			First,
			Second,
			Third,
			Fourth,
			AndSoOn
		}

		[Flags]
		public enum SomeBitmaskEnum
		{
			A = 0x2,
			B = 0x4,
			C = 0x8,
			All = 0xE
		}

		[Title("Default", null, TitleAlignments.Left, true, true)]
		public SomeBitmaskEnum DefaultEnumBitmask;

		[Title("Standard Enum", null, TitleAlignments.Left, true, true)]
		[EnumToggleButtons]
		public SomeEnum SomeEnumField;

		[EnumToggleButtons]
		[HideLabel]
		public SomeEnum WideEnumField;

		[Title("Bitmask Enum", null, TitleAlignments.Left, true, true)]
		[EnumToggleButtons]
		public SomeBitmaskEnum BitmaskEnumField;

		[EnumToggleButtons]
		[HideLabel]
		public SomeBitmaskEnum EnumFieldWide;
	}
}
