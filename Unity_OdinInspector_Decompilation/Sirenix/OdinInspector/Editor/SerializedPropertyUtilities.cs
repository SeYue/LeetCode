using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public static class SerializedPropertyUtilities
	{
		private static Dictionary<Type, Delegate> PrimitiveValueGetters = new Dictionary<Type, Delegate>
		{
			{
				typeof(int),
				(Func<SerializedProperty, int>)((SerializedProperty p) => p.get_intValue())
			},
			{
				typeof(bool),
				(Func<SerializedProperty, bool>)((SerializedProperty p) => p.get_boolValue())
			},
			{
				typeof(float),
				(Func<SerializedProperty, float>)((SerializedProperty p) => p.get_floatValue())
			},
			{
				typeof(string),
				(Func<SerializedProperty, string>)((SerializedProperty p) => p.get_stringValue())
			},
			{
				typeof(Color),
				(Func<SerializedProperty, Color>)((SerializedProperty p) => p.get_colorValue())
			},
			{
				typeof(LayerMask),
				(Func<SerializedProperty, LayerMask>)((SerializedProperty p) => LayerMask.op_Implicit(p.get_intValue()))
			},
			{
				typeof(Vector2),
				(Func<SerializedProperty, Vector2>)((SerializedProperty p) => p.get_vector2Value())
			},
			{
				typeof(Vector3),
				(Func<SerializedProperty, Vector3>)((SerializedProperty p) => p.get_vector3Value())
			},
			{
				typeof(Vector4),
				(Func<SerializedProperty, Vector4>)((SerializedProperty p) => p.get_vector4Value())
			},
			{
				typeof(Rect),
				(Func<SerializedProperty, Rect>)((SerializedProperty p) => p.get_rectValue())
			},
			{
				typeof(char),
				(Func<SerializedProperty, char>)((SerializedProperty p) => (char)p.get_intValue())
			},
			{
				typeof(AnimationCurve),
				(Func<SerializedProperty, AnimationCurve>)((SerializedProperty p) => p.get_animationCurveValue())
			},
			{
				typeof(Bounds),
				(Func<SerializedProperty, Bounds>)((SerializedProperty p) => p.get_boundsValue())
			},
			{
				typeof(Quaternion),
				(Func<SerializedProperty, Quaternion>)((SerializedProperty p) => p.get_quaternionValue())
			}
		};

		private static Dictionary<Type, Delegate> PrimitiveValueSetters = new Dictionary<Type, Delegate>
		{
			{
				typeof(int),
				(Action<SerializedProperty, int>)delegate(SerializedProperty p, int v)
				{
					p.set_intValue(v);
				}
			},
			{
				typeof(bool),
				(Action<SerializedProperty, bool>)delegate(SerializedProperty p, bool v)
				{
					p.set_boolValue(v);
				}
			},
			{
				typeof(float),
				(Action<SerializedProperty, float>)delegate(SerializedProperty p, float v)
				{
					p.set_floatValue(v);
				}
			},
			{
				typeof(string),
				(Action<SerializedProperty, string>)delegate(SerializedProperty p, string v)
				{
					p.set_stringValue(v);
				}
			},
			{
				typeof(Color),
				(Action<SerializedProperty, Color>)delegate(SerializedProperty p, Color v)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					p.set_colorValue(v);
				}
			},
			{
				typeof(LayerMask),
				(Action<SerializedProperty, LayerMask>)delegate(SerializedProperty p, LayerMask v)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					p.set_intValue(LayerMask.op_Implicit(v));
				}
			},
			{
				typeof(Vector2),
				(Action<SerializedProperty, Vector2>)delegate(SerializedProperty p, Vector2 v)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					p.set_vector2Value(v);
				}
			},
			{
				typeof(Vector3),
				(Action<SerializedProperty, Vector3>)delegate(SerializedProperty p, Vector3 v)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					p.set_vector3Value(v);
				}
			},
			{
				typeof(Vector4),
				(Action<SerializedProperty, Vector4>)delegate(SerializedProperty p, Vector4 v)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					p.set_vector4Value(v);
				}
			},
			{
				typeof(Rect),
				(Action<SerializedProperty, Rect>)delegate(SerializedProperty p, Rect v)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					p.set_rectValue(v);
				}
			},
			{
				typeof(char),
				(Action<SerializedProperty, char>)delegate(SerializedProperty p, char v)
				{
					p.set_intValue((int)v);
				}
			},
			{
				typeof(AnimationCurve),
				(Action<SerializedProperty, AnimationCurve>)delegate(SerializedProperty p, AnimationCurve v)
				{
					p.set_animationCurveValue(v);
				}
			},
			{
				typeof(Bounds),
				(Action<SerializedProperty, Bounds>)delegate(SerializedProperty p, Bounds v)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					p.set_boundsValue(v);
				}
			},
			{
				typeof(Quaternion),
				(Action<SerializedProperty, Quaternion>)delegate(SerializedProperty p, Quaternion v)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					p.set_quaternionValue(v);
				}
			}
		};

		private static Dictionary<string, Type> UnityTypes;

		private static Type GetUnityTypeWithName(string name)
		{
			if (UnityTypes == null)
			{
				UnityTypes = new Dictionary<string, Type>();
				foreach (Type item in from n in AssemblyUtilities.GetTypes(AssemblyTypeFlags.UnityTypes | AssemblyTypeFlags.UnityEditorTypes)
					where typeof(Object).IsAssignableFrom(n)
					select n)
				{
					if (UnityTypes.ContainsKey(item.Name))
					{
						UnityTypes[item.Name] = null;
					}
					else
					{
						UnityTypes[item.Name] = item;
					}
				}
			}
			UnityTypes.TryGetValue(name, out var value);
			return value;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static string GetProperTypeName(this SerializedProperty property)
		{
			if (property.get_type().StartsWith("PPtr<"))
			{
				return property.get_type().Substring(5).Trim('<', '>', '$');
			}
			return property.get_type();
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static bool IsCompatibleWithType(this SerializedProperty property, Type type)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Expected I4, but got Unknown
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			SerializedPropertyType propertyType = property.get_propertyType();
			switch (propertyType - -1)
			{
			case 0:
				return property.get_type() == type.Name;
			case 1:
				return type == typeof(int);
			case 2:
				return type == typeof(bool);
			case 3:
				return type == typeof(float);
			case 4:
				return type == typeof(string);
			case 5:
				return type == typeof(Color);
			case 6:
			{
				if (property.get_objectReferenceValue() != null)
				{
					return ((object)property.get_objectReferenceValue()).GetType().IsAssignableFrom(type);
				}
				string properTypeName = property.GetProperTypeName();
				if (properTypeName == "Prefab")
				{
					return type == typeof(GameObject);
				}
				return GetUnityTypeWithName(properTypeName)?.IsAssignableFrom(type) ?? false;
			}
			case 7:
				return type == typeof(LayerMask);
			case 8:
			{
				if (!type.IsEnum)
				{
					return false;
				}
				string[] names = Enum.GetNames(type);
				string[] enumNames = property.get_enumNames();
				if (names.Length != enumNames.Length)
				{
					return false;
				}
				for (int i = 0; i < names.Length; i++)
				{
					if (!string.Equals(names[i], enumNames[i].Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				return true;
			}
			case 9:
				return type == typeof(Vector2);
			case 10:
				return type == typeof(Vector3);
			case 11:
				return type == typeof(Vector4);
			case 12:
				return type == typeof(Rect);
			case 13:
				return false;
			case 14:
				return type == typeof(char);
			case 15:
				return type == typeof(AnimationCurve);
			case 16:
				return type == typeof(Bounds);
			case 17:
				return type == typeof(Gradient);
			case 18:
				return type == typeof(Quaternion);
			default:
				return false;
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Type GuessContainedType(this SerializedProperty property)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected I4, but got Unknown
			SerializedPropertyType propertyType = property.get_propertyType();
			switch (propertyType - -1)
			{
			case 0:
				return null;
			case 1:
				return typeof(int);
			case 2:
				return typeof(bool);
			case 3:
				return typeof(float);
			case 4:
				return typeof(string);
			case 5:
				return typeof(Color);
			case 6:
			{
				if (property.get_objectReferenceValue() != null)
				{
					return ((object)property.get_objectReferenceValue()).GetType();
				}
				string typeName = property.GetProperTypeName();
				List<Type> list = (from n in AssemblyUtilities.GetTypes(AssemblyTypeFlags.UnityTypes | AssemblyTypeFlags.UnityEditorTypes)
					where n.Name == typeName && typeof(Object).IsAssignableFrom(n)
					select n).ToList();
				if (list.Count == 1)
				{
					return list[0];
				}
				return null;
			}
			case 7:
				return typeof(LayerMask);
			case 8:
				return null;
			case 9:
				return typeof(Vector2);
			case 10:
				return typeof(Vector3);
			case 11:
				return typeof(Vector4);
			case 12:
				return typeof(Rect);
			case 13:
				return null;
			case 14:
				return typeof(char);
			case 15:
				return typeof(AnimationCurve);
			case 16:
				return typeof(Bounds);
			case 17:
				return typeof(Gradient);
			case 18:
				return typeof(Quaternion);
			default:
				return null;
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static bool CanSetGetValue(Type type)
		{
			if (typeof(Object).IsAssignableFrom(type))
			{
				return true;
			}
			return PrimitiveValueGetters.ContainsKey(type);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Func<SerializedProperty, T> GetValueGetter<T>()
		{
			if (typeof(Object).IsAssignableFrom(typeof(T)))
			{
				return (SerializedProperty p) => (T)(object)p.get_objectReferenceValue();
			}
			PrimitiveValueGetters.TryGetValue(typeof(T), out var value);
			return (Func<SerializedProperty, T>)value;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Action<SerializedProperty, T> GetValueSetter<T>()
		{
			if (typeof(Object).IsAssignableFrom(typeof(T)))
			{
				return delegate(SerializedProperty p, T v)
				{
					//IL_0007: Unknown result type (might be due to invalid IL or missing references)
					//IL_0011: Expected O, but got Unknown
					p.set_objectReferenceValue((Object)(object)v);
				};
			}
			PrimitiveValueSetters.TryGetValue(typeof(T), out var value);
			return (Action<SerializedProperty, T>)value;
		}
	}
}
