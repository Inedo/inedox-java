using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Inedo.Agents;
using Inedo.Diagnostics;
using Inedo.Documentation;
using Inedo.ExecutionEngine.Executer;
using Inedo.Extensibility;
using Inedo.Extensibility.Operations;
using Inedo.Extensions.PackageSources;
using Inedo.IO;
using Inedo.Web;

namespace Inedo.Extensions.Java.Operations
{
    [DisplayName("Maven")]
    [Description("Runs the Maven executable.")]
    [ScriptAlias("Maven")]
    [ScriptAlias("Execute-Maven")]
    [ScriptNamespace("Java", PreferUnqualified = false)]
    public sealed class MavenOperation : ExecuteOperation
    {
        [ScriptAlias("GoalsAndPhases")]
        [DisplayName("Goals/phases")]
        [Description("Space-separated list of Maven goals and/or phases.")]
        [SuggestableValue("validate", "versions:set", "compile", "test", "package", "verify", "install", "deploy")]
        public string GoalsAndPhases { get; set; }
        [ScriptAlias("In")]
        [DisplayName("Source directory")]
        [Description("The working directory in which Maven will be executed.")]
        public string SourceDirectory { get; set; }
        [ScriptAlias("Arguments")]
        [DisplayName("Additional arguments")]
        [Description("Raw command line arguments to pass to Maven.")]
        public string AdditionalArguments { get; set; }
        [ScriptAlias("MavenPath")]
        [DefaultValue("$MavenPath")]
        [DisplayName("Maven path")]
        [Description("Full path to mvn on the server.")]
        public string MavenPath { get; set; }
        [ScriptAlias("DependenciesFeed")]
        [DisplayName("Dependencies Feed")]
        [Description("If specified, this Maven repository will be used to install dependencies from.")]
        [PlaceholderText("Use Maven Central")]
        [SuggestableValue(typeof(MavenPackageSourceSuggestionProvider))]
        public string DependenciesFeed { get; set; }
        [ScriptAlias("PluginsFeed")]
        [DisplayName("Plugins Feed")]
        [Description("If specified, this Maven repository will be used to install plugins from.")]
        [PlaceholderText("Use Maven Central")]
        [SuggestableValue(typeof(MavenPackageSourceSuggestionProvider))]
        public string PluginsFeed { get; set; }
        [ScriptAlias("SettingsXml")]
        [DisplayName("Settings XML")]
        [Description("This cannot be used when using the Plugins Feed or Dependencies Feed parameter.")]
        public string SettingsFile { get; set; }

        [ScriptAlias("ImageBasedService")]
        public string ImageBasedService { get; set; }

        private bool UseContainer => !string.IsNullOrEmpty(this.ImageBasedService);

        public override async Task ExecuteAsync(IOperationExecutionContext context)
        {
            Func<string, string> resolvePath = context.ResolvePath;
            DockerHostShim docker = null;
            if (this.UseContainer)
            {
                this.LogDebug($"Performing containerized build using \"{this.ImageBasedService}\" image based service.");
                docker = await DockerHostShim.CreateAsync(context);
                resolvePath = docker.ResolveContainerPath;
            }

            if (!this.UseContainer && string.IsNullOrEmpty(this.MavenPath))
            {
                this.LogError("Could not determine the location of mvn on this server. To resolve this issue, ensure that Maven is available on this server and create a server-scoped variable named $MavenPath set to the location of mvn (or mvn.bat on Windows).");
                return;
            }

            if ((!string.IsNullOrWhiteSpace(this.DependenciesFeed) || !string.IsNullOrWhiteSpace(this.PluginsFeed)) && !string.IsNullOrWhiteSpace(this.SettingsFile))
            {
                this.LogError("Settings XML cannot be used with Plugins and Dependencies Feed");
                return;
            }

            var fileOps = context.Agent.GetService<IFileOperationsExecuter>();
            var settingsXmlPath = AH.NullIf(this.SettingsFile, string.Empty);

            if (!string.IsNullOrWhiteSpace(this.DependenciesFeed) || !string.IsNullOrWhiteSpace(this.PluginsFeed))
            {
                var serversNode = new XElement("servers");
                var progetProfile = new XElement("profile",
                    new XElement("id", "ProGet")
                );

                if (!string.IsNullOrWhiteSpace(this.DependenciesFeed))
                {
                    var sourceId = new PackageSourceId(this.DependenciesFeed);
                    var source = await AhPackages.GetPackageSourceAsync(sourceId, context, context.CancellationToken);
                    if (source == null)
                    {
                        this.LogError($"Dependencies feed \"{this.DependenciesFeed}\" not found.");
                        return;
                    }

                    if (source is not IMavenPackageSource maven)
                    {
                        this.LogError($"Dependencies feed \"{this.DependenciesFeed}\" is a {source.GetType().Name} source; it must be a Maven source for use with this operation.");
                        return;
                    }

                    progetProfile.Add(
                        new XElement("repositories",
                            new XElement("repository",
                                new XElement("id", maven.SourceId.GetFeedName()),
                                new XElement("url", maven.RepositoryUrl),
                                new XElement("snapshots", new XElement("enabled", true)),
                                new XElement("releases", new XElement("enabled", true))
                            )
                        )
                    );

                    if (!string.IsNullOrWhiteSpace(maven.ApiKey))
                    {
                        serversNode.Add(
                            new XElement("server",
                                new XElement("id", maven.SourceId.GetFeedName()),
                                new XElement("username", "api"),
                                new XElement("password", maven.ApiKey)
                            )
                        );
                    }
                    else if (!string.IsNullOrWhiteSpace(maven.Password))
                    {
                        serversNode.Add(
                            new XElement("server",
                                new XElement("id", maven.SourceId.GetFeedName()),
                                new XElement("username", maven.UserName),
                                new XElement("password", maven.Password)
                            )
                        );
                    }
                }

                if (!string.IsNullOrWhiteSpace(this.PluginsFeed))
                {
                    var sourceId = new PackageSourceId(this.PluginsFeed);
                    var source = await AhPackages.GetPackageSourceAsync(sourceId, context, context.CancellationToken);
                    if (source == null)
                    {
                        this.LogError($"Plugins feed \"{this.PluginsFeed}\" not found.");
                        return;
                    }

                    if (source is not IMavenPackageSource maven)
                    {
                        this.LogError($"Plugins feed \"{this.PluginsFeed}\" is a {source.GetType().Name} source; it must be a Maven source for use with this operation.");
                        return;
                    }

                    progetProfile.Add(
                        new XElement("pluginRepositories",
                            new XElement("pluginRepository",
                                new XElement("id", maven.SourceId.GetFeedName()),
                                new XElement("url", maven.RepositoryUrl),
                                new XElement("snapshots", new XElement("enabled", true)),
                                new XElement("releases", new XElement("enabled", true))
                            )
                        )
                    );

                    if (!string.IsNullOrWhiteSpace(maven.ApiKey))
                    {
                        serversNode.Add(
                            new XElement("server",
                                new XElement("id", maven.SourceId.GetFeedName()),
                                new XElement("username", "api"),
                                new XElement("password", maven.ApiKey)
                            )
                        );
                    }
                    else if (!string.IsNullOrWhiteSpace(maven.Password))
                    {
                        serversNode.Add(
                            new XElement("server",
                                new XElement("id", maven.SourceId.GetFeedName()),
                                new XElement("username", maven.UserName),
                                new XElement("password", maven.Password)
                            )
                        );
                    }
                }

                XNamespace jarva = "http://maven.apache.org/SETTINGS/1.0.0";
                XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
                var settingsXml = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement(jarva + "settings",
                        new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                        new XAttribute(xsi + "schemaLocation", "http://maven.apache.org/SETTINGS/1.0.0 http://maven.apache.org/xsd/settings-1.0.0.xsd"),
                        serversNode,
                        new XElement("profiles",
                            progetProfile
                        ),
                        new XElement("activeProfiles",
                            new XElement("activeProfile", "ProGet")
                        )
                    )
                );

                settingsXmlPath = PathEx.Combine(resolvePath(@"~\"), "settings.xml");
                await fileOps.WriteAllTextAsync(settingsXmlPath, settingsXml.ToString(), InedoLib.UTF8Encoding);
            }

            if (!fileOps.FileExists(this.MavenPath))
            {
                this.LogError("Maven not found at: " + this.MavenPath);
                return;
            }

            var sourceDirectory = resolvePath(this.SourceDirectory);
            this.LogDebug("Source directory: " + sourceDirectory);
            fileOps.CreateDirectory(sourceDirectory);

            if (settingsXmlPath != null)
                settingsXmlPath = "-s " + settingsXmlPath;

            if (!this.UseContainer)
            {
                await this.ExecuteCommandLineAsync(
                    context,
                    new RemoteProcessStartInfo
                    {
                        FileName = this.MavenPath,
                        Arguments = this.GoalsAndPhases + " " + this.AdditionalArguments + " " + settingsXmlPath,
                        WorkingDirectory = sourceDirectory
                    }
                );
            }
            else
            {
                await docker.ExecuteInContainerAsync(
                    new ContainerStartInfoShim(
                        this.ImageBasedService,
                        new RemoteProcessStartInfo
                        {
                            FileName = "maven",
                            Arguments = this.GoalsAndPhases + " " + this.AdditionalArguments + " " + settingsXmlPath,
                            WorkingDirectory = sourceDirectory
                        },
                        OutputDataReceived: this.LogProcessOutput,
                        ErrorDataReceived: this.LogProcessError
                    ),
                    context.CancellationToken
                );
            }
        }

        protected override void LogProcessOutput(string text)
        {
            if (text.Contains("[ERROR]"))
                this.LogError(text);
            else
                this.LogInformation(text);
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            return new ExtendedRichDescription(
                new RichDescription(
                    "Execute Maven ",
                    new Hilite(config[nameof(this.GoalsAndPhases)])
                ),
                new RichDescription(
                    "in ",
                    new DirectoryHilite(config[nameof(this.SourceDirectory)])
                )
            );
        }

        private sealed class DockerHostShim
        {
            private readonly object host;

            private DockerHostShim(object host) => this.host = host;

            public static async Task<DockerHostShim> CreateAsync(IOperationExecutionContext context)
            {
                try
                {
                    return (await CreateInternalAsync(context)) ?? throw new ExecutionFailureException($"Server {context.ServerName} does not have a Docker engine.");
                }
                catch
                {
                    throw new ExecutionFailureException($"Containerized builds are not supported in this version of {SDK.ProductName}.");
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public string ResolveContainerPath(string relativePath) => ((IDockerHost)this.host).ResolveContainerPath(relativePath);
            [MethodImpl(MethodImplOptions.NoInlining)]
            public Task<int> ExecuteInContainerAsync(ContainerStartInfoShim containerStartInfo, CancellationToken cancellationToken = default)
            {
                return ((IDockerHost)this.host).ExecuteInContainerAsync(
                    new ContainerStartInfo(containerStartInfo.Image, containerStartInfo.StartInfo, containerStartInfo.MapExecutionDirectory, OutputDataReceived: containerStartInfo.OutputDataReceived, ErrorDataReceived: containerStartInfo.ErrorDataReceived),
                    cancellationToken
                );
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static async Task<DockerHostShim> CreateInternalAsync(IOperationExecutionContext context)
            {
                var docker = await context.TryGetServiceAsync<IDockerHost>();
                if (docker != null)
                    return new DockerHostShim(docker);
                else
                    return null;
            }
        }

        private sealed record ContainerStartInfoShim(string Image, RemoteProcessStartInfo StartInfo, bool MapExecutionDirectory = true, Action<string> OutputDataReceived = null, Action<string> ErrorDataReceived = null);
    }
}
