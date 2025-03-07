using System;
using System.Reflection;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(9001.0, 0.0, 0.0)]
	internal class HideSerializableJsonDictionaryFromEditorWindowsInUnity2017Drawer<T> : OdinValueDrawer<T> where T : ScriptableObject
	{
		public override bool CanDrawTypeFilter(Type type)
		{
			return type.FullName == "UnityEditor.Experimental.UIElements.SerializableJsonDictionary";
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			MemberInfo memberInfo = propertyValueEntry.Property.Info.GetMemberInfo();
			if (memberInfo.MemberType != MemberTypes.Field || !(memberInfo.Name == "m_PersistentViewDataDictionary"))
			{
				CallNextDrawer(label);
			}
		}
	}
}
