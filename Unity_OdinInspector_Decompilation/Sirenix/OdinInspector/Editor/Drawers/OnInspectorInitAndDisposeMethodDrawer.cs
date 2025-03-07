using System;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class OnInspectorInitAndDisposeMethodDrawer : MethodDrawer
	{
		protected override bool CanDrawMethodProperty(InspectorProperty property)
		{
			ImmutableList<Attribute> attributes = property.Attributes;
			if (!attributes.HasAttribute<OnInspectorDisposeAttribute>())
			{
				return attributes.HasAttribute<OnInspectorInitAttribute>();
			}
			return true;
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
		}
	}
}
