using System;
using System.ComponentModel;
using Inedo.Agents;
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMaster.Extensibility.VariableFunctions;

namespace Inedo.BuildMasterExtensions.Java.Functions
{
    [ScriptAlias("AntPath")]
    [Description("Returns the full path to ant (or ant.exe on Windows) for the server in context.")]
    public sealed class AntPathVariableFunction : ScalarVariableFunctionBase
    {
        protected override object EvaluateScalar(IGenericBuildMasterContext context)
        {
            var operationContext = context as IOperationExecutionContext;
            if (operationContext == null)
                throw new InvalidOperationException("Operation execution context is not available.");

            var env = operationContext.Agent.GetService<IRemoteProcessExecuter>();
            var antPath = env.GetEnvironmentVariableValue("ANT_HOME");
            if (string.IsNullOrEmpty(antPath))
                return string.Empty;

            var fileOps = operationContext.Agent.GetService<IFileOperationsExecuter>();
            antPath = fileOps.CombinePath(antPath, "bin", "ant");
            if (fileOps.FileExists(antPath))
                return antPath;

            return antPath + ".exe";
        }
    }
}
