using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws Vector2 properties marked with <see cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class MinMaxSliderAttributeDrawer : OdinAttributeDrawer<MinMaxSliderAttribute, Vector2>
	{
		private ValueResolver<double> minGetter;

		private ValueResolver<double> maxGetter;

		private ValueResolver<Vector2> rangeGetter;

		protected override void Initialize()
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			if (base.Attribute.MinMaxValueGetter != null)
			{
				rangeGetter = ValueResolver.Get<Vector2>(base.Property, base.Attribute.MinMaxValueGetter, new Vector2(base.Attribute.MinValue, base.Attribute.MaxValue));
				return;
			}
			minGetter = ValueResolver.Get(base.Property, base.Attribute.MinValueGetter, (double)base.Attribute.MinValue);
			maxGetter = ValueResolver.Get(base.Property, base.Attribute.MaxValueGetter, (double)base.Attribute.MaxValue);
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			Vector2 value = default(Vector2);
			if (rangeGetter != null)
			{
				value = rangeGetter.GetValue();
			}
			else
			{
				((Vector2)(ref value))._002Ector((float)minGetter.GetValue(), (float)maxGetter.GetValue());
			}
			base.ValueEntry.SmartValue = SirenixEditorFields.MinMaxSlider(label, base.ValueEntry.SmartValue, value, base.Attribute.ShowFields);
		}
	}
}
