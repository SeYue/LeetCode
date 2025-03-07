using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public abstract class ValueResolverCreator
	{
		private struct ResolverAndPriority
		{
			public ValueResolverCreator ResolverCreator;

			public double Priority;
		}

		private static StringBuilder SB;

		private static ResolverAndPriority[] ValueResolverCreators;

		private static readonly Dictionary<Type, Delegate> FailedResolveFuncs;

		private static readonly Dictionary<Type, Delegate> FallbackResolveFuncs;

		private static readonly Dictionary<Type, MethodInfo> WeaklyTypedGetResolverMethods;

		private static readonly Dictionary<Type, MethodInfo> WeaklyTypedGetResolverFromContextMethods;

		private static readonly MethodInfo GetResolverMethodInfo;

		private static readonly MethodInfo GetResolverFromContextMethodInfo;

		private static readonly object[] GetResolverMethodParameters;

		private static readonly object[] GetResolverFromContextMethodParameters;

		static ValueResolverCreator()
		{
			SB = new StringBuilder();
			ValueResolverCreators = new ResolverAndPriority[8];
			FailedResolveFuncs = new Dictionary<Type, Delegate>(FastTypeComparer.Instance);
			FallbackResolveFuncs = new Dictionary<Type, Delegate>(FastTypeComparer.Instance);
			WeaklyTypedGetResolverMethods = new Dictionary<Type, MethodInfo>(FastTypeComparer.Instance);
			WeaklyTypedGetResolverFromContextMethods = new Dictionary<Type, MethodInfo>(FastTypeComparer.Instance);
			GetResolverMethodInfo = typeof(ValueResolverCreator).GetMethod("GetResolver", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[5]
			{
				typeof(InspectorProperty),
				typeof(string),
				typeof(object),
				typeof(bool),
				typeof(NamedValue[])
			}, null);
			GetResolverFromContextMethodInfo = typeof(ValueResolverCreator).GetMethod("GetResolverFromContext", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[1] { typeof(ValueResolverContext).MakeByRefType() }, null);
			GetResolverMethodParameters = new object[5];
			GetResolverFromContextMethodParameters = new object[1];
			List<Assembly> resolverAssemblies = ResolverUtilities.GetResolverAssemblies();
			for (int i = 0; i < resolverAssemblies.Count; i++)
			{
				Assembly assembly = resolverAssemblies[i];
				object[] array = assembly.SafeGetCustomAttributes(typeof(RegisterDefaultValueResolverCreatorAttribute), inherit: false);
				for (int j = 0; j < array.Length; j++)
				{
					RegisterDefaultValueResolverCreatorAttribute registerDefaultValueResolverCreatorAttribute = (RegisterDefaultValueResolverCreatorAttribute)array[j];
					try
					{
						object obj = Activator.CreateInstance(registerDefaultValueResolverCreatorAttribute.ResolverCreatorType);
						Register((ValueResolverCreator)obj, registerDefaultValueResolverCreatorAttribute.Order);
					}
					catch (Exception innerException)
					{
						while (innerException.InnerException != null && innerException is TargetInvocationException)
						{
							innerException = innerException.InnerException;
						}
						Debug.LogException(new Exception("Failed to create instance of registered default resolver of type '" + registerDefaultValueResolverCreatorAttribute.ResolverCreatorType.GetNiceFullName() + "'", innerException));
					}
				}
			}
		}

		public abstract ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context);

		public abstract string GetPossibleMatchesString(ref ValueResolverContext context);

		private static TResult FailedResolveResult<TResult>(ref ValueResolverContext context, int selectionIndex)
		{
			if (context.HasFallbackValue)
			{
				return (TResult)context.FallbackValue;
			}
			return default(TResult);
		}

		public static void Register(ValueResolverCreator valueResolverCreator, double order = 0.0)
		{
			ResolverAndPriority[] array = ValueResolverCreators;
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].ResolverCreator == null)
				{
					array[i].ResolverCreator = valueResolverCreator;
					array[i].Priority = order;
					flag = true;
					break;
				}
				if (order > array[i].Priority)
				{
					ShiftUp(ref array, i);
					array[i].ResolverCreator = valueResolverCreator;
					array[i].Priority = order;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				int num = array.Length;
				Expand(ref array);
				array[num].ResolverCreator = valueResolverCreator;
				array[num].Priority = order;
			}
			ValueResolverCreators = array;
		}

		private static void Expand(ref ResolverAndPriority[] array)
		{
			ResolverAndPriority[] array2 = new ResolverAndPriority[array.Length * 2];
			Array.Copy(array, array2, array.Length);
			array = array2;
		}

		private static void ShiftUp(ref ResolverAndPriority[] array, int index)
		{
			int num = array.Length - 1;
			if (array[num].ResolverCreator != null)
			{
				Expand(ref array);
			}
			for (int num2 = num; num2 >= index; num2--)
			{
				if (num2 + 1 < array.Length)
				{
					array[num2 + 1].ResolverCreator = array[num2].ResolverCreator;
					array[num2 + 1].Priority = array[num2].Priority;
				}
			}
		}

		public static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString)
		{
			return GetResolver(resultType, property, resolvedString, null, hasFallback: false, null);
		}

		public static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
		{
			return GetResolver(resultType, property, resolvedString, null, hasFallback: false, namedArgs);
		}

		public static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString, object fallbackValue)
		{
			return GetResolver(resultType, property, resolvedString, fallbackValue, hasFallback: true, null);
		}

		public static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString, object fallbackValue, params NamedValue[] namedArgs)
		{
			return GetResolver(resultType, property, resolvedString, fallbackValue, hasFallback: true, namedArgs);
		}

		private static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString, object fallbackValue, bool hasFallback, params NamedValue[] namedArgs)
		{
			if (!WeaklyTypedGetResolverMethods.TryGetValue(resultType, out var value))
			{
				value = GetResolverMethodInfo.MakeGenericMethod(resultType);
				WeaklyTypedGetResolverMethods.Add(resultType, value);
			}
			GetResolverMethodParameters[0] = property;
			GetResolverMethodParameters[1] = resolvedString;
			GetResolverMethodParameters[2] = fallbackValue;
			GetResolverMethodParameters[3] = hasFallback;
			GetResolverMethodParameters[4] = namedArgs;
			return (ValueResolver)value.Invoke(null, GetResolverMethodParameters);
		}

		public static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString)
		{
			return GetResolver<TResult>(property, resolvedString, null, hasFallback: false, null);
		}

		public static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
		{
			return GetResolver<TResult>(property, resolvedString, null, hasFallback: false, namedArgs);
		}

		public static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString, TResult fallbackValue)
		{
			return GetResolver<TResult>(property, resolvedString, fallbackValue, hasFallback: true, null);
		}

		public static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString, TResult fallbackValue, params NamedValue[] namedArgs)
		{
			return GetResolver<TResult>(property, resolvedString, fallbackValue, hasFallback: true, namedArgs);
		}

		private static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString, object fallbackValue, bool hasFallback, params NamedValue[] namedArgs)
		{
			ValueResolver<TResult> valueResolver = new ValueResolver<TResult>();
			valueResolver.Context.Property = property;
			valueResolver.Context.ResolvedString = resolvedString;
			valueResolver.Context.ResultType = typeof(TResult);
			valueResolver.Context.LogExceptions = true;
			if (hasFallback)
			{
				valueResolver.Context.FallbackValue = fallbackValue;
				valueResolver.Context.HasFallbackValue = true;
			}
			if (namedArgs != null)
			{
				for (int i = 0; i < namedArgs.Length; i++)
				{
					valueResolver.Context.NamedValues.Add(namedArgs[i]);
				}
			}
			valueResolver.Context.AddDefaultContextValues();
			InitResolver(valueResolver);
			return valueResolver;
		}

		public static ValueResolver GetResolverFromContextWeak(ref ValueResolverContext context)
		{
			if (!WeaklyTypedGetResolverFromContextMethods.TryGetValue(context.ResultType, out var value))
			{
				value = GetResolverFromContextMethodInfo.MakeGenericMethod(context.ResultType);
				WeaklyTypedGetResolverFromContextMethods.Add(context.ResultType, value);
			}
			GetResolverFromContextMethodParameters[0] = context;
			return (ValueResolver)value.Invoke(null, GetResolverFromContextMethodParameters);
		}

		public static ValueResolver<TResult> GetResolverFromContext<TResult>(ref ValueResolverContext context)
		{
			ValueResolver<TResult> valueResolver = new ValueResolver<TResult>();
			valueResolver.Context = context;
			valueResolver.Context.ResultType = typeof(TResult);
			InitResolver(valueResolver);
			return valueResolver;
		}

		private static string GetPossibleMatchesMessage(ref ValueResolverContext context)
		{
			SB.Length = 0;
			SB.AppendLine("Could not match the given string '" + context.ResolvedString + "' to any possible value resolution in the context of the type '" + context.ParentType.GetNiceName() + "'. The following kinds of value resolutions are possible:");
			SB.AppendLine();
			ResolverAndPriority[] valueResolverCreators = ValueResolverCreators;
			for (int i = 0; i < valueResolverCreators.Length; i++)
			{
				ValueResolverCreator resolverCreator = valueResolverCreators[i].ResolverCreator;
				if (resolverCreator == null)
				{
					break;
				}
				string possibleMatchesString = resolverCreator.GetPossibleMatchesString(ref context);
				if (possibleMatchesString != null)
				{
					SB.AppendLine(possibleMatchesString);
				}
			}
			SB.AppendLine();
			SB.AppendLine("And the following named values are available:");
			SB.AppendLine();
			SB.Append(context.NamedValues.GetValueOverviewString());
			return SB.ToString();
		}

		private static void InitResolver<TResult>(ValueResolver<TResult> resolver)
		{
			if (resolver.Context.IsResolved)
			{
				throw new InvalidOperationException("This resolver's context has already been marked resolved! You cannot resolve the same context twice!");
			}
			bool hasFallbackValue = resolver.Context.HasFallbackValue;
			ResolverAndPriority[] valueResolverCreators = ValueResolverCreators;
			ValueResolverFunc<TResult> valueResolverFunc = null;
			for (int i = 0; i < valueResolverCreators.Length; i++)
			{
				ValueResolverCreator resolverCreator = valueResolverCreators[i].ResolverCreator;
				if (resolverCreator == null)
				{
					break;
				}
				try
				{
					valueResolverFunc = resolverCreator.TryCreateResolverFunc<TResult>(ref resolver.Context);
				}
				catch (Exception ex)
				{
					InitFailedResolve(resolver, withError: false);
					resolver.Context.ErrorMessage = "Resolver creator '" + resolverCreator.GetType().Name + "' failed with exception:\n\n" + ex.ToString();
					return;
				}
				if (valueResolverFunc != null)
				{
					break;
				}
			}
			if (valueResolverFunc == null)
			{
				if (hasFallbackValue)
				{
					InitFailedResolve(resolver, withError: false);
				}
				else
				{
					InitFailedResolve(resolver, withError: true);
				}
			}
			else
			{
				resolver.Func = valueResolverFunc;
			}
			resolver.Context.MarkResolved();
		}

		private static void InitFailedResolve<TResult>(ValueResolver<TResult> resolver, bool withError)
		{
			if (withError)
			{
				resolver.Context.ErrorMessage = GetPossibleMatchesMessage(ref resolver.Context);
			}
			if (!FailedResolveFuncs.TryGetValue(typeof(TResult), out var value))
			{
				value = new ValueResolverFunc<TResult>(FailedResolveResult<TResult>);
				FailedResolveFuncs.Add(typeof(TResult), value);
			}
			resolver.Func = (ValueResolverFunc<TResult>)value;
		}

		protected static ValueResolverFunc<TResult> GetFailedResolverFunc<TResult>()
		{
			if (!FailedResolveFuncs.TryGetValue(typeof(TResult), out var value))
			{
				value = new ValueResolverFunc<TResult>(FailedResolveResult<TResult>);
				FailedResolveFuncs.Add(typeof(TResult), value);
			}
			return (ValueResolverFunc<TResult>)value;
		}
	}
}
