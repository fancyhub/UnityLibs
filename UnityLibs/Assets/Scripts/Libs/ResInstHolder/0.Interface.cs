using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public struct ResId
    {
        public string path;
    }
  
    public struct ResIdType
    {
        public ResId Id;
        public Type ResType;
    }

    public interface IResHolder : IDestroyable
    {
        public T Load<T>(ResId id) where T : UnityEngine.Object;
        public void Preload<T>(ResId id);
    }

    public interface IInstHolder : IDestroyable
    {

    }

    public interface IResInstHolder : IResHolder, IInstHolder
    {

    }
}