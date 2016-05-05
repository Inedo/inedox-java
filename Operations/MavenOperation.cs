using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inedo.BuildMaster.Extensibility.Operations;
using Inedo.Documentation;

namespace Inedo.BuildMasterExtensions.Java.Operations
{
    public sealed class MavenOperation : ExecuteOperation
    {
        public override Task ExecuteAsync(IOperationExecutionContext context)
        {
            throw new NotImplementedException();
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            throw new NotImplementedException();
        }
    }
}
