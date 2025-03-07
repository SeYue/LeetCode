using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(TableListAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" })]
	internal class TableListExamples
	{
		[Serializable]
		public class SomeCustomClass
		{
			[TableColumnWidth(57, true, Resizable = false)]
			[PreviewField(Alignment = ObjectFieldAlignment.Center)]
			public Texture Icon;

			[TextArea]
			public string Description;

			[VerticalGroup("Combined Column", 0f)]
			[LabelWidth(22f)]
			public string A;

			[VerticalGroup("Combined Column", 0f)]
			[LabelWidth(22f)]
			public string B;

			[VerticalGroup("Combined Column", 0f)]
			[LabelWidth(22f)]
			public string C;

			[TableColumnWidth(60, true)]
			[Button]
			[VerticalGroup("Actions", 0f)]
			public void Test1()
			{
			}

			[TableColumnWidth(60, true)]
			[Button]
			[VerticalGroup("Actions", 0f)]
			public void Test2()
			{
			}

			[OnInspectorInit]
			private void CreateData()
			{
				Description = ExampleHelper.GetString();
				Icon = (Texture)(object)ExampleHelper.GetTexture();
			}
		}

		[TableList(ShowIndexLabels = true)]
		public List<SomeCustomClass> TableListWithIndexLabels = new List<SomeCustomClass>
		{
			new SomeCustomClass(),
			new SomeCustomClass()
		};

		[TableList(DrawScrollView = true, MaxScrollViewHeight = 200, MinScrollViewHeight = 100)]
		public List<SomeCustomClass> MinMaxScrollViewTable = new List<SomeCustomClass>
		{
			new SomeCustomClass(),
			new SomeCustomClass()
		};

		[TableList(AlwaysExpanded = true, DrawScrollView = false)]
		public List<SomeCustomClass> AlwaysExpandedTable = new List<SomeCustomClass>
		{
			new SomeCustomClass(),
			new SomeCustomClass()
		};

		[TableList(ShowPaging = true)]
		public List<SomeCustomClass> TableWithPaging = new List<SomeCustomClass>
		{
			new SomeCustomClass(),
			new SomeCustomClass()
		};
	}
}
