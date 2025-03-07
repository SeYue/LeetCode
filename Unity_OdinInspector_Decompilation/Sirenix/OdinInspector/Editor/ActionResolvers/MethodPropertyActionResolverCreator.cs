using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
	public class MethodPropertyActionResolverCreator : ActionResolverCreator
	{
		public override string GetPossibleMatchesString(ref ActionResolverContext context)
		{
			return null;
		}

		public override ResolvedAction TryCreateAction(ref ActionResolverContext context)
		{
			InspectorProperty property = context.Property;
			if (string.IsNullOrEmpty(context.ResolvedString) && property.Info.PropertyType == PropertyType.Method)
			{
				MethodInfo methodInfo = (property.Info.GetMemberInfo() as MethodInfo) ?? property.Info.GetMethodDelegate().Method;
				if (methodInfo.IsGenericMethodDefinition)
				{
					context.ErrorMessage = "Cannot invoke a generic method definition such as '" + methodInfo.GetNiceName() + "'.";
					return ActionResolverCreator.FailedResolveAction;
				}
				NamedValues argSetup = default(NamedValues);
				if (ActionResolverCreator.IsCompatibleMethod(methodInfo, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out context.ErrorMessage))
				{
					if ((object)property.Info.GetMethodDelegate() != null)
					{
						return ActionResolverCreator.GetDelegateInvoker(property.Info.GetMethodDelegate(), argSetup);
					}
					return ActionResolverCreator.GetMethodInvoker(methodInfo, argSetup, property.ParentType.IsValueType);
				}
				if (context.ErrorMessage != null)
				{
					return ActionResolverCreator.FailedResolveAction;
				}
			}
			return null;
		}
	}
}
