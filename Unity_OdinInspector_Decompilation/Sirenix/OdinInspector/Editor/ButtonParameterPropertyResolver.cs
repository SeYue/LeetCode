using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class ButtonParameterPropertyResolver : OdinPropertyResolver
	{
		private class GetterSetter<T> : IValueGetterSetter<object, T>, IValueGetterSetter
		{
			private readonly object[] parameterValues;

			private readonly int index;

			public bool IsReadonly => false;

			public Type OwnerType => typeof(object);

			public Type ValueType => typeof(T);

			public GetterSetter(object[] parameterValues, int index)
			{
				this.parameterValues = parameterValues;
				this.index = index;
			}

			public T GetValue(ref object owner)
			{
				object obj = parameterValues[index];
				if (obj == null)
				{
					return default(T);
				}
				try
				{
					return (T)obj;
				}
				catch
				{
					return default(T);
				}
			}

			public object GetValue(object owner)
			{
				return parameterValues[index];
			}

			public void SetValue(ref object owner, T value)
			{
				parameterValues[index] = value;
			}

			public void SetValue(object owner, object value)
			{
				parameterValues[index] = value;
			}
		}

		public const string RETURN_VALUE_NAME = "$Result";

		private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

		private Dictionary<StringSlice, int> indexNameLookup = new Dictionary<StringSlice, int>(StringSliceEqualityComparer.Instance);

		private MethodInfo methodInfo;

		private ParameterInfo[] parameters;

		private object[] parameterValues;

		private object returnedValue;

		private Type returnType;

		public override bool CanResolveForPropertyFilter(InspectorProperty property)
		{
			if (property.Info.PropertyType != PropertyType.Method)
			{
				return false;
			}
			MethodInfo methodInfo = property.Info.GetMemberInfo() as MethodInfo;
			if (methodInfo == null)
			{
				methodInfo = property.Info.GetMethodDelegate().Method;
			}
			if (methodInfo.IsGenericMethodDefinition)
			{
				return false;
			}
			return true;
		}

		protected override void Initialize()
		{
			methodInfo = base.Property.Info.GetMemberInfo() as MethodInfo;
			if (methodInfo == null)
			{
				methodInfo = base.Property.Info.GetMethodDelegate().Method;
			}
			returnType = methodInfo.ReturnType;
			if (returnType == typeof(void))
			{
				returnType = null;
			}
			if (returnType == null)
			{
				parameters = methodInfo.GetParameters();
				parameterValues = new object[parameters.Length];
			}
			else
			{
				ParameterInfo[] array = methodInfo.GetParameters();
				parameters = new ParameterInfo[array.Length + 1];
				parameterValues = new object[parameters.Length];
				for (int i = 0; i < array.Length; i++)
				{
					parameters[i] = array[i];
				}
				parameters[parameters.Length - 1] = methodInfo.ReturnParameter;
			}
			for (int j = 0; j < parameters.Length; j++)
			{
				string text = ((returnType != null && j == parameters.Length - 1) ? "$Result" : parameters[j].Name);
				indexNameLookup[text] = j;
				object defaultValue = parameters[j].DefaultValue;
				if (defaultValue != DBNull.Value)
				{
					parameterValues[j] = defaultValue;
				}
			}
		}

		public override int ChildNameToIndex(string name)
		{
			if (indexNameLookup.TryGetValue(name, out var value))
			{
				return value;
			}
			return -1;
		}

		public override int ChildNameToIndex(ref StringSlice name)
		{
			if (indexNameLookup.TryGetValue(name, out var value))
			{
				return value;
			}
			return -1;
		}

		public override InspectorPropertyInfo GetChildInfo(int childIndex)
		{
			if (childInfos.TryGetValue(childIndex, out var value))
			{
				return value;
			}
			ParameterInfo parameterInfo = parameters[childIndex];
			Type type = parameterInfo.ParameterType;
			if (type.IsByRef)
			{
				type = type.GetElementType();
			}
			Type type2 = null;
			IValueGetterSetter getterSetter = null;
			try
			{
				type2 = typeof(GetterSetter<>).MakeGenericType(type);
				getterSetter = Activator.CreateInstance(type2, parameterValues, childIndex) as IValueGetterSetter;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			value = InspectorPropertyInfo.CreateValue((returnType != null && childIndex == parameters.Length - 1) ? "$Result" : parameterInfo.Name, childIndex, SerializationBackend.None, getterSetter, parameterInfo.GetAttributes());
			childInfos[childIndex] = value;
			return value;
		}

		protected override int CalculateChildCount()
		{
			return parameterValues.Length;
		}
	}
}
