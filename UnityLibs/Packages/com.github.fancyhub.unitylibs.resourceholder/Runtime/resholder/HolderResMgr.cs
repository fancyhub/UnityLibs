using System;
using System.Collections.Generic;

namespace FH
{
    public struct ResLoaderTicket
    {
        public int Id;
    }

    public delegate void ResLoaderAsyncCallBack(ResLoaderTicket ticket, UnityEngine.Object obj);

    public interface IResLoader
    {
        public T Load<T>(string path) where T : UnityEngine.Object;
        public void Unload(string path, Type type);

        public ResLoaderTicket LoadAsync<T>(string path, int priority, ResLoaderAsyncCallBack call_back) where T : UnityEngine.Object;
        public void CancelLoadAsync(ResLoaderTicket ticket);
    }

    internal struct ResKey : IEquatable<ResKey>
    {
        public readonly string Path;
        public readonly Type Type;

        public ResKey(string path, Type type)
        {
            if (path == null)
                Path = string.Empty;
            else
                Path = path;
            Type = type;
        }

        public static ResKey Create<T>(string path) where T : UnityEngine.Object
        {
            return new ResKey(path, typeof(T));
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public bool Equals(ResKey other)
        {
            if (Path != other.Path || Type != other.Type)
                return false;
            return true;
        }

        public bool IsValid { get { return Path != string.Empty && Type != null; } }
    }

    public enum EResStatus
    {
        None,
        Loading,
        Succ,
        Fail
    }



    internal interface IInnerHolder : ICPtr
    {
        public uint Id { get; }
        public void OnAsyncLoaded(ResKey key, UnityEngine.Object obj);
    }

    internal class ResVal
    {
        internal UnityEngine.Object Obj;
        internal bool Loading;
        internal LinkedList<CPtr<IInnerHolder>> InnerHolders = new LinkedList<CPtr<IInnerHolder>>();
    }

    internal class HolderResMgr
    {
        public Dictionary<uint, IInnerHolder> _holder_dict;
        public IResLoader _ResLoader;
        public Dictionary<ResLoaderTicket, (ResKey, IInnerHolder)> _Tickes;
        public Dictionary<ResKey, ResVal> _ResDict;
        /*
        public T Load<T>(IInnerHolder inner_holder, string path) where T : UnityEngine.Object
        {
            ResKey res_key = ResKey.Create<T>(path);

            _holder_dict[inner_holder.Id] = inner_holder;
            if (!_ResDict.TryGetValue(res_key, out var v))
            {
                v = new ResVal();
                v.Loading = true;
                _ResDict.Add(res_key, v);
            }

            if(v.Loading)
            {

            }

            {
                if (v.Loading)
                {
                    v.InnerHolders.Add(inner_holder.Id);
                    return v.Obj as T;
                }
            }
            else
            {

            }

            v.Obj = _ResLoader.Load<T>(path);
            v.Loading = false;
            v.InnerHolders.Add(inner_holder.Id);

            foreach (var p in v.Tickets)
            {

            }

            if (v.Obj != null)
            {

            }
            return null;
        }

        public ResLoaderTicket LoadAsync<T>(string path, int priority, ResLoaderAsyncCallBack call_back) where T : UnityEngine.Object;
        public void CancelLoad(ResLoaderTicket ticket);
        public void Unload(UnityEngine.Object obj);
        */
    }
}
