using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
	public abstract class ActionResolverCreator
	{
		private struct ResolverAndPriority
		{
			public ActionResolverCreator ResolverCreator;

			public double Priority;
		}

		private static StringBuilder SB;

		private static ResolverAndPriority[] ActionResolverCreators;

		protected static readonly ResolvedAction FailedResolveAction;

		static ActionResolverCreator()
		{
			SB = new StringBuilder();
			ActionResolverCreators = new ResolverAndPriority[8];
			FailedResolveAction = delegate
			{
			};
			List<Assembly> resolverAssemblies = ResolverUtilities.GetResolverAssemblies();
			for (int i = 0; i < resolverAssemblies.Count; i++)
			{
				Assembly assembly = resolverAssemblies[i];
				object[] array = assembly.SafeGetCustomAttributes(typeof(RegisterDefaultActionResolverAttribute), inherit: false);
				for (int j = 0; j < array.Length; j++)
				{
					RegisterDefaultActionResolverAttribute registerDefaultActionResolverAttribute = (RegisterDefaultActionResolverAttribute)array[j];
					try
					{
						object obj = Activator.CreateInstance(registerDefaultActionResolverAttribute.ResolverType);
						Register((ActionResolverCreator)obj, registerDefaultActionResolverAttribute.Order);
					}
					catch (Exception innerException)
					{
						while (innerException.InnerException != null && innerException is TargetInvocationException)
						{
							innerException = innerException.InnerException;
						}
						Debug.LogException(new Exception("Failed to create instance of registered default resolver of type '" + registerDefaultActionResolverAttribute.ResolverType.GetNiceFullName() + "'", innerException));
					}
				}
			}
		}

		public abstract ResolvedAction TryCreateAction(ref ActionResolverContext context);

		public abstract string GetPossibleMatchesString(ref ActionResolverContext context);

		public static void Register(ActionResolverCreator valueResolverCreator, double order = 0.0)
		{
			ResolverAndPriority[] array = ActionResolverCreators;
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
			ActionResolverCreators = array;
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

		public static ActionResolver GetResolver(InspectorProperty property, string resolvedString)
		{
			return GetResolver(property, resolvedString, null);
		}

		public static ActionResolver GetResolver(InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
		{
			ActionResolver actionResolver = new ActionResolver();
			actionResolver.Context.Property = property;
			actionResolver.Context.ResolvedString = resolvedString;
			actionResolver.Context.LogExceptions = true;
			if (namedArgs != null)
			{
				for (int i = 0; i < namedArgs.Length; i++)
				{
					actionResolver.Context.NamedValues.Add(namedArgs[i]);
				}
			}
			actionResolver.Context.AddDefaultContextValues();
			InitResolver(actionResolver);
			return actionResolver;
		}

		public static ActionResolver GetResolverFromContext(ref ActionResolverContext context)
		{
			ActionResolver actionResolver = new ActionResolver();
			actionResolver.Context = context;
			InitResolver(actionResolver);
			return actionResolver;
		}

		private static string GetPossibleMatchesMessage(ref ActionResolverContext context)
		{
			SB.Length = 0;
			SB.AppendLine("Could not match the given string '" + context.ResolvedString + "' to any action that can be performed in the context of the type '" + context.Property.ParentType.GetNiceName() + "'. The following kinds of actions are possible:");
			SB.AppendLine();
			ResolverAndPriority[] actionResolverCreators = ActionResolverCreators;
			for (int i = 0; i < actionResolverCreators.Length; i++)
			{
				ActionResolverCreator resolverCreator = actionResolverCreators[i].ResolverCreator;
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

		private static void InitResolver(ActionResolver resolver)
		{
			if (resolver.Context.IsResolved)
			{
				throw new InvalidOperationException("This resolver's context has already been marked resolved! You cannot resolve the same context twice!");
			}
			ResolverAndPriority[] actionResolverCreators = ActionResolverCreators;
			ResolvedAction resolvedAction = null;
			for (int i = 0; i < actionResolverCreators.Length; i++)
			{
				ActionResolverCreator resolverCreator = actionResolverCreators[i].ResolverCreator;
				if (resolverCreator == null)
				{
					break;
				}
				try
				{
					resolvedAction = resolverCreator.TryCreateAction(ref resolver.Context);
				}
				catch (Exception ex)
				{
					resolver.Context.ErrorMessage = "Resolver creator '" + resolverCreator.GetType().Name + "' failed with exception:\n\n" + ex.ToString();
					resolver.Action = FailedResolveAction;
					return;
				}
				if (resolvedAction != null)
				{
					break;
				}
			}
			if (resolvedAction == null)
			{
				resolver.Context.ErrorMessage = GetPossibleMatchesMessage(ref resolver.Context);
				resolver.Action = FailedResolveAction;
			}
			else
			{
				resolver.Action = resolvedAction;
			}
			resolver.Context.MarkResolved();
		}

		protected static ResolvedAction GetDelegateInvoker(Delegate @delegate, NamedValues argSetup)
		{
			object[] parameterValues = new object[argSetup.Count];
			ParameterInfo[] parameters = @delegate.Method.GetParameters();
			bool[] byRefParameters = new bool[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				byRefParameters[i] = parameters[i].ParameterType.IsByRef;
			}
			return delegate(ref ActionResolverContext context, int selectionIndex)
			{
				for (int j = 0; j < parameterValues.Length; j++)
				{
					object value = context.NamedValues.GetValue(argSetup[j].Name);
					parameterValues[j] = ConvertUtility.WeakConvert(value, argSetup[j].Type);
				}
				@delegate.DynamicInvoke(parameterValues);
				if (context.SyncRefParametersWithNamedValues)
				{
					for (int k = 0; k < parameterValues.Length; k++)
					{
						if (byRefParameters[k])
						{
							NamedValue namedValue = argSetup[k];
							if (!context.NamedValues.TryGetValue(namedValue.Name, out var value2))
							{
								throw new Exception("Expected named value '" + namedValue.Name + "' was not present!");
							}
							context.NamedValues.Set(namedValue.Name, ConvertUtility.WeakConvert(parameterValues[k], value2.Type));
						}
					}
				}
			};
		}

		protected static ResolvedAction GetMethodInvoker(MethodInfo method, NamedValues argSetup, bool parentIsValueType)
		{
			object[] parameterValues = new object[argSetup.Count];
			bool isStatic = method.IsStatic;
			ParameterInfo[] parameters = method.GetParameters();
			bool[] byRefParameters = new bool[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				byRefParameters[i] = parameters[i].ParameterType.IsByRef;
			}
			return delegate(ref ActionResolverContext context, int selectionIndex)
			{
				for (int j = 0; j < parameterValues.Length; j++)
				{
					object value = context.NamedValues.GetValue(argSetup[j].Name);
					parameterValues[j] = ConvertUtility.WeakConvert(value, argSetup[j].Type);
				}
				object obj = (isStatic ? null : context.GetParentValue(selectionIndex));
				method.Invoke(obj, parameterValues);
				if (!isStatic && parentIsValueType)
				{
					context.SetParentValue(selectionIndex, obj);
				}
				if (context.SyncRefParametersWithNamedValues)
				{
					for (int k = 0; k < parameterValues.Length; k++)
					{
						if (byRefParameters[k])
						{
							NamedValue namedValue = argSetup[k];
							if (!context.NamedValues.TryGetValue(namedValue.Name, out var value2))
							{
								throw new Exception("Expected named value '" + namedValue.Name + "' was not present!");
							}
							context.NamedValues.Set(namedValue.Name, ConvertUtility.WeakConvert(parameterValues[k], value2.Type));
						}
					}
				}
			};
		}

		protected unsafe static bool IsCompatibleMethod(MethodInfo method, ref NamedValues namedValues, ref NamedValues argSetup, bool requiresBackcasting, out string errorMessage)
		{
			ParameterInfo[] parameters = method.GetParameters();
			int count = namedValues.Count;
			if (parameters.Length > count)
			{
				errorMessage = "Method '" + method.GetNiceName() + "' has too many parameters (" + parameters.Length + "). The following '" + count + "' parameters are available: \n\n" + namedValues.GetValueOverviewString();
				return false;
			}
			bool* ptr = stackalloc bool[(int)checked(unchecked((nuint)(uint)count) * (nuint)1u)];
			foreach (ParameterInfo parameterInfo in parameters)
			{
				string name = parameterInfo.Name;
				Type type = parameterInfo.ParameterType;
				bool flag = false;
				if (type.IsByRef)
				{
					flag = true;
					type = type.GetElementType();
				}
				bool flag2 = false;
				for (int j = 0; j < count; j++)
				{
					if (ptr[j])
					{
						continue;
					}
					NamedValue namedValue = namedValues[j];
					if (namedValue.Name == name)
					{
						if (!ConvertUtility.CanConvert(namedValue.Type, type))
						{
							errorMessage = "Method '" + method.Name + "' has an invalid signature; the parameter '" + name + "' of type '" + parameterInfo.ParameterType.GetNiceName() + "' cannot be assigned from the available type '" + namedValue.Type.GetNiceName() + "'. The following parameters are available: \n\n" + namedValues.GetValueOverviewString();
							return false;
						}
						if (requiresBackcasting && flag && !ConvertUtility.CanConvert(type, namedValue.Type))
						{
							errorMessage = "Method '" + method.Name + "' has an invalid signature; the ref/in/out parameter '" + name + "' of type '" + parameterInfo.ParameterType.GetNiceName() + "' can be assigned from the available type '" + namedValue.Type.GetNiceName() + "', but type '" + namedValue.Type.GetNiceName() + "' cannot be reassigned assigned back to type '" + parameterInfo.ParameterType.GetNiceName() + "'. The following parameters are available: \n\n" + namedValues.GetValueOverviewString();
							return false;
						}
						argSetup.Add(namedValue.Name, type, null);
						ptr[j] = true;
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					for (int k = 0; k < count; k++)
					{
						if (!ptr[k])
						{
							NamedValue namedValue2 = namedValues[k];
							if (namedValue2.Type == type)
							{
								flag2 = true;
								argSetup.Add(namedValue2.Name, type, null);
								ptr[k] = true;
								break;
							}
						}
					}
				}
				if (!flag2 && type != typeof(string))
				{
					for (int l = 0; l < count; l++)
					{
						if (!ptr[l])
						{
							NamedValue namedValue3 = namedValues[l];
							if (ConvertUtility.CanConvert(namedValue3.Type, type) && (!requiresBackcasting || ConvertUtility.CanConvert(type, namedValue3.Type)))
							{
								flag2 = true;
								argSetup.Add(namedValue3.Name, type, null);
								ptr[l] = true;
								break;
							}
						}
					}
				}
				if (!flag2)
				{
					errorMessage = "Method '" + method.Name + "' has an invalid signature; no values could be assigned to the parameter '" + name + "' of type '" + parameterInfo.ParameterType.GetNiceName() + "'. The following parameter values are available: \n\n" + namedValues.GetValueOverviewString();
					return false;
				}
			}
			errorMessage = null;
			return true;
		}
	}
}
