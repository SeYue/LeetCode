using System.Collections.Generic;

public static class FxPath
{
	static Dictionary<EFxType, string> m_fxDict = new Dictionary<EFxType, string>()
	{
		{EFxType.Fx_Expand, "Assets/AssetBundle/Prefab/Fx/Fx_Expand.prefab"},
		{EFxType.Fx_NotToWork, "Assets/AssetBundle/Prefab/Fx/Fx_NotToWork.prefab"},
		{EFxType.Fx_Place, "Assets/AssetBundle/Prefab/Fx/Fx_Place.prefab"},
		{EFxType.Fx_Synthesis, "Assets/AssetBundle/Prefab/Fx/Fx_Synthesis.prefab" },
		{EFxType.Fx_SelectBuilding, "Assets/AssetBundle/Prefab/Fx/Fx_SelectBuilding.prefab" },
		{EFxType.Fx_SelectBuilding_House, "Assets/AssetBundle/Prefab/Fx/Fx_SelectBuilding_House.prefab" },
		{EFxType.Fx_SelectBuilding_House_Floor, "Assets/AssetBundle/Prefab/Fx/Fx_SelectBuilding_House_Floor.prefab" },
		{EFxType.Fx_SelectBuilding_House_Res, "Assets/AssetBundle/Prefab/Fx/Fx_SelectBuilding_House_Res.prefab" },
		{EFxType.Fx_Arrow , "Assets/AssetBundle/Prefab/Fx/Fx_Arrow.prefab"},
		{EFxType.Fx_SelectBuilding_Buff, "Assets/AssetBundle/Prefab/Fx/Fx_SelectBuilding_Buff.prefab" },
		{EFxType.Fx_SelectBuilding_Buff_Floor,"Assets/AssetBundle/Prefab/Fx/Fx_SelectBuilding_Buff_Floor.prefab" },
		{EFxType.Fx_SelectBuilding_Buff_House, "Assets/AssetBundle/Prefab/Fx/Fx_SelectBuilding_Buff_House.prefab" },
		{EFxType.Fx_SelectBuilding_1, "Assets/AssetBundle/Prefab/Fx/Fx_SelectBuilding_1.prefab" },
		{EFxType.Fx_Synthetic_Connection, "Assets/AssetBundle/Prefab/Fx/Fx_Synthetic_Connection.prefab" },
		{EFxType.Fx_SelectBuilding_Synthetic, "Assets/AssetBundle/Prefab/Fx/Fx_SelectBuilding_Synthetic.prefab" },
		{EFxType.Fx_Synthetic_Connection_Big, "Assets/AssetBundle/Prefab/Fx/Fx_Synthetic_Connection_Big.prefab" },
		{EFxType.Fx_Gold_Finish, "Assets/AssetBundle/Prefab/Fx/UI/Fx_Gold_Finish.prefab" },
		{EFxType.Fx_Other_Finish, "Assets/AssetBundle/Prefab/Fx/UI/Fx_Other_Finish.prefab" },
		{EFxType.Fx_Crystal_Finish, "Assets/AssetBundle/Prefab/Fx/UI/Fx_Crystal_Finish.prefab" },
		{EFxType.Fx_Forest_Unlock, "Assets/AssetBundle/Prefab/Fx/UI/Fx_Forest_Unlock.prefab"},
		{EFxType.Fx_Sea_Unlock, "Assets/AssetBundle/Prefab/Fx/UI/Fx_Sea_Unlock.prefab"},
		{EFxType.Fx_SnowMountain_Unlock, "Assets/AssetBundle/Prefab/Fx/UI/Fx_SnowMountain_Unlock.prefab"},
		{EFxType.Fx_Volcano_Unlock, "Assets/AssetBundle/Prefab/Fx/UI/Fx_Volcano_Unlock.prefab" },
		{EFxType.Fx_Open_Briefcase_Finish, "Assets/AssetBundle/Prefab/Fx/UI/Fx_Open_Briefcase_Finish.prefab" },
		{EFxType.Fx_Level_Up, "Assets/AssetBundle/Prefab/Fx/UI/Fx_Level_Up.prefab" },
		{EFxType.Fx_Land_Unlock, "Assets/AssetBundle/Prefab/Fx/UI/Fx_Land_Unlock.prefab" },
		{EFxType.Fx_Building_Unlock, "Assets/AssetBundle/Prefab/Fx/UI/Fx_Building_Unlock.prefab" },
		{EFxType.Fx_PeopleSign_Finish, "Assets/AssetBundle/Prefab/Fx/Fx_PeopleSign_Finish.prefab" },
		{EFxType.Fx_Combination , "Assets/AssetBundle/Prefab/Fx/Fx_Combination.prefab" },
		{EFxType.Fx_Combination_Line, "Assets/AssetBundle/Prefab/Fx/Fx_Combination_Line.prefab" },
		{EFxType.Fx_Cross_Up_Arrow, "Assets/AssetBundle/Prefab/Fx/Fx_Cross_Up_Arrow.prefab" },
		{EFxType.Fx_Book_Up_Arrow, "Assets/AssetBundle/Prefab/Fx/Fx_Book_Up_Arrow.prefab" },
		{EFxType.Fx_Park_Up_Arrow, "Assets/AssetBundle/Prefab/Fx/Fx_Park_Up_Arrow.prefab" },
		{EFxType.Fx_Speed_Up_Arrow, "Assets/AssetBundle/Prefab/Fx/Fx_Speed_Up_Arrow.prefab" },
		{EFxType.Fx_Speed_Up, "Assets/AssetBundle/Prefab/Fx/Fx_Speed_Up.prefab" },
		{EFxType.Fx_Cross_Up, "Assets/AssetBundle/Prefab/Fx/Fx_Cross_Up.prefab" },
		{EFxType.Fx_Book_Up, "Assets/AssetBundle/Prefab/Fx/Fx_Book_Up.prefab" },
		{EFxType.Fx_Park_Up, "Assets/AssetBundle/Prefab/Fx/Fx_Park_Up.prefab" },
		{EFxType.Fx_Rage_Accelerate, "Assets/AssetBundle/Prefab/Fx/Fx_Rage_Accelerate.prefab" },
		{EFxType.Fx_Rage_Strong, "Assets/AssetBundle/Prefab/Fx/Fx_Rage_Strong.prefab" },
		{EFxType.Fx_Unlocking_Area, "Assets/AssetBundle/Prefab/Fx/Fx_Unlocking_Area.prefab" },
		{EFxType.Fx_Unlocking_Area_Sign, "Assets/AssetBundle/Prefab/Stages/brand.prefab" },
		{EFxType.Fx_Violet, "Assets/AssetBundle/Prefab/Fx/Fx_Violet.prefab" },
		{EFxType.Fx_Combination_First, "Assets/AssetBundle/Prefab/Fx/Fx_Combination_First.prefab" },
		{EFxType.Fx_Synthesis_Hint, "Assets/AssetBundle/Prefab/Fx/Fx_Synthesis_Hint.prefab" },
		{EFxType.Fx_Long_Press, "Assets/AssetBundle/Prefab/Fx/UI/Fx_Long_Press.prefab" },
		{EFxType.Fx_SelectBuilding_Move, "Assets/AssetBundle/Prefab/Fx/Fx_SelectBuilding_Move.prefab" },
		{EFxType.Fx_Harvest, "Assets/AssetBundle/Prefab/Fx/Fx_Harvest.prefab" },
		{EFxType.base_floor, "Assets/AssetBundle/Prefab/Stages/base_floor.prefab" },
		{EFxType.base1, "Assets/AssetBundle/Prefab/Stages/base1.prefab" },
		{EFxType.Robot, "Assets/AssetBundle/Prefab/Citizens/Robot.prefab" },
		{EFxType.Fx_Robot_Appear, "Assets/AssetBundle/Prefab/Fx/Fx_Robot_Appear.prefab" },
		{EFxType.Fx_Robot_Disappear, "Assets/AssetBundle/Prefab/Fx/Fx_Robot_Disappear.prefab" },
		{EFxType.Fx_Obstacle, "Assets/AssetBundle/Prefab/Fx/Fx_Obstacle.prefab" },
		{EFxType.Fx_Expand_Finish, "Assets/AssetBundle/Prefab/Fx/Fx_Expand_Finish.prefab" },
		{EFxType.Fx_Quality, "Assets/AssetBundle/Prefab/Fx/Fx_Quality.prefab" },
		{EFxType.Fx_Quality_1, "Assets/AssetBundle/Prefab/Fx/Fx_Quality_1.prefab" },
		{EFxType.Fx_Fire, "Assets/AssetBundle/Prefab/Fx/Fx_Fire.prefab" },
		{EFxType.Fx_Fire_Appear, "Assets/AssetBundle/Prefab/Fx/Fx_Fire_Appear.prefab" },
		{EFxType.Fx_WaterFall, "Assets/AssetBundle/Prefab/Fx/Fx_WaterFall.prefab" },
		{EFxType.Fx_Up, "Assets/AssetBundle/Prefab/Fx/UI/Fx_Up.prefab" },
	};

	public static string GetFxPath(EFxType type)
	{
		if (type == EFxType.None)
		{
			return string.Empty;
		}
		string path = string.Empty;
		if (m_fxDict.TryGetValue(type, out path))
		{
			return path;
		}
		else
		{
			Debugger.LogError("[wei]fx path is null:{0}", type);
			return path;
		}
	}
}

public enum EFxType
{
	None,
	Fx_Expand,
	Fx_NotToWork,
	Fx_Synthesis,
	Fx_Place,
	Fx_PeopleSign_Finish,
	Fx_SelectBuilding,
	Fx_SelectBuilding_House,
	Fx_SelectBuilding_House_Floor,
	Fx_SelectBuilding_House_Res,
	Fx_Arrow,
	Fx_SelectBuilding_Buff,
	Fx_SelectBuilding_Buff_Floor,
	Fx_SelectBuilding_Buff_House,
	Fx_Gold_Finish,
	Fx_Other_Finish,
	Fx_Crystal_Finish,
	Fx_Forest_Unlock,
	Fx_Sea_Unlock,
	Fx_SnowMountain_Unlock,
	Fx_Volcano_Unlock,
	Fx_SelectBuilding_1,
	Fx_Synthetic_Connection,
	Fx_Synthetic_Connection_Big,
	Fx_Level_Up,
	Fx_SelectBuilding_Synthetic,
	Fx_Open_Briefcase_Finish,
	Fx_Land_Unlock,
	Fx_Building_Unlock,
	Fx_Combination_Line,
	Fx_Combination,
	Fx_Cross_Up,
	Fx_Park_Up,
	Fx_Book_Up,
	Fx_Cross_Up_Arrow,
	Fx_Park_Up_Arrow,
	Fx_Book_Up_Arrow,
	Fx_Rage_Accelerate,
	Fx_Unlocking_Area,
	Fx_Unlocking_Area_Sign,
	Fx_Speed_Up_Arrow,
	Fx_Speed_Up,
	Fx_Rage_Strong,
	Fx_Violet,
	Fx_Combination_First,
	Fx_Synthesis_Hint,
	Fx_SelectBuilding_Move,
	Fx_Harvest,
	Fx_Long_Press,
	base_floor,
	base1,
	Robot,
	Fx_Robot_Appear,
	Fx_Robot_Disappear,
	Fx_Obstacle,
	Fx_Expand_Finish,
	Fx_Quality,
	Fx_Quality_1,
	Fx_Fire,
	Fx_Fire_Appear,
	Fx_WaterFall,
	Fx_Up,
}