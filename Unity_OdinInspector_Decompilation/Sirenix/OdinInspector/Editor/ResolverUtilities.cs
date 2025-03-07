using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	public static class ResolverUtilities
	{
		public static List<Assembly> GetResolverAssemblies()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			List<Assembly> list = new List<Assembly>(assemblies.Length);
			foreach (Assembly assembly in assemblies)
			{
				if (assembly.SafeIsDefined(typeof(ContainsOdinResolversAttribute), inherit: true) || (assembly.GetAssemblyTypeFlag() & AssemblyTypeFlags.CustomTypes) != 0)
				{
					list.Add(assembly);
				}
			}
			return list;
		}

		public static double GetResolverPriority(Type resolverType)
		{
			ResolverPriorityAttribute attribute = resolverType.GetAttribute<ResolverPriorityAttribute>(inherit: true);
			if (attribute != null)
			{
				return attribute.Priority;
			}
			if (resolverType.Assembly == typeof(OdinEditor).Assembly)
			{
				return -0.1;
			}
			return 0.0;
		}
	}
}
