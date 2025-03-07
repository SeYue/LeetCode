using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ValidateInputAttribute), "ValidateInput is used to display error boxes in case of invalid values.\nIn this case the GameObject must have a MeshRenderer component.")]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	internal class ValidateInputExamples
	{
		[HideLabel]
		[Title("Default message", "You can just provide a default message that is always used", TitleAlignments.Left, true, true)]
		[ValidateInput("MustBeNull", "This field should be null.", InfoMessageType.Error)]
		public MyScriptyScriptableObject DefaultMessage;

		[Space(12f)]
		[HideLabel]
		[Title("Dynamic message", "Or the validation method can dynamically provide a custom message", TitleAlignments.Left, true, true)]
		[ValidateInput("HasMeshRendererDynamicMessage", "Prefab must have a MeshRenderer component", InfoMessageType.Error)]
		public GameObject DynamicMessage;

		[Space(12f)]
		[HideLabel]
		[Title("Dynamic message type", "The validation method can also control the type of the message", TitleAlignments.Left, true, true)]
		[ValidateInput("HasMeshRendererDynamicMessageAndType", "Prefab must have a MeshRenderer component", InfoMessageType.Error)]
		public GameObject DynamicMessageAndType;

		[Space(8f)]
		[HideLabel]
		[InfoBox("Change GameObject value to update message type", InfoMessageType.None, null)]
		public InfoMessageType MessageType;

		[Space(12f)]
		[HideLabel]
		[Title("Dynamic default message", "Use $ to indicate a member string as default message", TitleAlignments.Left, true, true)]
		[ValidateInput("AlwaysFalse", "$Message", InfoMessageType.Warning)]
		public string Message = "Dynamic ValidateInput message";

		private bool AlwaysFalse(string value)
		{
			return false;
		}

		private bool MustBeNull(MyScriptyScriptableObject scripty)
		{
			return (Object)(object)scripty == (Object)null;
		}

		private bool HasMeshRendererDefaultMessage(GameObject gameObject)
		{
			if ((Object)(object)gameObject == (Object)null)
			{
				return true;
			}
			return (Object)(object)gameObject.GetComponentInChildren<MeshRenderer>() != (Object)null;
		}

		private bool HasMeshRendererDynamicMessage(GameObject gameObject, ref string errorMessage)
		{
			if ((Object)(object)gameObject == (Object)null)
			{
				return true;
			}
			if ((Object)(object)gameObject.GetComponentInChildren<MeshRenderer>() == (Object)null)
			{
				errorMessage = "\"" + ((Object)gameObject).get_name() + "\" must have a MeshRenderer component";
				return false;
			}
			return true;
		}

		private bool HasMeshRendererDynamicMessageAndType(GameObject gameObject, ref string errorMessage, ref InfoMessageType? messageType)
		{
			if ((Object)(object)gameObject == (Object)null)
			{
				return true;
			}
			if ((Object)(object)gameObject.GetComponentInChildren<MeshRenderer>() == (Object)null)
			{
				errorMessage = "\"" + ((Object)gameObject).get_name() + "\" should have a MeshRenderer component";
				messageType = MessageType;
				return false;
			}
			return true;
		}
	}
}
