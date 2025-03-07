using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Color palette property drawer.
	/// </summary>
	internal sealed class ColorPaletteDrawer : OdinValueDrawer<ColorPalette>
	{
		private bool isEditing;

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<ColorPalette> propertyValueEntry = base.ValueEntry;
			propertyValueEntry.SmartValue.Name = propertyValueEntry.SmartValue.Name ?? "Palette Name";
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginToolbarBoxHeader();
			GUILayout.Label(propertyValueEntry.SmartValue.Name, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			if (SirenixEditorGUI.IconButton(EditorIcons.Pen))
			{
				isEditing = !isEditing;
			}
			SirenixEditorGUI.EndToolbarBoxHeader();
			if (propertyValueEntry.SmartValue.Colors == null)
			{
				propertyValueEntry.SmartValue.Colors = new List<Color>();
			}
			if (SirenixEditorGUI.BeginFadeGroup(propertyValueEntry.SmartValue, propertyValueEntry, isEditing))
			{
				CallNextDrawer(null);
			}
			SirenixEditorGUI.EndFadeGroup();
			if (SirenixEditorGUI.BeginFadeGroup(propertyValueEntry.SmartValue, propertyValueEntry.SmartValue, !isEditing))
			{
				Color color = default(Color);
				bool stretchPalette = GlobalConfig<ColorPaletteManager>.Instance.StretchPalette;
				int swatchSize = GlobalConfig<ColorPaletteManager>.Instance.SwatchSize;
				int swatchSpacing = GlobalConfig<ColorPaletteManager>.Instance.SwatchSpacing;
				ColorPaletteAttributeDrawer.DrawColorPaletteColorPicker(propertyValueEntry, propertyValueEntry.SmartValue, ref color, propertyValueEntry.SmartValue.ShowAlpha, stretchPalette, swatchSize, 20f, swatchSpacing);
			}
			SirenixEditorGUI.EndFadeGroup();
			SirenixEditorGUI.EndToolbarBox();
		}
	}
}
