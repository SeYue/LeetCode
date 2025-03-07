using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Sirenix.OdinInspector.Editor.TypeSearch;

namespace Sirenix.OdinInspector.Editor
{
	public class DefaultDrawerChainResolver : DrawerChainResolver
	{
		public static readonly DefaultDrawerChainResolver Instance = new DefaultDrawerChainResolver();

		private static readonly Dictionary<Type, Func<OdinDrawer>> FastDrawerCreators = new Dictionary<Type, Func<OdinDrawer>>(FastTypeComparer.Instance);

		private static TypeSearchResult[] CachedResultArray = new TypeSearchResult[20];

		public override DrawerChain GetDrawerChain(InspectorProperty property)
		{
			List<OdinDrawer> list = new List<OdinDrawer>(10);
			int resultCount = 0;
			DrawerUtilities.GetDefaultPropertyDrawers(property, ref CachedResultArray, ref resultCount);
			for (int i = 0; i < resultCount; i++)
			{
				list.Add(CreateDrawer(CachedResultArray[i].MatchedType));
			}
			return new ListDrawerChain(property, list);
		}

		private static OdinDrawer CreateDrawer(Type drawerType)
		{
			if (!FastDrawerCreators.TryGetValue(drawerType, out var value))
			{
				ConstructorInfo constructor = drawerType.GetConstructor(Type.EmptyTypes);
				DynamicMethod dynamicMethod = new DynamicMethod(drawerType.FullName + "_FastCreator", typeof(OdinDrawer), Type.EmptyTypes);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.Emit(OpCodes.Newobj, constructor);
				iLGenerator.Emit(OpCodes.Ret);
				value = (Func<OdinDrawer>)dynamicMethod.CreateDelegate(typeof(Func<OdinDrawer>));
				FastDrawerCreators.Add(drawerType, value);
			}
			return value();
		}
	}
}
