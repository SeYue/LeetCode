using Sirenix.OdinInspector.Editor.ActionResolvers;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class OnInspectorInitStateUpdater : AttributeStateUpdater<OnInspectorInitAttribute>
	{
		protected override void Initialize()
		{
			ActionResolver actionResolver = ActionResolver.Get(base.Property, base.Attribute.Action);
			actionResolver.DoActionForAllSelectionIndices();
			ErrorMessage = actionResolver.ErrorMessage;
		}
	}
}
