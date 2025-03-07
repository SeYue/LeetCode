using System.IO;
using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public sealed class FilePathValidator : AttributeValidator<FilePathAttribute, string>
	{
		private bool requireExistingPath;

		private ValueResolver<string> parentPathProvider;

		protected override void Initialize()
		{
			requireExistingPath = base.Attribute.RequireExistingPath;
			if (requireExistingPath)
			{
				parentPathProvider = ValueResolver.GetForString(base.Property, base.Attribute.ParentFolder);
			}
		}

		protected override void Validate(ValidationResult result)
		{
			if (requireExistingPath)
			{
				string text = base.ValueEntry.SmartValue ?? string.Empty;
				string text2 = parentPathProvider.GetValue() ?? string.Empty;
				if (!string.IsNullOrEmpty(text2))
				{
					text = Path.Combine(text2, text);
				}
				if (File.Exists(text))
				{
					result.ResultType = ValidationResultType.Valid;
					return;
				}
				result.ResultType = ValidationResultType.Error;
				result.Message = "The path does not exist.";
			}
			else
			{
				result.ResultType = ValidationResultType.IgnoreResult;
			}
		}
	}
}
