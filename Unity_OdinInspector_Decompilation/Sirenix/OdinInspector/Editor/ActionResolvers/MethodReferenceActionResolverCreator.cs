using System;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
	public class MethodReferenceActionResolverCreator : ActionResolverCreator
	{
		public override string GetPossibleMatchesString(ref ActionResolverContext context)
		{
			return "Method References: \"MethodName\"";
		}

		public override ResolvedAction TryCreateAction(ref ActionResolverContext context)
		{
			if (string.IsNullOrEmpty(context.ResolvedString))
			{
				return null;
			}
			string resolvedString = context.ResolvedString;
			if (!TypeExtensions.IsValidIdentifier(resolvedString))
			{
				return null;
			}
			BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
			bool flag = context.Property == context.Property.Tree.RootProperty && context.Property.Tree.IsStatic;
			if (!flag)
			{
				bindingFlags |= BindingFlags.Instance;
			}
			Type parentType = context.ParentType;
			NamedValues argSetup = default(NamedValues);
			string errorMessage;
			MethodInfo compatibleMethod = GetCompatibleMethod(parentType, resolvedString, bindingFlags, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out errorMessage);
			if (errorMessage != null)
			{
				context.ErrorMessage = errorMessage;
				return ActionResolverCreator.FailedResolveAction;
			}
			if (compatibleMethod == null && !flag)
			{
				Type baseType = parentType.BaseType;
				BindingFlags bindingFlags2 = bindingFlags;
				bindingFlags2 &= ~BindingFlags.FlattenHierarchy;
				bindingFlags2 |= BindingFlags.DeclaredOnly;
				do
				{
					compatibleMethod = GetCompatibleMethod(baseType, resolvedString, bindingFlags, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out errorMessage);
					if (errorMessage != null)
					{
						context.ErrorMessage = errorMessage;
						return ActionResolverCreator.FailedResolveAction;
					}
					if (compatibleMethod != null)
					{
						break;
					}
					baseType = baseType.BaseType;
				}
				while (baseType != null);
			}
			if (compatibleMethod != null)
			{
				return ActionResolverCreator.GetMethodInvoker(compatibleMethod, argSetup, context.ParentType.IsValueType);
			}
			return null;
		}

		private static MethodInfo GetCompatibleMethod(Type type, string methodName, BindingFlags flags, ref NamedValues namedValues, ref NamedValues argSetup, bool requiresBackcasting, out string errorMessage)
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
			if (!ActionResolverCreator.IsCompatibleMethod(method, ref namedValues, ref argSetup, requiresBackcasting, out errorMessage))
			{
				return null;
			}
			return method;
		}
	}
}
