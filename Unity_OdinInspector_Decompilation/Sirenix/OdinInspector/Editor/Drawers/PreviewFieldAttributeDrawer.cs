using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.PreviewFieldAttribute" /> as a square ObjectField which renders a preview for UnityEngine.Object types.
	/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
	/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
	/// </summary>
	[AllowGUIEnabledForReadonly]
	public sealed class PreviewFieldAttributeDrawer<T> : OdinAttributeDrawer<PreviewFieldAttribute, T> where T : Object
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			Sirenix.Utilities.Editor.ObjectFieldAlignment alignment = (Sirenix.Utilities.Editor.ObjectFieldAlignment)((!base.Attribute.AlignmentHasValue) ? ((int)GlobalConfig<GeneralDrawerConfig>.Instance.SquareUnityObjectAlignment) : ((int)base.Attribute.Alignment));
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			object weakSmartValue = base.ValueEntry.WeakSmartValue;
			propertyValueEntry.WeakSmartValue = SirenixEditorFields.UnityPreviewObjectField(label, weakSmartValue as Object, base.ValueEntry.BaseValueType, base.ValueEntry.Property.GetAttribute<AssetsOnlyAttribute>() == null, (base.Attribute.Height == 0f) ? GlobalConfig<GeneralDrawerConfig>.Instance.SquareUnityObjectFieldHeight : base.Attribute.Height, alignment);
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.Values.ForceMarkDirty();
			}
		}
	}
}
