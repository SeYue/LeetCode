namespace Sirenix.OdinInspector.Editor.Validation
{
	public static class ValidatorExtensions
	{
		public static ValidationResultType ToValidationResultType(this InfoMessageType messageType)
		{
			return messageType switch
			{
				InfoMessageType.Error => ValidationResultType.Error, 
				InfoMessageType.Warning => ValidationResultType.Warning, 
				_ => ValidationResultType.Valid, 
			};
		}
	}
}
