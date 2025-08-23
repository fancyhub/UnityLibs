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
    public class DebugUICommandExecutor
    {
        public class Command
        {
            public readonly string Cmd;
            public readonly MethodInfo Action;
            public readonly ParameterInfo[] Parameters;
            public object[] Args;
            public Command(string cmd, MethodInfo action)
            {
                Action = action;
                Cmd = cmd;
                Parameters = action.GetParameters();
            }

            public void Execute(string[] args)
            {
                if (Parameters.Length == 0)
                {
                    Action.Invoke(null, null);
                    return;
                }

                if (Args == null)
                    Args = new object[Parameters.Length];

                bool succ = true;
                for (int i = 0; i < Args.Length; i++)
                {
                    if ((i + 1) < args.Length)
                    {
                        Args[i] = Convert.ChangeType(args[i + 1], Parameters[i].ParameterType);
                    }
                    else if (Parameters[i].HasDefaultValue)
                    {
                        Args[i] = Parameters[i].DefaultValue;
                    }
                    else
                    {
                        succ = false;
                        break;
                    }
                }

                if (!succ)
                    return;

                Action.Invoke(null, Args);
            }
        }

        private Dictionary<string, Command> _Cmds = new();

        public void Reg(string cmd, MethodInfo action)
        {
            if (action == null || string.IsNullOrEmpty(cmd))
                return;
            Command command = new Command(cmd.Trim(), action);
            _Cmds[command.Cmd] = command;
        }


        public void Exec(string script, List<(string name,string value)> replacements = null)
        {
            //1. 检查
            if (string.IsNullOrEmpty(script))
                return;

            //2. 替换
            if (replacements != null)
            {
                foreach(var replace in replacements)
                {
                    script = script.Replace($"${replace.name}", replace.value);
                }                
            }

            //3. 分行
            string[] lines = script.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in lines)
            {
                //3.1 去除注释
                var line = p.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                if (line.StartsWith("//"))
                    continue;
                int index = line.IndexOf("//");
                if (index >= 0)
                    line = line.Substring(0, index);

                //3.2 分割参数
                string[] args = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (args.Length == 0)
                    continue;

                //3.3 找到cmd
                if (!_Cmds.TryGetValue(args[0], out Command cmd))
                {
                    continue;
                }

                //3.4 执行
                cmd.Execute(args);
            }
        }
         
    }
}
