using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMasterExtensions.Java.Operations;
using Inedo.IO;

namespace Inedo.BuildMasterExtensions.Java
{
    internal sealed class AntActionImporter : IActionOperationConverter<AntAction, AntOperation>
    {
        public ConvertedOperation<AntOperation> ConvertActionToOperation(AntAction action, IActionConverterContext context)
        {
            var configPath = (context.Configurer as JavaExtensionConfigurer)?.AntPath;

            return new AntOperation
            {
                AntPath = AH.NullIf(context.ConvertLegacyExpression(configPath), string.Empty),
                BuildPath = context.ConvertLegacyExpression(PathEx.Combine(action.OverriddenSourceDirectory ?? string.Empty, action.BuildPath)),
                ProjectBuildTarget = context.ConvertLegacyExpression(action.ProjectBuildTarget),
                BuildProperties = action.BuildProperties?.Length > 0 ? action.BuildProperties : null
            };
        }
    }
}
