using System;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ShowDrawerChainAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System" })]
	internal class ShowDrawerChainExamples
	{
		[InfoBox("Any drawer not used in the draw chain will be greyed out in the drawer chain so that you can more easily debug the draw chain. You can see this by toggling the above toggle field.\n\nIf you have any custom drawers they will show up with green names in the drawer chain.", InfoMessageType.Info, null)]
		[ShowDrawerChain]
		[HideIf("ToggleHideIf", true)]
		public GameObject SomeObject;

		[Range(0f, 10f)]
		[ShowDrawerChain]
		public float SomeRange;

		[HorizontalGroup(0f, 0, 0, 0f, PaddingRight = -1f)]
		[ShowInInspector]
		[PropertyOrder(-1f)]
		public bool ToggleHideIf
		{
			get
			{
				GUIHelper.RequestRepaint();
				return EditorApplication.get_timeSinceStartup() % 3.0 < 1.5;
			}
		}

		[HorizontalGroup(0f, 0, 0, 0f)]
		[ShowInInspector]
		[HideLabel]
		[ProgressBar(0.0, 1.5, 0.15f, 0.47f, 0.74f)]
		private double Animate => Math.Abs(EditorApplication.get_timeSinceStartup() % 3.0 - 1.5);
	}
}
