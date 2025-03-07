using System;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public struct ValidationSetup
	{
		public Validator Validator;

		public MemberInfo Member;

		public object Value;

		public object ParentInstance;

		public object Root;

		[Obsolete("There is no longer any strict distinction between value and member validation, as validation is run on properties instead.", false)]
		public ValidationKind Kind;
	}
}
