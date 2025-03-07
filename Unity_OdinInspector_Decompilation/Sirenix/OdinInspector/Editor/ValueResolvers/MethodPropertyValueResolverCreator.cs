using System;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public class MethodPropertyValueResolverCreator : BaseMemberValueResolverCreator
	{
		public override string GetPossibleMatchesString(ref ValueResolverContext context)
		{
			return null;
		}

		public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
		{
			InspectorProperty property = context.Property;
			if (string.IsNullOrEmpty(context.ResolvedString) && property.Info.PropertyType == PropertyType.Method)
			{
				MethodInfo methodInfo = (property.Info.GetMemberInfo() as MethodInfo) ?? property.Info.GetMethodDelegate().Method;
				if (methodInfo.IsGenericMethodDefinition)
				{
					context.ErrorMessage = "Cannot invoke a generic method definition such as '" + methodInfo.GetNiceName() + "'.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				Type returnType = methodInfo.ReturnType;
				if (returnType == typeof(void) || !ConvertUtility.CanConvert(returnType, typeof(TResult)))
				{
					return null;
				}
				NamedValues argSetup = default(NamedValues);
				if (BaseMemberValueResolverCreator.IsCompatibleMethod(methodInfo, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out context.ErrorMessage))
				{
					if ((object)property.Info.GetMethodDelegate() != null)
					{
						return BaseMemberValueResolverCreator.GetDelegateGetter<TResult>(property.Info.GetMethodDelegate(), argSetup);
					}
					return BaseMemberValueResolverCreator.GetMethodGetter<TResult>(methodInfo, argSetup, property.ParentType.IsValueType);
				}
				if (context.ErrorMessage != null)
				{
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
			}
			return null;
		}
	}
}
