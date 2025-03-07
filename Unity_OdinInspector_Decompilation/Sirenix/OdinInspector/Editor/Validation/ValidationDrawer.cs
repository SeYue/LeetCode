using System;
using System.Collections.Generic;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	[DrawerPriority(0.0, 10000.0, 0.0)]
	public class ValidationDrawer<T> : OdinValueDrawer<T>, IDisposable
	{
		private List<ValidationResult> validationResults;

		private bool rerunFullValidation;

		private object shakeGroupKey;

		private ValidationComponent validationComponent;

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			ValidationComponent component = property.GetComponent<ValidationComponent>();
			if (component == null)
			{
				return false;
			}
			if (property.GetAttribute<DontValidateAttribute>() != null)
			{
				return false;
			}
			return component.ValidatorLocator.PotentiallyHasValidatorsFor(property);
		}

		protected override void Initialize()
		{
			validationComponent = base.Property.GetComponent<ValidationComponent>();
			validationComponent.ValidateProperty(ref validationResults);
			if (validationResults.Count > 0)
			{
				shakeGroupKey = UniqueDrawerKey.Create(base.Property, this);
				base.Property.Tree.OnUndoRedoPerformed += OnUndoRedoPerformed;
				base.ValueEntry.OnValueChanged += OnValueChanged;
				base.ValueEntry.OnChildValueChanged += OnChildValueChanged;
			}
			else
			{
				base.SkipWhenDrawing = true;
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Invalid comparison between Unknown and I4
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Invalid comparison between Unknown and I4
			if (validationResults.Count == 0)
			{
				CallNextDrawer(label);
				return;
			}
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
			SirenixEditorGUI.BeginShakeableGroup(shakeGroupKey);
			for (int i = 0; i < validationResults.Count; i++)
			{
				ValidationResult validationResult = validationResults[i];
				if ((int)Event.get_current().get_type() == 8 && (rerunFullValidation || validationResult.Setup.Validator.RevalidationCriteria == RevalidationCriteria.Always))
				{
					ValidationResultType resultType = validationResult.ResultType;
					validationResult.Setup.ParentInstance = base.Property.ParentValues[0];
					validationResult.Setup.Value = base.ValueEntry.Values[0];
					validationResult.RerunValidation();
					if (resultType != validationResult.ResultType && validationResult.ResultType != 0)
					{
						SirenixEditorGUI.StartShakingGroup(shakeGroupKey);
					}
				}
				if (validationResult.ResultType == ValidationResultType.Error)
				{
					SirenixEditorGUI.ErrorMessageBox(validationResult.Message);
				}
				else if (validationResult.ResultType == ValidationResultType.Warning)
				{
					SirenixEditorGUI.WarningMessageBox(validationResult.Message);
				}
				else if (validationResult.ResultType == ValidationResultType.Valid && !string.IsNullOrEmpty(validationResult.Message))
				{
					SirenixEditorGUI.InfoMessageBox(validationResult.Message);
				}
			}
			if ((int)Event.get_current().get_type() == 8)
			{
				rerunFullValidation = false;
			}
			CallNextDrawer(label);
			SirenixEditorGUI.EndShakeableGroup(shakeGroupKey);
			GUILayout.EndVertical();
		}

		public void Dispose()
		{
			if (validationResults.Count > 0)
			{
				base.Property.Tree.OnUndoRedoPerformed -= OnUndoRedoPerformed;
				base.ValueEntry.OnValueChanged -= OnValueChanged;
				base.ValueEntry.OnChildValueChanged -= OnChildValueChanged;
			}
			validationResults = null;
		}

		private void OnUndoRedoPerformed()
		{
			rerunFullValidation = true;
		}

		private void OnValueChanged(int index)
		{
			rerunFullValidation = true;
		}

		private void OnChildValueChanged(int index)
		{
			rerunFullValidation = true;
		}
	}
}
