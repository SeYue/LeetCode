using System;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public abstract class BaseMemberValueResolverCreator : ValueResolverCreator
	{
		protected static ValueResolverFunc<TResult> GetFieldGetter<TResult>(FieldInfo field)
		{
			if (field.IsStatic)
			{
				return delegate
				{
					return ConvertUtility.Convert<TResult>(field.GetValue(null));
				};
			}
			return delegate(ref ValueResolverContext context, int selectionIndex)
			{
				return ConvertUtility.Convert<TResult>(field.GetValue(context.GetParentValue(selectionIndex)));
			};
		}

		protected static ValueResolverFunc<TResult> GetPropertyGetter<TResult>(PropertyInfo property, bool parentIsValueType)
		{
			if (property.IsStatic())
			{
				return delegate
				{
					return ConvertUtility.Convert<TResult>(property.GetValue(null, null));
				};
			}
			return delegate(ref ValueResolverContext context, int selectionIndex)
			{
				object parentValue = context.GetParentValue(selectionIndex);
				TResult result = ConvertUtility.Convert<TResult>(property.GetValue(parentValue, null));
				if (parentIsValueType)
				{
					context.SetParentValue(selectionIndex, parentValue);
				}
				return result;
			};
		}

		protected static ValueResolverFunc<TResult> GetMethodGetter<TResult>(MethodInfo method, NamedValues argSetup, bool parentIsValueType)
		{
			object[] parameterValues = new object[argSetup.Count];
			bool isStatic = method.IsStatic;
			ParameterInfo[] parameters = method.GetParameters();
			bool[] byRefParameters = new bool[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				byRefParameters[i] = parameters[i].ParameterType.IsByRef;
			}
			return delegate(ref ValueResolverContext context, int selectionIndex)
			{
				for (int j = 0; j < parameterValues.Length; j++)
				{
					object value = context.NamedValues.GetValue(argSetup[j].Name);
					parameterValues[j] = ConvertUtility.WeakConvert(value, argSetup[j].Type);
				}
				object obj = (isStatic ? null : context.GetParentValue(selectionIndex));
				TResult result = ConvertUtility.Convert<TResult>(method.Invoke(obj, parameterValues));
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
				return result;
			};
		}

		protected static ValueResolverFunc<TResult> GetDelegateGetter<TResult>(Delegate @delegate, NamedValues argSetup)
		{
			object[] parameterValues = new object[argSetup.Count];
			ParameterInfo[] parameters = @delegate.Method.GetParameters();
			bool[] byRefParameters = new bool[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				byRefParameters[i] = parameters[i].ParameterType.IsByRef;
			}
			return delegate(ref ValueResolverContext context, int selectionIndex)
			{
				for (int j = 0; j < parameterValues.Length; j++)
				{
					object value = context.NamedValues.GetValue(argSetup[j].Name);
					parameterValues[j] = ConvertUtility.WeakConvert(value, argSetup[j].Type);
				}
				TResult result = ConvertUtility.Convert<TResult>(@delegate.DynamicInvoke(parameterValues));
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
				return result;
			};
		}

		protected static MethodInfo GetCompatibleMethod(Type type, string methodName, BindingFlags flags, ref NamedValues namedValues, ref NamedValues argSetup, bool requiresBackcasting, out string errorMessage)
		{
			MethodInfo method;
			try
			{
				method = type.GetMethod(methodName, flags);
			}
			catch (AmbiguousMatchException)
			{
				errorMessage = "Could not find exact method named '" + methodName + "' because there are several methods with that name defined, and so it is an ambiguous match.";
				return null;
			}
			if (method == null)
			{
				errorMessage = null;
				return null;
			}
			if (IsCompatibleMethod(method, ref namedValues, ref argSetup, requiresBackcasting, out errorMessage))
			{
				return method;
			}
			return null;
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
