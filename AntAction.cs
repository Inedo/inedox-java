using System;
using System.Text;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.Java
{
    /// <summary>
    /// Builds an Ant build file.
    /// </summary>
    [ActionProperties(
        "Build Ant Project",
        "Builds an Ant project using a build file.",
        "Java")]
    [CustomEditor(typeof(AntActionEditor))]
    [RequiresInterface(typeof(IRemoteProcessExecuter))]
    public sealed class AntAction : CommandLineActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AntAction"/> class.
        /// </summary>
        public AntAction()
        {
        }

        /// <summary>
        /// Gets or sets the project's target for the build script.
        /// </summary>
        [Persistent]
        public string ProjectBuildTarget { get; set; }
        /// <summary>
        /// Gets or sets the project's path for the build script.
        /// </summary>
        [Persistent]
        public string BuildPath { get; set; }
        /// <summary>
        /// Gets or sets the project's properties for the build script.
        /// </summary>
        /// <remarks>
        /// This should be an array of strings of the form property=value.
        /// </remarks>
        [Persistent]
        public string[] BuildProperties { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.BuildProperties != null && this.BuildProperties.Length > 0)
            {
                return string.Format(
                    "Build {0} Target:{1} Properties:{2}",
                    this.BuildPath,
                    this.ProjectBuildTarget,
                    string.Join("; ", this.BuildProperties));
            }
            else
            {
                return string.Format(
                    "Build {0} Target:{1}",
                    this.BuildPath,
                    this.ProjectBuildTarget);
            }
        }

        /// <summary>
        /// This method is called to execute the Action.
        /// </summary>
        protected override void Execute()
        {
            var cfg = (JavaExtensionConfigurer)GetExtensionConfigurer();
            if (string.IsNullOrEmpty(cfg.AntPath))
                throw new InvalidOperationException("Ant path is not specified in the Java extension configuration.");

            using (var agent = (IRemoteProcessExecuter)Util.Agents.CreateAgentFromId(this.ServerId))
            {
                var buffer = new StringBuilder();
                buffer.AppendFormat("-buildfile \"{0}\" ", this.BuildPath);

                if (this.BuildProperties != null && this.BuildProperties.Length > 0)
                {
                    foreach (var property in this.BuildProperties)
                        buffer.AppendFormat("\"-D{0}\" ", property);
                }

                buffer.AppendFormat("\"{0}\"", this.ProjectBuildTarget);

                this.ExecuteCommandLine(agent, cfg.AntPath, buffer.ToString(), this.RemoteConfiguration.SourceDirectory);
            }
        }
        /// <summary>
        /// When implemented in a derived class, processes an arbitrary command
        /// on the appropriate server.
        /// </summary>
        /// <param name="name">Name of command to process.</param>
        /// <param name="args">Optional command arguments.</param>
        /// <returns>
        /// Result of the command.
        /// </returns>
        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
