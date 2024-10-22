//自动生成的
using System;
using System.Collections;
using System.Collections.Generic;
using LocStr = FH.LocKey;

namespace FH{

    public enum EItemType
    {
		/// <summary>
		/// 无
		/// </summary>
		None = 0,
		/// <summary>
		/// 武器
		/// </summary>
		Weapon = 1,
		/// <summary>
		/// 消耗品
		/// </summary>
		Cosume = 2,
	}

    public enum EItemSubType
    {
		/// <summary>
		/// 无
		/// </summary>
		None = 0,
		/// <summary>
		/// 手枪
		/// </summary>
		ShotGun = 1,
		/// <summary>
		/// 加农炮
		/// </summary>
		Cannon = 2,
	}

    public enum EItemQuality
    {
		/// <summary>
		/// 普通
		/// </summary>
		None = 0,
		/// <summary>
		/// 灰色
		/// </summary>
		Gray = 2,
		/// <summary>
		/// 绿色
		/// </summary>
		Green = 3,
		/// <summary>
		/// 紫色
		/// </summary>
		Purple = 4,
	}

    public enum EItemFlag
    {
		/// <summary>
		/// 无
		/// </summary>
		None = 0,
		/// <summary>
		/// 可堆叠
		/// </summary>
		Stack = 1,
		/// <summary>
		/// 可删除
		/// </summary>
		CanDelete = 2,
	}

    [System.Serializable]
    public sealed partial class TItemData 
    {
		/// <summary>
		/// PK
		/// 物品ID
		/// </summary>
		public int Id;
		/// <summary>
		/// 名称
		/// </summary>
		public LocId Name;
		/// <summary>
		/// 类型
		/// </summary>
		public EItemType Type;
		/// <summary>
		/// 子类
		/// </summary>
		public EItemSubType SubType;
		/// <summary>
		/// 品质
		/// </summary>
		public EItemQuality Quality;
		/// <summary>
		/// 测试Pair
		/// </summary>
		public (int,bool) PairField;
		/// <summary>
		/// 测试Pair
		/// </summary>
		public (int,bool) PairField2;
		/// <summary>
		/// 测试Pair
		/// </summary>
		public (int,int) PairField3;
		/// <summary>
		/// 测试PairList
		/// </summary>
		public (int,long)[] PairFieldList;
		/// <summary>
		/// 测试PairList
		/// </summary>
		public (int,long)[] PairFieldList2;
		/// <summary>
		/// 测试List
		/// </summary>
		public int[] ListField;

    }

    [System.Serializable]
    public sealed partial class TLoc 
    {
		/// <summary>
		/// PK
		/// id
		/// </summary>
		public int Id;
		/// <summary>
		/// 
		/// </summary>
		public string Val;

    }

    [System.Serializable]
    public sealed partial class TTestComposeKey 
    {
		/// <summary>
		/// PK[Level]
		/// 角色Id
		/// </summary>
		public uint Id;
		/// <summary>
		/// 等级
		/// </summary>
		public int Level;
		/// <summary>
		/// 名字
		/// </summary>
		public LocId Name;
		/// <summary>
		/// 位置
		/// </summary>
		public (float,float,float) Pos;
		/// <summary>
		/// 标记位
		/// </summary>
		public EItemFlag Flags;

    }
}
