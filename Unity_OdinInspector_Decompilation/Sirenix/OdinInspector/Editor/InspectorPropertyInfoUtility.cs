using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Internal;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Sirenix.OdinInspector.Editor
{
	public static class InspectorPropertyInfoUtility
	{
		private struct GroupDataAndInfo
		{
			public GroupData Data;

			public InspectorPropertyInfo Info;
		}

		private struct GroupAttributeInfo
		{
			public InspectorPropertyInfo InspectorPropertyInfo;

			public PropertyGroupAttribute Attribute;

			public bool Exclude;
		}

		private class GroupData
		{
			public string Name;

			public string ID;

			public GroupData Parent;

			public PropertyGroupAttribute ConsolidatedAttribute;

			public List<GroupAttributeInfo> Attributes = new List<GroupAttributeInfo>();

			public readonly List<GroupData> ChildGroups = new List<GroupData>();
		}

		private static readonly Dictionary<Type, bool> TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cache = new Dictionary<Type, bool>(FastTypeComparer.Instance);

		private static readonly HashSet<string> AlwaysSkipUnityProperties = new HashSet<string>
		{
			"m_PathID", "m_FileID", "m_ObjectHideFlags", "m_PrefabParentObject", "m_PrefabInternal", "m_PrefabInternal", "m_GameObject", "m_Enabled", "m_Script", "m_EditorHideFlags",
			"m_EditorClassIdentifier"
		};

		private static Type System_Object_Type = typeof(object);

		private static Type UnityEngine_Object_Type = typeof(Object);

		private static Type UnityEngine_Component_Type = typeof(Component);

		private static Type UnityEngine_MonoBehaviour_Type = typeof(MonoBehaviour);

		private static Type UnityEngine_Behaviour_Type = typeof(Behaviour);

		private static Type UnityEngine_ScriptableObject_Type = typeof(ScriptableObject);

		private static readonly HashSet<string> AlwaysSkipUnityPropertiesForComponents = new HashSet<string> { "m_Name" };

		private static readonly DoubleLookupDictionary<Type, string, string> UnityPropertyMemberNameReplacements = new DoubleLookupDictionary<Type, string, string>
		{
			{
				typeof(Bounds),
				new Dictionary<string, string> { { "m_Extent", "m_Extents" } }
			},
			{
				typeof(LayerMask),
				new Dictionary<string, string> { { "m_Bits", "m_Mask" } }
			}
		};

		private static readonly Dictionary<Type, MemberInfo[]> TypeMembers_Cache = new Dictionary<Type, MemberInfo[]>(FastTypeComparer.Instance);

		private static readonly HashSet<Type> NeverProcessUnityPropertiesFor = new HashSet<Type>
		{
			typeof(Matrix4x4),
			typeof(Color32),
			typeof(AnimationCurve),
			typeof(Gradient),
			typeof(Coroutine)
		};

		private static readonly HashSet<Type> AlwaysSkipUnityPropertiesDeclaredBy = new HashSet<Type>
		{
			typeof(Object),
			typeof(ScriptableObject),
			typeof(Component),
			typeof(Behaviour),
			typeof(MonoBehaviour),
			typeof(StateMachineBehaviour)
		};

		private static readonly List<Attribute> ChildProcessedAttributes = new List<Attribute>();

		private static readonly Dictionary<InspectorPropertyInfo, float> GroupMemberOrders_Cached = new Dictionary<InspectorPropertyInfo, float>();

		private static readonly Dictionary<string, GroupData> GroupTree_Cached = new Dictionary<string, GroupData>();

		private static readonly Dictionary<InspectorPropertyInfo, InspectorPropertyInfo> RemovedMembers_Cached = new Dictionary<InspectorPropertyInfo, InspectorPropertyInfo>();

		private static Dictionary<SerializationBackend, Dictionary<Type, List<InspectorPropertyInfo>>> UnityPropertyInfoCache = new Dictionary<SerializationBackend, Dictionary<Type, List<InspectorPropertyInfo>>>();

		/// <summary>
		/// Gets all <see cref="T:Sirenix.OdinInspector.Editor.InspectorPropertyInfo" />s for a given type.
		/// </summary>
		/// <param name="parentProperty">The parent property.</param>
		/// <param name="type">The type to get infos for.</param>
		/// <param name="includeSpeciallySerializedMembers">if set to true members that are serialized by Odin will be included.</param>
		public static InspectorPropertyInfo[] GetDefaultPropertiesForType(InspectorProperty parentProperty, Type type, bool includeSpeciallySerializedMembers)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return CreateDefaultInspectorProperties(parentProperty, type, includeSpeciallySerializedMembers);
		}

		private static T Find<T>(this IList<Attribute> attributes) where T : Attribute
		{
			for (int i = 0; i < attributes.Count; i++)
			{
				if (attributes[i] is T)
				{
					return attributes[i] as T;
				}
			}
			return null;
		}

		private static bool Contains<T>(this IList<Attribute> attributes) where T : Attribute
		{
			for (int i = 0; i < attributes.Count; i++)
			{
				if (attributes[i] is T)
				{
					return true;
				}
			}
			return false;
		}

		public static bool TryCreate(InspectorProperty parentProperty, MemberInfo member, bool includeSpeciallySerializedMembers, out InspectorPropertyInfo result)
		{
			if (ChildProcessedAttributes.Count > 0)
			{
				ChildProcessedAttributes.Clear();
			}
			List<Attribute> childProcessedAttributes = ChildProcessedAttributes;
			ProcessAttributes(parentProperty, member, childProcessedAttributes);
			bool flag = childProcessedAttributes.Contains<ShowInInspectorAttribute>();
			if (!flag && ((IList<Attribute>)childProcessedAttributes).Contains<HideInInspector>())
			{
				result = null;
				return false;
			}
			if (member.IsStatic())
			{
				if (flag)
				{
					return TryCreate(member, SerializationBackend.None, allowEditable: true, out result, childProcessedAttributes);
				}
				result = null;
				return false;
			}
			SerializationBackend serializationBackendOfProperty = GetSerializationBackendOfProperty(parentProperty);
			SerializationBackend serializationBackend = GetSerializationBackend(parentProperty, member, serializationBackendOfProperty);
			if (!flag && serializationBackendOfProperty == SerializationBackend.None)
			{
				SerializationBackend serializationBackend2 = GetSerializationBackend(parentProperty, member, SerializationBackend.Odin);
				if (serializationBackend2 != SerializationBackend.None)
				{
					flag = true;
				}
			}
			if (flag || serializationBackend != SerializationBackend.None)
			{
				return TryCreate(member, serializationBackend, allowEditable: true, out result, childProcessedAttributes);
			}
			result = null;
			return false;
		}

		private static List<Attribute> CopyList(List<Attribute> attributes)
		{
			int count = attributes.Count;
			List<Attribute> list = new List<Attribute>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(attributes[i]);
			}
			return list;
		}

		private static bool TryCreate(MemberInfo member, SerializationBackend backend, bool allowEditable, out InspectorPropertyInfo result, List<Attribute> attributes)
		{
			result = null;
			if (member is FieldInfo)
			{
				result = InspectorPropertyInfo.CreateForMember(member, allowEditable, backend, CopyList(attributes));
			}
			else if (member is PropertyInfo)
			{
				PropertyInfo propertyInfo = member as PropertyInfo;
				PropertyInfo propertyInfo2 = propertyInfo.DeAliasProperty();
				bool flag = true;
				if (!propertyInfo2.CanRead || !propertyInfo.CanRead)
				{
					flag = false;
				}
				if (flag)
				{
					result = InspectorPropertyInfo.CreateForMember(member, allowEditable, backend, CopyList(attributes));
				}
			}
			else if (member is MethodInfo)
			{
				MethodInfo methodInfo = member as MethodInfo;
				if (methodInfo.IsGenericMethodDefinition)
				{
					return false;
				}
				result = InspectorPropertyInfo.CreateForMember(member, allowEditable: false, SerializationBackend.None, CopyList(attributes));
			}
			if (result != null)
			{
				PropertyOrderAttribute attribute = result.GetAttribute<PropertyOrderAttribute>();
				if (attribute != null)
				{
					result.Order = attribute.Order;
				}
				return true;
			}
			return false;
		}

		private static int GetMemberCategoryOrder(MemberInfo member)
		{
			if (member == null)
			{
				return 0;
			}
			if (member is FieldInfo)
			{
				return 0;
			}
			if (member is PropertyInfo)
			{
				return 1;
			}
			if (member is MethodInfo)
			{
				return 2;
			}
			return 3;
		}

		/// <summary>
		/// Gets an aliased version of a member, with the declaring type name included in the member name, so that there are no conflicts with private fields and properties with the same name in different classes in the same inheritance hierarchy.
		/// </summary>
		public static MemberInfo GetPrivateMemberAlias(MemberInfo member, string prefixString = null, string separatorString = null)
		{
			if (member is FieldInfo)
			{
				if (separatorString != null)
				{
					return new MemberAliasFieldInfo(member as FieldInfo, prefixString ?? member.DeclaringType.Name, separatorString);
				}
				return new MemberAliasFieldInfo(member as FieldInfo, prefixString ?? member.DeclaringType.Name);
			}
			if (member is PropertyInfo)
			{
				if (separatorString != null)
				{
					return new MemberAliasPropertyInfo(member as PropertyInfo, prefixString ?? member.DeclaringType.Name, separatorString);
				}
				return new MemberAliasPropertyInfo(member as PropertyInfo, prefixString ?? member.DeclaringType.Name);
			}
			if (member is MethodInfo)
			{
				if (separatorString != null)
				{
					return new MemberAliasMethodInfo(member as MethodInfo, prefixString ?? member.DeclaringType.Name, separatorString);
				}
				return new MemberAliasMethodInfo(member as MethodInfo, prefixString ?? member.DeclaringType.Name);
			}
			throw new NotImplementedException();
		}

		public static List<InspectorPropertyInfo> CreateMemberProperties(InspectorProperty parentProperty, Type type, bool includeSpeciallySerializedMembers)
		{
			List<InspectorPropertyInfo> list = new List<InspectorPropertyInfo>();
			AssemblyTypeFlags assemblyTypeFlag = type.Assembly.GetAssemblyTypeFlag();
			if ((assemblyTypeFlag == AssemblyTypeFlags.UnityEditorTypes || assemblyTypeFlag == AssemblyTypeFlags.UnityTypes) && !typeof(Object).IsAssignableFrom(type) && !NeverProcessUnityPropertiesFor.Contains(type) && (UnityNetworkingUtility.SyncListType == null || !type.ImplementsOpenGenericClass(UnityNetworkingUtility.SyncListType)) && !typeof(UnityAction).IsAssignableFrom(type) && !type.ImplementsOpenGenericClass(typeof(UnityAction<>)) && !type.ImplementsOpenGenericClass(typeof(UnityAction<, >)) && !type.ImplementsOpenGenericClass(typeof(UnityAction<, , >)) && !type.ImplementsOpenGenericClass(typeof(UnityAction<, , , >)))
			{
				PopulateUnityProperties(parentProperty, type, list);
			}
			if (list.Count == 0)
			{
				PopulateMemberInspectorProperties(parentProperty, type, includeSpeciallySerializedMembers, list);
			}
			return (from n in list
				orderby n.Order, GetMemberCategoryOrder(n.GetMemberInfo())
				select n).ToList();
		}

		public static InspectorPropertyInfo[] PerformAndBakePostGroupOrdering(List<InspectorPropertyInfo> rootProperties, Dictionary<InspectorPropertyInfo, float> groupMemberOrdering = null)
		{
			IOrderedEnumerable<InspectorPropertyInfo> source = rootProperties.OrderBy((InspectorPropertyInfo n) => (n.PropertyType == PropertyType.Group && n.Order == 0f) ? FindFirstMemberOfGroup(n).Order : n.Order);
			if (groupMemberOrdering != null)
			{
				source = source.ThenBy((InspectorPropertyInfo n) => groupMemberOrdering[n]);
			}
			return source.ThenBy((InspectorPropertyInfo n) => GetMemberCategoryOrder(n.GetMemberInfo())).ToArray();
		}

		private static InspectorPropertyInfo[] CreateDefaultInspectorProperties(InspectorProperty parentProperty, Type type, bool includeSpeciallySerializedMembers)
		{
			List<InspectorPropertyInfo> list = CreateMemberProperties(parentProperty, type, includeSpeciallySerializedMembers);
			Dictionary<InspectorPropertyInfo, float> groupMemberOrders = GroupMemberOrders_Cached;
			BuildPropertyGroups(parentProperty, type, list, includeSpeciallySerializedMembers, ref groupMemberOrders);
			InspectorPropertyInfo[] result = PerformAndBakePostGroupOrdering(list, groupMemberOrders);
			if (groupMemberOrders.Count > 0)
			{
				groupMemberOrders.Clear();
			}
			return result;
		}

		private static InspectorPropertyInfo FindFirstMemberOfGroup(InspectorPropertyInfo groupInfo)
		{
			for (int i = 0; i < groupInfo.GetGroupInfos().Length; i++)
			{
				InspectorPropertyInfo inspectorPropertyInfo = groupInfo.GetGroupInfos()[i];
				if (inspectorPropertyInfo.PropertyType == PropertyType.Group)
				{
					InspectorPropertyInfo inspectorPropertyInfo2 = FindFirstMemberOfGroup(inspectorPropertyInfo);
					if (inspectorPropertyInfo2 != null)
					{
						return inspectorPropertyInfo2;
					}
					continue;
				}
				return inspectorPropertyInfo;
			}
			return null;
		}

		public static InspectorPropertyInfo[] BuildPropertyGroupsAndFinalize(InspectorProperty parentProperty, Type typeOfOwner, List<InspectorPropertyInfo> rootMemberProperties, bool includeSpeciallySerializedMembers)
		{
			for (int i = 0; i < rootMemberProperties.Count; i++)
			{
				rootMemberProperties[i].UpdateOrderFromAttributes();
			}
			Dictionary<InspectorPropertyInfo, float> groupMemberOrders = GroupMemberOrders_Cached;
			BuildPropertyGroups(parentProperty, typeOfOwner, rootMemberProperties, includeSpeciallySerializedMembers, ref groupMemberOrders);
			InspectorPropertyInfo[] result = PerformAndBakePostGroupOrdering(rootMemberProperties, groupMemberOrders);
			if (groupMemberOrders.Count > 0)
			{
				groupMemberOrders.Clear();
			}
			return result;
		}

		public static void BuildPropertyGroups(InspectorProperty parentProperty, Type typeOfOwner, List<InspectorPropertyInfo> rootMemberProperties, bool includeSpeciallySerializedMembers, ref Dictionary<InspectorPropertyInfo, float> groupMemberOrders)
		{
			if (rootMemberProperties.Count == 0)
			{
				return;
			}
			if (groupMemberOrders == null)
			{
				groupMemberOrders = new Dictionary<InspectorPropertyInfo, float>(rootMemberProperties.Count);
			}
			else if (groupMemberOrders.Count > 0)
			{
				groupMemberOrders.Clear();
			}
			for (int i = 0; i < rootMemberProperties.Count; i++)
			{
				groupMemberOrders.Add(rootMemberProperties[i], i);
			}
			Dictionary<InspectorPropertyInfo, float> lambdaRefMemberOrders = groupMemberOrders;
			Dictionary<string, GroupData> groupTree_Cached = GroupTree_Cached;
			if (groupTree_Cached.Count > 0)
			{
				groupTree_Cached.Clear();
			}
			for (int j = 0; j < rootMemberProperties.Count; j++)
			{
				InspectorPropertyInfo inspectorPropertyInfo = rootMemberProperties[j];
				ImmutableList<Attribute> attributes = inspectorPropertyInfo.Attributes;
				for (int k = 0; k < attributes.Count; k++)
				{
					PropertyGroupAttribute propertyGroupAttribute = attributes[k] as PropertyGroupAttribute;
					if (propertyGroupAttribute != null)
					{
						RegisterGroupAttribute(inspectorPropertyInfo, propertyGroupAttribute, groupTree_Cached);
					}
				}
			}
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, GroupData> item2 in groupTree_Cached)
			{
				if (!ProcessGroups(item2.Value, groupTree_Cached))
				{
					list.Add(item2.Key);
				}
			}
			for (int l = 0; l < list.Count; l++)
			{
				groupTree_Cached.Remove(list[l]);
			}
			List<GroupDataAndInfo> list2 = new List<GroupDataAndInfo>();
			foreach (GroupData value in groupTree_Cached.Values)
			{
				InspectorPropertyInfo info = CreatePropertyGroups(parentProperty, typeOfOwner, value, lambdaRefMemberOrders, includeSpeciallySerializedMembers);
				list2.Add(new GroupDataAndInfo
				{
					Data = value,
					Info = info
				});
			}
			if (groupTree_Cached.Count > 0)
			{
				groupTree_Cached.Clear();
			}
			Dictionary<InspectorPropertyInfo, InspectorPropertyInfo> removedMembers_Cached = RemovedMembers_Cached;
			if (removedMembers_Cached.Count > 0)
			{
				removedMembers_Cached.Clear();
			}
			for (int m = 0; m < list2.Count; m++)
			{
				GroupDataAndInfo groupDataAndInfo = list2[m];
				IOrderedEnumerable<InspectorPropertyInfo> source = from n in RecurseGroupMembers(groupDataAndInfo.Data)
					orderby lambdaRefMemberOrders[n]
					select n;
				InspectorPropertyInfo inspectorPropertyInfo2 = source.First();
				int num = rootMemberProperties.IndexOf(inspectorPropertyInfo2);
				string finalGroupName = "#" + groupDataAndInfo.Data.Name;
				int num2 = rootMemberProperties.FindIndex((InspectorPropertyInfo n) => n.PropertyName == finalGroupName);
				if (num2 >= 0)
				{
					InspectorPropertyInfo inspectorPropertyInfo3 = rootMemberProperties[num2];
					if (TryHidePropertyWithGroup(parentProperty, inspectorPropertyInfo3, groupDataAndInfo.Info, includeSpeciallySerializedMembers, out var newAliasForHiddenProperty))
					{
						rootMemberProperties[num2] = newAliasForHiddenProperty;
						removedMembers_Cached[inspectorPropertyInfo3] = groupDataAndInfo.Info;
						groupMemberOrders[newAliasForHiddenProperty] = groupMemberOrders[inspectorPropertyInfo3];
					}
				}
				if (num >= 0)
				{
					removedMembers_Cached.Add(rootMemberProperties[num], groupDataAndInfo.Info);
					groupMemberOrders[groupDataAndInfo.Info] = groupMemberOrders[rootMemberProperties[num]];
					rootMemberProperties[num] = groupDataAndInfo.Info;
				}
				else
				{
					InspectorPropertyInfo item = removedMembers_Cached[inspectorPropertyInfo2];
					num = rootMemberProperties.IndexOf(item);
					rootMemberProperties.Insert(num + 1, groupDataAndInfo.Info);
					groupMemberOrders[groupDataAndInfo.Info] = groupMemberOrders[rootMemberProperties[num]] + 0.1f;
				}
			}
			for (int num3 = 0; num3 < list2.Count; num3++)
			{
				GroupDataAndInfo groupDataAndInfo2 = list2[num3];
				IEnumerable<InspectorPropertyInfo> enumerable = RecurseGroupMembers(groupDataAndInfo2.Data);
				foreach (InspectorPropertyInfo item3 in enumerable)
				{
					if (!removedMembers_Cached.ContainsKey(item3))
					{
						removedMembers_Cached.Add(item3, groupDataAndInfo2.Info);
					}
					rootMemberProperties.Remove(item3);
				}
			}
			if (removedMembers_Cached.Count > 0)
			{
				removedMembers_Cached.Clear();
			}
		}

		private static void RegisterGroupAttribute(InspectorPropertyInfo member, PropertyGroupAttribute attribute, Dictionary<string, GroupData> groupTree)
		{
			string[] array = attribute.GroupID.Split('/');
			string text = array[0];
			if (!groupTree.TryGetValue(text, out var value))
			{
				value = new GroupData();
				value.ID = text;
				value.Name = text;
				groupTree.Add(text, value);
			}
			for (int i = 1; i < array.Length; i++)
			{
				string step = array[i];
				GroupData groupData = value.ChildGroups.FirstOrDefault((GroupData n) => n.Name == step);
				if (groupData == null)
				{
					groupData = new GroupData();
					groupData.ID = string.Join("/", array.Take(i + 1).ToArray());
					groupData.Name = step;
					groupData.Parent = value;
					value.ChildGroups.Add(groupData);
				}
				value = groupData;
			}
			GroupAttributeInfo item = default(GroupAttributeInfo);
			item.InspectorPropertyInfo = member;
			item.Attribute = attribute;
			value.Attributes.Add(item);
		}

		private static bool ProcessGroups(GroupData groupData, Dictionary<string, GroupData> groupTree)
		{
			if (groupData.Attributes.Count == 0)
			{
				foreach (GroupData item3 in from n in RecurseGroups(groupData)
					where n.Attributes.Count > 0
					select n)
				{
					foreach (GroupAttributeInfo attribute in item3.Attributes)
					{
						Debug.LogError((object)("Group attribute '" + attribute.Attribute.GetType().Name + "' on member '" + attribute.InspectorPropertyInfo.PropertyName + "' expected a group with the name '" + groupData.Name + "' to exist in declaring type '" + attribute.InspectorPropertyInfo.TypeOfOwner.GetNiceName() + "'. Its ID was '" + item3.ID + "'."));
					}
				}
				return false;
			}
			string name = groupData.Name;
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == '.')
				{
					Debug.LogError((object)("Group name '" + groupData.Name + "' is invalid; group names or paths cannot contain '.'!"));
					return false;
				}
			}
			groupData.ConsolidatedAttribute = (PropertyGroupAttribute)SerializationUtility.CreateCopy(groupData.Attributes[0].Attribute);
			Type type = groupData.ConsolidatedAttribute.GetType();
			for (int j = 1; j < groupData.Attributes.Count; j++)
			{
				GroupAttributeInfo groupAttributeInfo = groupData.Attributes[j];
				if (groupAttributeInfo.Attribute.GetType() != type)
				{
					Debug.LogError((object)("Cannot have group attributes of different types with the same group name, on the same type (or its inherited types): Group type mismatch: the group '" + groupData.ID + "' is expecting attributes of type '" + type.Name + "', but got an attribute of type '" + groupAttributeInfo.Attribute.GetType().Name + "' on the property '" + groupAttributeInfo.InspectorPropertyInfo.TypeOfOwner.GetNiceName() + "." + groupAttributeInfo.InspectorPropertyInfo.PropertyName + "'."));
					groupData.Attributes.RemoveAt(j--);
				}
				else
				{
					try
					{
						groupData.ConsolidatedAttribute = groupData.ConsolidatedAttribute.Combine(groupAttributeInfo.Attribute);
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
					}
				}
			}
			ISubGroupProviderAttribute subGroupProviderAttribute = groupData.ConsolidatedAttribute as ISubGroupProviderAttribute;
			if (subGroupProviderAttribute != null)
			{
				string[] array = groupData.ID.Split('/');
				Dictionary<string, PropertyGroupAttribute> dictionary = new Dictionary<string, PropertyGroupAttribute>();
				foreach (PropertyGroupAttribute subGroupAttribute in subGroupProviderAttribute.GetSubGroupAttributes())
				{
					string[] array2 = subGroupAttribute.GroupID.Split('/');
					bool flag = true;
					if (array2.Length != array.Length + 1)
					{
						flag = false;
					}
					if (flag)
					{
						for (int k = 0; k < array.Length; k++)
						{
							if (array2[k] != array[k])
							{
								flag = false;
								break;
							}
						}
					}
					if (flag)
					{
						GroupData groupData2 = groupData.ChildGroups.FirstOrDefault((GroupData n) => n.Name == subGroupAttribute.GroupName);
						if (groupData2 == null)
						{
							groupData2 = new GroupData();
							groupData2.ID = subGroupAttribute.GroupID;
							groupData2.Name = subGroupAttribute.GroupName;
							groupData2.Parent = groupData;
							groupData.ChildGroups.Add(groupData2);
						}
						if (!dictionary.ContainsKey(subGroupAttribute.GroupID))
						{
							dictionary.Add(subGroupAttribute.GroupID, subGroupAttribute);
						}
						GroupAttributeInfo item = default(GroupAttributeInfo);
						item.InspectorPropertyInfo = groupData.Attributes[0].InspectorPropertyInfo;
						item.Attribute = subGroupAttribute;
						item.Exclude = true;
						groupData2.Attributes.Add(item);
					}
					else
					{
						Debug.LogError((object)("Subgroup '" + subGroupAttribute.GroupID + "' of type '" + subGroupAttribute.GetType().Name + "' for group '" + groupData.ID + "' of type '" + groupData.ConsolidatedAttribute.GetType().Name + "' must have an ID that starts with '" + groupData.ID + "' and continue one path step further."));
					}
				}
				for (int l = 0; l < groupData.Attributes.Count; l++)
				{
					GroupAttributeInfo item2 = groupData.Attributes[l];
					string newPath = subGroupProviderAttribute.RepathMemberAttribute(item2.Attribute);
					if (newPath == null || !(newPath != item2.Attribute.GroupID))
					{
						continue;
					}
					if (!dictionary.ContainsKey(newPath))
					{
						Debug.LogError((object)("Member '" + item2.InspectorPropertyInfo.PropertyName + "' of " + groupData.ConsolidatedAttribute.GetType().Name + " group '" + groupData.ID + "' was repathed to subgroup at path '" + newPath + "', but no such subgroup was defined."));
					}
					else
					{
						groupData.Attributes.RemoveAt(l--);
						item2.Attribute = dictionary[newPath];
						GroupData groupData3 = groupData.ChildGroups.First((GroupData n) => n.ID == newPath);
						groupData3.Attributes.Add(item2);
					}
				}
			}
			for (int m = 0; m < groupData.ChildGroups.Count; m++)
			{
				if (!ProcessGroups(groupData.ChildGroups[m], groupTree))
				{
					groupData.ChildGroups.RemoveAt(m);
					m--;
				}
			}
			HashSet<string> hashSet = new HashSet<string>();
			for (int num = 0; num < groupData.Attributes.Count; num++)
			{
				GroupAttributeInfo groupAttributeInfo2 = groupData.Attributes[num];
				if (groupAttributeInfo2.InspectorPropertyInfo.PropertyType != PropertyType.Group && !groupAttributeInfo2.Exclude)
				{
					string propertyName = groupAttributeInfo2.InspectorPropertyInfo.PropertyName;
					if (!hashSet.Add(propertyName))
					{
						groupData.Attributes.RemoveAt(num--);
					}
				}
			}
			return true;
		}

		private static InspectorPropertyInfo CreatePropertyGroups(InspectorProperty parentProperty, Type typeOfOwner, GroupData groupData, Dictionary<InspectorPropertyInfo, float> memberOrder, bool includeSpeciallySerializedMembers)
		{
			List<InspectorPropertyInfo> list = new List<InspectorPropertyInfo>();
			foreach (GroupAttributeInfo attribute in groupData.Attributes)
			{
				if (!attribute.Exclude)
				{
					list.Add(attribute.InspectorPropertyInfo);
				}
			}
			foreach (GroupData childGroup in groupData.ChildGroups)
			{
				InspectorPropertyInfo inspectorPropertyInfo = CreatePropertyGroups(parentProperty, typeOfOwner, childGroup, memberOrder, includeSpeciallySerializedMembers);
				InspectorPropertyInfo inspectorPropertyInfo2 = null;
				float num = 0f;
				foreach (InspectorPropertyInfo item in RecurseGroupMembers(childGroup))
				{
					float num2 = memberOrder[item];
					if (inspectorPropertyInfo2 == null || num2 < num)
					{
						num = num2;
						inspectorPropertyInfo2 = item;
					}
				}
				int num3 = list.IndexOf(inspectorPropertyInfo2);
				if (num3 >= 0)
				{
					memberOrder[inspectorPropertyInfo] = memberOrder[list[num3]];
					list[num3] = inspectorPropertyInfo;
				}
				else
				{
					memberOrder[inspectorPropertyInfo] = memberOrder[inspectorPropertyInfo2];
					list.Insert(0, inspectorPropertyInfo);
				}
				string text = "#" + inspectorPropertyInfo.PropertyName;
				for (int i = 0; i < list.Count; i++)
				{
					InspectorPropertyInfo inspectorPropertyInfo3 = list[i];
					if (inspectorPropertyInfo3 != inspectorPropertyInfo && inspectorPropertyInfo3.PropertyName == text && TryHidePropertyWithGroup(parentProperty, inspectorPropertyInfo3, inspectorPropertyInfo, includeSpeciallySerializedMembers, out var newAliasForHiddenProperty))
					{
						memberOrder[newAliasForHiddenProperty] = memberOrder[list[i]];
						list[i] = newAliasForHiddenProperty;
					}
				}
			}
			foreach (GroupData childGroup2 in groupData.ChildGroups)
			{
				IEnumerable<InspectorPropertyInfo> enumerable = RecurseGroupMembers(childGroup2);
				foreach (InspectorPropertyInfo item2 in enumerable)
				{
					list.Remove(item2);
				}
			}
			list.Sort(delegate(InspectorPropertyInfo a, InspectorPropertyInfo b)
			{
				int num5 = a.Order.CompareTo(b.Order);
				if (num5 != 0)
				{
					return num5;
				}
				num5 = memberOrder[a].CompareTo(memberOrder[b]);
				return (num5 != 0) ? num5 : GetMemberCategoryOrder(a.GetMemberInfo()).CompareTo(GetMemberCategoryOrder(b.GetMemberInfo()));
			});
			float num4 = groupData.ConsolidatedAttribute.Order;
			if (num4 == 0f)
			{
				num4 = float.MaxValue;
				foreach (InspectorPropertyInfo item3 in RecurseGroupMembers(groupData))
				{
					if (item3.Order < num4)
					{
						num4 = item3.Order;
					}
				}
			}
			return InspectorPropertyInfo.CreateGroup("#" + groupData.Name, typeOfOwner, num4, list.ToArray(), new List<Attribute> { groupData.ConsolidatedAttribute });
		}

		private static bool TryHidePropertyWithGroup(InspectorProperty parentProperty, InspectorPropertyInfo hidden, InspectorPropertyInfo group, bool includeSpeciallySerializedMembers, out InspectorPropertyInfo newAliasForHiddenProperty)
		{
			if (hidden.PropertyType == PropertyType.Group)
			{
				string text = group.TypeOfOwner.GetNiceName() + "." + group.PropertyName;
				string text2 = hidden.TypeOfOwner.GetNiceName() + "." + hidden.PropertyName;
				Debug.LogWarning((object)("Property group '" + text + "' conflicts with already existing group property '" + text2 + "'. Group property '" + text + "' will be removed from the property tree."));
				newAliasForHiddenProperty = null;
				return false;
			}
			if (hidden.GetMemberInfo() != null)
			{
				MemberInfo privateMemberAlias = GetPrivateMemberAlias(hidden.GetMemberInfo(), hidden.TypeOfOwner.GetNiceName(), " -> ");
				string name = privateMemberAlias.Name;
				string text3 = group.TypeOfOwner.GetNiceName() + "." + group.PropertyName;
				string text4 = hidden.TypeOfOwner.GetNiceName() + "." + hidden.PropertyName;
				if (TryCreate(parentProperty, privateMemberAlias, includeSpeciallySerializedMembers, out newAliasForHiddenProperty))
				{
					Debug.LogWarning((object)("Property group '" + text3 + "' hides member property '" + text4 + "'. Alias property '" + name + "' created for member property '" + text4 + "'."));
					return true;
				}
				Debug.LogWarning((object)("Property group '" + text3 + "' tries to hide member property '" + text4 + "', but failed to create alias property '" + name + "' for member property '" + text4 + "'; group property '" + text3 + "' will be removed."));
				return false;
			}
			newAliasForHiddenProperty = null;
			return false;
		}

		private static IEnumerable<GroupData> RecurseGroups(GroupData groupData)
		{
			yield return groupData;
			for (int i = 0; i < groupData.ChildGroups.Count; i++)
			{
				GroupData groupData2 = groupData.ChildGroups[i];
				foreach (GroupData item in RecurseGroups(groupData2))
				{
					yield return item;
				}
			}
		}

		private static IEnumerable<InspectorPropertyInfo> RecurseGroupMembers(GroupData groupData)
		{
			for (int i = 0; i < groupData.Attributes.Count; i++)
			{
				yield return groupData.Attributes[i].InspectorPropertyInfo;
			}
			for (int j = 0; j < groupData.ChildGroups.Count; j++)
			{
				GroupData groupData2 = groupData.ChildGroups[j];
				foreach (GroupData child in RecurseGroups(groupData2))
				{
					for (int k = 0; k < child.Attributes.Count; k++)
					{
						yield return child.Attributes[k].InspectorPropertyInfo;
					}
				}
			}
		}

		private static void PopulateUnityProperties(InspectorProperty parentProperty, Type type, List<InspectorPropertyInfo> result)
		{
			SerializationBackend serializationBackendOfProperty = GetSerializationBackendOfProperty(parentProperty);
			if (!UnityPropertyInfoCache.TryGetValue(serializationBackendOfProperty, out var value))
			{
				value = new Dictionary<Type, List<InspectorPropertyInfo>>(FastTypeComparer.Instance);
				UnityPropertyInfoCache.Add(serializationBackendOfProperty, value);
			}
			if (!value.TryGetValue(type, out var value2))
			{
				value2 = new List<InspectorPropertyInfo>();
				FindUnityProperties(parentProperty, type, value2);
				value.Add(type, value2);
			}
			int count = value2.Count;
			for (int i = 0; i < count; i++)
			{
				InspectorPropertyInfo item = value2[i].CreateCopy();
				result.Add(item);
			}
		}

		private static void FindUnityProperties(InspectorProperty parentProperty, Type type, List<InspectorPropertyInfo> result)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Expected O, but got Unknown
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Expected O, but got Unknown
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Expected O, but got Unknown
			//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0404: Invalid comparison between Unknown and I4
			//IL_046f: Unknown result type (might be due to invalid IL or missing references)
			if (type.IsAbstract || type.IsInterface || type.IsArray)
			{
				return;
			}
			Object val = null;
			SerializedProperty prop;
			if (typeof(Component).IsAssignableFrom(type))
			{
				GameObject val2 = new GameObject("temp");
				Component val3 = (Component)((!type.IsAssignableFrom(typeof(Transform))) ? ((object)val2.AddComponent(type)) : ((object)val2.get_transform()));
				SerializedObject val4 = new SerializedObject((Object)(object)val3);
				prop = val4.GetIterator();
				val = (Object)(object)val2;
			}
			else if (typeof(ScriptableObject).IsAssignableFrom(type))
			{
				ScriptableObject val5 = ScriptableObject.CreateInstance(type);
				SerializedObject val6 = new SerializedObject((Object)(object)val5);
				prop = val6.GetIterator();
				val = (Object)(object)val5;
			}
			else if (UnityVersion.IsVersionOrGreater(2017, 1))
			{
				GameObject gameObject = new GameObject();
				UnityPropertyEmitter.Handle handle = UnityPropertyEmitter.CreateEmittedMonoBehaviourProperty("InspectorPropertyInfo_UnityPropertyExtractor", type, 1, ref gameObject);
				prop = handle.UnityProperty;
				val = (Object)(object)gameObject;
			}
			else
			{
				prop = UnityPropertyEmitter.CreateEmittedScriptableObjectProperty("InspectorPropertyInfo_UnityPropertyExtractor", type, 1);
				if (prop != null)
				{
					val = prop.get_serializedObject().get_targetObject();
				}
			}
			try
			{
				if (prop == null || !prop.Next(true))
				{
					return;
				}
				List<MemberInfo> list = (from n in type.GetAllMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					where (n is FieldInfo || n is PropertyInfo) && !AlwaysSkipUnityPropertiesDeclaredBy.Contains(n.DeclaringType)
					select n).ToList();
				do
				{
					if (AlwaysSkipUnityProperties.Contains(prop.get_name()) || (typeof(Component).IsAssignableFrom(type) && AlwaysSkipUnityPropertiesForComponents.Contains(prop.get_name())))
					{
						continue;
					}
					string memberName = prop.get_name();
					if (UnityPropertyMemberNameReplacements.ContainsKeys(type, memberName))
					{
						memberName = UnityPropertyMemberNameReplacements[type][memberName];
					}
					MemberInfo memberInfo = list.FirstOrDefault((MemberInfo n) => n.Name == memberName || n.Name == prop.get_name());
					if (memberInfo == null)
					{
						string propName2 = prop.get_displayName().Replace(" ", "");
						bool flag = false;
						if (string.Equals(propName2, "material", StringComparison.InvariantCultureIgnoreCase))
						{
							flag = true;
							propName2 = "sharedMaterial";
						}
						else if (string.Equals(propName2, "mesh", StringComparison.InvariantCultureIgnoreCase))
						{
							flag = true;
							propName2 = "sharedMesh";
						}
						memberInfo = list.FirstOrDefault((MemberInfo n) => string.Equals(n.Name, propName2, StringComparison.InvariantCultureIgnoreCase) && prop.IsCompatibleWithType(n.GetReturnType()));
						if (flag && memberInfo == null)
						{
							propName2 = prop.get_displayName().Replace(" ", "");
							memberInfo = list.FirstOrDefault((MemberInfo n) => string.Equals(n.Name, propName2, StringComparison.InvariantCultureIgnoreCase) && prop.IsCompatibleWithType(n.GetReturnType()));
						}
					}
					if (memberInfo == null)
					{
						string propName = prop.get_displayName();
						List<MemberInfo> list2 = list.Where((MemberInfo n) => (propName.Contains(n.Name, StringComparison.InvariantCultureIgnoreCase) || n.Name.Contains(propName, StringComparison.InvariantCultureIgnoreCase)) && prop.IsCompatibleWithType(n.GetReturnType())).ToList();
						if (list2.Count == 1)
						{
							memberInfo = list2[0];
						}
					}
					if (memberInfo == null)
					{
						Type type2 = prop.GuessContainedType();
						if (type2 != null && SerializedPropertyUtilities.CanSetGetValue(type2))
						{
							result.Add(InspectorPropertyInfo.CreateForUnityProperty(prop.get_name(), type, type2, prop.get_editable(), (Attribute[])null));
							continue;
						}
					}
					if (memberInfo == null)
					{
						if (!(prop.get_name() == "Array") || (int)prop.get_propertyType() != -1)
						{
							Debug.LogWarning((object)string.Concat("Failed to find corresponding member for Unity property '", prop.get_name(), "/", prop.get_displayName(), "' on type ", type.GetNiceName(), ", and cannot alias a Unity property of type '", prop.get_propertyType(), "/", prop.get_type(), "'. This property will be missing in the inspector."));
						}
					}
					else
					{
						list.Remove(memberInfo);
						List<Attribute> attributes = new List<Attribute>();
						ProcessAttributes(parentProperty, memberInfo, attributes);
						if (TryCreate(memberInfo, GetSerializationBackend(parentProperty, memberInfo), prop.get_editable(), out var result2, attributes))
						{
							result2.PropertyName = prop.get_name();
							result.Add(result2);
						}
					}
				}
				while (prop.Next(false));
			}
			catch (InvalidOperationException)
			{
			}
			finally
			{
				if (val != (Object)null)
				{
					Object.DestroyImmediate(val);
				}
			}
		}

		private static void PopulateMemberInspectorProperties(InspectorProperty parentProperty, Type type, bool includeSpeciallySerializedMembers, List<InspectorPropertyInfo> properties)
		{
			if (type.IsPrimitive || type == typeof(string))
			{
				return;
			}
			Type baseType = type.BaseType;
			if (baseType != null && baseType != System_Object_Type && baseType != UnityEngine_Object_Type && baseType != UnityEngine_Component_Type && baseType != UnityEngine_MonoBehaviour_Type && baseType != UnityEngine_Behaviour_Type && baseType != UnityEngine_ScriptableObject_Type)
			{
				PopulateMemberInspectorProperties(parentProperty, baseType, includeSpeciallySerializedMembers, properties);
			}
			if (!TypeMembers_Cache.TryGetValue(type, out var value))
			{
				value = type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				TypeMembers_Cache.Add(type, value);
			}
			foreach (MemberInfo memberInfo in value)
			{
				if (!(memberInfo is FieldInfo) && !(memberInfo is PropertyInfo) && !(memberInfo is MethodInfo))
				{
					continue;
				}
				if (memberInfo is PropertyInfo)
				{
					PropertyInfo propertyInfo = memberInfo as PropertyInfo;
					ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
					if (indexParameters.Length != 0)
					{
						continue;
					}
				}
				if (!TryCreate(parentProperty, memberInfo, includeSpeciallySerializedMembers, out var result))
				{
					continue;
				}
				InspectorPropertyInfo inspectorPropertyInfo = null;
				int index = -1;
				for (int j = 0; j < properties.Count; j++)
				{
					if (properties[j].PropertyName == result.PropertyName)
					{
						index = j;
						inspectorPropertyInfo = properties[j];
						break;
					}
				}
				if (inspectorPropertyInfo != null)
				{
					bool flag = true;
					if (memberInfo.SignaturesAreEqual(inspectorPropertyInfo.GetMemberInfo()))
					{
						flag = false;
						properties.RemoveAt(index);
					}
					if (flag)
					{
						MemberInfo privateMemberAlias = GetPrivateMemberAlias(inspectorPropertyInfo.GetMemberInfo(), inspectorPropertyInfo.TypeOfOwner.GetNiceName(), " -> ");
						string name = privateMemberAlias.Name;
						if (TryCreate(parentProperty, privateMemberAlias, includeSpeciallySerializedMembers, out var result2))
						{
							properties[index] = result2;
						}
						else
						{
							string text = result.TypeOfOwner.GetNiceName() + "." + result.GetMemberInfo().Name;
							string text2 = inspectorPropertyInfo.TypeOfOwner.GetNiceName() + "." + inspectorPropertyInfo.PropertyName;
							properties.RemoveAt(index);
						}
					}
				}
				properties.Add(result);
			}
		}

		private static SerializationBackend GetSerializationBackendOfProperty(InspectorProperty property)
		{
			if (property.ValueEntry == null)
			{
				property = property.ParentValueProperty ?? property;
			}
			return property.Info.SerializationBackend;
		}

		public static SerializationBackend GetSerializationBackend(InspectorProperty parentProperty, MemberInfo member)
		{
			return GetSerializationBackend(parentProperty, member, GetSerializationBackendOfProperty(parentProperty));
		}

		private static SerializationBackend GetSerializationBackend(InspectorProperty parentProperty, MemberInfo member, SerializationBackend parentBackend)
		{
			if (!(member is FieldInfo) && !(member is PropertyInfo))
			{
				return SerializationBackend.None;
			}
			if (parentProperty.ValueEntry == null)
			{
				parentProperty = parentProperty.ParentValueProperty ?? parentProperty;
			}
			InspectorProperty inspectorProperty = ((parentProperty.ValueEntry == null || !typeof(Object).IsAssignableFrom(parentProperty.ValueEntry.TypeOfValue)) ? parentProperty.SerializationRoot : parentProperty);
			if (inspectorProperty.ValueEntry == null)
			{
				return SerializationBackend.None;
			}
			if (parentBackend == SerializationBackend.None && inspectorProperty != parentProperty)
			{
				return SerializationBackend.None;
			}
			ISerializationPolicy policy = SerializationPolicies.Unity;
			bool flag = TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cached(inspectorProperty.ValueEntry.TypeOfValue);
			IOverridesSerializationPolicy overridesSerializationPolicy = inspectorProperty.ValueEntry.WeakValues[0] as IOverridesSerializationPolicy;
			if (flag && overridesSerializationPolicy != null)
			{
				policy = overridesSerializationPolicy.SerializationPolicy ?? SerializationPolicies.Unity;
			}
			if (inspectorProperty != parentProperty)
			{
				if (parentBackend == SerializationBackend.Odin)
				{
					if (!UnitySerializationUtility.OdinWillSerialize(member, serializeUnityFields: true, policy))
					{
						return SerializationBackend.None;
					}
					return SerializationBackend.Odin;
				}
				if (parentBackend.IsUnity)
				{
					if (SerializationBackend.UnityPolymorphic.CanSerializeMember(member))
					{
						return SerializationBackend.UnityPolymorphic;
					}
					if (!SerializationBackend.Unity.CanSerializeMember(member))
					{
						return SerializationBackend.None;
					}
					return SerializationBackend.Unity;
				}
				if (parentBackend.CanSerializeMember(member))
				{
					return parentBackend;
				}
				return SerializationBackend.None;
			}
			if (flag)
			{
				bool serializeUnityFields = false;
				if (overridesSerializationPolicy != null)
				{
					serializeUnityFields = overridesSerializationPolicy.OdinSerializesUnityFields;
				}
				if (UnitySerializationUtility.OdinWillSerialize(member, serializeUnityFields, policy))
				{
					return SerializationBackend.Odin;
				}
			}
			if (SerializationBackend.UnityPolymorphic.CanSerializeMember(member))
			{
				return SerializationBackend.UnityPolymorphic;
			}
			if (SerializationBackend.Unity.CanSerializeMember(member))
			{
				return SerializationBackend.Unity;
			}
			return SerializationBackend.None;
		}

		public static void ProcessAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
		{
			List<OdinAttributeProcessor> childProcessors = parentProperty.Tree.AttributeProcessorLocator.GetChildProcessors(parentProperty, member);
			for (int i = 0; i < childProcessors.Count; i++)
			{
				try
				{
					childProcessors[i].ProcessChildMemberAttributes(parentProperty, member, attributes);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		public static bool TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cached(Type type)
		{
			if (!TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cache.TryGetValue(type, out var value))
			{
				value = type.IsDefined(typeof(ShowOdinSerializedPropertiesInInspectorAttribute), inherit: true);
				TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cache.Add(type, value);
			}
			return value;
		}
	}
}
