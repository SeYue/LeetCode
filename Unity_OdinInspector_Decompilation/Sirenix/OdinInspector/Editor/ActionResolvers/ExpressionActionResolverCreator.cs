using System;
using Sirenix.Utilities.Editor.Expressions;

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
	public class ExpressionActionResolverCreator : ActionResolverCreator
	{
		private static readonly ResolvedAction EmptyExpression = delegate
		{
		};

		public override string GetPossibleMatchesString(ref ActionResolverContext context)
		{
			return "C# Expressions: \"@expression\"";
		}

		public override ResolvedAction TryCreateAction(ref ActionResolverContext context)
		{
			if (string.IsNullOrEmpty(context.ResolvedString) || context.ResolvedString[0] != '@')
			{
				return null;
			}
			if (context.ResolvedString.Length == 1)
			{
				return EmptyExpression;
			}
			string expression = context.ResolvedString.Substring(1);
			int count = context.NamedValues.Count;
			bool flag = context.Property == context.Property.Tree.RootProperty && context.Property.Tree.IsStatic;
			string[] array = new string[count];
			Type[] array2 = new Type[count];
			for (int i = 0; i < count; i++)
			{
				NamedValue namedValue = context.NamedValues[i];
				array[i] = namedValue.Name;
				array2[i] = namedValue.Type;
			}
			string errorMessage;
			Delegate method = ExpressionUtility.ParseExpression(expression, flag, context.ParentType, array2, array, out errorMessage);
			if (errorMessage != null)
			{
				context.ErrorMessage = errorMessage;
				return ActionResolverCreator.FailedResolveAction;
			}
			object[] parameterValues = new object[count + ((!flag) ? 1 : 0)];
			return GetExpressionLambda(method, flag, context.ParentType.IsValueType, parameterValues);
		}

		private static ResolvedAction GetExpressionLambda(Delegate method, bool isStatic, bool parentIsValueType, object[] parameterValues)
		{
			return delegate(ref ActionResolverContext context, int selectionIndex)
			{
				int num = 0;
				object[] array = parameterValues;
				if (!isStatic)
				{
					array[0] = context.GetParentValue(selectionIndex);
					num = 1;
				}
				for (int i = num; i < array.Length; i++)
				{
					array[i] = context.NamedValues[i - num].CurrentValue;
				}
				method.DynamicInvoke(parameterValues);
				if (!isStatic && parentIsValueType)
				{
					context.SetParentValue(selectionIndex, array[0]);
				}
			};
		}
	}
}
