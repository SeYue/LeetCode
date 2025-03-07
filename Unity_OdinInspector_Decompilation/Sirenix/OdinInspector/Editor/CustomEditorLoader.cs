using Sirenix.Utilities;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	[InitializeOnLoad]
	internal static class CustomEditorLoader
	{
		static CustomEditorLoader()
		{
			if (GlobalConfig<InspectorConfig>.HasInstanceLoaded)
			{
				GlobalConfig<InspectorConfig>.Instance.UpdateOdinEditors();
				return;
			}
			UnityEditorEventUtility.DelayAction(delegate
			{
				GlobalConfig<InspectorConfig>.Instance.UpdateOdinEditors();
			});
		}
	}
}
