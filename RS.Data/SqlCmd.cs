using RS.Common;
using RS.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RS.Data
{
    public class SqlCmd
    {
        private string executablePath = null;

        public SqlCmd(string path)
        {
            executablePath = path;
        }

        public int RunScript(string filePath, ConnectionData connectionData, Dictionary<string, string> variables = null, Action<string> progress = null)
        {
            if (filePath.IsNullOrWhiteSpace())
                throw new HandledException("Script file not defined.");

            if (!File.Exists(filePath))
                throw new HandledException("Script file not found.");

            if (!File.Exists(executablePath))
                throw new HandledException("SQLCMD not found.");

            var arguments = "-d \"" + connectionData.Database + "\" -P \"" + connectionData.Password + "\" -S \"" + connectionData.Instance + "\" -U \"" + connectionData.Username + "\" -i \"" + filePath + "\" -b";
            arguments = AddArgumentVariable(arguments, variables);

            var result = Shell.ExecuteShellCommand(executablePath, arguments, progress);

            return result;
        }

        private string AddArgumentVariable(string arguments, Dictionary<string, string> variables)
        {
            if (variables == null)
                return arguments;

            arguments = arguments ?? string.Empty;

            foreach (var item in variables)
                arguments += " -v " + item.Key + "=\"" + item.Value + "\"";

            return arguments;
        }
    }
}