using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	public static class UnityTypeCacheUtility
	{
		public static readonly bool IsAvailable;

		private static readonly MethodInfo UnityEditor_TypeCache_GetTypesDerivedFrom_Method;

		static UnityTypeCacheUtility()
		{
			Type type = typeof(Editor).Assembly.GetType("UnityEditor.TypeCache");
			if (type != null)
			{
				UnityEditor_TypeCache_GetTypesDerivedFrom_Method = type.GetMethod("GetTypesDerivedFrom", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(Type) }, null);
				if (UnityEditor_TypeCache_GetTypesDerivedFrom_Method != null)
				{
					IsAvailable = true;
				}
			}
		}

		public static IList<Type> GetTypesDerivedFrom(Type type)
		{
			if (!IsAvailable)
			{
				throw new NotSupportedException();
			}
			return (IList<Type>)UnityEditor_TypeCache_GetTypesDerivedFrom_Method.Invoke(null, new object[1] { type });
		}
	}
}
