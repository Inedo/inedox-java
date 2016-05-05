using System;
using System.Web.UI.WebControls;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Java
{
    /// <summary>
    /// Custom editor for the Build Ant Project action.
    /// </summary>
    internal sealed class AntActionEditor : ActionEditorBase
    {
        private ValidatingTextBox txtProjectFilePath;
        private ValidatingTextBox txtBuildTarget;
        private TextBox txtAdditionalProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildNAntProjectActionEditor"/> class.
        /// </summary>
        public AntActionEditor()
        {
            this.ValidateBeforeSave += this.AntActionEditor_ValidateBeforeSave;
        }

        /// <summary>
        /// Gets a value indicating whether a textbox to edit the source directory should be displayed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if a textbox to edit the source directory should be displayed; otherwise, <c>false</c>.
        /// </value>
        public override bool DisplaySourceDirectory
        {
            get { return true; }
        }

        public override void BindToForm(ActionBase extension)
        {
            EnsureChildControls();

            var action = (AntAction)extension;
            this.txtProjectFilePath.Text = Util.NullIf(action.BuildPath, "build.xml") ?? "";
            this.txtBuildTarget.Text = action.ProjectBuildTarget ?? string.Empty;
            this.txtAdditionalProperties.Text = string.Join(Environment.NewLine, action.BuildProperties ?? new string[0]);
        }
        public override ActionBase CreateFromForm()
        {
            EnsureChildControls();

            return new AntAction()
            {
                BuildPath = this.txtProjectFilePath.Text,
                ProjectBuildTarget = this.txtBuildTarget.Text,
                BuildProperties = this.txtAdditionalProperties.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
            };
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based
        /// implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            this.txtProjectFilePath = new ValidatingTextBox
            {
                Width = 300,
                DefaultText = "build.xml"
            };

            this.txtBuildTarget = new ValidatingTextBox
            {
                Width = 300,
                Required = true
            };

            this.txtAdditionalProperties = new TextBox
            {
                Width = 300,
                Rows = 5,
                TextMode = TextBoxMode.MultiLine
            };

            this.Controls.Add(
                new FormFieldGroup("Build File Path",
                    "The path to the build file.",
                    false,
                    new StandardFormField("Build File Path:", this.txtProjectFilePath)
                ),
                new FormFieldGroup("Build Target",
                    "The Build Target property, for example: compile dist. Multiple targets should be separated by spaces. Single targets with spaces in their name must be quoted. If no target is specified here, the project file's default task will be run. If no default is specified in the project file, Ant v1.6.0 and later will run all top-level tasks.",
                    false,
                    new StandardFormField("Build Target:", this.txtBuildTarget)
                ),
                new FormFieldGroup("Build Properties",
                    "Additional properties, separated by newlines. Example:<br />WarningLevel=2<br />Optimize=false",
                    true,
                    new StandardFormField("Build Properties:", this.txtAdditionalProperties)
                )
            );
        }

        /// <summary>
        /// Handles the ValidateBeforeSave event of the BuildNAntProjectActionEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Inedo.BuildMaster.Web.Controls.Extensions.ValidationEventArgs&lt;Inedo.BuildMaster.Extensibility.Actions.ActionBase&gt;"/> instance containing the event data.</param>
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
