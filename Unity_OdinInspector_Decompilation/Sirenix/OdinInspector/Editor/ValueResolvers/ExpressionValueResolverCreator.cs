using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor.Expressions;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public class ExpressionValueResolverCreator : ValueResolverCreator
	{
		public override string GetPossibleMatchesString(ref ValueResolverContext context)
		{
			return "C# Expressions: \"@expression\"";
		}

		public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
		{
			if (string.IsNullOrEmpty(context.ResolvedString) || context.ResolvedString.Length < 2 || context.ResolvedString[0] != '@')
			{
				return null;
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
			Delegate @delegate = ExpressionUtility.ParseExpression(expression, flag, context.ParentType, array2, array, out errorMessage);
			if (errorMessage != null)
			{
				context.ErrorMessage = errorMessage;
				return ValueResolverCreator.GetFailedResolverFunc<TResult>();
			}
			Type returnType = @delegate.Method.ReturnType;
			if (returnType == typeof(void))
			{
				context.ErrorMessage = "Expression cannot evaluate to 'void'; it must evaluate to a value that can be assigned or converted to required type '" + typeof(TResult).GetNiceName() + "'";
				return ValueResolverCreator.GetFailedResolverFunc<TResult>();
			}
			if (!ConvertUtility.CanConvert(returnType, typeof(TResult)))
			{
				context.ErrorMessage = "Cannot convert expression result type '" + @delegate.Method.ReturnType.GetNiceName() + "' to required type '" + typeof(TResult).GetNiceName() + "'";
				return ValueResolverCreator.GetFailedResolverFunc<TResult>();
			}
			object[] parameterValues = new object[count + ((!flag) ? 1 : 0)];
			return GetExpressionLambda<TResult>(@delegate, flag, context.ParentType.IsValueType, parameterValues);
		}

		private static ValueResolverFunc<TResult> GetExpressionLambda<TResult>(Delegate method, bool isStatic, bool parentIsValueType, object[] parameterValues)
		{
			return delegate(ref ValueResolverContext context, int selectionIndex)
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
				TResult result = ConvertUtility.Convert<TResult>(method.DynamicInvoke(parameterValues));
				if (!isStatic && parentIsValueType)
				{
					context.SetParentValue(selectionIndex, array[0]);
				}
				return result;
			};
		}
	}
}
