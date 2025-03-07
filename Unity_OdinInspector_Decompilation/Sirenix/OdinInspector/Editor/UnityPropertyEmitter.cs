using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Provides utilities for emitting ScriptableObject and MonoBehaviour-derived types with specific property names and types, and providing instances of <see cref="T:UnityEditor.SerializedProperty" /> with those names and types.
	/// </summary>
	public static class UnityPropertyEmitter
	{
		/// <summary>
		/// A handle for a set of emitted Unity objects. When disposed (or collected by the GC) this handle will queue the emitted object instances for destruction.
		/// </summary>
		public class Handle : IDisposable
		{
			/// <summary>
			/// The unity property to represent.
			/// </summary>
			public readonly SerializedProperty UnityProperty;

			/// <summary>
			/// The Unity objects to represent.
			/// </summary>
			public readonly Object[] Objects;

			private int disposed;

			/// <summary>
			/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.UnityPropertyEmitter.Handle" /> class.
			/// </summary>
			/// <param name="unityProperty">The unity property to represent.</param>
			/// <param name="objects">The objects to represent.</param>
			public Handle(SerializedProperty unityProperty, Object[] objects)
			{
				UnityProperty = unityProperty;
				Objects = objects;
			}

			/// <summary>
			/// Finalizes an instance of the <see cref="T:Sirenix.OdinInspector.Editor.UnityPropertyEmitter.Handle" /> class.
			/// </summary>
			~Handle()
			{
				Dispose();
			}

			public void Dispose()
			{
				if (Interlocked.Increment(ref disposed) == 1)
				{
					lock (MarkedForDestruction_LOCK)
					{
						MarkedForDestruction.AddRange(Objects);
					}
				}
			}
		}

		public const string EMIT_ASSEMBLY_NAME = "Sirenix.OdinInspector.EmittedUnityProperties";

		public const string HOST_GO_NAME = "ODIN_EMIT_HOST_GO_ac922281-4f8a-4e1b-8a45-65af1a8350b3";

		public const HideFlags HOST_GO_HIDE_FLAGS = 29;

		private static AssemblyBuilder emittedAssembly;

		private static ModuleBuilder emittedModule;

		private static readonly Dictionary<Type, Type> PreCreatedScriptableObjectTypes;

		private static readonly DoubleLookupDictionary<string, Type, Type> MonoBehaviourTypeCache;

		private static readonly DoubleLookupDictionary<string, Type, Type> ScriptableObjectTypeCache;

		private static GameObject hostGO;

		private static readonly object MarkedForDestruction_LOCK;

		private static readonly List<Object> MarkedForDestruction;

		private static GameObject HostGO
		{
			get
			{
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Expected O, but got Unknown
				if ((Object)(object)hostGO == (Object)null)
				{
					hostGO = GameObject.Find("ODIN_EMIT_HOST_GO_ac922281-4f8a-4e1b-8a45-65af1a8350b3");
					if ((Object)(object)hostGO == (Object)null)
					{
						hostGO = new GameObject("ODIN_EMIT_HOST_GO_ac922281-4f8a-4e1b-8a45-65af1a8350b3");
						((Object)hostGO).set_hideFlags((HideFlags)29);
					}
				}
				return hostGO;
			}
		}

		static UnityPropertyEmitter()
		{
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Expected O, but got Unknown
			PreCreatedScriptableObjectTypes = new Dictionary<Type, Type>
			{
				{
					typeof(AnimationCurve),
					typeof(EmittedAnimationCurveContainer)
				},
				{
					typeof(Gradient),
					typeof(EmittedGradientContainer)
				}
			};
			MonoBehaviourTypeCache = new DoubleLookupDictionary<string, Type, Type>();
			ScriptableObjectTypeCache = new DoubleLookupDictionary<string, Type, Type>();
			MarkedForDestruction_LOCK = new object();
			MarkedForDestruction = new List<Object>();
			EditorApplication.update = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.update, (Delegate)new CallbackFunction(DestroyMarkedObjects));
		}

		private static void DestroyMarkedObjects()
		{
			lock (MarkedForDestruction_LOCK)
			{
				for (int i = 0; i < MarkedForDestruction.Count; i++)
				{
					Object val = MarkedForDestruction[i];
					if (val != (Object)null)
					{
						Object.DestroyImmediate(val);
					}
				}
				MarkedForDestruction.Clear();
			}
		}

		/// <summary>
		/// Creates an emitted MonoBehaviour-based <see cref="T:UnityEditor.SerializedProperty" />.
		/// </summary>
		/// <param name="fieldName">Name of the field to emit.</param>
		/// <param name="valueType">Type of the value to create a property for.</param>
		/// <param name="targetCount">The target count of the tree to create a property for.</param>
		/// <param name="gameObject">The game object that the MonoBehaviour of the property is located on.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// fieldName is null
		/// or
		/// valueType is null
		/// </exception>
		/// <exception cref="T:System.ArgumentException">Target count must be equal to or higher than 1.</exception>
		public static Handle CreateEmittedMonoBehaviourProperty(string fieldName, Type valueType, int targetCount, ref GameObject gameObject)
		{
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Expected O, but got Unknown
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Expected O, but got Unknown
			DestroyMarkedObjects();
			if (fieldName == null)
			{
				throw new ArgumentNullException("fieldName");
			}
			if (valueType == null)
			{
				throw new ArgumentNullException("valueType");
			}
			if (targetCount < 1)
			{
				throw new ArgumentException("Target count must be equal to or higher than 1.");
			}
			if ((Object)(object)gameObject == (Object)null)
			{
				gameObject = HostGO;
			}
			if (!MonoBehaviourTypeCache.TryGetInnerValue(fieldName, valueType, out var value))
			{
				value = EmitMonoBehaviourType(fieldName, valueType);
				MonoBehaviourTypeCache.AddInner(fieldName, valueType, value);
			}
			MonoBehaviour[] array = (MonoBehaviour[])(object)new MonoBehaviour[targetCount];
			for (int i = 0; i < targetCount; i++)
			{
				array[i] = (MonoBehaviour)gameObject.AddComponent(value);
				((Object)array[i]).set_hideFlags(((Object)gameObject).get_hideFlags());
			}
			SerializedObject val = new SerializedObject((Object[])(object)array);
			return new Handle(val.FindProperty(fieldName), (Object[])(object)array);
		}

		/// <summary>
		/// Creates an emitted ScriptableObject-based <see cref="T:UnityEditor.SerializedProperty" />.
		/// </summary>
		/// <param name="fieldName">Name of the field to emit.</param>
		/// <param name="valueType">Type of the value to create a property for.</param>
		/// <param name="targetCount">The target count of the tree to create a property for.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// fieldName is null
		/// or
		/// valueType is null
		/// </exception>
		/// <exception cref="T:System.ArgumentException">Target count must be equal to or higher than 1.</exception>
		public static SerializedProperty CreateEmittedScriptableObjectProperty(string fieldName, Type valueType, int targetCount)
		{
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			DestroyMarkedObjects();
			if (fieldName == null)
			{
				throw new ArgumentNullException("fieldName");
			}
			if (valueType == null)
			{
				throw new ArgumentNullException("valueType");
			}
			if (targetCount < 1)
			{
				throw new ArgumentException("Target count must be equal to or higher than 1.");
			}
			if (PreCreatedScriptableObjectTypes.TryGetValue(valueType, out var value))
			{
				fieldName = "value";
			}
			else if (!ScriptableObjectTypeCache.TryGetInnerValue(fieldName, valueType, out value))
			{
				value = EmitScriptableObjectType(fieldName, valueType);
				ScriptableObjectTypeCache.AddInner(fieldName, valueType, value);
			}
			ScriptableObject[] array = (ScriptableObject[])(object)new ScriptableObject[targetCount];
			for (int i = 0; i < targetCount; i++)
			{
				array[i] = ScriptableObject.CreateInstance(value);
			}
			SerializedObject val = new SerializedObject((Object[])(object)array);
			return val.FindProperty(fieldName);
		}

		private static void EnsureEmitModule()
		{
			if (emittedAssembly == null)
			{
				FixUnityAboutWindowBeforeEmit.Fix();
				AssemblyName assemblyName = new AssemblyName("Sirenix.OdinInspector.EmittedUnityProperties");
				assemblyName.CultureInfo = CultureInfo.InvariantCulture;
				assemblyName.Flags = AssemblyNameFlags.None;
				assemblyName.ProcessorArchitecture = ProcessorArchitecture.MSIL;
				assemblyName.VersionCompatibility = AssemblyVersionCompatibility.SameDomain;
				emittedAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
				emittedModule = emittedAssembly.DefineDynamicModule("Sirenix.OdinInspector.EmittedUnityProperties", emitSymbolInfo: true);
			}
		}

		private static Type EmitMonoBehaviourType(string memberName, Type valueType)
		{
			string typeName = "Sirenix.OdinInspector.EmittedUnityProperties.EmittedMBProperty_" + memberName + "_" + valueType.GetCompilableNiceFullName();
			Type inheritedType = typeof(EmittedMonoBehaviour<>).MakeGenericType(valueType);
			return EmitType(memberName, typeName, inheritedType, valueType);
		}

		private static Type EmitScriptableObjectType(string memberName, Type valueType)
		{
			string typeName = "Sirenix.OdinInspector.EmittedUnityProperties.EmittedSOProperty_" + memberName + "_" + valueType.GetCompilableNiceFullName();
			Type inheritedType = typeof(EmittedScriptableObject<>).MakeGenericType(valueType);
			return EmitType(memberName, typeName, inheritedType, valueType);
		}

		private static Type EmitType(string memberName, string typeName, Type inheritedType, Type valueType)
		{
			EnsureEmitModule();
			MethodInfo method = inheritedType.GetMethod("SetValue");
			MethodInfo method2 = inheritedType.GetMethod("GetValue");
			MethodInfo getMethod = inheritedType.GetProperty("BackingFieldInfo").GetGetMethod();
			TypeBuilder typeBuilder = emittedModule.DefineType(typeName, TypeAttributes.Sealed, inheritedType);
			typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(CompilerGeneratedAttribute).GetConstructor(Type.EmptyTypes), new object[0]));
			FieldBuilder fieldBuilder = typeBuilder.DefineField(memberName, valueType, FieldAttributes.Public);
			fieldBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(SerializeField).GetConstructor(Type.EmptyTypes), new object[0]));
			FieldBuilder field = typeBuilder.DefineField("backingFieldInfo", typeof(FieldInfo), FieldAttributes.Private | FieldAttributes.Static);
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType, (from n in method.GetParameters()
				select n.ParameterType).ToArray());
			ILGenerator iLGenerator = methodBuilder.GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldarg_1);
			iLGenerator.Emit(OpCodes.Stfld, fieldBuilder);
			iLGenerator.Emit(OpCodes.Ret);
			typeBuilder.DefineMethodOverride(methodBuilder, method);
			MethodBuilder methodBuilder2 = typeBuilder.DefineMethod(method2.Name, MethodAttributes.Public | MethodAttributes.Virtual, method2.ReturnType, (from n in method2.GetParameters()
				select n.ParameterType).ToArray());
			ILGenerator iLGenerator2 = methodBuilder2.GetILGenerator();
			iLGenerator2.Emit(OpCodes.Ldarg_0);
			iLGenerator2.Emit(OpCodes.Ldfld, fieldBuilder);
			iLGenerator2.Emit(OpCodes.Ret);
			typeBuilder.DefineMethodOverride(methodBuilder2, method2);
			MethodBuilder methodBuilder3 = typeBuilder.DefineMethod(getMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual, getMethod.ReturnType, (from n in getMethod.GetParameters()
				select n.ParameterType).ToArray());
			ILGenerator iLGenerator3 = methodBuilder3.GetILGenerator();
			iLGenerator3.Emit(OpCodes.Ldsfld, field);
			iLGenerator3.Emit(OpCodes.Ret);
			typeBuilder.DefineMethodOverride(methodBuilder3, getMethod);
			Type type = typeBuilder.CreateType();
			FieldInfo field2 = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public);
			FieldInfo field3 = type.GetField("backingFieldInfo", BindingFlags.Static | BindingFlags.NonPublic);
			field3.SetValue(null, field2);
			return type;
		}
	}
}
