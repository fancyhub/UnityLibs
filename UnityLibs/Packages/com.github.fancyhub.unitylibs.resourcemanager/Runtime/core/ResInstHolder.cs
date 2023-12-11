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
        public int Loading;
    }

    public interface IResHolder : ICPtr
    {
        UnityEngine.Object Load(string path, bool sprite);
        void PreLoad(string path, bool sprite, int priority = 0);
        void GetAllRes(List<ResRef> out_list);
        HolderStat GetStat();
    }

    public interface IPreInstHolder : ICPtr
    {
        void PreInst(string path, int count);
        void ClearPreInst();
    }

    public interface IInstHolder : ICPtr
    {
        GameObject CreateEmpty();
        GameObject Create(string path);

        bool Release(GameObject obj);
        void ReleaseAll();

        void GetAllInst(List<ResRef> out_list);
    }


    //聚合类
    public interface IResInstHolder : IResHolder, IInstHolder, IPreInstHolder
    {
        void GetAll(List<ResRef> out_list);
    }
}
