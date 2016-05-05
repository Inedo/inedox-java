using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.BuildMasterExtensions.Java.Operations;

namespace Inedo.BuildMasterExtensions.JUnit
{
    internal sealed class JUnitActionImporter : IActionOperationConverter<JUnitAction, JUnitOperation>
    {
        public ConvertedOperation<JUnitOperation> ConvertActionToOperation(JUnitAction action, IActionConverterContext context)
        {
            return new JUnitOperation
            {
                Includes = context.ConvertLegacyMask(new[] { action.SearchPattern }, true).Includes,
                JavaPath = AH.NullIf(context.ConvertLegacyExpression(action.JavaPath), string.Empty),
                SourceDirectory = AH.NullIf(context.ConvertLegacyExpression(action.OverriddenSourceDirectory), string.Empty),
                ExtensionDirectories = action.ExtensionDirectories
            };
        }
    }
}
