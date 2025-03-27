/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;


namespace FH
{
    public struct HolderStat
    {
        public int Total;
        public int Succ;
        public int Fail;

        public bool IsLoading { get { return Total > (Succ + Fail); } }
        public bool IsAllDone { get { return Total <= (Succ + Fail); } }
        public bool IsAllSucc { get { return Total <= Succ; } }

        public void Add(HolderStat other)
        {
            Total += other.Total;
            Succ += other.Succ;
            Fail += other.Fail;
        }

        public override string ToString()
        {
            return $"Total:{Total}, Succ:{Succ}, Fail:{Fail}, AllDone:{IsAllDone}";
        }
    }

    public interface IHolderCallBack
    {
        void OnHolderCallBack();
    }

    public interface IHolderObserver
    {
        public void SetCallBack(IHolderCallBack callback);
    }

    public interface IResHolder : ICPtr, IHolderObserver
    {
        public UnityEngine.Object Load(string path, EResPathType pathType = EResPathType.Default);
        public void PreLoad(string path, EResPathType pathType = EResPathType.Default, int priority = 0);
        public void GetAllRes(List<ResRef> out_list);
        public HolderStat GetResStat();
    }

    public interface IPreInstHolder : ICPtr
    {
        public void PreInst(string path, int count);
        public void ClearPreInst();
    }

    public interface IInstHolder : ICPtr, IHolderObserver
    {
        public GameObject Create(string path);

        public void PreCreate(string path, int count = 1, int priority = 0);

        public bool Release(GameObject obj);

        public void GetAllInst(List<ResRef> out_list);

        public HolderStat GetInstStat();
    }

    //聚合类
    public interface IResInstHolder : IResHolder, IInstHolder, IPreInstHolder, IHolderObserver
    {
        public void GetAll(List<ResRef> out_list);
        public HolderStat GetStat();
    }
}
