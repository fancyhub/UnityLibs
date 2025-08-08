/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/07/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Reflection;

namespace FH.DebugUI
{
    public static class DebugUIInputFieldFactory
    {
        public static DebugUIInputField CreateInputField(ParameterInfo paramInfo)
        {
            return CreateInputField(paramInfo.Name, paramInfo.ParameterType, paramInfo.DefaultValue);
        }

        public static DebugUIInputField CreateInputField(DebugUICommand.Param paramInfo)
        {
            DebugUIInputField ret = null;
            switch (paramInfo.Type)
            {
                case DebugUICommand.EParamType.String:
                    ret = new DebugUIInputFieldString(paramInfo.Name);
                    break;

                case DebugUICommand.EParamType.Int32:
                    ret = new DebugUIInputFieldInt32(paramInfo.Name);
                    break;

                case DebugUICommand.EParamType.Int64:
                    ret = new DebugUIInputFieldInt64(paramInfo.Name);
                    break;

                case DebugUICommand.EParamType.Float:
                    ret = new DebugUIInputFieldFloat(paramInfo.Name);
                    break;
                default:
                    ret = new DebugUIInputFieldInvalid(paramInfo.Name);
                    break;
            }
            return ret;
        }

        public static DebugUIInputField CreateInputField(string name, Type paramType, object defaultValue = null)
        {
            DebugUIInputField ret = null;
            if (paramType == typeof(string))
            {
                ret = new DebugUIInputFieldString(name, defaultValue);
            }
            else if (paramType == typeof(int))
            {
                ret = new DebugUIInputFieldInt32(name, defaultValue);
            }
            else if (paramType == typeof(uint))
            {
                ret = new DebugUIInputFieldUInt32(name, defaultValue);
            }
            else if (paramType == typeof(float))
            {
                ret = new DebugUIInputFieldFloat(name, defaultValue);
            }
            else if (paramType == typeof(long))
            {
                ret = new DebugUIInputFieldInt64(name, defaultValue);
            }
            else if (paramType == typeof(ulong))
            {
                ret = new DebugUIInputFieldUInt64(name, defaultValue);
            }
            else if (paramType == typeof(bool))
            {
                ret = new DebugUIInputFieldBool(name, defaultValue);
            }
            else if (paramType.IsEnum)
            {
                ret = new DebugUIInputFieldEnum(name, paramType, defaultValue);
            }
            else
            {
                ret = new DebugUIInputFieldInvalid(name, paramType);
            }
            return ret;
        }
    }
}
