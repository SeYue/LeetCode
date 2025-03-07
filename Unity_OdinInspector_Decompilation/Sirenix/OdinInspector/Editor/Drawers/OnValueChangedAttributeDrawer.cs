using System;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.OnValueChangedAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.OnCollectionChangedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnValueChangedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnInspectorGUIAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
	public sealed class OnValueChangedAttributeDrawer<T> : OdinAttributeDrawer<OnValueChangedAttribute, T>, IDisposable
	{
		private ActionResolver onChangeAction;

		private bool subscribedToOnUndoRedo;

		protected override void Initialize()
		{
			if (base.Attribute.InvokeOnUndoRedo)
			{
				base.Property.Tree.OnUndoRedoPerformed += OnUndoRedo;
				subscribedToOnUndoRedo = true;
			}
			onChangeAction = ActionResolver.Get(base.Property, base.Attribute.Action);
			Action<int> value = TriggerAction;
			base.ValueEntry.OnValueChanged += value;
			if (base.Attribute.IncludeChildren || typeof(T).IsValueType)
			{
				base.ValueEntry.OnChildValueChanged += value;
			}
			if (base.Attribute.InvokeOnInitialize && !onChangeAction.HasError)
			{
				onChangeAction.DoActionForAllSelectionIndices();
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (onChangeAction.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(onChangeAction.ErrorMessage);
			}
			CallNextDrawer(label);
		}

		private void OnUndoRedo()
		{
			for (int i = 0; i < base.ValueEntry.ValueCount; i++)
			{
				TriggerAction(i);
			}
		}

		private void TriggerAction(int selectionIndex)
		{
			onChangeAction.DoAction(selectionIndex);
		}

		public void Dispose()
		{
			if (subscribedToOnUndoRedo)
			{
				base.Property.Tree.OnUndoRedoPerformed -= OnUndoRedo;
			}
		}
	}
}
