using System;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(10.0, 0.0, 0.0)]
	public sealed class FixUnityNullDrawer<T> : OdinValueDrawer<T> where T : class
	{
		public override bool CanDrawTypeFilter(Type type)
		{
			return !typeof(Object).IsAssignableFrom(typeof(T));
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Invalid comparison between Unknown and I4
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			if (propertyValueEntry.ValueState == PropertyValueState.NullReference && !propertyValueEntry.SerializationBackend.SupportsPolymorphism)
			{
				bool flag = false;
				for (InspectorProperty parent = propertyValueEntry.Property.Parent; parent != null; parent = parent.Parent)
				{
					if (parent.ValueEntry != null && (parent.ValueEntry.TypeOfValue == typeof(T) || parent.ValueEntry.BaseValueType == typeof(T)))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					SirenixEditorGUI.ErrorMessageBox("Possible Unity serialization recursion detected; cutting off drawing pre-emptively.");
					return;
				}
				if ((int)Event.get_current().get_type() == 8)
				{
					SerializedObject val = null;
					if (base.Property.Info.IsUnityPropertyOnly)
					{
						val = base.Property.Tree.UnitySerializedObject;
					}
					for (int i = 0; i < propertyValueEntry.ValueCount; i++)
					{
						object value = UnitySerializationUtility.CreateDefaultUnityInitializedObject(typeof(T));
						propertyValueEntry.WeakValues.ForceSetValue(i, value);
					}
					base.Property.RecordForUndo("Odin fixing null Unity-backed values");
					propertyValueEntry.ApplyChanges();
					PropertyTree tree = base.Property.Tree;
					if (base.Property.Info.IsUnityPropertyOnly && val != null)
					{
						val.ApplyModifiedPropertiesWithoutUndo();
					}
					base.Property.Update(forceUpdate: true);
				}
			}
			CallNextDrawer(label);
		}
	}
}
