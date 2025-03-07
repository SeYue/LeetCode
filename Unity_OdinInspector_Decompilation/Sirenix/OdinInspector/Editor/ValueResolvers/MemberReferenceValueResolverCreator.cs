using System;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public class MemberReferenceValueResolverCreator : BaseMemberValueResolverCreator
	{
		public override string GetPossibleMatchesString(ref ValueResolverContext context)
		{
			if (context.ResultType == typeof(string))
			{
				return "Member References: \"$MemberName\"";
			}
			return "Member References: \"MemberName\"";
		}

		public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
		{
			if (string.IsNullOrEmpty(context.ResolvedString) || context.ResolvedString.Length < 2)
			{
				return null;
			}
			bool flag;
			string text;
			if (context.ResolvedString[0] == '$')
			{
				flag = true;
				text = context.ResolvedString.Substring(1);
			}
			else
			{
				if (typeof(TResult) == typeof(string) && context.HasFallbackValue)
				{
					return null;
				}
				flag = false;
				text = context.ResolvedString;
			}
			if (!TypeExtensions.IsValidIdentifier(text))
			{
				if (flag)
				{
					context.ErrorMessage = "'" + text + "' is not a valid C# member identifier.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				return null;
			}
			BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
			bool flag2 = context.Property == context.Property.Tree.RootProperty && context.Property.Tree.IsStatic;
			if (!flag2)
			{
				bindingFlags |= BindingFlags.Instance;
			}
			Type parentType = context.ParentType;
			NamedValues argSetup = default(NamedValues);
			MemberInfo memberInfo = parentType.GetField(text, bindingFlags);
			if (memberInfo == null)
			{
				memberInfo = parentType.GetProperty(text, bindingFlags);
			}
			string errorMessage;
			if (memberInfo == null)
			{
				memberInfo = BaseMemberValueResolverCreator.GetCompatibleMethod(parentType, text, bindingFlags, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out errorMessage);
				if (errorMessage != null)
				{
					context.ErrorMessage = errorMessage;
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
			}
			if (memberInfo == null && !flag2)
			{
				Type baseType = parentType.BaseType;
				BindingFlags bindingFlags2 = bindingFlags;
				bindingFlags2 &= ~BindingFlags.FlattenHierarchy;
				bindingFlags2 |= BindingFlags.DeclaredOnly;
				do
				{
					memberInfo = baseType.GetField(text, bindingFlags2);
					if (memberInfo == null)
					{
						memberInfo = baseType.GetProperty(text, bindingFlags2);
					}
					if (memberInfo == null)
					{
						memberInfo = BaseMemberValueResolverCreator.GetCompatibleMethod(baseType, text, bindingFlags, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out errorMessage);
						if (errorMessage != null)
						{
							context.ErrorMessage = errorMessage;
							return ValueResolverCreator.GetFailedResolverFunc<TResult>();
						}
					}
					if (memberInfo != null)
					{
						break;
					}
					baseType = baseType.BaseType;
				}
				while (baseType != null);
			}
			if (memberInfo != null)
			{
				Type returnType = memberInfo.GetReturnType();
				if (returnType == typeof(void) || !ConvertUtility.CanConvert(returnType, typeof(TResult)))
				{
					if (memberInfo is MethodInfo)
					{
						if (returnType == typeof(void))
						{
							context.ErrorMessage = "Method " + memberInfo.Name + " cannot return void; it must return a value that can be assigned or converted to the type '" + typeof(TResult).GetNiceName() + "'";
						}
						else
						{
							context.ErrorMessage = "Cannot convert method " + memberInfo.Name + "'s return type '" + returnType.GetNiceName() + "' to required type '" + typeof(TResult).GetNiceName() + "'";
						}
					}
					else
					{
						context.ErrorMessage = "Cannot convert member " + memberInfo.Name + "'s contained type '" + returnType.GetNiceName() + "' to required type '" + typeof(TResult).GetNiceName() + "'";
					}
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				if (memberInfo is FieldInfo)
				{
					return BaseMemberValueResolverCreator.GetFieldGetter<TResult>(memberInfo as FieldInfo);
				}
				if (memberInfo is PropertyInfo)
				{
					return BaseMemberValueResolverCreator.GetPropertyGetter<TResult>(memberInfo as PropertyInfo, context.Property.ParentType.IsValueType);
				}
				return BaseMemberValueResolverCreator.GetMethodGetter<TResult>(memberInfo as MethodInfo, argSetup, context.Property.ParentType.IsValueType);
			}
			if (flag)
			{
				context.ErrorMessage = "Could not find a field, property or method with the name '" + text + "' on the type '" + parentType.GetNiceName() + "'.";
				return ValueResolverCreator.GetFailedResolverFunc<TResult>();
			}
			return null;
		}
	}
}
