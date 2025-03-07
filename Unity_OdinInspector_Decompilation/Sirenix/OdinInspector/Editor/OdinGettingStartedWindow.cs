using System.IO;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// The Odin Getting Started Window
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />
	public class OdinGettingStartedWindow : OdinEditorWindow
	{
		internal static GUIStyle sectionTitleLabelStyle;

		internal static GUIStyle cardTitleStyle;

		internal static GUIStyle logoTitleStyle;

		internal static GUIStyle cardStylePadding;

		internal static GUIStyle cardStyle;

		internal static GUIStyle cardHorizontalStyle;

		private Vector2 scrollPoss;

		private float width;

		private Rect currSectionRect;

		private int currBtnCount;

		private static string editorOnlyMode;

		private SlidePageNavigationHelper<OdinGettingStartedWindowData.Page> pager;

		/// <summary>
		/// Gets a value indicating whether the window should draw a scroll view.
		/// </summary>
		public override bool UseScrollView
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		/// <summary>
		/// Shows the window.
		/// </summary>
		public static void ShowWindow()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			((EditorWindow)EditorWindow.GetWindow<OdinGettingStartedWindow>()).set_position(GUIHelper.GetEditorWindowRect().AlignCenter(715f, 660f));
		}

		/// <summary>
		/// Initialize get called by OnEnable and by OnGUI after assembly reloads
		/// which often happens when you recompile or enter and exit play mode.
		/// </summary>
		protected override void Initialize()
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			pager = new SlidePageNavigationHelper<OdinGettingStartedWindowData.Page>();
			pager.TabGroup.ExpandHeight = false;
			pager.PushPage(OdinGettingStartedWindowData.MainPage, "Overview");
			WindowPadding = new Vector4(0f, 0f, 0f, 0f);
		}

		/// <summary>
		/// Draws the Odin Editor Window.
		/// </summary>
		protected override void OnGUI()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			InitStyles();
			Rect rect = EditorGUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
			Rect position;
			if (EditorGUIUtility.get_isProSkin())
			{
				float yMax = ((Rect)(ref rect)).get_yMax();
				position = ((EditorWindow)this).get_position();
				float num = ((Rect)(ref position)).get_width();
				position = ((EditorWindow)this).get_position();
				EditorGUI.DrawRect(new Rect(0f, yMax, num, ((Rect)(ref position)).get_height()), SirenixGUIStyles.DarkEditorBackground);
			}
			else
			{
				position = ((EditorWindow)this).get_position();
				EditorGUI.DrawRect(new Rect(0f, 0f, ((Rect)(ref position)).get_width(), ((Rect)(ref rect)).get_yMax()), SirenixGUIStyles.BoxBackgroundColor);
			}
			DrawHeader();
			DrawPaging();
			EditorGUILayout.EndVertical();
			SirenixEditorGUI.DrawBorders(rect, 0, 0, 0, 1, SirenixGUIStyles.BorderColor);
			DrawPages();
		}

		[ResponsiveButtonGroup("_DefaultResponsiveButtonGroup", DefaultButtonSize = ButtonSizes.Medium)]
		private void Manual()
		{
			Application.OpenURL("https://odininspector.com/tutorials");
		}

		[LabelText("API Docs")]
		[ResponsiveButtonGroup("_DefaultResponsiveButtonGroup")]
		private void APIDocumentation()
		{
			Application.OpenURL("https://odininspector.com/documentation");
		}

		[ResponsiveButtonGroup("_DefaultResponsiveButtonGroup")]
		private void Roadmap()
		{
			Application.OpenURL("https://odininspector.com/roadmap");
		}

		[ResponsiveButtonGroup("_DefaultResponsiveButtonGroup")]
		private void IssueTracker()
		{
			Application.OpenURL("https://bitbucket.org/sirenix/odin-inspector/issues");
		}

		[ResponsiveButtonGroup("_DefaultResponsiveButtonGroup")]
		private void Support()
		{
			Application.OpenURL("https://odininspector.com/support");
		}

		[ResponsiveButtonGroup("_DefaultResponsiveButtonGroup")]
		private void Discord()
		{
			Application.OpenURL("https://discord.gg/WTYJEra");
		}

		private void DrawPaging()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = GUILayoutUtility.GetRect(0f, 25f);
			pager.DrawPageNavigation(rect);
		}

		private void DrawHeader()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Expected O, but got Unknown
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Expected O, but got Unknown
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Expected O, but got Unknown
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Invalid comparison between Unknown and I4
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Invalid comparison between Unknown and I4
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = GUILayoutUtility.GetRect(0f, 70f);
			GUI.Label(rect.AlignCenterY(45f), new GUIContent(" Odin Inspector", (Texture)(object)EditorIcons.OdinInspectorLogo), logoTitleStyle);
			if (OdinInspectorVersion.HasLicensee)
			{
				string licensee = OdinInspectorVersion.Licensee;
				GUI.Label(rect.AlignCenterY(45f).AddY(20f).AddX(80f), new GUIContent("Licensed to " + licensee), SirenixGUIStyles.LeftAlignedGreyMiniLabel);
			}
			if (editorOnlyMode == null)
			{
				editorOnlyMode = (EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled() ? "Enabled" : "Disabled");
			}
			GUIContent val = new GUIContent("Odin Inspector " + OdinInspectorVersion.BuildName + " " + OdinInspectorVersion.Version);
			GUIContent val2 = new GUIContent("Editor Only Mode: " + editorOnlyMode);
			float num = Mathf.Max(new float[3]
			{
				SirenixGUIStyles.CenteredGreyMiniLabel.CalcSize(val).x,
				SirenixGUIStyles.CenteredGreyMiniLabel.CalcSize(val2).x,
				165f
			});
			Rect val3 = rect.AlignRight(num + 10f);
			((Rect)(ref val3)).set_x(((Rect)(ref val3)).get_x() - 10f);
			((Rect)(ref val3)).set_y(((Rect)(ref val3)).get_y() + 8f);
			((Rect)(ref val3)).set_height(17f);
			if ((int)Event.get_current().get_type() == 7)
			{
				GUI.Label(val3, val2, SirenixGUIStyles.CenteredGreyMiniLabel);
			}
			((Rect)(ref val3)).set_y(((Rect)(ref val3)).get_y() + 15f);
			if ((int)Event.get_current().get_type() == 7)
			{
				GUI.Label(val3, val, SirenixGUIStyles.CenteredGreyMiniLabel);
			}
			((Rect)(ref val3)).set_y(((Rect)(ref val3)).get_y() + (((Rect)(ref val3)).get_height() + 4f));
			if (GUI.Button(val3, "View Release Notes", SirenixGUIStyles.MiniButton))
			{
				Application.OpenURL("https://odininspector.com/patch-notes");
			}
			SirenixEditorGUI.DrawHorizontalLineSeperator(((Rect)(ref rect)).get_x(), ((Rect)(ref rect)).get_y(), ((Rect)(ref rect)).get_width());
			SirenixEditorGUI.DrawHorizontalLineSeperator(((Rect)(ref rect)).get_x(), ((Rect)(ref rect)).get_yMax(), ((Rect)(ref rect)).get_width());
		}

		private void InitStyles()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected O, but got Unknown
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected O, but got Unknown
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Expected O, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Expected O, but got Unknown
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Expected O, but got Unknown
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Expected O, but got Unknown
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Expected O, but got Unknown
			object obj = sectionTitleLabelStyle;
			if (obj == null)
			{
				GUIStyle val = new GUIStyle(SirenixGUIStyles.SectionHeaderCentered);
				obj = (object)val;
				val.set_fontSize(17);
				val.set_margin(new RectOffset(0, 0, 10, 10));
			}
			sectionTitleLabelStyle = (GUIStyle)obj;
			object obj2 = cardTitleStyle;
			if (obj2 == null)
			{
				GUIStyle val2 = new GUIStyle(SirenixGUIStyles.SectionHeader);
				obj2 = (object)val2;
				val2.set_fontSize(15);
				val2.set_fontStyle((FontStyle)1);
				val2.set_margin(new RectOffset(0, 0, 0, 4));
			}
			cardTitleStyle = (GUIStyle)obj2;
			object obj3 = logoTitleStyle;
			if (obj3 == null)
			{
				GUIStyle val3 = new GUIStyle(SirenixGUIStyles.SectionHeader);
				obj3 = (object)val3;
				val3.set_fontSize(23);
				val3.set_padding(new RectOffset(20, 20, 0, 0));
				val3.set_alignment((TextAnchor)3);
			}
			logoTitleStyle = (GUIStyle)obj3;
			object obj4 = cardStylePadding;
			if (obj4 == null)
			{
				GUIStyle val4 = new GUIStyle();
				obj4 = (object)val4;
				val4.set_padding(new RectOffset(15, 15, 15, 15));
				val4.set_stretchHeight(false);
			}
			cardStylePadding = (GUIStyle)obj4;
			object obj5 = cardStyle;
			if (obj5 == null)
			{
				GUIStyle val5 = new GUIStyle(GUIStyle.op_Implicit("sv_iconselector_labelselection"));
				obj5 = (object)val5;
				val5.set_padding(new RectOffset(15, 15, 15, 15));
				val5.set_margin(new RectOffset(0, 0, 0, 0));
				val5.set_stretchHeight(false);
			}
			cardStyle = (GUIStyle)obj5;
			object obj6 = cardHorizontalStyle;
			if (obj6 == null)
			{
				GUIStyle val6 = new GUIStyle();
				obj6 = (object)val6;
				val6.set_padding(new RectOffset(5, 4, 0, 0));
			}
			cardHorizontalStyle = (GUIStyle)obj6;
		}

		private void DrawPages()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Invalid comparison between Unknown and I4
			GUIHelper.PushLabelWidth(10f);
			scrollPoss = EditorGUILayout.BeginScrollView(scrollPoss, (GUILayoutOption[])GUILayoutOptions.ExpandHeight());
			Rect val = EditorGUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
			if (width == 0f || (int)Event.get_current().get_type() == 7)
			{
				width = ((Rect)(ref val)).get_width();
			}
			pager.BeginGroup();
			foreach (SlidePageNavigationHelper<OdinGettingStartedWindowData.Page>.Page enumeratePage in pager.EnumeratePages)
			{
				if (enumeratePage.BeginPage())
				{
					DrawPage(enumeratePage.Value);
				}
				enumeratePage.EndPage();
			}
			pager.EndGroup();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			GUIHelper.PopLabelWidth();
		}

		internal void DrawFooter()
		{
			base.OnGUI();
		}

		private void DrawPage(OdinGettingStartedWindowData.Page value)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Invalid comparison between Unknown and I4
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < value.Sections.Length; i++)
			{
				OdinGettingStartedWindowData.Section section = value.Sections[i];
				Rect position = ((EditorWindow)this).get_position();
				int num = ((((Rect)(ref position)).get_width() < 470f) ? 1 : section.ColCount);
				if (section.Title != null)
				{
					GUILayout.Label(new GUIContent(section.Title), sectionTitleLabelStyle, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
				section.OnInspectorGUI(this);
				if (section.Cards.Length != 0)
				{
					float height = 0f;
					bool flag = false;
					for (int j = 0; j < section.Cards.Length; j++)
					{
						if (j % num == 0)
						{
							if (j != 0 && num != 0)
							{
								flag = false;
								EditorGUILayout.EndHorizontal();
								GUILayout.Space(10f);
							}
							Rect val = (currSectionRect = EditorGUILayout.BeginHorizontal(cardHorizontalStyle, (GUILayoutOption[])(object)new GUILayoutOption[0]));
							height = ((Rect)(ref val)).get_height();
							flag = true;
						}
						if (num == 0)
						{
							GUILayout.FlexibleSpace();
						}
						Rect val2 = EditorGUILayout.BeginVertical(cardStylePadding, (GUILayoutOption[])GUILayoutOptions.Width(width / (float)num - 12f));
						((Rect)(ref val2)).set_height(height);
						if ((int)Event.get_current().get_type() == 7)
						{
							if (section.Cards[j].Style != null)
							{
								section.Cards[j].Style.Draw(val2, GUIContent.none, 0);
							}
							else
							{
								GUIHelper.PushColor(new Color(1f, 1f, 1f, EditorGUIUtility.get_isProSkin() ? 0.25f : 0.45f));
								cardStyle.Draw(val2, GUIContent.none, 0);
								GUIHelper.PopColor();
							}
						}
						DrawCard(section.Cards[j]);
						EditorGUILayout.EndVertical();
						if (j % num == 0)
						{
							GUILayout.FlexibleSpace();
						}
					}
					if (flag)
					{
						EditorGUILayout.EndHorizontal();
					}
				}
				GUILayout.Space(8f);
				if (i != value.Sections.Length - 1)
				{
					SirenixEditorGUI.DrawThickHorizontalSeparator(10f, 0f);
				}
			}
		}

		private void DrawCard(OdinGettingStartedWindowData.Card card)
		{
			if (card.Title != null)
			{
				GUILayout.Label(card.Title, cardTitleStyle, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			if (card.Description != null)
			{
				GUILayout.Label(card.Description, SirenixGUIStyles.MultiLineLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			if (card.Title != null || card.Description != null)
			{
				GUILayout.Space(5f);
			}
			currBtnCount = 0;
			bool flag = card.AssetPathFromPackage != null && !File.Exists(card.AssetPathFromPackage);
			if (flag)
			{
				GUIHelper.PushGUIEnabled(enabled: false);
			}
			for (int i = 0; i < card.CustomActions.Length; i++)
			{
				OdinGettingStartedWindowData.BtnAction action = card.CustomActions[i];
				if (Button(action.Name))
				{
					UnityEditorEventUtility.DelayAction(delegate
					{
						action.Action();
					});
				}
			}
			if (card.SubPage != null && Button(card.SubPageTitle ?? card.SubPage.Title))
			{
				pager.PushPage(card.SubPage, card.SubPage.Title);
			}
			if (flag)
			{
				GUIHelper.PopGUIEnabled();
			}
			if (!flag || card.Package == null)
			{
				return;
			}
			bool enabled = File.Exists(card.Package);
			string txt = "Import " + Path.GetFileNameWithoutExtension(card.Package);
			GUIHelper.PushGUIEnabled(enabled);
			if (Button(txt))
			{
				UnityEditorEventUtility.DelayAction(delegate
				{
					AssetDatabase.ImportPackage(card.Package, true);
				});
			}
			GUIHelper.PopGUIEnabled();
		}

		private bool Button(string txt)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = GUILayoutUtility.GetRect(0f, 26f);
			((Rect)(ref rect)).set_y(((Rect)(ref currSectionRect)).get_yMax() - 15f);
			((Rect)(ref rect)).set_y(((Rect)(ref rect)).get_y() - (float)(++currBtnCount * 26));
			((Rect)(ref rect)).set_height(22f);
			return GUI.Button(rect, txt, SirenixGUIStyles.Button);
		}
	}
}
