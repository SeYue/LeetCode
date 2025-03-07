using System;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// The default method drawer that draws most buttons.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 0.11)]
	public sealed class DefaultMethodDrawer : MethodDrawer
	{
		[ShowOdinSerializedPropertiesInInspector]
		private class MethodResultInspector
		{
			[HideReferenceObjectPicker]
			[HideLabel]
			public object Value;
		}

		internal static bool DontDrawMethodParameters;

		private bool drawParameters;

		private bool hasReturnValue;

		private bool shouldDrawResult;

		private string name;

		private ButtonAttribute buttonAttribute;

		private float buttonHeight;

		private GUIStyle style;

		private GUIStyle toggleBtnStyle;

		private ValueResolver<string> labelGetter;

		private GUIContent label;

		private ButtonStyle btnStyle;

		private bool expanded;

		private Color btnColor;

		private bool hasGUIColorAttribute;

		private bool hasInvokedOnce;

		private ActionResolver buttonActionResolver;

		private ValueResolver<object> buttonValueResolver;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Expected O, but got Unknown
			expanded = false;
			buttonAttribute = base.Property.GetAttribute<ButtonAttribute>();
			buttonHeight = base.Property.Context.GetGlobal("ButtonHeight", 0f).Value;
			style = base.Property.Context.GetGlobal<GUIStyle>("ButtonStyle", (GUIStyle)null).Value;
			hasGUIColorAttribute = base.Property.GetAttribute<GUIColorAttribute>() != null;
			drawParameters = base.Property.Children.Count > 0 && !DontDrawMethodParameters && (buttonAttribute == null || buttonAttribute.DisplayParameters);
			name = base.Property.NiceName;
			label = new GUIContent(name);
			hasReturnValue = base.Property.Children.Count > 0 && base.Property.Children[base.Property.Children.Count - 1].Name == "$Result";
			shouldDrawResult = GlobalConfig<GeneralDrawerConfig>.Instance.ShowButtonResultsByDefault;
			if (buttonAttribute != null)
			{
				if (!buttonAttribute.DisplayParameters)
				{
					if (hasReturnValue)
					{
						buttonValueResolver = ValueResolver.Get<object>(base.Property, null);
					}
					else
					{
						buttonActionResolver = ActionResolver.Get(base.Property, null);
					}
				}
				btnStyle = buttonAttribute.Style;
				expanded = buttonAttribute.Expanded;
				if (!string.IsNullOrEmpty(buttonAttribute.Name))
				{
					labelGetter = ValueResolver.GetForString(base.Property, buttonAttribute.Name);
				}
				if (buttonHeight == 0f && buttonAttribute.ButtonHeight > 0)
				{
					buttonHeight = buttonAttribute.ButtonHeight;
				}
				if (buttonAttribute.DrawResultIsSet)
				{
					shouldDrawResult = buttonAttribute.DrawResult;
				}
			}
			if (!shouldDrawResult && hasReturnValue && base.Property.Children.Count == 1)
			{
				drawParameters = false;
			}
			if (style == null)
			{
				if (buttonHeight > 20f)
				{
					style = SirenixGUIStyles.Button;
				}
				else
				{
					style = EditorStyles.get_miniButton();
				}
			}
			if (drawParameters && btnStyle == ButtonStyle.FoldoutButton && !expanded)
			{
				if (buttonHeight > 20f)
				{
					style = SirenixGUIStyles.ButtonLeft;
					toggleBtnStyle = SirenixGUIStyles.ButtonRight;
				}
				else
				{
					style = EditorStyles.get_miniButtonLeft();
					toggleBtnStyle = EditorStyles.get_miniButtonRight();
				}
			}
		}

		/// <summary>
		/// Draws the property layout.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent lbl)
		{
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			if (buttonActionResolver != null && buttonActionResolver.HasError)
			{
				buttonActionResolver.DrawError();
			}
			if (buttonValueResolver != null && buttonValueResolver.HasError)
			{
				buttonValueResolver.DrawError();
			}
			if (labelGetter == null)
			{
				label = lbl;
			}
			else if (labelGetter.HasError)
			{
				labelGetter.DrawError();
			}
			btnColor = GUI.get_color();
			Color color = (hasGUIColorAttribute ? GUIColorAttributeDrawer.CurrentOuterColor : btnColor);
			GUIHelper.PushColor(color);
			float value = base.Property.Context.GetGlobal("ButtonHeight", 0f).Value;
			GUIStyle value2 = base.Property.Context.GetGlobal<GUIStyle>("ButtonStyle", (GUIStyle)null).Value;
			if ((buttonHeight != value && value != 0f) || (value2 != null && style != value2))
			{
				Initialize();
			}
			if (labelGetter != null && label != null)
			{
				string text = label.get_text();
				try
				{
					label.set_text(labelGetter.GetValue());
				}
				catch
				{
					label.set_text(text);
				}
			}
			base.Property.Label = label;
			if (!drawParameters)
			{
				DrawNormalButton();
			}
			else if (btnStyle == ButtonStyle.FoldoutButton)
			{
				if (expanded)
				{
					DrawNormalButton();
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
					DrawParameters(appendButton: false);
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				}
				else
				{
					DrawFoldoutButton();
				}
			}
			else if (btnStyle == ButtonStyle.CompactBox)
			{
				DrawCompactBoxButton();
			}
			else if (btnStyle == ButtonStyle.Box)
			{
				DrawBoxButton();
			}
			GUIHelper.PopColor();
		}

		private void DrawBoxButton()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginToolbarBoxHeader();
			if (expanded)
			{
				EditorGUILayout.LabelField(label, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			else
			{
				base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, label);
			}
			SirenixEditorGUI.EndToolbarBoxHeader();
			DrawParameters(appendButton: true);
			SirenixEditorGUI.EndToolbarBox();
		}

		private void DrawCompactBoxButton()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			SirenixEditorGUI.BeginBox();
			Rect val = SirenixEditorGUI.BeginToolbarBoxHeader().AlignRight(70f).Padding(1f);
			((Rect)(ref val)).set_height(((Rect)(ref val)).get_height() - 1f);
			GUIHelper.PushColor(btnColor);
			if (GUI.Button(val, "Invoke"))
			{
				InvokeButton();
			}
			GUIHelper.PopColor();
			if (expanded)
			{
				EditorGUILayout.LabelField(label, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			else
			{
				base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, label);
			}
			SirenixEditorGUI.EndToolbarBoxHeader();
			DrawParameters(appendButton: false);
			SirenixEditorGUI.EndToolbarBox();
		}

		private void DrawNormalButton()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			Rect val = ((buttonHeight > 0f) ? GUILayoutUtility.GetRect(GUIContent.none, style, (GUILayoutOption[])GUILayoutOptions.Height(buttonHeight)) : GUILayoutUtility.GetRect(GUIContent.none, style));
			val = EditorGUI.IndentedRect(val);
			Color color = GUI.get_color();
			GUI.set_color(btnColor);
			if (GUI.Button(val, (label != null) ? label : GUIHelper.TempContent(string.Empty), style))
			{
				InvokeButton();
			}
			GUI.set_color(color);
		}

		private void DrawFoldoutButton()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			Rect val = ((buttonHeight > 0f) ? GUILayoutUtility.GetRect(GUIContent.none, style, (GUILayoutOption[])GUILayoutOptions.Height(buttonHeight)) : GUILayoutUtility.GetRect(GUIContent.none, style));
			val = EditorGUI.IndentedRect(val);
			GUIHelper.PushColor(btnColor);
			Rect val2 = val.AlignRight(20f);
			if (GUI.Button(val2, GUIContent.none, toggleBtnStyle))
			{
				base.Property.State.Expanded = !base.Property.State.Expanded;
			}
			((Rect)(ref val)).set_width(((Rect)(ref val)).get_width() - ((Rect)(ref val2)).get_width());
			if (!base.Property.State.Expanded)
			{
				((Rect)(ref val2)).set_x(((Rect)(ref val2)).get_x() - 1f);
				((Rect)(ref val2)).set_yMin(((Rect)(ref val2)).get_yMin() - 1f);
			}
			if (base.Property.State.Expanded)
			{
				EditorIcons.TriangleDown.Draw(val2, 16f);
			}
			else
			{
				EditorIcons.TriangleLeft.Draw(val2, 16f);
			}
			if (GUI.Button(val, label, style))
			{
				InvokeButton();
			}
			GUIHelper.PopColor();
			EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
			DrawParameters(appendButton: false);
			EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
		}

		private void DrawParameters(bool appendButton)
		{
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			if (SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded || expanded))
			{
				GUILayout.Space(0f);
				for (int i = 0; i < base.Property.Children.Count; i++)
				{
					bool flag = false;
					if (hasReturnValue && i == base.Property.Children.Count - 1)
					{
						if (!shouldDrawResult || (!hasInvokedOnce && i != 0))
						{
							break;
						}
						if (i != 0)
						{
							SirenixEditorGUI.DrawThickHorizontalSeparator();
						}
						flag = true;
					}
					if (flag && !hasInvokedOnce)
					{
						GUIHelper.PushGUIEnabled(enabled: false);
					}
					base.Property.Children[i].Draw();
					if (flag && !hasInvokedOnce)
					{
						GUIHelper.PopGUIEnabled();
					}
				}
				if (appendButton)
				{
					Rect val = EditorGUILayout.BeginVertical(SirenixGUIStyles.BottomBoxPadding, (GUILayoutOption[])(object)new GUILayoutOption[0]).Expand(3f);
					SirenixEditorGUI.DrawHorizontalLineSeperator(((Rect)(ref val)).get_x(), ((Rect)(ref val)).get_y(), ((Rect)(ref val)).get_width());
					DrawNormalButton();
					EditorGUILayout.EndVertical();
				}
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		private void InvokeButton()
		{
			try
			{
				bool flag = hasReturnValue && Event.get_current().get_button() == 1;
				GUIHelper.RemoveFocusControl();
				GUIHelper.RequestRepaint();
				if (((base.Property.Info.GetMemberInfo() as MethodInfo) ?? base.Property.Info.GetMethodDelegate().Method).IsGenericMethodDefinition)
				{
					Debug.LogError((object)"Cannot invoke a generic method definition.");
					return;
				}
				if (base.Property.ParentValueProperty != null)
				{
					base.Property.ParentValueProperty.RecordForUndo("Clicked Button '" + base.Property.NiceName + "'", forceCompleteObjectUndo: true);
				}
				if (buttonAttribute != null && buttonAttribute.DirtyOnClick)
				{
					foreach (Object item in base.Property.SerializationRoot.ValueEntry.WeakValues.OfType<Object>())
					{
						InspectorUtilities.RegisterUnityObjectDirty(item);
					}
				}
				if (buttonActionResolver != null)
				{
					buttonActionResolver.DoActionForAllSelectionIndices();
				}
				else if (buttonValueResolver != null)
				{
					for (int i = 0; i < base.Property.Tree.WeakTargets.Count; i++)
					{
						object value = buttonValueResolver.GetValue(i);
						base.Property.Children[base.Property.Children.Count - 1].ValueEntry.WeakValues[i] = value;
					}
				}
				else
				{
					MethodInfo methodInfo = (MethodInfo)base.Property.Info.GetMemberInfo();
					if (methodInfo != null)
					{
						InvokeMethodInfo(methodInfo);
					}
					else
					{
						InvokeDelegate();
					}
				}
				if (flag)
				{
					object weakSmartValue = base.Property.Children[base.Property.Children.Count - 1].ValueEntry.WeakSmartValue;
					OdinEditorWindow.InspectObjectInDropDown(new MethodResultInspector
					{
						Value = weakSmartValue
					});
				}
			}
			finally
			{
				GUIUtility.ExitGUI();
			}
		}

		private void InvokeDelegate()
		{
			//IL_0122: Expected O, but got Unknown
			try
			{
				int num = (hasReturnValue ? (base.Property.Children.Count - 1) : base.Property.Children.Count);
				object[] array = new object[num];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = base.Property.Children[i].ValueEntry.WeakSmartValue;
				}
				object weakSmartValue = base.Property.Info.GetMethodDelegate().DynamicInvoke(array);
				for (int j = 0; j < array.Length; j++)
				{
					base.Property.Children[j].ValueEntry.WeakSmartValue = array[j];
				}
				if (hasReturnValue)
				{
					base.Property.Children[base.Property.Children.Count - 1].ValueEntry.WeakSmartValue = weakSmartValue;
				}
				if (!hasInvokedOnce)
				{
					base.Property.Tree.DelayActionUntilRepaint(delegate
					{
						hasInvokedOnce = true;
					});
				}
			}
			catch (TargetInvocationException ex)
			{
				if (ex.IsExitGUIException())
				{
					throw ex.AsExitGUIException();
				}
				Debug.LogException((Exception)ex);
			}
			catch (ExitGUIException val)
			{
				ExitGUIException val2 = val;
				throw val2;
			}
			catch (Exception ex2)
			{
				if (ex2.IsExitGUIException())
				{
					throw ex2.AsExitGUIException();
				}
				Debug.LogException(ex2);
			}
		}

		private void InvokeMethodInfo(MethodInfo methodInfo)
		{
			//IL_0170: Expected O, but got Unknown
			InspectorProperty parentValueProperty = base.Property.ParentValueProperty;
			ImmutableList parentValues = base.Property.ParentValues;
			int num = (hasReturnValue ? (base.Property.Children.Count - 1) : base.Property.Children.Count);
			for (int i = 0; i < parentValues.Count; i++)
			{
				object obj = parentValues[i];
				if (obj == null && !methodInfo.IsStatic)
				{
					continue;
				}
				try
				{
					object[] array = new object[num];
					for (int j = 0; j < array.Length; j++)
					{
						array[j] = base.Property.Children[j].ValueEntry.WeakSmartValue;
					}
					object weakSmartValue = ((!methodInfo.IsStatic) ? methodInfo.Invoke(obj, array) : methodInfo.Invoke(null, array));
					for (int k = 0; k < array.Length; k++)
					{
						base.Property.Children[k].ValueEntry.WeakSmartValue = array[k];
					}
					if (hasReturnValue)
					{
						base.Property.Children[base.Property.Children.Count - 1].ValueEntry.WeakSmartValue = weakSmartValue;
					}
					if (!hasInvokedOnce)
					{
						base.Property.Tree.DelayActionUntilRepaint(delegate
						{
							hasInvokedOnce = true;
						});
					}
				}
				catch (TargetInvocationException ex)
				{
					if (ex.IsExitGUIException())
					{
						throw ex.AsExitGUIException();
					}
					Debug.LogException((Exception)ex);
				}
				catch (ExitGUIException val)
				{
					ExitGUIException val2 = val;
					throw val2;
				}
				catch (Exception ex2)
				{
					if (ex2.IsExitGUIException())
					{
						throw ex2.AsExitGUIException();
					}
					Debug.LogException(ex2);
				}
				if (parentValueProperty != null && obj.GetType().IsValueType)
				{
					parentValueProperty.ValueEntry.WeakValues[i] = obj;
				}
			}
		}
	}
}
