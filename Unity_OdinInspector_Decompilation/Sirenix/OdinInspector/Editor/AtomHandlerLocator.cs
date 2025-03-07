using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class AtomHandlerLocator
	{
		private static readonly Dictionary<Type, Type> AtomHandlerTypes;

		private static readonly Dictionary<Type, IAtomHandler> AtomHandlers;

		static AtomHandlerLocator()
		{
			AtomHandlerTypes = new Dictionary<Type, Type>();
			AtomHandlers = new Dictionary<Type, IAtomHandler>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				if (!assembly.SafeIsDefined(typeof(AtomContainerAttribute), inherit: false))
				{
					continue;
				}
				Type[] array = assembly.SafeGetTypes();
				foreach (Type type in array)
				{
					if (!typeof(IAtomHandler).IsAssignableFrom(type) || type.IsAbstract || !type.IsDefined(typeof(AtomHandlerAttribute), inherit: false) || type.GetConstructor(Type.EmptyTypes) == null)
					{
						continue;
					}
					Type[] argumentsOfInheritedOpenGenericInterface = type.GetArgumentsOfInheritedOpenGenericInterface(typeof(IAtomHandler<>));
					if (argumentsOfInheritedOpenGenericInterface != null)
					{
						Type type2 = argumentsOfInheritedOpenGenericInterface[0];
						if (type2.IsAbstract)
						{
							Debug.LogError((object)("The type '" + type2.GetNiceName() + "' cannot be marked atomic, as it is abstract."));
						}
						else
						{
							AtomHandlerTypes.Add(type2, type);
						}
					}
				}
			}
		}

		public static bool IsMarkedAtomic(this Type type)
		{
			return AtomHandlerTypes.ContainsKey(type);
		}

		public static IAtomHandler GetAtomHandler(Type type)
		{
			if (!AtomHandlerTypes.ContainsKey(type))
			{
				return null;
			}
			if (!AtomHandlers.TryGetValue(type, out var value))
			{
				value = (IAtomHandler)Activator.CreateInstance(AtomHandlerTypes[type]);
				AtomHandlers[type] = value;
			}
			return value;
		}

		public static IAtomHandler<T> GetAtomHandler<T>()
		{
			return (IAtomHandler<T>)GetAtomHandler(typeof(T));
		}
	}
}
