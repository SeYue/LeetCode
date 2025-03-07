using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ShowInInspectorAttribute), "You can use the ShowInInspector attribute on static members to make them appear in the inspector as well.")]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" })]
	internal class StaticInspectorsExample
	{
		[Serializable]
		public struct MySomeStruct
		{
			[HideLabel]
			[PreviewField(45f)]
			[HorizontalGroup("Split", 45f, 0, 0, 0f)]
			public Texture2D Icon;

			[FoldoutGroup("Split/$Icon", 0f)]
			[HorizontalGroup("Split/$Icon/Properties", 0f, 0, 0, 0f, LabelWidth = 40f)]
			public int Foo;

			[HorizontalGroup("Split/$Icon/Properties", 0f, 0, 0, 0f)]
			public int Bar;
		}

		[ShowInInspector]
		public static List<MySomeStruct> SomeStaticField;

		[ShowInInspector]
		[PropertyRange(0.0, 0.10000000149011612)]
		public static float FixedDeltaTime
		{
			get
			{
				return Time.get_fixedDeltaTime();
			}
			set
			{
				Time.set_fixedDeltaTime(value);
			}
		}

		[Button(ButtonSizes.Large)]
		[PropertyOrder(-1f)]
		public static void AddToList()
		{
			int num = SomeStaticField.Count + 1000;
			SomeStaticField.Capacity = num;
			while (SomeStaticField.Count < num)
			{
				SomeStaticField.Add(new MySomeStruct
				{
					Icon = ExampleHelper.GetTexture()
				});
			}
		}

		[OnInspectorInit]
		private static void CreateData()
		{
			List<MySomeStruct> list = new List<MySomeStruct>();
			MySomeStruct item = new MySomeStruct
			{
				Icon = ExampleHelper.GetTexture()
			};
			list.Add(item);
			item = new MySomeStruct
			{
				Icon = ExampleHelper.GetTexture()
			};
			list.Add(item);
			item = new MySomeStruct
			{
				Icon = ExampleHelper.GetTexture()
			};
			list.Add(item);
			SomeStaticField = list;
		}
	}
}
