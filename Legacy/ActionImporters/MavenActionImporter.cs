using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMasterExtensions.Java.Operations;

namespace Inedo.BuildMasterExtensions.Java
{
    public sealed class MavenActionImporter : IActionOperationConverter<MavenAction, MavenOperation>
    {
        public ConvertedOperation<MavenOperation> ConvertActionToOperation(MavenAction action, IActionConverterContext context)
        {
            return new MavenOperation
            {
                MavenPath = AH.NullIf(context.ConvertLegacyExpression(action.MavenPath), string.Empty),
                GoalsAndPhases = string.Join(" ", action.GoalsAndPhases ?? new string[0]),
                AdditionalArguments = AH.NullIf(string.Join(" ", action.AdditionalArguments ?? new string[0]), string.Empty),
                SourceDirectory = AH.NullIf(context.ConvertLegacyExpression(action.OverriddenSourceDirectory), string.Empty)
            };
        }
    }
}
