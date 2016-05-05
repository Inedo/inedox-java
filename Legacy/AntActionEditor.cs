using System;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Java
{
    internal sealed class AntActionEditor : ActionEditorBase
    {
        private ValidatingTextBox txtProjectFilePath;
        private ValidatingTextBox txtBuildTarget;
        private ValidatingTextBox txtAdditionalProperties;

        public AntActionEditor()
        {
            this.ValidateBeforeSave += this.AntActionEditor_ValidateBeforeSave;
        }

        public override bool DisplaySourceDirectory => true;

        public override void BindToForm(ActionBase extension)
        {
            var action = (AntAction)extension;
            this.txtProjectFilePath.Text = AH.NullIf(action.BuildPath, "build.xml") ?? "";
            this.txtBuildTarget.Text = action.ProjectBuildTarget ?? string.Empty;
            this.txtAdditionalProperties.Text = string.Join(Environment.NewLine, action.BuildProperties ?? new string[0]);
        }
        public override ActionBase CreateFromForm()
        {
            return new AntAction
            {
                BuildPath = this.txtProjectFilePath.Text,
                ProjectBuildTarget = this.txtBuildTarget.Text,
                BuildProperties = this.txtAdditionalProperties.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
            };
        }

        protected override void CreateChildControls()
        {
            this.txtProjectFilePath = new ValidatingTextBox
            {
                DefaultText = "build.xml"
            };

            this.txtBuildTarget = new ValidatingTextBox
            {
                Required = true
            };

            this.txtAdditionalProperties = new ValidatingTextBox
            {
                Rows = 5,
                TextMode = TextBoxMode.MultiLine
            };

            this.Controls.Add(
                new SlimFormField("Build file path:", this.txtProjectFilePath),
                new SlimFormField("Build target:", this.txtBuildTarget)
                {
                    HelpText = "The Build Target property, for example: compile dist. Multiple targets should be separated by spaces. Single targets with spaces in their name must be quoted. If no target is specified here, the project file's default task will be run. If no default is specified in the project file, Ant v1.6.0 and later will run all top-level tasks."
                },
                new SlimFormField("Build properties:", this.txtAdditionalProperties)
                {
                    HelpText = new LiteralHtml("Additional properties, separated by newlines. Example:<br />WarningLevel=2<br />Optimize=false", false)
                }
            );
        }

        private void AntActionEditor_ValidateBeforeSave(object sender, ValidationEventArgs<ActionBase> e)
        {
            var properties = this.txtAdditionalProperties.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var property in properties)
            {
                if (property.Length < 3 || !property.Substring(1, property.Length - 2).Contains("="))
                {
                    e.ValidLevel = ValidationLevel.Error;
                    e.Message = "Build properties must be in the form property=value, separated by newlines.";
                    return;
                }
            }
        }
    }
}
