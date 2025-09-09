/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13 18:18:49
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    /// <summary>
    /// 主要是一些 数值类型的没有引用，没办法多个地方引用
    /// 主要是为了数据绑定
    /// </summary>
    public sealed class RefPrimitive<T>
    {
        public T _value;

        public RefPrimitive()
        {
            _value = default(T);
        }

        public RefPrimitive(T v)
        {
            _value = v;
        }

        public T Value { get { return _value; } set { _value = value; } }

        public T GetValue()
        {
            return _value;
        }

        public void SetValue(T v)
        {
            _value = v;
        }


        public static implicit operator T(RefPrimitive<T> v)
        {
            return v._value;
        }

        public static implicit operator RefPrimitive<T>(T v)
        {
            return new RefPrimitive<T>(v);
        }
    }
}
