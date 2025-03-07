using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Common base implementation for progress bar attribute drawers.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class BaseProgressBarAttributeDrawer<T> : OdinAttributeDrawer<ProgressBarAttribute, T>
	{
		private ValueResolver<T> minResolver;

		private ValueResolver<T> maxResolver;

		private ValueResolver<Color> foregroundColorResolver;

		private ValueResolver<Color> backgroundColorResolver;

		private ValueResolver<string> labelResolver;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			minResolver = ValueResolver.Get(base.Property, base.Attribute.MinGetter, ConvertUtility.Convert<double, T>(base.Attribute.Min));
			maxResolver = ValueResolver.Get(base.Property, base.Attribute.MaxGetter, ConvertUtility.Convert<double, T>(base.Attribute.Max));
			foregroundColorResolver = ValueResolver.Get<Color>(base.Property, base.Attribute.ColorGetter, base.Attribute.Color);
			backgroundColorResolver = ValueResolver.Get<Color>(base.Property, base.Attribute.BackgroundColorGetter, ProgressBarConfig.Default.BackgroundColor);
			labelResolver = ValueResolver.GetForString(base.Property, base.Attribute.CustomValueStringGetter);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			ValueResolver.DrawErrors(minResolver, maxResolver, foregroundColorResolver, backgroundColorResolver, labelResolver);
			ProgressBarConfig config = GetConfig();
			Rect controlRect = EditorGUILayout.GetControlRect(label != null, ((float)config.Height < EditorGUIUtility.get_singleLineHeight()) ? EditorGUIUtility.get_singleLineHeight() : ((float)config.Height), (GUILayoutOption[])(object)new GUILayoutOption[0]);
			T value = minResolver.GetValue();
			T value2 = maxResolver.GetValue();
			string value3 = labelResolver.GetValue();
			EditorGUI.BeginChangeCheck();
			T smartValue = DrawProgressBar(controlRect, label, ConvertUtility.Convert<T, double>(value), ConvertUtility.Convert<T, double>(value2), config, value3);
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = smartValue;
			}
		}

		private ProgressBarConfig GetConfig()
		{
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Invalid comparison between Unknown and I4
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			ProgressBarConfig @default = ProgressBarConfig.Default;
			@default.Height = base.Attribute.Height;
			@default.DrawValueLabel = (base.Attribute.DrawValueLabelHasValue ? base.Attribute.DrawValueLabel : ((!base.Attribute.Segmented) ? true : false));
			@default.ValueLabelAlignment = (TextAlignment)(base.Attribute.ValueLabelAlignmentHasValue ? ((int)base.Attribute.ValueLabelAlignment) : ((!base.Attribute.Segmented) ? 1 : 2));
			if (base.Attribute.CustomValueStringGetter != null)
			{
				@default.DrawValueLabel = false;
			}
			if ((int)Event.get_current().get_type() == 7)
			{
				@default.ForegroundColor = foregroundColorResolver.GetValue();
				@default.BackgroundColor = backgroundColorResolver.GetValue();
			}
			return @default;
		}

		/// <summary>
		/// Generic implementation of progress bar field drawing.
		/// </summary>
		protected abstract T DrawProgressBar(Rect rect, GUIContent label, double min, double max, ProgressBarConfig config, string valueLabel);

		/// <summary>
		/// Converts the generic value to a double.
		/// </summary>
		/// <param name="value">The generic value to convert.</param>
		/// <returns>The generic value as a double.</returns>
		protected abstract double ConvertToDouble(T value);
	}
}
