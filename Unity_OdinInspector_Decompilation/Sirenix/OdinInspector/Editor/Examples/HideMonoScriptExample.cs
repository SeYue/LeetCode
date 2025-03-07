using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(HideMonoScriptAttribute), Description = "The HideMonoScript attribute lets you hide the script reference at the top of the inspector of Unity objects.You can use this to reduce some of the clutter in your inspector.\n\nYou can also enable this behaviour globally from the general options in Tools > Odin Inspector > Preferences > General.")]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	internal class HideMonoScriptExample
	{
		[InfoBox("Click the pencil icon to open new inspector for these fields.", InfoMessageType.Info, null)]
		public HideMonoScriptScriptableObject Hidden;

		public ShowMonoScriptScriptableObject Shown;

		[OnInspectorInit]
		private void CreateData()
		{
			Hidden = ExampleHelper.GetScriptableObject<HideMonoScriptScriptableObject>("Hidden");
			Shown = ExampleHelper.GetScriptableObject<ShowMonoScriptScriptableObject>("Shown");
		}

		[OnInspectorDispose]
		private void CleanupData()
		{
			if ((Object)(object)Hidden != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)Hidden);
			}
			if ((Object)(object)Shown != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)Shown);
			}
		}
	}
}
