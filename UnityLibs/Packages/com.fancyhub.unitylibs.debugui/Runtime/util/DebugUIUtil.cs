/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/07/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;


namespace FH.DebugUI
{
    public static class DebugUIUtil
    {
        public static List<(T attr, MethodInfo method)> FindMethodsWithAttribute<T>(BindingFlags bindFlags = BindingFlags.Static | BindingFlags.Public)
            where T : System.Attribute
        {
            Type attributeType = typeof(T);
            List<(T, MethodInfo)> result = new();

            // 获取当前应用程序域中的所有程序集
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    // 获取程序集中的所有类型
                    Type[] types = assembly.GetTypes();

                    foreach (Type type in types)
                    {
                        // 获取类型中的所有方法
                        MethodInfo[] methods = type.GetMethods(bindFlags);

                        foreach (MethodInfo method in methods)
                        {
                            T attr = method.GetCustomAttribute<T>();
                            if (attr != null)
                            {
                                result.Add((attr, method));
                            }
                        }
                    }
                }
                catch (Exception )
                {
                    // 处理可能的异常（某些程序集可能无法访问）
                }
            }
            return result;
        }
    }
}
