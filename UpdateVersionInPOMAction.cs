using System;
using System.Text;
using System.Text.RegularExpressions;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Files;
using Inedo.BuildMaster.Web;
using Inedo.Linq;
using System.Xml;

namespace Inedo.BuildMasterExtensions.Java
{
    [ActionProperties("Update Version in POM",
        "Updates the package version value in a pom.xml file.",
        "Java")]
    [CustomEditor(typeof(UpdateVersionInPOMActionEditor))]
    public class UpdateVersionInPOMAction : RemoteActionBase
    {

        [Persistent]
        public string[] FileMasks { get; set; }
        [Persistent]
        public bool Recursive { get; set; }
        [Persistent]
        public string Version { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Set Version values in files matching ({0}) in {1} to {2}",
                string.Join(", ", this.FileMasks ?? new string[0]),
                Util.CoalesceStr(this.OverriddenSourceDirectory, "(default directory)"),
                this.Version
            );
        }

        public UpdateVersionInPOMAction()
        {
            this.FileMasks = new[] { "*\\pom.xml" };
            this.Version = "%RELNO%.%BLDNO%";
        }
        protected override void Execute()
        {

            this.LogInformation("Setting Assembly Version Attributes to {0}...", this.Version);

            using (var fileOps = (IFileOperationsExecuter)Util.Agents.CreateAgentFromId(this.ServerId))
            {
                var entry = fileOps.GetDirectoryEntry(
                    new GetDirectoryEntryCommand
                    {
                        Path = this.RemoteConfiguration.SourceDirectory,
                        IncludeRootPath = true,
                        Recurse = this.Recursive
                    }
                ).Entry;

                var matches = Util.Files.Comparison.GetMatches(
                    this.RemoteConfiguration.SourceDirectory,
                    entry,
                    this.FileMasks
                ).OfType<FileEntryInfo>();

                if (!matches.Any())
                {
                    this.LogWarning("No matching files found.");
                    return;
                }

                foreach (var match in matches)
                {
                    this.LogDebug("Writing package version to {0}...", match.Path);

                    var text = Encoding.UTF8.GetString(fileOps.ReadAllFileBytes(match.Path));
                    text = TransformFile(text, this.Version);
                    fileOps.WriteFile(
                        match.Path,
                        null,
                        null,
                        Encoding.UTF8.GetBytes(text),
                        false
                    );
                }

            }
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            throw new NotImplementedException();
        }

        internal string TransformFile(string Input, string Version)
        {
            string retVal = String.Empty;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Input);
            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
            string xmlns = doc.DocumentElement.GetAttribute("xmlns");
            ns.AddNamespace("a", xmlns);
            XmlNode node = doc.SelectSingleNode("//a:project/a:version", ns);
            if (null != node)
                node.InnerText = Version;
            else
            {
                var proj = doc.SelectSingleNode("//project");
                XmlNode newnode = doc.CreateNode(XmlNodeType.Element, "version", null);
                newnode.InnerText = Version;
                proj.AppendChild(newnode);
            }
            return doc.OuterXml;
        }
    }
}
