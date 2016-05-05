using System;
using System.ComponentModel;
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMaster.Extensibility.VariableFunctions;

namespace Inedo.BuildMasterExtensions.Java.Functions
{
    [ScriptAlias("MavenPath")]
    [Description("Returns the full path to mvn (or mvn.bat on Windows) for the server in context.")]
    public sealed class MavenPathVariableFunction : ScalarVariableFunctionBase
    {
        protected override object EvaluateScalar(IGenericBuildMasterContext context)
        {
            var operationContext = context as IOperationExecutionContext;
            if (operationContext == null)
                throw new InvalidOperationException("Operation execution context is not available.");

            var env = operationContext.Agent.GetService<IEnvironmentVariableAccessor>();
            var antPath = env.GetVariableValue("M2_HOME");
            if (string.IsNullOrEmpty(antPath))
                return string.Empty;

            var fileOps = operationContext.Agent.GetService<IFileOperationsExecuter>();
            antPath = fileOps.CombinePath(antPath, "bin", "mvn");
            if (fileOps.FileExists(antPath))
                return antPath;

            return antPath + ".bat";
        }
    }
}
