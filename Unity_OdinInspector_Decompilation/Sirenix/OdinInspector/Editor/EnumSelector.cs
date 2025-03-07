using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// A feature-rich enum selector with support for flag enums.
	/// </summary>
	/// <example>
	/// <code>
	/// KeyCode someEnumValue;
	///
	/// [OnInspectorGUI]
	/// void OnInspectorGUI()
	/// {
	///     // Use the selector manually. See the documentation for OdinSelector for more information.
	///     if (GUILayout.Button("Open Enum Selector"))
	///     {
	///         EnumSelector&lt;KeyCode&gt; selector = new EnumSelector&lt;KeyCode&gt;();
	///         selector.SetSelection(this.someEnumValue);
	///         selector.SelectionConfirmed += selection =&gt; this.someEnumValue = selection.FirstOrDefault();
	///         selector.ShowInPopup(); // Returns the Odin Editor Window instance, in case you want to mess around with that as well.
	///     }
	///
	///     // Draw an enum dropdown field which uses the EnumSelector popup:
	///     this.someEnumValue = EnumSelector&lt;KeyCode&gt;.DrawEnumField(new GUIContent("My Label"), this.someEnumValue);
	/// }
	///
	/// // All Odin Selectors can be rendered anywhere with Odin. This includes the EnumSelector.
	/// EnumSelector&lt;KeyCode&gt; inlineSelector;
	///
	/// [ShowInInspector]
	/// EnumSelector&lt;KeyCode&gt; InlineSelector
	/// {
	///     get { return this.inlineSelector ?? (this.inlineSelector = new EnumSelector&lt;KeyCode&gt;()); }
	///     set { }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinSelector`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.TypeSelector" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />
	public class EnumSelector<T> : OdinSelector<T>
	{
		private static readonly StringBuilder SB = new StringBuilder();

		private static readonly Func<T, T, bool> EqualityComparer = PropertyValueEntry<T>.EqualityComparer;

		private static Color highlightLineColor = (EditorGUIUtility.get_isProSkin() ? new Color(0.5f, 1f, 0f, 1f) : new Color(0.015f, 0.68f, 0.015f, 1f));

		private static Color selectedMaskBgColor = (EditorGUIUtility.get_isProSkin() ? new Color(0.5f, 1f, 0f, 0.1f) : new Color(0.02f, 0.537f, 0f, 0.31f));

		private static readonly string title = typeof(T).Name.SplitPascalCase();

		private float maxEnumLabelWidth;

		private ulong curentValue;

		private ulong curentMouseOverValue;

		public static bool DrawSearchToolbar = true;

		private bool wasMouseDown;

		/// <summary>
		/// By default, the enum type will be drawn as the title for the selector. No title will be drawn if the string is null or empty.
		/// </summary>
		public override string Title
		{
			get
			{
				if (GlobalConfig<GeneralDrawerConfig>.Instance.DrawEnumTypeTitle)
				{
					return title;
				}
				return null;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is flag enum.
		/// </summary>
		public bool IsFlagEnum => EnumTypeUtilities<T>.IsFlagEnum;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.EnumSelector`1" /> class.
		/// </summary>
		public EnumSelector()
		{
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Expected O, but got Unknown
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Expected O, but got Unknown
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			if (!typeof(T).IsEnum)
			{
				throw new NotSupportedException(typeof(T).GetNiceFullName() + " is not an enum type.");
			}
			if (Event.get_current() != null)
			{
				string[] names = Enum.GetNames(typeof(T));
				foreach (string text in names)
				{
					maxEnumLabelWidth = Mathf.Max(maxEnumLabelWidth, SirenixGUIStyles.Label.CalcSize(new GUIContent(text)).x);
				}
				if (Title != null)
				{
					string text2 = Title + "                      ";
					maxEnumLabelWidth = Mathf.Max(maxEnumLabelWidth, SirenixGUIStyles.Label.CalcSize(new GUIContent(text2)).x);
				}
			}
		}

		/// <summary>
		/// Populates the tree with all enum values.
		/// </summary>
		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			tree.Selection.SupportsMultiSelect = IsFlagEnum;
			tree.Config.DrawSearchToolbar = DrawSearchToolbar;
			EnumTypeUtilities<T>.EnumMember[] allEnumMemberInfos = EnumTypeUtilities<T>.AllEnumMemberInfos;
			EnumTypeUtilities<T>.EnumMember[] array = allEnumMemberInfos;
			for (int i = 0; i < array.Length; i++)
			{
				EnumTypeUtilities<T>.EnumMember enumMember = array[i];
				if (!enumMember.Hide)
				{
					tree.Add(enumMember.NiceName, enumMember);
				}
			}
			if (IsFlagEnum)
			{
				tree.DefaultMenuStyle.Offset += 15f;
				if (!(from x in allEnumMemberInfos
					where x.Value != null
					select Convert.ToInt64(x.Value)).Contains(0L))
				{
					tree.MenuItems.Insert(0, new OdinMenuItem(tree, GetNoneValueString(), new EnumTypeUtilities<T>.EnumMember
					{
						Value = (T)(object)0,
						Name = "None",
						NiceName = "None",
						IsObsolete = false,
						Message = ""
					}));
				}
				tree.EnumerateTree().ForEach(delegate(OdinMenuItem x)
				{
					x.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(x.OnDrawItem, new Action<OdinMenuItem>(DrawEnumFlagItem));
				});
				DrawConfirmSelectionButton = false;
			}
			else
			{
				tree.EnumerateTree().ForEach(delegate(OdinMenuItem x)
				{
					x.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(x.OnDrawItem, new Action<OdinMenuItem>(DrawEnumItem));
				});
			}
			tree.EnumerateTree().ForEach(delegate(OdinMenuItem x)
			{
				x.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(x.OnDrawItem, new Action<OdinMenuItem>(DrawEnumInfo));
			});
		}

		private void DrawEnumInfo(OdinMenuItem obj)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Expected O, but got Unknown
			if (obj.Value is EnumTypeUtilities<T>.EnumMember)
			{
				EnumTypeUtilities<T>.EnumMember enumMember = (EnumTypeUtilities<T>.EnumMember)obj.Value;
				bool flag = !string.IsNullOrEmpty(enumMember.Message);
				if (enumMember.IsObsolete)
				{
					Rect val = obj.Rect.Padding(5f, 3f).AlignRight(16f).AlignCenterY(16f);
					GUI.DrawTexture(val, (Texture)(object)EditorIcons.TestInconclusive);
				}
				else if (flag)
				{
					Rect val2 = obj.Rect.Padding(5f, 3f).AlignRight(16f).AlignCenterY(16f);
					GUI.DrawTexture(val2, (Texture)(object)EditorIcons.ConsoleInfoIcon);
				}
				if (flag)
				{
					GUI.Label(obj.Rect, new GUIContent("", enumMember.Message));
				}
			}
		}

		private void DrawEnumItem(OdinMenuItem obj)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Invalid comparison between Unknown and I4
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Invalid comparison between Unknown and I4
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			Rect rect;
			if ((int)Event.get_current().get_type() == 0)
			{
				rect = obj.Rect;
				if (((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
				{
					obj.Select();
					Event.get_current().Use();
					wasMouseDown = true;
				}
			}
			if (wasMouseDown)
			{
				GUIHelper.RequestRepaint();
			}
			if (wasMouseDown && (int)Event.get_current().get_type() == 3)
			{
				rect = obj.Rect;
				if (((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
				{
					obj.Select();
				}
			}
			if ((int)Event.get_current().get_type() != 1)
			{
				return;
			}
			wasMouseDown = false;
			if (obj.IsSelected)
			{
				rect = obj.Rect;
				if (((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
				{
					obj.MenuTree.Selection.ConfirmSelection();
				}
			}
		}

		[OnInspectorGUI]
		[PropertyOrder(-1000f)]
		private void SpaceToggleEnumFlag()
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Invalid comparison between Unknown and I4
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Invalid comparison between Unknown and I4
			if (base.SelectionTree != OdinMenuTree.ActiveMenuTree || !IsFlagEnum || (int)Event.get_current().get_keyCode() != 32 || (int)Event.get_current().get_type() != 4 || base.SelectionTree == null)
			{
				return;
			}
			foreach (OdinMenuItem item in base.SelectionTree.Selection)
			{
				ToggleEnumFlag(item);
			}
			TriggerSelectionChanged();
			Event.get_current().Use();
		}

		/// <summary>
		/// When ShowInPopup is called, without a specified window width, this method gets called.
		/// Here you can calculate and give a good default width for the popup.
		/// The default implementation returns 0, which will let the popup window determine the width itself. This is usually a fixed value.
		/// </summary>
		protected override float DefaultWindowWidth()
		{
			return Mathf.Clamp(maxEnumLabelWidth + 50f, 160f, 400f);
		}

		private void DrawEnumFlagItem(OdinMenuItem obj)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Invalid comparison between Unknown and I4
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Invalid comparison between Unknown and I4
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
			Rect rect;
			if ((int)Event.get_current().get_type() == 0 || (int)Event.get_current().get_type() == 1)
			{
				rect = obj.Rect;
				if (((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
				{
					if ((int)Event.get_current().get_type() == 0)
					{
						ToggleEnumFlag(obj);
						TriggerSelectionChanged();
					}
					Event.get_current().Use();
				}
			}
			if ((int)Event.get_current().get_type() != 7)
			{
				return;
			}
			ulong num = (ulong)Convert.ToInt64(GetMenuItemEnumValue(obj));
			bool flag = (num & (num - 1)) == 0;
			if (num != 0L && !flag)
			{
				rect = obj.Rect;
				if (((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
				{
					curentMouseOverValue = num;
				}
				else if (num == curentMouseOverValue)
				{
					curentMouseOverValue = 0uL;
				}
			}
			bool flag2 = (num & curentValue) == num && (num != 0L || curentValue == 0);
			if (num != 0 && flag && (num & curentMouseOverValue) == num && (num != 0L || curentMouseOverValue == 0))
			{
				EditorGUI.DrawRect(obj.Rect.AlignLeft(6f).Padding(2f), highlightLineColor);
			}
			if (!(flag2 || flag))
			{
				return;
			}
			Rect val = obj.Rect.AlignLeft(30f).AlignCenter(((Texture)EditorIcons.TestPassed).get_width(), ((Texture)EditorIcons.TestPassed).get_height());
			if (flag2)
			{
				if (flag)
				{
					if (!EditorGUIUtility.get_isProSkin())
					{
						Color color = GUI.get_color();
						GUI.set_color(new Color(1f, 0.7f, 1f, 1f));
						GUI.DrawTexture(val, (Texture)(object)EditorIcons.TestPassed);
						GUI.set_color(color);
					}
					else
					{
						GUI.DrawTexture(val, (Texture)(object)EditorIcons.TestPassed);
					}
				}
				else
				{
					Rect rect2 = obj.Rect;
					rect = obj.Rect;
					EditorGUI.DrawRect(rect2.AlignTop(((Rect)(ref rect)).get_height() - (float)(EditorGUIUtility.get_isProSkin() ? 1 : 0)), selectedMaskBgColor);
				}
			}
			else
			{
				GUI.DrawTexture(val, (Texture)(object)EditorIcons.TestNormal);
			}
		}

		private void ToggleEnumFlag(OdinMenuItem obj)
		{
			ulong num = (ulong)Convert.ToInt64(GetMenuItemEnumValue(obj));
			if ((num & curentValue) == num)
			{
				curentValue = ((num == 0L) ? 0 : (curentValue & ~num));
			}
			else
			{
				curentValue |= num;
			}
			if (Event.get_current().get_clickCount() >= 2)
			{
				Event.get_current().Use();
			}
		}

		/// <summary>
		/// Gets the currently selected enum value.
		/// </summary>
		public override IEnumerable<T> GetCurrentSelection()
		{
			if (IsFlagEnum)
			{
				yield return (T)Enum.ToObject(typeof(T), curentValue);
			}
			else if (base.SelectionTree.Selection.Count > 0)
			{
				yield return (T)Enum.ToObject(typeof(T), GetMenuItemEnumValue(base.SelectionTree.Selection.Last()));
			}
		}

		/// <summary>
		/// Selects an enum.
		/// </summary>
		public override void SetSelection(T selected)
		{
			if (IsFlagEnum)
			{
				curentValue = (ulong)Convert.ToInt64(selected);
				return;
			}
			IEnumerable<OdinMenuItem> collection = from x in base.SelectionTree.EnumerateTree()
				where Convert.ToInt64(GetMenuItemEnumValue(x)) == Convert.ToInt64(selected)
				select x;
			base.SelectionTree.Selection.AddRange(collection);
		}

		private static object GetMenuItemEnumValue(OdinMenuItem item)
		{
			if (item.Value is EnumTypeUtilities<T>.EnumMember)
			{
				return ((EnumTypeUtilities<T>.EnumMember)item.Value).Value;
			}
			return 0;
		}

		/// <summary>
		/// Draws an enum selector field using the enum selector.
		/// </summary>
		public static T DrawEnumField(GUIContent label, GUIContent contentLabel, T value, GUIStyle style = null)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Invalid comparison between Unknown and I4
			SirenixEditorGUI.GetFeatureRichControlRect(label, out var controlId, out var _, out var valueRect);
			if (OdinSelector<T>.DrawSelectorButton(valueRect, contentLabel, style ?? EditorStyles.get_popup(), controlId, out Action<EnumSelector<T>> bindSelector, out Func<IEnumerable<T>> resultGetter))
			{
				EnumSelector<T> enumSelector = new EnumSelector<T>();
				if (!EditorGUI.get_showMixedValue())
				{
					enumSelector.SetSelection(value);
				}
				OdinEditorWindow odinEditorWindow = ((OdinSelector<T>)enumSelector).ShowInPopup(new Vector2(((Rect)(ref valueRect)).get_xMin(), ((Rect)(ref valueRect)).get_yMax()));
				if (EnumTypeUtilities<T>.IsFlagEnum)
				{
					odinEditorWindow.OnClose += enumSelector.SelectionTree.Selection.ConfirmSelection;
				}
				bindSelector(enumSelector);
				if ((int)Application.get_platform() == 16)
				{
					GUIUtility.ExitGUI();
				}
			}
			if (resultGetter != null)
			{
				value = resultGetter().FirstOrDefault();
			}
			return value;
		}

		/// <summary>
		/// Draws an enum selector field using the enum selector.
		/// </summary>
		public static T DrawEnumField(GUIContent label, T value, GUIStyle style = null)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			string text = ((!EditorGUI.get_showMixedValue()) ? GetValueString(value) : "—");
			return DrawEnumField(label, new GUIContent(text), value, style);
		}

		/// <summary>
		/// Draws an enum selector field using the enum selector.
		/// </summary>
		public static T DrawEnumField(Rect rect, GUIContent label, GUIContent contentLabel, T value, GUIStyle style = null)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Invalid comparison between Unknown and I4
			SirenixEditorGUI.GetFeatureRichControl(rect, out var controlId, out var _);
			if (OdinSelector<T>.DrawSelectorButton(rect, contentLabel, style ?? EditorStyles.get_popup(), controlId, out Action<EnumSelector<T>> bindSelector, out Func<IEnumerable<T>> resultGetter))
			{
				EnumSelector<T> enumSelector = new EnumSelector<T>();
				if (!EditorGUI.get_showMixedValue())
				{
					enumSelector.SetSelection(value);
				}
				OdinEditorWindow odinEditorWindow = ((OdinSelector<T>)enumSelector).ShowInPopup(new Vector2(((Rect)(ref rect)).get_xMin(), ((Rect)(ref rect)).get_yMax()));
				if (EnumTypeUtilities<T>.IsFlagEnum)
				{
					odinEditorWindow.OnClose += enumSelector.SelectionTree.Selection.ConfirmSelection;
				}
				bindSelector(enumSelector);
				if ((int)Application.get_platform() == 16)
				{
					GUIUtility.ExitGUI();
				}
			}
			if (resultGetter != null)
			{
				value = resultGetter().FirstOrDefault();
			}
			return value;
		}

		/// <summary>
		/// Draws an enum selector field using the enum selector.
		/// </summary>
		public static T DrawEnumField(Rect rect, GUIContent label, T value, GUIStyle style = null)
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			string text = ((EnumTypeUtilities<T>.IsFlagEnum && Convert.ToInt64(value) == 0L) ? GetNoneValueString() : (EditorGUI.get_showMixedValue() ? "—" : value.ToString().SplitPascalCase()));
			return DrawEnumField(rect, label, new GUIContent(text), value, style);
		}

		private static string GetNoneValueString()
		{
			string name = Enum.GetName(typeof(T), 0);
			if (name != null)
			{
				return name.SplitPascalCase();
			}
			return "None";
		}

		private static string GetValueString(T value)
		{
			EnumTypeUtilities<T>.EnumMember[] allEnumMemberInfos = EnumTypeUtilities<T>.AllEnumMemberInfos;
			for (int i = 0; i < allEnumMemberInfos.Length; i++)
			{
				EnumTypeUtilities<T>.EnumMember enumMember = allEnumMemberInfos[i];
				if (EqualityComparer(enumMember.Value, value))
				{
					return enumMember.NiceName;
				}
			}
			if (EnumTypeUtilities<T>.IsFlagEnum)
			{
				long num = Convert.ToInt64(value);
				if (num == 0L)
				{
					return GetNoneValueString();
				}
				SB.Length = 0;
				for (int j = 0; j < allEnumMemberInfos.Length; j++)
				{
					EnumTypeUtilities<T>.EnumMember enumMember2 = allEnumMemberInfos[j];
					long num2 = Convert.ToInt64(enumMember2.Value);
					if (num2 != 0L && (num & num2) == num2)
					{
						if (SB.Length > 0)
						{
							SB.Append(", ");
						}
						SB.Append(enumMember2.NiceName);
					}
				}
				return SB.ToString();
			}
			return value.ToString().SplitPascalCase();
		}
	}
}
