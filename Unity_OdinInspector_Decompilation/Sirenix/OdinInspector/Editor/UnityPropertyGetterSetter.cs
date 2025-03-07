using System;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public sealed class UnityPropertyGetterSetter<TOwner, TValue> : IValueGetterSetter<TOwner, TValue>, IValueGetterSetter
	{
		private static readonly Func<SerializedProperty, TValue> ValueGetter = SerializedPropertyUtilities.GetValueGetter<TValue>();

		private static readonly Action<SerializedProperty, TValue> ValueSetter = SerializedPropertyUtilities.GetValueSetter<TValue>();

		private InspectorProperty property;

		public bool IsReadonly => !property.Info.IsEditable;

		public Type OwnerType => typeof(TOwner);

		public Type ValueType => typeof(TValue);

		public UnityPropertyGetterSetter(InspectorProperty property)
		{
			this.property = property;
		}

		public TValue GetValue(ref TOwner owner)
		{
			if (ValueGetter == null || ValueSetter == null)
			{
				Debug.LogError((object)("Can't get a value of type " + typeof(TValue).GetNiceName() + " directly from a Unity property."));
				return default(TValue);
			}
			SerializedProperty unityPropertyForPath = property.Tree.GetUnityPropertyForPath(property.UnityPropertyPath);
			if (unityPropertyForPath == null || unityPropertyForPath.get_serializedObject().get_targetObject() is EmittedScriptableObject)
			{
				Debug.LogError((object)("Could not get Unity property at path " + property.UnityPropertyPath + " on root object of type " + property.Tree.TargetType.GetNiceName()));
				return default(TValue);
			}
			return ValueGetter(unityPropertyForPath);
		}

		public object GetValue(object owner)
		{
			TOwner owner2 = (TOwner)owner;
			return GetValue(ref owner2);
		}

		public void SetValue(ref TOwner owner, TValue value)
		{
			if (ValueGetter == null || ValueSetter == null)
			{
				Debug.LogError((object)("Can't set a value of type " + typeof(TValue).GetNiceName() + " directly to a Unity property."));
				return;
			}
			SerializedProperty unityPropertyForPath = property.Tree.GetUnityPropertyForPath(property.UnityPropertyPath);
			if (unityPropertyForPath == null || unityPropertyForPath.get_serializedObject().get_targetObject() is EmittedScriptableObject)
			{
				Debug.LogError((object)("Could not get Unity property at path " + property.UnityPropertyPath));
			}
			else
			{
				ValueSetter(unityPropertyForPath, value);
			}
		}

		public void SetValue(object owner, object value)
		{
			TOwner owner2 = (TOwner)owner;
			SetValue(ref owner2, (TValue)value);
		}
	}
}
