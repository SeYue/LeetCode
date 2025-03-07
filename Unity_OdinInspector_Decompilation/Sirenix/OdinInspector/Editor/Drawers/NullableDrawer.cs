using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Property drawer for nullables.
	/// </summary>
	public sealed class NullableDrawer<T> : OdinValueDrawer<T?>, IDefinesGenericMenuItems where T : struct
	{
		[ShowOdinSerializedPropertiesInInspector]
		private class Wrapper
		{
			public NullableValue<T> Value;

			public void SetValue(T? value)
			{
				if (value.HasValue)
				{
					Value = new NullableValue<T>();
					Value.Value = value.Value;
				}
			}
		}

		private PropertyTree<Wrapper> tree;

		protected override void Initialize()
		{
			Wrapper[] array = new Wrapper[base.ValueEntry.ValueCount];
			for (int i = 0; i < array.Length; i++)
			{
				Wrapper wrapper = (array[i] = new Wrapper());
			}
			tree = new PropertyTree<Wrapper>(array);
			tree.UpdateTree();
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T?> propertyValueEntry = base.ValueEntry;
			for (int i = 0; i < tree.Targets.Count; i++)
			{
				tree.Targets[i].SetValue(propertyValueEntry.Values[i]);
			}
			tree.GetRootProperty(0).Label = label;
			tree.Draw(applyUndo: false);
			for (int j = 0; j < tree.Targets.Count; j++)
			{
				Wrapper wrapper = tree.Targets[j];
				if (wrapper.Value == null)
				{
					propertyValueEntry.Values[j] = null;
				}
				else
				{
					propertyValueEntry.Values[j] = wrapper.Value.Value;
				}
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Expected O, but got Unknown
			GUIContent val = new GUIContent("Set to null");
			IPropertyValueEntry<T?> entry = (IPropertyValueEntry<T?>)property.ValueEntry;
			if (entry.IsEditable && entry.SmartValue.HasValue)
			{
				genericMenu.AddItem(val, false, (MenuFunction)delegate
				{
					property.Tree.DelayActionUntilRepaint(delegate
					{
						entry.SmartValue = null;
					});
				});
			}
			else
			{
				genericMenu.AddDisabledItem(val);
			}
		}
	}
}
