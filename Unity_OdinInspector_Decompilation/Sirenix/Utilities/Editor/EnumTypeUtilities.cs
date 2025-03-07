using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	public static class EnumTypeUtilities<T>
	{
		public struct EnumMember
		{
			public T Value;

			public string Name;

			public string NiceName;

			public bool IsObsolete;

			public string Message;

			public bool Hide;
		}

		private static readonly string[] enumNames;

		private static readonly string[] niceNames;

		private static readonly EnumMember[] allMembers;

		private static readonly EnumMember[] visibleMembers;

		private static readonly Dictionary<T, int> enumValueIndexLookup;

		private static readonly Type InspectorNameAttribute_Type;

		private static readonly FieldInfo InspectorNameAttribute_displayName;

		private static readonly bool isFlagEnum;

		public static bool IsFlagEnum => isFlagEnum;

		public static string[] Names => enumNames;

		public static string[] NiceNames => niceNames;

		public static EnumMember[] AllEnumMemberInfos => allMembers;

		public static EnumMember[] VisibleEnumMemberInfos => visibleMembers;

		static EnumTypeUtilities()
		{
			enumValueIndexLookup = new Dictionary<T, int>();
			if (!typeof(T).IsEnum)
			{
				throw new InvalidCastException(string.Concat(typeof(T), " Is not an enum type"));
			}
			InspectorNameAttribute_Type = typeof(Object).Assembly.GetType("UnityEngine.InspectorNameAttribute");
			if (InspectorNameAttribute_Type != null)
			{
				InspectorNameAttribute_displayName = InspectorNameAttribute_Type.GetField("displayName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			FieldInfo[] fields = typeof(T).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
			enumNames = new string[fields.Length];
			niceNames = new string[fields.Length];
			allMembers = new EnumMember[fields.Length];
			List<EnumMember> list = new List<EnumMember>(fields.Length);
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo fieldInfo = fields[i];
				EnumMember enumMember = default(EnumMember);
				try
				{
					enumMember.Value = (T)Enum.Parse(typeof(T), fieldInfo.Name);
					enumMember.Name = fieldInfo.Name;
					enumMember.NiceName = enumMember.Name.SplitPascalCase();
					ObsoleteAttribute attribute = fieldInfo.GetAttribute<ObsoleteAttribute>(inherit: true);
					InfoBoxAttribute attribute2 = fieldInfo.GetAttribute<InfoBoxAttribute>(inherit: true);
					HideInInspector attribute3 = ((ICustomAttributeProvider)fieldInfo).GetAttribute<HideInInspector>();
					LabelTextAttribute attribute4 = fieldInfo.GetAttribute<LabelTextAttribute>(inherit: true);
					enumMember.IsObsolete = attribute != null;
					enumMember.Message = ((attribute == null) ? "" : attribute.Message);
					enumMember.Hide = attribute3 != null;
					if (attribute4 != null)
					{
						enumMember.NiceName = attribute4.Text ?? "";
						if (attribute4.NicifyText)
						{
							enumMember.NiceName = ObjectNames.NicifyVariableName(enumMember.NiceName);
						}
					}
					if (InspectorNameAttribute_displayName != null)
					{
						object[] customAttributes = fieldInfo.GetCustomAttributes(InspectorNameAttribute_Type, inherit: false);
						if (customAttributes.Length != 0)
						{
							enumMember.NiceName = ((string)InspectorNameAttribute_displayName.GetValue(customAttributes[0])) ?? "";
						}
					}
				}
				catch (Exception ex)
				{
					enumMember.Message = ex.Message;
				}
				enumMember.Message = enumMember.Message ?? "";
				allMembers[i] = enumMember;
				enumNames[i] = enumMember.Name;
				niceNames[i] = enumMember.NiceName;
				enumValueIndexLookup[enumMember.Value] = i;
				if (!enumMember.Hide)
				{
					list.Add(enumMember);
				}
			}
			visibleMembers = list.ToArray();
			isFlagEnum = typeof(T).IsDefined<FlagsAttribute>();
		}

		public static T[] DecomposeEnumFlagValues(T enumFlagValue)
		{
			if (!typeof(T).IsEnum)
			{
				throw new InvalidCastException();
			}
			List<T> list = new List<T>();
			Array values = Enum.GetValues(typeof(T));
			long num = Convert.ToInt64(enumFlagValue);
			for (int i = 0; i < values.Length; i++)
			{
				T val = (T)values.GetValue(i);
				if ((num & Convert.ToInt64(val)) != 0L)
				{
					list.Add(val);
				}
			}
			return list.ToArray();
		}

		public static int GetIndexOfEnumValue(T enumValue)
		{
			if (enumValueIndexLookup.TryGetValue(enumValue, out var value))
			{
				return value;
			}
			throw new Exception("No member with the value " + enumValue.ToString() + " was found on the Enum " + typeof(T).GetNiceFullName());
		}

		public static EnumMember GetEnumMemberInfo(T value)
		{
			int indexOfEnumValue = GetIndexOfEnumValue(value);
			return allMembers[indexOfEnumValue];
		}
	}
}
