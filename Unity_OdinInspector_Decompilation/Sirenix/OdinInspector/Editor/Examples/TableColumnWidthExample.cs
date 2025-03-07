using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(TableColumnWidthAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" })]
	internal class TableColumnWidthExample
	{
		[Serializable]
		public class MyItem
		{
			[PreviewField(Height = 20f)]
			[TableColumnWidth(30, true, Resizable = false)]
			public Texture2D Icon;

			[TableColumnWidth(60, true)]
			public int ID;

			public string Name;

			[OnInspectorInit]
			private void CreateData()
			{
				Icon = ExampleHelper.GetTexture();
			}
		}

		[TableList]
		public List<MyItem> List = new List<MyItem>
		{
			new MyItem(),
			new MyItem(),
			new MyItem()
		};
	}
}
