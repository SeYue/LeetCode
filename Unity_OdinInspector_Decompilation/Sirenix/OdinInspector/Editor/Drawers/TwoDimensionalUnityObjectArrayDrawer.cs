using System.Collections;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalUnityObjectArrayDrawer<TArray, TElement> : TwoDimensionalArrayDrawer<TArray, TElement> where TArray : IList where TElement : Object
	{
		protected override TElement DrawElement(Rect rect, TElement value)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			bool flag = !base.TableMatrixAttribute.IsReadOnly;
			value = SirenixEditorFields.PreviewObjectField(rect, value, dragOnly: false, flag, flag);
			return value;
		}

		protected override bool CompareElement(TElement a, TElement b)
		{
			return (Object)(object)a == (Object)(object)b;
		}
	}
}
