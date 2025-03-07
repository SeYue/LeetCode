using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities.Editor.Expressions;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Type property drawer
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DrawerPriority(0.0, 0.0, 2001.0)]
	public class TypeDrawer<T> : OdinValueDrawer<T> where T : Type
	{
		private static readonly TwoWaySerializationBinder Binder = new DefaultSerializationBinder();

		public string TypeNameTemp;

		public bool IsValid = true;

		public string UniqueControlName;

		public bool WasFocusedControl;

		protected override void Initialize()
		{
			UniqueControlName = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Invalid comparison between Unknown and I4
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<T> entry = base.ValueEntry;
			if (!IsValid)
			{
				GUIHelper.PushColor(Color.get_red());
			}
			GUI.SetNextControlName(UniqueControlName);
			Rect val = EditorGUILayout.GetControlRect((GUILayoutOption[])(object)new GUILayoutOption[0]);
			if (label != null)
			{
				val = EditorGUI.PrefixLabel(val, label);
			}
			Rect rect = val;
			Rect val2 = val.AlignRight(18f);
			EditorGUIUtility.AddCursorRect(val2, (MouseCursor)0);
			if (GUI.Button(val2, GUIContent.none, GUIStyle.get_none()))
			{
				TypeSelector typeSelector = new TypeSelector(AssemblyTypeFlags.All, supportsMultiSelect: false);
				typeSelector.SelectionConfirmed += delegate(IEnumerable<Type> t)
				{
					Type type2 = t.FirstOrDefault();
					entry.Property.Tree.DelayAction(delegate
					{
						entry.WeakSmartValue = type2;
						IsValid = true;
						entry.ApplyChanges();
					});
				};
				typeSelector.SetSelection(entry.SmartValue);
				typeSelector.ShowInPopup(val, 350f);
			}
			if ((int)Event.get_current().get_type() == 8)
			{
				TypeNameTemp = ((entry.SmartValue != null) ? Binder.BindToName(entry.SmartValue) : null);
			}
			EditorGUI.BeginChangeCheck();
			TypeNameTemp = SirenixEditorFields.DelayedTextField(rect, TypeNameTemp);
			EditorIcons.TriangleDown.Draw(val2);
			if (!IsValid)
			{
				GUIHelper.PopColor();
			}
			bool flag = GUI.GetNameOfFocusedControl() == UniqueControlName;
			bool flag2 = false;
			if (flag != WasFocusedControl)
			{
				flag2 = !flag;
				WasFocusedControl = flag;
			}
			if (EditorGUI.EndChangeCheck())
			{
				if (TypeNameTemp == null || string.IsNullOrEmpty(TypeNameTemp.Trim()))
				{
					entry.SmartValue = null;
					IsValid = true;
				}
				else
				{
					Type type = Binder.BindToType(TypeNameTemp);
					if (type == null)
					{
						type = AssemblyUtilities.GetTypeByCachedFullName(TypeNameTemp);
					}
					if (type == null)
					{
						ExpressionUtility.TryParseTypeNameAsCSharpIdentifier(TypeNameTemp, out type);
					}
					if (type == null)
					{
						IsValid = false;
					}
					else
					{
						entry.WeakSmartValue = type;
						IsValid = true;
					}
				}
			}
			if (flag2)
			{
				TypeNameTemp = ((entry.SmartValue == null) ? "" : Binder.BindToName(entry.SmartValue));
				IsValid = true;
			}
		}
	}
}
