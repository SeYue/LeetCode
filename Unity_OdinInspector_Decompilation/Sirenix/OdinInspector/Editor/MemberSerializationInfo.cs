using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Sirenix.OdinInspector.Internal;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Sirenix.OdinInspector.Editor
{
	internal class MemberSerializationInfo
	{
		public readonly string[] Notes;

		public readonly MemberInfo MemberInfo;

		public readonly SerializationFlags Info;

		public readonly SerializationBackendFlags Backend;

		public readonly InfoMessageType OdinMessageType;

		public readonly InfoMessageType UnityMessageType;

		private MemberSerializationInfo(MemberInfo member, string[] notes, SerializationFlags flags, SerializationBackendFlags serializationBackend)
		{
			MemberInfo = member;
			Notes = notes;
			Info = flags;
			Backend = serializationBackend;
			OdinMessageType = InfoMessageType.None;
			UnityMessageType = InfoMessageType.None;
			if (flags.HasAll(SerializationFlags.DefaultSerializationPolicy) && flags.HasNone(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin | SerializationFlags.NonSerializedAttribute) && flags.HasAny(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute) && (flags.HasAll(SerializationFlags.Field) || flags.HasAll(SerializationFlags.AutoProperty)))
			{
				if (serializationBackend.HasNone(SerializationBackendFlags.Odin))
				{
					OdinMessageType = InfoMessageType.Info;
				}
				if (flags.HasNone(SerializationFlags.Property) && !UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
				{
					UnityMessageType = InfoMessageType.Info;
				}
			}
			if (Info.HasAny(SerializationFlags.SerializedByOdin) && Info.HasAny(SerializationFlags.SerializedByUnity))
			{
				OdinMessageType = InfoMessageType.Warning;
				UnityMessageType = InfoMessageType.Warning;
			}
			if (Info.HasAll(SerializationFlags.SerializeFieldAttribute | SerializationFlags.NonSerializedAttribute))
			{
				UnityMessageType = InfoMessageType.Warning;
			}
			if (Info.HasAll(SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute))
			{
				if (Info.HasAll(SerializationFlags.SerializedByOdin))
				{
					OdinMessageType = InfoMessageType.Warning;
				}
				if (Info.HasAll(SerializationFlags.SerializedByUnity))
				{
					UnityMessageType = InfoMessageType.Warning;
				}
			}
			if (serializationBackend.HasAll(SerializationBackendFlags.UnityAndOdin) && Info.HasAll(SerializationFlags.SerializedByOdin | SerializationFlags.TypeSupportedByUnity) && Info.HasNone(SerializationFlags.Property | SerializationFlags.SerializedByUnity))
			{
				OdinMessageType = InfoMessageType.Warning;
			}
			if (!serializationBackend.HasAll(SerializationBackendFlags.Odin) && flags.HasAny(SerializationFlags.OdinSerializeAttribute))
			{
				OdinMessageType = InfoMessageType.Error;
			}
			if (Info.HasAll(SerializationFlags.DefaultSerializationPolicy) && Info.HasAny(SerializationFlags.OdinSerializeAttribute) && !Info.HasAny(SerializationFlags.SerializedByOdin))
			{
				OdinMessageType = InfoMessageType.Error;
			}
			if (Info.HasAny(SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute) && !Info.HasAny(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin | SerializationFlags.NonSerializedAttribute))
			{
				if (serializationBackend.HasAll(SerializationBackendFlags.Odin))
				{
					OdinMessageType = InfoMessageType.Error;
				}
				if (!Info.HasAny(SerializationFlags.Property) && UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
				{
					UnityMessageType = InfoMessageType.Error;
				}
			}
			if (Info.HasAll(SerializationFlags.Public | SerializationFlags.Field) && !Info.HasAny(SerializationFlags.NonSerializedAttribute) && !Info.HasAny(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin))
			{
				if (Info.HasAll(SerializationFlags.DefaultSerializationPolicy) && serializationBackend.HasAll(SerializationBackendFlags.Odin))
				{
					OdinMessageType = InfoMessageType.Error;
				}
				if (!Info.HasAny(SerializationFlags.Property) && UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
				{
					UnityMessageType = InfoMessageType.Error;
				}
			}
		}

		public static List<MemberSerializationInfo> CreateSerializationOverview(Type type, SerializationBackendFlags serializationBackend, bool includeBaseTypes)
		{
			bool serializeUnityFields;
			ISerializationPolicy serializationPolicy = GetSerializationPolicy(type, out serializeUnityFields);
			return (from x in type.GetAllMembers(includeBaseTypes ? (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) : (BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
				where x is FieldInfo || x is PropertyInfo
				where !x.Name.StartsWith("<")
				where (x.DeclaringType.Assembly.GetAssemblyTypeFlag() & (AssemblyTypeFlags.UnityTypes | AssemblyTypeFlags.UnityEditorTypes)) == 0
				where !x.DeclaringType.Assembly.FullName.StartsWith("Sirenix.")
				select CreateInfoFor(x, serializationBackend, serializeUnityFields, serializationPolicy) into x
				orderby x.OdinMessageType == InfoMessageType.Error descending, x.UnityMessageType == InfoMessageType.Error descending, x.OdinMessageType == InfoMessageType.Warning descending, x.UnityMessageType == InfoMessageType.Warning descending, x.OdinMessageType == InfoMessageType.Info descending, x.UnityMessageType == InfoMessageType.Info descending, x.Info.HasAny(SerializationFlags.SerializedByOdin) descending, x.Info.HasAny(SerializationFlags.SerializedByUnity) descending, x.MemberInfo.Name descending
				select x).ToList();
		}

		private static ISerializationPolicy GetSerializationPolicy(Type type, out bool serializeUnityFields)
		{
			serializeUnityFields = false;
			if (!typeof(IOverridesSerializationPolicy).IsAssignableFrom(type))
			{
				return SerializationPolicies.Unity;
			}
			IOverridesSerializationPolicy overridesSerializationPolicy = null;
			Object[] array = Resources.FindObjectsOfTypeAll(type);
			foreach (Object val in array)
			{
				if (val != null)
				{
					overridesSerializationPolicy = val as IOverridesSerializationPolicy;
					break;
				}
			}
			if (!type.IsAbstract && !type.IsGenericTypeDefinition)
			{
				object uninitializedObject = FormatterServices.GetUninitializedObject(type);
				overridesSerializationPolicy = uninitializedObject as IOverridesSerializationPolicy;
			}
			if (overridesSerializationPolicy != null)
			{
				serializeUnityFields = overridesSerializationPolicy.OdinSerializesUnityFields;
				return overridesSerializationPolicy.SerializationPolicy ?? SerializationPolicies.Unity;
			}
			return SerializationPolicies.Unity;
		}

		private static MemberSerializationInfo CreateInfoFor(MemberInfo member, SerializationBackendFlags serializationBackend, bool serializeUnityFields, ISerializationPolicy serializationPolicy)
		{
			SerializationFlags serializationFlags = (SerializationFlags)0;
			if (member is FieldInfo)
			{
				FieldInfo fieldInfo = member as FieldInfo;
				serializationFlags |= SerializationFlags.Field;
				if (fieldInfo.IsPublic)
				{
					serializationFlags |= SerializationFlags.Public;
				}
			}
			else if (member is PropertyInfo)
			{
				PropertyInfo propertyInfo = member as PropertyInfo;
				serializationFlags |= SerializationFlags.Property;
				if ((propertyInfo.GetGetMethod() != null && propertyInfo.GetGetMethod().IsPublic) || (propertyInfo.GetSetMethod() != null && propertyInfo.GetSetMethod().IsPublic))
				{
					serializationFlags |= SerializationFlags.Public;
				}
				if (propertyInfo.IsAutoProperty(allowVirtual: true))
				{
					serializationFlags |= SerializationFlags.AutoProperty;
				}
			}
			if (serializationPolicy != null && serializationPolicy.ID == SerializationPolicies.Unity.ID)
			{
				serializationFlags |= SerializationFlags.DefaultSerializationPolicy;
			}
			if ((serializationBackend & SerializationBackendFlags.Unity) != 0 && UnitySerializationUtility.GuessIfUnityWillSerialize(member))
			{
				serializationFlags |= SerializationFlags.SerializedByUnity;
			}
			if ((serializationBackend & SerializationBackendFlags.Odin) != 0 && UnitySerializationUtility.OdinWillSerialize(member, serializeUnityFields, serializationPolicy))
			{
				serializationFlags |= SerializationFlags.SerializedByOdin;
			}
			if (((ICustomAttributeProvider)member).IsDefined<SerializeField>())
			{
				serializationFlags |= SerializationFlags.SerializeFieldAttribute;
			}
			if (member.IsDefined<OdinSerializeAttribute>())
			{
				serializationFlags |= SerializationFlags.OdinSerializeAttribute;
			}
			if (member.IsDefined<NonSerializedAttribute>())
			{
				serializationFlags |= SerializationFlags.NonSerializedAttribute;
			}
			if (serializationBackend.HasAll(SerializationBackendFlags.Unity) && UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
			{
				serializationFlags |= SerializationFlags.TypeSupportedByUnity;
			}
			return new MemberSerializationInfo(member, CreateNotes(member, serializationFlags, serializationBackend, serializationPolicy.ID), serializationFlags, serializationBackend);
		}

		private static string[] CreateNotes(MemberInfo member, SerializationFlags flags, SerializationBackendFlags serializationBackend, string serializationPolicyId)
		{
			List<string> list = new List<string>();
			StringBuilder stringBuilder = new StringBuilder();
			if (serializationBackend.HasNone(SerializationBackendFlags.Odin) || flags.HasAll(SerializationFlags.DefaultSerializationPolicy))
			{
				if (flags.HasAll(SerializationFlags.Property | SerializationFlags.AutoProperty))
				{
					stringBuilder.AppendFormat("The auto property '{0}' ", member.GetNiceName());
				}
				else if (flags.HasAll(SerializationFlags.Property))
				{
					stringBuilder.AppendFormat("The non-auto property '{0}' ", member.GetNiceName());
				}
				else if (flags.HasAll(SerializationFlags.Public))
				{
					stringBuilder.AppendFormat("The public field '{0}' ", member.GetNiceName());
				}
				else
				{
					stringBuilder.AppendFormat("The field '{0}' ", member.GetNiceName());
				}
				if (flags.HasAny(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin))
				{
					stringBuilder.Append("is serialized by ");
					if (flags.HasAll(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin))
					{
						stringBuilder.Append("both Unity and Odin ");
					}
					else if (flags.HasAll(SerializationFlags.SerializedByUnity))
					{
						stringBuilder.Append("Unity ");
					}
					else
					{
						stringBuilder.Append("Odin ");
					}
					stringBuilder.Append("because ");
					SerializationFlags serializationFlags = flags & (SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.NonSerializedAttribute);
					if (flags.HasAll(SerializationFlags.OdinSerializeAttribute) && serializationBackend.HasAll(SerializationBackendFlags.Odin))
					{
						serializationFlags |= SerializationFlags.OdinSerializeAttribute;
					}
					if (flags.HasAll(SerializationFlags.Public | SerializationFlags.Property))
					{
						serializationFlags &= ~SerializationFlags.Public;
					}
					switch (serializationFlags)
					{
					case SerializationFlags.Public:
						stringBuilder.Append("its access modifier is public. ");
						break;
					case SerializationFlags.SerializeFieldAttribute:
						stringBuilder.Append("the [SerializeField] attribute is defined. ");
						break;
					case SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute:
						stringBuilder.Append("the [SerializeField] attribute is defined, and it's public. ");
						break;
					case SerializationFlags.OdinSerializeAttribute:
						stringBuilder.Append("the [OdinSerialize] attribute is defined. ");
						break;
					case SerializationFlags.Public | SerializationFlags.OdinSerializeAttribute:
						stringBuilder.Append("the [OdinSerialize] attribute is defined, and it's public.");
						break;
					case SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute:
						stringBuilder.Append("the [SerializeField] and [OdinSerialize] attributes are defined. ");
						break;
					case SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute:
						stringBuilder.Append("its access modifier is public and the [SerializeField] and [OdinSerialize] attribute are defined. ");
						break;
					case SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute:
					case SerializationFlags.Public | SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute:
					case SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute:
					case SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute | SerializationFlags.NonSerializedAttribute:
						stringBuilder.Append("the [OdinSerialize] and [NonSerialized] attribute are defined. ");
						break;
					default:
						stringBuilder.Append("(MISSING CASE: " + serializationFlags.ToString() + ")");
						break;
					}
					if (stringBuilder.Length > 0)
					{
						list.Add(stringBuilder.ToString());
						stringBuilder.Length = 0;
					}
					if (serializationBackend.HasAll(SerializationBackendFlags.Unity) && flags.HasNone(SerializationFlags.SerializedByUnity))
					{
						stringBuilder.Append("The member is not being serialized by Unity since ");
						if (flags.HasAll(SerializationFlags.Property))
						{
							stringBuilder.Append("Unity does not serialize properties.");
						}
						else if (!UnitySerializationUtility.GuessIfUnityWillSerialize(member.GetReturnType()))
						{
							stringBuilder.Append("Unity does not support the type.");
						}
						else if (flags.HasAll(SerializationFlags.NonSerializedAttribute))
						{
							stringBuilder.Append("the [NonSerialized] attribute is defined.");
						}
						else if (!flags.HasAny(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute))
						{
							stringBuilder.Append("it is neither a public field nor does it have the [SerializeField] attribute.");
						}
						else
						{
							stringBuilder.Append("# Missing case, please report: " + flags);
						}
					}
					if (stringBuilder.Length > 0)
					{
						list.Add(stringBuilder.ToString());
						stringBuilder.Length = 0;
					}
					if (!flags.HasAll(SerializationFlags.SerializedByOdin))
					{
						stringBuilder.Append("Member is not serialized by Odin because ");
						if ((serializationBackend & SerializationBackendFlags.Odin) != 0)
						{
							if (flags.HasAll(SerializationFlags.SerializedByUnity))
							{
								stringBuilder.Append("the member is already serialized by Unity. ");
							}
						}
						else
						{
							stringBuilder.Append("Odin serialization is not implemented. ");
							if (flags.HasAll(SerializationFlags.OdinSerializeAttribute))
							{
								stringBuilder.Append("The use of [OdinSerialize] attribute is invalid.");
							}
						}
					}
				}
				else
				{
					if (flags.HasAll(SerializationFlags.Property) && serializationBackend.HasAll(SerializationBackendFlags.Odin))
					{
						stringBuilder.Append("is skipped by Odin because ");
						PropertyInfo propertyInfo = member as PropertyInfo;
						if (propertyInfo.GetGetMethod(nonPublic: true) == null)
						{
							stringBuilder.Append("the property has no getter. ");
						}
						else if (propertyInfo.GetSetMethod(nonPublic: true) == null)
						{
							stringBuilder.Append("the property has no setter. ");
						}
						else if (flags.HasNone(SerializationFlags.OdinSerializeAttribute))
						{
							stringBuilder.Append("the [OdinSerialize] attribute has not been applied to it. ");
						}
						else
						{
							stringBuilder.Append("MISSING CASE (please report). ");
						}
						if (flags.HasAll(SerializationFlags.NonSerializedAttribute))
						{
							stringBuilder.Append("( Note: the [NonSerialized] attribute is unnecessary. ) ");
						}
					}
					else if (flags.HasAll(SerializationFlags.Property))
					{
						stringBuilder.Append("is skipped by Unity because Unity does not serialize properties. ");
						if (flags.HasAll(SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute))
						{
							stringBuilder.Append("The use of [SerializeField] and [OdinSerialize] attributes is invalid. ");
						}
						else if (flags.HasAll(SerializationFlags.OdinSerializeAttribute))
						{
							stringBuilder.Append("The use of [OdinSerialize] attribute is invalid. ");
						}
						else if (flags.HasAny(SerializationFlags.SerializeFieldAttribute))
						{
							stringBuilder.Append("The use of [SerializeField] attribute is invalid. ");
						}
						if (flags.HasAny(SerializationFlags.NonSerializedAttribute))
						{
							stringBuilder.Append("The use of [NonSerialized] attribute is unnecessary.");
						}
					}
					else
					{
						stringBuilder.Append("is skipped by ");
						switch (serializationBackend)
						{
						case SerializationBackendFlags.Unity:
							stringBuilder.Append("Unity ");
							break;
						case SerializationBackendFlags.Odin:
							stringBuilder.Append("Odin ");
							break;
						case SerializationBackendFlags.UnityAndOdin:
							stringBuilder.Append("both Unity and Odin ");
							break;
						}
						stringBuilder.Append("because ");
						if (serializationBackend == SerializationBackendFlags.None)
						{
							stringBuilder.Append("there is no serialization backend? ");
						}
						else if (flags.HasAll(SerializationFlags.NonSerializedAttribute))
						{
							stringBuilder.Append("the [NonSerialized] attribute is defined. ");
						}
						else if (serializationBackend.HasAll(SerializationBackendFlags.UnityAndOdin) && flags.HasNone(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute))
						{
							stringBuilder.Append("the field is neither public nor is a [SerializeField] or [OdinSerialize] attribute defined. ");
						}
						else if (flags.HasNone(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute))
						{
							stringBuilder.Append("the field is neither public nor is the [SerializeField] attribute defined. ");
						}
						else if (serializationBackend == SerializationBackendFlags.Unity && flags.HasAny(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute))
						{
							stringBuilder.Append("Unity does not support the type " + member.GetReturnType().GetNiceName());
						}
						if (stringBuilder.Length > 0)
						{
							list.Add(stringBuilder.ToString());
							stringBuilder.Length = 0;
						}
						if ((serializationBackend & SerializationBackendFlags.Odin) == 0 && flags.HasAll(SerializationFlags.OdinSerializeAttribute))
						{
							list.Add("Odin serialization is not implemented. The use of [OdinSerialize] attribute is invalid.");
						}
					}
					if (flags.HasAll(SerializationFlags.SerializeFieldAttribute | SerializationFlags.NonSerializedAttribute) && flags.HasNone(SerializationFlags.OdinSerializeAttribute))
					{
						list.Add("Use of [SerializeField] along with [NonSerialized] attributes is weird. Remove either the [SerializeField] or [NonSerialized] attribute.");
					}
				}
			}
			else
			{
				if (flags.HasAll(SerializationFlags.AutoProperty))
				{
					stringBuilder.Append(flags.HasAll(SerializationFlags.Public) ? "The public auto property " : "The auto property ");
				}
				else if (flags.HasAll(SerializationFlags.Property))
				{
					stringBuilder.Append(flags.HasAll(SerializationFlags.Public) ? "The public property " : "The property ");
				}
				else
				{
					stringBuilder.Append(flags.HasAll(SerializationFlags.Public) ? "The public field " : "The field ");
				}
				stringBuilder.AppendFormat("'{0}' ", member.GetNiceName());
				if (flags.HasAll(SerializationFlags.SerializedByUnity))
				{
					stringBuilder.Append("is serialized by Unity since ");
					if (flags.HasAll(SerializationFlags.Field))
					{
						switch (flags & (SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute))
						{
						case SerializationFlags.Public:
							stringBuilder.Append("its access modifier is public. ");
							break;
						case SerializationFlags.SerializeFieldAttribute:
							stringBuilder.Append("the [SerializeField] attribute is defined. ");
							break;
						case SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute:
							stringBuilder.Append("the [SerializeField] attribute is defined, and it's public. ");
							break;
						default:
							stringBuilder.Append("(MISSING CASE: " + flags.ToString() + ")");
							break;
						}
					}
					else
					{
						stringBuilder.Append("is serialized by Unity? Unity should not be serializing any properties? ");
					}
				}
				else
				{
					stringBuilder.Append("is skipped by Unity ");
					if (flags.HasAll(SerializationFlags.Field))
					{
						if (flags.HasNone(SerializationFlags.TypeSupportedByUnity))
						{
							stringBuilder.Append("because Unity does not support the type " + member.GetReturnType().GetNiceName());
						}
						else if (flags.HasAll(SerializationFlags.NonSerializedAttribute))
						{
							stringBuilder.Append("because the [NonSerialized] attribute is defined. ");
						}
						else if (flags.HasNone(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute))
						{
							stringBuilder.Append("because the field is not public nor is the [SerializeField] attribute is not defined. ");
						}
						else
						{
							stringBuilder.Append("(MISSING CASE: " + flags.ToString() + ")");
						}
					}
					else
					{
						stringBuilder.Append("because Unity does not serialize properties. ");
					}
				}
				if (stringBuilder.Length > 0)
				{
					list.Add(stringBuilder.ToString());
					stringBuilder.Length = 0;
				}
				if (flags.HasAll(SerializationFlags.AutoProperty))
				{
					stringBuilder.Append("The auto property ");
				}
				else if (flags.HasAll(SerializationFlags.Property))
				{
					stringBuilder.Append("The property ");
				}
				else
				{
					stringBuilder.Append("The field ");
				}
				if (flags.HasAll(SerializationFlags.SerializedByOdin))
				{
					stringBuilder.Append("is serialized by Odin because of custom serialization policy: " + serializationPolicyId);
				}
				else
				{
					stringBuilder.Append("is skipped by Odin because of custom serialization policy: " + serializationPolicyId);
				}
				if (stringBuilder.Length > 0)
				{
					list.Add(stringBuilder.ToString());
					stringBuilder.Length = 0;
				}
				if (flags.HasAll(SerializationFlags.SerializedByUnity | SerializationFlags.SerializedByOdin))
				{
					list.Add("The member is serialized by both Unity and Odin. Consider ensuring that only one serializer is in use.");
				}
			}
			if (stringBuilder.Length > 0)
			{
				list.Add(stringBuilder.ToString());
				stringBuilder.Length = 0;
			}
			if (serializationBackend.HasAll(SerializationBackendFlags.UnityAndOdin))
			{
				if (flags.HasAll(SerializationFlags.SerializedByOdin | SerializationFlags.TypeSupportedByUnity) && flags.HasNone(SerializationFlags.Property | SerializationFlags.SerializedByUnity))
				{
					stringBuilder.Append("The type '" + member.GetReturnType().GetNiceName() + "' appears to be supported by Unity. Are you certain that you want to use Odin for serializing it?");
				}
				else if (flags.HasAll(SerializationFlags.SerializedByOdin) && flags.HasNone(SerializationFlags.TypeSupportedByUnity))
				{
					stringBuilder.Append("The type '" + member.GetReturnType().GetNiceName() + "' is not supported by Unity" + GuessWhyUnityDoesNotSupport(member.GetReturnType()));
				}
			}
			else if (serializationBackend.HasAll(SerializationBackendFlags.Unity) && flags.HasNone(SerializationFlags.TypeSupportedByUnity))
			{
				stringBuilder.Append("The type '" + member.GetReturnType().GetNiceName() + "' is not supported by Unity" + GuessWhyUnityDoesNotSupport(member.GetReturnType()));
			}
			if (stringBuilder.Length > 0)
			{
				list.Add(stringBuilder.ToString());
				stringBuilder.Length = 0;
			}
			if (serializationBackend.HasAll(SerializationBackendFlags.Unity) && serializationBackend.HasNone(SerializationBackendFlags.Odin) && flags.HasNone(SerializationFlags.TypeSupportedByUnity) && flags.HasAny(SerializationFlags.Public | SerializationFlags.SerializeFieldAttribute | SerializationFlags.OdinSerializeAttribute) && flags.HasAny(SerializationFlags.Field | SerializationFlags.AutoProperty))
			{
				string text = "You could implement Odin serializing by inheriting " + member.DeclaringType.GetNiceName() + " from ";
				if (typeof(MonoBehaviour).IsAssignableFrom(member.DeclaringType))
				{
					stringBuilder.Append(text + typeof(SerializedMonoBehaviour).GetNiceName());
				}
				else if (UnityNetworkingUtility.NetworkBehaviourType != null && UnityNetworkingUtility.NetworkBehaviourType.IsAssignableFrom(member.DeclaringType))
				{
					stringBuilder.Append(text + " SerializedNetworkBehaviour");
				}
				else if (typeof(Behaviour).IsAssignableFrom(member.DeclaringType))
				{
					stringBuilder.Append(text + typeof(SerializedBehaviour).GetNiceName());
				}
				else if (typeof(ScriptableObject).IsAssignableFrom(member.DeclaringType))
				{
					stringBuilder.Append(text + typeof(SerializedScriptableObject).GetNiceName());
				}
			}
			if (stringBuilder.Length > 0)
			{
				list.Add(stringBuilder.ToString());
				stringBuilder.Length = 0;
			}
			if (serializationBackend.HasAll(SerializationBackendFlags.Odin) && flags.HasAll(SerializationFlags.Property | SerializationFlags.SerializedByOdin))
			{
				stringBuilder.Append("It's recommended to use backing fields for serialization instead of properties.");
			}
			if (stringBuilder.Length > 0)
			{
				list.Add(stringBuilder.ToString());
				stringBuilder.Length = 0;
			}
			return list.ToArray();
		}

		private static string GuessWhyUnityDoesNotSupport(Type type)
		{
			if (type == typeof(Coroutine))
			{
				return " because Unity will never serialize Coroutines.";
			}
			if (typeof(Delegate).IsAssignableFrom(type))
			{
				return " because Unity does not support delegates.";
			}
			if (type.IsInterface)
			{
				return " because the type is an interface.";
			}
			if (type.IsAbstract)
			{
				return " because the type is abstract.";
			}
			if (type == typeof(object))
			{
				return " because Unity does not support serializing System.Object.";
			}
			if (typeof(Enum).IsAssignableFrom(type))
			{
				Type underlyingType = Enum.GetUnderlyingType(type);
				if (UnityVersion.IsVersionOrGreater(5, 6) && (underlyingType == typeof(long) || underlyingType == typeof(ulong)))
				{
					return " because Unity does not support enums with underlying type of long or ulong.";
				}
				if (UnityVersion.Major <= 5 && UnityVersion.Minor < 6 && underlyingType != typeof(int) && underlyingType != typeof(byte))
				{
					return " because prior to Version 5.6 Unity only supports enums with underlying type of int or byte.";
				}
				return ". Was unable to determine why Unity does not support enum with underlying type of: " + underlyingType.GetNiceName() + ".";
			}
			if (typeof(UnityEventBase).IsAssignableFrom(type) && type.IsGenericType)
			{
				return " because the type is a generic implementation of UnityEventBase.";
			}
			if (type.IsArray)
			{
				if (type.GetArrayRank() > 1 || type.GetElementType().IsArray || type.GetElementType().ImplementsOpenGenericClass(typeof(List<>)))
				{
					return " because Unity does not support multi-dimensional arrays.";
				}
				if (!UnitySerializationUtility.GuessIfUnityWillSerialize(type.GetElementType()))
				{
					return " because Unity does not support the type " + type.GetElementType().GetNiceName() + " as an array element.";
				}
			}
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				Type type2 = type.GetArgumentsOfInheritedOpenGenericClass(typeof(List<>))[0];
				if (type2.IsArray)
				{
					return " because Unity does not support Lists of arrays.";
				}
				if (type2.ImplementsOpenGenericClass(typeof(List<>)))
				{
					return " because Unity does not support Lists of Lists.";
				}
				if (!UnitySerializationUtility.GuessIfUnityWillSerialize(type2))
				{
					return " because Unity does not support the element type of " + type2.GetNiceName() + ".";
				}
			}
			if (type.IsGenericType || type.GetGenericArguments().Length != 0)
			{
				return " because Unity does not support generic types.";
			}
			if (type.Assembly == typeof(string).Assembly)
			{
				return " because Unity does not serialize [Serializable] structs and classes if they are defined in mscorlib.";
			}
			if (!type.IsDefined<SerializableAttribute>(inherit: false))
			{
				return " because the type is missing a [Serializable] attribute.";
			}
			return ". Was unable to determine reason, please report this to Sirenix.";
		}
	}
}
