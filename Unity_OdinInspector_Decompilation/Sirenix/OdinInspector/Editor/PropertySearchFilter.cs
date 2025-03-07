using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class PropertySearchFilter
	{
		protected static readonly Func<string, string, bool> FuzzyStringMatcher = FuzzySearch.Contains;

		protected static readonly Func<string, string, bool> ExactStringMatcher = (string search, string str) => str.Contains(search);

		public bool Recursive = true;

		public bool UseFuzzySearch = true;

		public SearchFilterOptions FilterOptions = SearchFilterOptions.All;

		public Func<InspectorProperty, string, bool> MatchFunctionOverride;

		public InspectorProperty Property;

		public string SearchTerm;

		public List<SearchResult> SearchResults;

		protected string searchFieldControlName = "PropertySearchFilter_" + Guid.NewGuid().ToString();

		public bool HasSearchResults => SearchResults != null;

		public PropertySearchFilter()
		{
		}

		public PropertySearchFilter(InspectorProperty property)
		{
			Property = property;
		}

		public PropertySearchFilter(InspectorProperty property, SearchableAttribute config)
		{
			Property = property;
			UseFuzzySearch = config.FuzzySearch;
			Recursive = config.Recursive;
			FilterOptions = config.FilterOptions;
		}

		public virtual void UpdateSearch(string searchFilter)
		{
			SearchTerm = searchFilter;
			UpdateSearch();
		}

		public virtual void UpdateSearch()
		{
			SearchResults = Search(SearchTerm);
		}

		public virtual List<SearchResult> Search(string searchTerm)
		{
			if (string.IsNullOrEmpty(SearchTerm))
			{
				return null;
			}
			List<SearchResult> list = new List<SearchResult>();
			if (Recursive)
			{
				foreach (InspectorProperty child in Property.Children)
				{
					list.AddRange(Search(searchTerm, child));
				}
				return list;
			}
			foreach (InspectorProperty child2 in Property.Children)
			{
				if (IsMatch(child2, searchTerm))
				{
					list.Add(new SearchResult
					{
						MatchedProperty = child2
					});
				}
			}
			return list;
		}

		protected virtual IEnumerable<SearchResult> Search(string searchTerm, InspectorProperty property)
		{
			if (IsMatch(property, searchTerm))
			{
				SearchResult searchResult = new SearchResult();
				searchResult.MatchedProperty = property;
				foreach (InspectorProperty child in property.Children)
				{
					searchResult.ChildResults.AddRange(Search(searchTerm, child));
				}
				yield return searchResult;
				yield break;
			}
			foreach (InspectorProperty child2 in property.Children)
			{
				foreach (SearchResult item in Search(searchTerm, child2))
				{
					yield return item;
				}
			}
		}

		public virtual bool IsMatch(InspectorProperty property, string searchTerm)
		{
			if (MatchFunctionOverride != null)
			{
				return MatchFunctionOverride(property, searchTerm);
			}
			if (property.Name == "InternalOnInspectorGUI")
			{
				return false;
			}
			if (property.Info.PropertyType == PropertyType.Group && (property.Name == "#_DefaultTabGroup" || property.Name == "#_DefaultBoxGroup"))
			{
				return false;
			}
			Func<string, string, bool> func = (UseFuzzySearch ? FuzzyStringMatcher : ExactStringMatcher);
			if (HasSearchFlag(SearchFilterOptions.PropertyName) && func(searchTerm, property.Name))
			{
				return true;
			}
			if (HasSearchFlag(SearchFilterOptions.PropertyNiceName) && func(searchTerm, property.NiceName))
			{
				return true;
			}
			if (property.ValueEntry != null)
			{
				if (HasSearchFlag(SearchFilterOptions.TypeOfValue) && func(searchTerm, property.ValueEntry.TypeOfValue.GetNiceFullName()))
				{
					return true;
				}
				object weakSmartValue = property.ValueEntry.WeakSmartValue;
				if (HasSearchFlag(SearchFilterOptions.ISearchFilterableInterface) && weakSmartValue is ISearchFilterable)
				{
					return (weakSmartValue as ISearchFilterable).IsMatch(searchTerm);
				}
				if (HasSearchFlag(SearchFilterOptions.ValueToString))
				{
					string arg = ((weakSmartValue == null) ? "null" : weakSmartValue.ToString());
					if (func(searchTerm, arg))
					{
						return true;
					}
				}
			}
			return false;
		}

		public virtual bool HasSearchFlag(SearchFilterOptions flag)
		{
			return (FilterOptions & flag) == flag;
		}

		public virtual void DrawSearchResults()
		{
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			if (SearchResults == null)
			{
				return;
			}
			InspectorProperty inspectorProperty = Property;
			for (int i = 0; i < SearchResults.Count; i++)
			{
				SearchResult searchResult = SearchResults[i];
				searchResult.MatchedProperty.Update();
				bool flag = false;
				if (searchResult.MatchedProperty.Parent != null && searchResult.MatchedProperty.DrawCount == 0)
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
					flag = true;
					if (searchResult.MatchedProperty.Parent != inspectorProperty)
					{
						InspectorProperty parent = searchResult.MatchedProperty.Parent;
						string text = BuildNiceRelativePath("", parent);
						while (true)
						{
							parent = parent.Parent;
							if (parent == null || parent == Property)
							{
								break;
							}
							text = BuildNiceRelativePath(text, parent);
						}
						Rect controlRect;
						if (!string.IsNullOrEmpty(text))
						{
							controlRect = EditorGUILayout.GetControlRect((GUILayoutOption[])(object)new GUILayoutOption[0]);
							GUI.Label(controlRect, GUIHelper.TempContent(text), EditorStyles.get_miniBoldLabel());
						}
						else
						{
							controlRect = EditorGUILayout.GetControlRect(true, 2f, (GUILayoutOption[])(object)new GUILayoutOption[0]);
						}
						SirenixEditorGUI.DrawHorizontalLineSeperator(((Rect)(ref controlRect)).get_xMin(), ((Rect)(ref controlRect)).get_yMax() - 0.5f, ((Rect)(ref controlRect)).get_width());
						GUILayout.Space(3f);
					}
				}
				DrawSearchResult(searchResult);
				if (flag)
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				}
				inspectorProperty = searchResult.MatchedProperty.Parent;
			}
		}

		private static string BuildNiceRelativePath(string path, InspectorProperty addProperty)
		{
			if (addProperty.IsTreeRoot)
			{
				return path;
			}
			if (addProperty.Info.PropertyType == PropertyType.Group && (addProperty.Name == "#_DefaultTabGroup" || addProperty.Name == "#_DefaultBoxGroup"))
			{
				return path;
			}
			string text = addProperty.NiceName;
			if (addProperty.Info.PropertyType == PropertyType.Group)
			{
				text = text.TrimStart('#');
			}
			if (string.IsNullOrEmpty(path))
			{
				return text;
			}
			return text + " > " + path;
		}

		public virtual void DrawSearchResult(SearchResult result)
		{
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			result.MatchedProperty.Update();
			if (result.MatchedProperty.DrawCount == 0)
			{
				result.MatchedProperty.Draw();
			}
			InspectorProperty inspectorProperty = null;
			for (int i = 0; i < result.ChildResults.Count; i++)
			{
				SearchResult searchResult = result.ChildResults[i];
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				if (searchResult.MatchedProperty.Parent != result.MatchedProperty && searchResult.MatchedProperty.Parent != inspectorProperty)
				{
					InspectorProperty parent = searchResult.MatchedProperty.Parent;
					string text = BuildNiceRelativePath("", parent);
					while (true)
					{
						parent = parent.Parent;
						if (parent == null || parent == Property)
						{
							break;
						}
						text = BuildNiceRelativePath(text, parent);
					}
					Rect val = EditorGUILayout.GetControlRect((GUILayoutOption[])(object)new GUILayoutOption[0]).AddXMin(GUIHelper.CurrentIndentAmount);
					GUI.Label(val, GUIHelper.TempContent(text), SirenixGUIStyles.LeftAlignedGreyMiniLabel);
					SirenixEditorGUI.DrawHorizontalLineSeperator(((Rect)(ref val)).get_xMin(), ((Rect)(ref val)).get_yMax() - 0.5f, ((Rect)(ref val)).get_width());
					inspectorProperty = searchResult.MatchedProperty.Parent;
				}
				DrawSearchResult(searchResult);
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
		}

		public virtual void DrawDefaultSearchFieldLayout(GUIContent label)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			Rect controlRect = EditorGUILayout.GetControlRect(label != null, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			controlRect = ((label == null) ? controlRect.AddXMin(GUIHelper.CurrentIndentAmount) : EditorGUI.PrefixLabel(controlRect, label));
			string text = SirenixEditorGUI.SearchField(controlRect, SearchTerm, forceFocus: false, searchFieldControlName);
			if (text != SearchTerm)
			{
				SearchTerm = text;
				Property.Tree.DelayActionUntilRepaint(delegate
				{
					UpdateSearch();
					GUIHelper.RequestRepaint();
				});
			}
			Rect controlRect2 = EditorGUILayout.GetControlRect(true, 3f, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			SirenixEditorGUI.DrawThickHorizontalSeperator(controlRect2.AddXMin(GUIHelper.CurrentIndentAmount));
			GUILayout.Space(2f);
		}
	}
}
