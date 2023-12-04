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
        UnityEngine.Object Load(string path,bool sprite);
        void PreLoad(string path, bool sprite, int priority = 0);
        void GetAll(List<ResRef> out_list);
        HolderStat GetStat();
    }

    public interface IPreInstHolder :  ICPtr
    {
        void PreInst(string path, int count);
        void ClearPreInst();
    }

    public interface IInstRelease: ICPtr
    {
        bool Release(GameObject obj);
        void ReleaseAll();
    }

    public interface IInstHolder : IInstRelease
    {
        GameObject Create(string path);        
    }

    public interface IEmptyInstHolder : IInstRelease
    {
        GameObject CreateEmpty();
    }

    //聚合类
    public interface IResInstHolder : IEmptyInstHolder, IResHolder, IInstHolder, IPreInstHolder
    {
    }     
}
