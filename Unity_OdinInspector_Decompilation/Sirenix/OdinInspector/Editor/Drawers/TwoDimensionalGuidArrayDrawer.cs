using System;
using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalGuidArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Guid> where TArray : IList
	{
		protected override Guid DrawElement(Rect rect, Guid value)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return SirenixEditorFields.GuidField(rect.Padding(2f), value);
		}
	}
}
