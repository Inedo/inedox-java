using System;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Java
{
    internal sealed class MavenActionEditor : ActionEditorBase
    {
        private SourceControlFileFolderPicker ctlMavenPath;
        private ValidatingTextBox txtAdditionalArguments;
        private ValidatingTextBox txtGoalsAndPhases;

        public MavenActionEditor()
        {
        }

        public override bool DisplaySourceDirectory { get { return true; } }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var maven = (MavenAction)extension;
            this.txtGoalsAndPhases.Text = string.Join(" ", maven.GoalsAndPhases ?? new string[0]);
            this.txtAdditionalArguments.Text = string.Join(Environment.NewLine, maven.AdditionalArguments ?? new string[0]);
            this.ctlMavenPath.Text = string.IsNullOrEmpty(maven.MavenPath)
                ? this.ctlMavenPath.Text
                : maven.MavenPath;
        }
        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new MavenAction
            {
                MavenPath = this.ctlMavenPath.Text,
                GoalsAndPhases = this.txtGoalsAndPhases.Text.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
                AdditionalArguments = Regex.Split(this.txtAdditionalArguments.Text.Trim(' '), "\r?\n")
            };
        }

        protected override void CreateChildControls()
        {
            this.ctlMavenPath = new SourceControlFileFolderPicker
            {
                ServerId = this.ServerId,
                Required = true
            };

            this.txtAdditionalArguments = new ValidatingTextBox
            {
                TextMode = TextBoxMode.MultiLine,
                Rows = 4,
                Columns = 100,
                Width = Unit.Pixel(300)
            };

            this.txtGoalsAndPhases = new ValidatingTextBox
            {
                Required = false,
                Width = Unit.Pixel(300)
            };

            this.Controls.Add(
                new FormFieldGroup(
                    "Maven Path",
                    "The path to the Maven executable (mvn.bat on Windows).",
                    false,
                    new StandardFormField("Path:", ctlMavenPath)),
                new FormFieldGroup(
                    "Goals and Phases",
                    "The goals and/or phases of the Maven build, separated by spaces.",
                    false,
                    new StandardFormField("Goals/Phases:", txtGoalsAndPhases)),
                new FormFieldGroup(
                    "Additional Arguments",
                    "Any additional arguments for mvn, entered one per line.",
                    true,
                    new StandardFormField("Additional Arguments:", txtAdditionalArguments))
            );
        }
    }
}
