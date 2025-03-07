using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(GUIColorAttribute))]
	internal class GUIColorExamples
	{
		[GUIColor(0.3f, 0.8f, 0.8f, 1f)]
		public int ColoredInt1;

		[GUIColor(0.3f, 0.8f, 0.8f, 1f)]
		public int ColoredInt2;

		[ButtonGroup("_DefaultGroup", 0f)]
		[GUIColor(0f, 1f, 0f, 1f)]
		private void Apply()
		{
		}

		[ButtonGroup("_DefaultGroup", 0f)]
		[GUIColor(1f, 0.6f, 0.4f, 1f)]
		private void Cancel()
		{
		}

		[InfoBox("You can also reference a color member to dynamically change the color of a property.", InfoMessageType.Info, null)]
		[GUIColor("GetButtonColor")]
		[Button("I Am Fabulous", ButtonSizes.Gigantic)]
		private static void IAmFabulous()
		{
		}

		[Button(ButtonSizes.Large)]
		[GUIColor("@Color.Lerp(Color.red, Color.green, Mathf.Abs(Mathf.Sin((float)EditorApplication.timeSinceStartup)))")]
		private static void Expressive()
		{
		}

		private static Color GetButtonColor()
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			GUIHelper.RequestRepaint();
			return Color.HSVToRGB(Mathf.Cos((float)EditorApplication.get_timeSinceStartup() + 1f) * 0.225f + 0.325f, 1f, 1f);
		}
	}
}
