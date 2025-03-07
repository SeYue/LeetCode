using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Contains meta-data information about a property in the inspector, that can be used to create an actual property instance.
	/// </summary>
	public sealed class InspectorPropertyInfo
	{
		private struct TypeSignature4
		{
			public Type T1;

			public Type T2;

			public Type T3;

			public Type T4;

			public int Hash;

			public TypeSignature4(Type t1, Type t2, Type t3, Type t4)
			{
				T1 = t1;
				T2 = t2;
				T3 = t3;
				T4 = t4;
				int num = 1;
				int hashCode = t1.GetHashCode();
				num = 137 * num + (hashCode ^ (hashCode >> 16));
				hashCode = t2.GetHashCode();
				num = 137 * num + (hashCode ^ (hashCode >> 16));
				hashCode = t3.GetHashCode();
				num = 137 * num + (hashCode ^ (hashCode >> 16));
				hashCode = t4.GetHashCode();
				num = (Hash = 137 * num + (hashCode ^ (hashCode >> 16)));
			}
		}

		private class TypeSignatureComparer : IEqualityComparer<TypeSignature4>
		{
			public bool Equals(TypeSignature4 x, TypeSignature4 y)
			{
				if (x.Hash != y.Hash)
				{
					return false;
				}
				if (x.T1 != y.T1)
				{
					return false;
				}
				if (x.T2 != y.T2)
				{
					return false;
				}
				if (x.T3 != y.T3)
				{
					return false;
				}
				if (x.T4 != y.T4)
				{
					return false;
				}
				return true;
			}

			public int GetHashCode(TypeSignature4 obj)
			{
				return obj.Hash;
			}
		}

		private MemberInfo[] memberInfos;

		private List<Attribute> attributes;

		private ImmutableList<Attribute> attributesImmutable;

		private Type typeOfOwner;

		private Type typeOfValue;

		private IValueGetterSetter getterSetter;

		private InspectorPropertyInfo[] groupInfos;

		private bool isUnityPropertyOnly;

		private Delegate @delegate;

		private static readonly DoubleLookupDictionary<Type, Type, Func<MemberInfo, bool, IValueGetterSetter>> GetterSetterCreators = new DoubleLookupDictionary<Type, Type, Func<MemberInfo, bool, IValueGetterSetter>>(FastTypeComparer.Instance, FastTypeComparer.Instance);

		private static readonly Dictionary<TypeSignature4, Func<IValueGetterSetter, IValueGetterSetter>> AliasGetterSetterCreators = new Dictionary<TypeSignature4, Func<IValueGetterSetter, IValueGetterSetter>>(new TypeSignatureComparer());

		private static readonly Type[] GetterSetterConstructorSignature = new Type[2]
		{
			typeof(MemberInfo),
			typeof(bool)
		};

		private static readonly Type[] AliasGetterSetterConstructorSignature = new Type[1];

		/// <summary>
		/// The name of the property.
		/// </summary>
		public string PropertyName { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether this InspectorPropertyInfo has any backing members.
		/// </summary>
		public bool HasBackingMembers
		{
			get
			{
				if (memberInfos != null)
				{
					return memberInfos.Length != 0;
				}
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this InspectorPropertyInfo has only a single backing member.
		/// </summary>
		public bool HasSingleBackingMember
		{
			get
			{
				if (memberInfos != null)
				{
					return memberInfos.Length == 1;
				}
				return false;
			}
		}

		/// <summary>
		/// The member info of the property. If the property has many member infos, such as if it is a group property, the first member info of <see cref="P:Sirenix.OdinInspector.Editor.InspectorPropertyInfo.MemberInfos" /> is returned.
		/// </summary>
		[Obsolete("Use GetMemberInfo() instead, and note that there might not be a member at all, even if there is a value.", true)]
		public MemberInfo MemberInfo => GetMemberInfo();

		/// <summary>
		/// Indicates which type of property it is.
		/// </summary>
		public PropertyType PropertyType { get; private set; }

		/// <summary>
		/// The serialization backend for this property.
		/// </summary>
		public SerializationBackend SerializationBackend { get; private set; }

		/// <summary>
		/// The type on which this property is declared.
		/// </summary>
		public Type TypeOfOwner => typeOfOwner;

		/// <summary>
		/// The base type of the value which this property represents. If there is no value, this will be null.
		/// </summary>
		public Type TypeOfValue => typeOfValue;

		/// <summary>
		/// Whether this property is editable or not.
		/// </summary>
		public bool IsEditable { get; private set; }

		/// <summary>
		/// All member infos of the property. There will only be more than one member if it is an <see cref="!:InspectorPropertyGroupInfo" />.
		/// </summary>
		[Obsolete("Use GetMemberInfos() instead, and note that there might not be any members at all, even if there is a value.", true)]
		public MemberInfo[] MemberInfos => memberInfos;

		/// <summary>
		/// The order value of this property. Properties are (by convention) ordered by ascending order, IE, lower order values are shown first in the inspector. The final actual ordering of properties is decided upon by the property resolver.
		/// </summary>
		public float Order { get; set; }

		/// <summary>
		/// The attributes associated with this property.
		/// </summary>
		public ImmutableList<Attribute> Attributes
		{
			get
			{
				if (attributes == null)
				{
					return null;
				}
				if (attributesImmutable == null)
				{
					attributesImmutable = new ImmutableList<Attribute>(attributes);
				}
				return attributesImmutable;
			}
		}

		/// <summary>
		/// Whether this property only exists as a Unity <see cref="T:UnityEditor.SerializedProperty" />, and has no associated managed member to represent it.
		/// This case requires some special one-off custom behaviour in a few places.
		/// </summary>
		public bool IsUnityPropertyOnly => isUnityPropertyOnly;

		public static InspectorPropertyInfo CreateForDelegate(string name, float order, Type typeOfOwner, Delegate @delegate, params Attribute[] attributes)
		{
			return CreateForDelegate(name, order, typeOfOwner, @delegate, (IEnumerable<Attribute>)attributes);
		}

		public static InspectorPropertyInfo CreateForDelegate(string name, float order, Type typeOfOwner, Delegate @delegate, IEnumerable<Attribute> attributes)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (typeOfOwner == null)
			{
				throw new ArgumentNullException("typeOfOwner");
			}
			if ((object)@delegate == null)
			{
				throw new ArgumentNullException("@delegate");
			}
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == '.')
				{
					throw new ArgumentException("Property names may not contain '.'; was given the name '" + name + "'.");
				}
			}
			InspectorPropertyInfo inspectorPropertyInfo = new InspectorPropertyInfo();
			inspectorPropertyInfo.memberInfos = new MemberInfo[0];
			inspectorPropertyInfo.typeOfOwner = typeOfOwner;
			inspectorPropertyInfo.Order = order;
			inspectorPropertyInfo.PropertyName = name;
			inspectorPropertyInfo.PropertyType = PropertyType.Method;
			inspectorPropertyInfo.SerializationBackend = SerializationBackend.None;
			if (attributes == null)
			{
				inspectorPropertyInfo.attributes = new List<Attribute>();
			}
			else
			{
				inspectorPropertyInfo.attributes = attributes.Where((Attribute attr) => attr != null).ToList();
			}
			inspectorPropertyInfo.@delegate = @delegate;
			return inspectorPropertyInfo;
		}

		public static InspectorPropertyInfo CreateForUnityProperty(string unityPropertyName, Type typeOfOwner, Type typeOfValue, bool isEditable, params Attribute[] attributes)
		{
			return CreateForUnityProperty(unityPropertyName, typeOfOwner, typeOfValue, isEditable, (IEnumerable<Attribute>)attributes);
		}

		public static InspectorPropertyInfo CreateForUnityProperty(string unityPropertyName, Type typeOfOwner, Type typeOfValue, bool isEditable, IEnumerable<Attribute> attributes)
		{
			if (unityPropertyName == null)
			{
				throw new ArgumentNullException("unityPropertyName");
			}
			if (typeOfOwner == null)
			{
				throw new ArgumentNullException("typeOfOwner");
			}
			if (typeOfValue == null)
			{
				throw new ArgumentNullException("typeOfValue");
			}
			for (int i = 0; i < unityPropertyName.Length; i++)
			{
				if (unityPropertyName[i] == '.')
				{
					throw new ArgumentException("Property names may not contain '.'; was given the name '" + unityPropertyName + "'.");
				}
			}
			InspectorPropertyInfo inspectorPropertyInfo = new InspectorPropertyInfo();
			inspectorPropertyInfo.memberInfos = new MemberInfo[0];
			inspectorPropertyInfo.typeOfOwner = typeOfOwner;
			inspectorPropertyInfo.typeOfValue = typeOfValue;
			inspectorPropertyInfo.PropertyName = unityPropertyName;
			inspectorPropertyInfo.PropertyType = PropertyType.Value;
			inspectorPropertyInfo.SerializationBackend = SerializationBackend.Unity;
			inspectorPropertyInfo.IsEditable = isEditable;
			if (attributes == null)
			{
				inspectorPropertyInfo.attributes = new List<Attribute>();
			}
			else
			{
				inspectorPropertyInfo.attributes = attributes.Where((Attribute attr) => attr != null).ToList();
			}
			inspectorPropertyInfo.isUnityPropertyOnly = true;
			return inspectorPropertyInfo;
		}

		public static InspectorPropertyInfo CreateValue(string name, float order, SerializationBackend serializationBackend, IValueGetterSetter getterSetter, params Attribute[] attributes)
		{
			return CreateValue(name, order, serializationBackend, getterSetter, (IList<Attribute>)attributes);
		}

		public static InspectorPropertyInfo CreateValue(string name, float order, SerializationBackend serializationBackend, IValueGetterSetter getterSetter, IList<Attribute> attributes)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (getterSetter == null)
			{
				throw new ArgumentNullException("getterSetter");
			}
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == '.')
				{
					throw new ArgumentException("Property names may not contain '.'; was given the name '" + name + "'.");
				}
			}
			InspectorPropertyInfo inspectorPropertyInfo = new InspectorPropertyInfo();
			inspectorPropertyInfo.memberInfos = new MemberInfo[0];
			inspectorPropertyInfo.typeOfOwner = getterSetter.OwnerType;
			inspectorPropertyInfo.typeOfValue = getterSetter.ValueType;
			if (attributes == null)
			{
				inspectorPropertyInfo.attributes = new List<Attribute>();
			}
			else
			{
				int count = attributes.Count;
				inspectorPropertyInfo.attributes = new List<Attribute>(count + 4);
				for (int j = 0; j < count; j++)
				{
					Attribute attribute = attributes[j];
					if (attribute != null)
					{
						inspectorPropertyInfo.attributes.Add(attribute);
					}
				}
			}
			inspectorPropertyInfo.PropertyName = name;
			inspectorPropertyInfo.PropertyType = PropertyType.Value;
			inspectorPropertyInfo.SerializationBackend = serializationBackend ?? SerializationBackend.None;
			inspectorPropertyInfo.IsEditable = !getterSetter.IsReadonly;
			inspectorPropertyInfo.Order = order;
			inspectorPropertyInfo.getterSetter = getterSetter;
			return inspectorPropertyInfo;
		}

		public static InspectorPropertyInfo CreateForMember(InspectorProperty parentProperty, MemberInfo member, bool allowEditable, params Attribute[] attributes)
		{
			List<Attribute> list = new List<Attribute>(attributes.Length);
			for (int i = 0; i < attributes.Length; i++)
			{
				list.Add(attributes[i]);
			}
			return CreateForMember(member, allowEditable, InspectorPropertyInfoUtility.GetSerializationBackend(parentProperty, member), list);
		}

		public static InspectorPropertyInfo CreateForMember(InspectorProperty parentProperty, MemberInfo member, bool allowEditable, IEnumerable<Attribute> attributes)
		{
			return CreateForMember(member, allowEditable, InspectorPropertyInfoUtility.GetSerializationBackend(parentProperty, member), attributes.ToList());
		}

		public static InspectorPropertyInfo CreateForMember(MemberInfo member, bool allowEditable, SerializationBackend serializationBackend, params Attribute[] attributes)
		{
			List<Attribute> list = new List<Attribute>(attributes.Length);
			for (int i = 0; i < attributes.Length; i++)
			{
				list.Add(attributes[i]);
			}
			return CreateForMember(member, allowEditable, serializationBackend, list);
		}

		public static InspectorPropertyInfo CreateForMember(MemberInfo member, bool allowEditable, SerializationBackend serializationBackend, IEnumerable<Attribute> attributes)
		{
			return CreateForMember(member, allowEditable, serializationBackend, attributes.ToList());
		}

		public static InspectorPropertyInfo CreateForMember(MemberInfo member, bool allowEditable, SerializationBackend serializationBackend, List<Attribute> attributes)
		{
			if (member == null)
			{
				throw new ArgumentNullException("member");
			}
			if (!(member is FieldInfo) && !(member is PropertyInfo) && !(member is MethodInfo))
			{
				throw new ArgumentException("Can only create inspector properties for field, property and method members.");
			}
			if (member is MethodInfo && serializationBackend != SerializationBackend.None)
			{
				throw new ArgumentException("Serialization backend can only be None for method members.");
			}
			if (member is MethodInfo && allowEditable)
			{
				allowEditable = false;
			}
			if (allowEditable && member is FieldInfo && (member as FieldInfo).IsLiteral)
			{
				allowEditable = false;
			}
			string text = null;
			if (member is MethodInfo)
			{
				MethodInfo methodInfo = member as MethodInfo;
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length != 0)
				{
					text = methodInfo.GetNiceName();
				}
			}
			if (text == null)
			{
				text = member.Name;
			}
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '.')
				{
					int num = text.LastIndexOf(".") + 1;
					if (num < text.Length)
					{
						text = text.Substring(num);
						break;
					}
					throw new ArgumentException("A member name somehow had a '.' as the last character. This shouldn't be possible, but the '" + member.Name + "' has messed things up for everyone now. Good job!");
				}
			}
			InspectorPropertyInfo inspectorPropertyInfo = new InspectorPropertyInfo();
			if (member.IsDefined(typeof(OmitFromPrefabModificationPathsAttribute), inherit: false))
			{
				text = "#" + text;
			}
			inspectorPropertyInfo.memberInfos = new MemberInfo[1] { member };
			inspectorPropertyInfo.PropertyName = text;
			inspectorPropertyInfo.PropertyType = ((member is MethodInfo) ? PropertyType.Method : PropertyType.Value);
			inspectorPropertyInfo.SerializationBackend = serializationBackend ?? SerializationBackend.None;
			if (attributes == null)
			{
				inspectorPropertyInfo.attributes = new List<Attribute>();
			}
			else
			{
				inspectorPropertyInfo.attributes = attributes;
				for (int num2 = attributes.Count - 1; num2 >= 0; num2--)
				{
					Attribute attribute = attributes[num2];
					if (attribute == null)
					{
						attributes.RemoveAt(num2);
					}
					else
					{
						PropertyOrderAttribute propertyOrderAttribute = attribute as PropertyOrderAttribute;
						if (propertyOrderAttribute != null)
						{
							inspectorPropertyInfo.Order = propertyOrderAttribute.Order;
						}
					}
				}
			}
			inspectorPropertyInfo.typeOfOwner = member.DeclaringType;
			if (member is FieldInfo || member is PropertyInfo)
			{
				inspectorPropertyInfo.getterSetter = GetEmittedGetterSetterCreator(valueType: inspectorPropertyInfo.typeOfValue = member.GetReturnType(), ownerType: member.DeclaringType)(member, !allowEditable);
				inspectorPropertyInfo.IsEditable = allowEditable && !attributes.HasAttribute<ReadOnlyAttribute>() && !inspectorPropertyInfo.getterSetter.IsReadonly;
			}
			return inspectorPropertyInfo;
		}

		public static InspectorPropertyInfo CreateGroup(string name, Type typeOfOwner, float order, InspectorPropertyInfo[] groupInfos, params Attribute[] attributes)
		{
			return CreateGroup(name, typeOfOwner, order, groupInfos, (IEnumerable<Attribute>)attributes);
		}

		public static InspectorPropertyInfo CreateGroup(string name, Type typeOfOwner, float order, InspectorPropertyInfo[] groupInfos, IEnumerable<Attribute> attributes)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (typeOfOwner == null)
			{
				throw new ArgumentNullException("typeOfOwner");
			}
			if (groupInfos == null)
			{
				throw new ArgumentNullException("groupInfos");
			}
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == '.')
				{
					throw new ArgumentException("Group names or paths may not contain '.'; was given the path/name '" + name + "'.");
				}
			}
			if (name.Length == 0 || name[0] != '#')
			{
				throw new ArgumentException("The first character in a property group name must be '#'; was given the name '" + name + "'.");
			}
			InspectorPropertyInfo inspectorPropertyInfo = new InspectorPropertyInfo();
			int num = 0;
			for (int j = 0; j < groupInfos.Length; j++)
			{
				num += groupInfos[j].GetMemberInfos().Length;
			}
			inspectorPropertyInfo.memberInfos = new MemberInfo[num];
			num = 0;
			for (int k = 0; k < groupInfos.Length; k++)
			{
				MemberInfo[] array = groupInfos[k].GetMemberInfos();
				for (int l = 0; l < array.Length; l++)
				{
					inspectorPropertyInfo.memberInfos[num++] = array[l];
				}
			}
			if (attributes == null)
			{
				inspectorPropertyInfo.attributes = new List<Attribute>();
			}
			else if (attributes is List<Attribute>)
			{
				inspectorPropertyInfo.attributes = (List<Attribute>)attributes;
			}
			else
			{
				inspectorPropertyInfo.attributes = new List<Attribute>(attributes);
			}
			inspectorPropertyInfo.Order = order;
			inspectorPropertyInfo.typeOfOwner = typeOfOwner;
			inspectorPropertyInfo.PropertyName = name;
			inspectorPropertyInfo.PropertyType = PropertyType.Group;
			inspectorPropertyInfo.SerializationBackend = SerializationBackend.None;
			inspectorPropertyInfo.IsEditable = false;
			inspectorPropertyInfo.groupInfos = groupInfos;
			return inspectorPropertyInfo;
		}

		private InspectorPropertyInfo()
		{
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			if (PropertyType == PropertyType.Group)
			{
				return string.Concat(GetAttribute<PropertyGroupAttribute>().GroupID, " (type: ", PropertyType, ", order: ", Order, ")");
			}
			return string.Concat(PropertyName, " (type: ", PropertyType, ", backend: ", SerializationBackend, ", order: ", Order, ")");
		}

		/// <summary>
		/// Gets the first attribute of a given type on this property.
		/// </summary>
		public T GetAttribute<T>() where T : Attribute
		{
			if (attributes != null)
			{
				for (int i = 0; i < attributes.Count; i++)
				{
					T val = attributes[i] as T;
					if (val != null)
					{
						return val;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the first attribute of a given type on this property, which is not contained in a given hashset.
		/// </summary>
		/// <param name="exclude">The attributes to exclude.</param>
		public T GetAttribute<T>(HashSet<Attribute> exclude) where T : Attribute
		{
			if (attributes != null)
			{
				for (int i = 0; i < attributes.Count; i++)
				{
					T val = attributes[i] as T;
					if (val != null && (exclude == null || !exclude.Contains(val)))
					{
						return val;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Gets all attributes of a given type on the property.
		/// </summary>
		public IEnumerable<T> GetAttributes<T>() where T : Attribute
		{
			if (attributes == null)
			{
				yield break;
			}
			for (int i = 0; i < attributes.Count; i++)
			{
				T val = attributes[i] as T;
				if (val != null)
				{
					yield return val;
				}
			}
		}

		/// <summary>
		/// The <see cref="T:Sirenix.OdinInspector.Editor.InspectorPropertyInfo" />s of all the individual properties in this group.
		/// </summary>
		public InspectorPropertyInfo[] GetGroupInfos()
		{
			return groupInfos;
		}

		public MemberInfo GetMemberInfo()
		{
			if (memberInfos.Length != 0)
			{
				return memberInfos[0];
			}
			return null;
		}

		public MemberInfo[] GetMemberInfos()
		{
			return memberInfos;
		}

		public IValueGetterSetter GetGetterSetter()
		{
			return getterSetter;
		}

		/// <summary>
		/// Gets the property's method delegate, if there is one. Note that this is null if a method property is backed by an actual method member.
		/// </summary>
		public Delegate GetMethodDelegate()
		{
			return @delegate;
		}

		public bool TryGetStrongGetterSetter<TOwner, TValue>(out IValueGetterSetter<TOwner, TValue> result)
		{
			if (PropertyType != 0)
			{
				result = null;
				return false;
			}
			result = getterSetter as IValueGetterSetter<TOwner, TValue>;
			if (result != null)
			{
				return true;
			}
			result = (IValueGetterSetter<TOwner, TValue>)GetEmittedAliasGetterSetterCreator(typeof(TOwner), typeof(TValue), getterSetter.OwnerType, getterSetter.ValueType)(getterSetter);
			return result != null;
		}

		public List<Attribute> GetEditableAttributesList()
		{
			return attributes;
		}

		private static Func<MemberInfo, bool, IValueGetterSetter> GetEmittedGetterSetterCreator(Type ownerType, Type valueType)
		{
			if (!GetterSetterCreators.TryGetInnerValue(ownerType, valueType, out var value))
			{
				Type type = typeof(GetterSetter<, >).MakeGenericType(ownerType, valueType);
				ConstructorInfo constructor = type.GetConstructor(GetterSetterConstructorSignature);
				DynamicMethod dynamicMethod = new DynamicMethod("GetterSetterCreator<" + ownerType.GetNiceName() + ", " + valueType.GetNiceName() + ">", typeof(IValueGetterSetter), new Type[2]
				{
					typeof(MemberInfo),
					typeof(bool)
				});
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Ldarg_1);
				iLGenerator.Emit(OpCodes.Newobj, constructor);
				iLGenerator.Emit(OpCodes.Ret);
				value = (Func<MemberInfo, bool, IValueGetterSetter>)dynamicMethod.CreateDelegate(typeof(Func<MemberInfo, bool, IValueGetterSetter>));
				GetterSetterCreators.AddInner(ownerType, valueType, value);
			}
			return value;
		}

		private static Func<IValueGetterSetter, IValueGetterSetter> GetEmittedAliasGetterSetterCreator(Type ownerType, Type valueType, Type propertyOwnerType, Type propertyValueType)
		{
			TypeSignature4 key = new TypeSignature4(ownerType, valueType, propertyOwnerType, propertyValueType);
			if (!AliasGetterSetterCreators.TryGetValue(key, out var value))
			{
				Type type = typeof(AliasGetterSetter<, , , >).MakeGenericType(ownerType, valueType, propertyOwnerType, propertyValueType);
				AliasGetterSetterConstructorSignature[0] = typeof(IValueGetterSetter<, >).MakeGenericType(propertyOwnerType, propertyValueType);
				ConstructorInfo constructor = type.GetConstructor(AliasGetterSetterConstructorSignature);
				DynamicMethod dynamicMethod = new DynamicMethod("AliasGetterSetterCreator<" + ownerType.GetNiceName() + ", " + valueType.GetNiceName() + ">", typeof(IValueGetterSetter), new Type[1] { typeof(IValueGetterSetter) });
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				if (propertyOwnerType.IsAssignableFrom(ownerType) && propertyValueType.IsAssignableFrom(valueType))
				{
					iLGenerator.Emit(OpCodes.Ldarg_0);
					iLGenerator.Emit(OpCodes.Castclass, AliasGetterSetterConstructorSignature[0]);
					iLGenerator.Emit(OpCodes.Newobj, constructor);
					iLGenerator.Emit(OpCodes.Ret);
				}
				else
				{
					iLGenerator.Emit(OpCodes.Ldnull);
					iLGenerator.Emit(OpCodes.Ret);
				}
				value = (Func<IValueGetterSetter, IValueGetterSetter>)dynamicMethod.CreateDelegate(typeof(Func<IValueGetterSetter, IValueGetterSetter>));
				AliasGetterSetterCreators.Add(key, value);
			}
			return value;
		}

		public InspectorPropertyInfo CreateCopy()
		{
			InspectorPropertyInfo inspectorPropertyInfo = new InspectorPropertyInfo();
			inspectorPropertyInfo.memberInfos = memberInfos;
			inspectorPropertyInfo.attributes = attributes.ToList();
			inspectorPropertyInfo.typeOfOwner = typeOfOwner;
			inspectorPropertyInfo.typeOfValue = typeOfValue;
			inspectorPropertyInfo.getterSetter = getterSetter;
			inspectorPropertyInfo.groupInfos = groupInfos;
			inspectorPropertyInfo.isUnityPropertyOnly = isUnityPropertyOnly;
			inspectorPropertyInfo.@delegate = @delegate;
			inspectorPropertyInfo.PropertyName = PropertyName;
			inspectorPropertyInfo.PropertyType = PropertyType;
			inspectorPropertyInfo.SerializationBackend = SerializationBackend;
			inspectorPropertyInfo.IsEditable = IsEditable;
			inspectorPropertyInfo.Order = Order;
			return inspectorPropertyInfo;
		}

		internal void UpdateOrderFromAttributes()
		{
			if (attributes == null)
			{
				return;
			}
			for (int i = 0; i < attributes.Count; i++)
			{
				PropertyOrderAttribute propertyOrderAttribute = attributes[i] as PropertyOrderAttribute;
				if (propertyOrderAttribute != null)
				{
					Order = propertyOrderAttribute.Order;
				}
			}
		}
	}
}
