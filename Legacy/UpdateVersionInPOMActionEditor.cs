using System;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Java
{
    internal sealed class UpdateVersionInPOMActionEditor : ActionEditorBase
    {
        private ValidatingTextBox txtFileMasks;
        private CheckBox chkRecursive;
        private ValidatingTextBox txtVersion;

        public UpdateVersionInPOMActionEditor()
        {
        }

        public override bool DisplaySourceDirectory
        {
            get { return true; }
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var action = (UpdateVersionInPOMAction)extension;
            this.txtFileMasks.Text = string.Join(Environment.NewLine, action.FileMasks ?? new string[0]);
            this.chkRecursive.Checked = action.Recursive;
            this.txtVersion.Text = action.Version;
        }
        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new UpdateVersionInPOMAction
            {
                FileMasks = this.txtFileMasks.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries),
                Recursive = this.chkRecursive.Checked,
                Version = this.txtVersion.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.txtFileMasks = new ValidatingTextBox
            {
                Required = true,
                TextMode = TextBoxMode.MultiLine,
                Width = 300,
                Rows = 5,
                Text = "*\\pom.xml"
            };

            this.chkRecursive = new CheckBox
            {
                Text = "Also search in subdirectories"
            };

            this.txtVersion = new ValidatingTextBox
            {
                Width = 300,
                Required = true,
                Text = "%RELNO%.%BLDNO%"
            };

            this.Controls.Add(
                new FormFieldGroup(
                    "Files",
                    "Specify the masks (one per line) used to determine if a file should be searched for version elements to replace.",
                    false,
                    new StandardFormField(
                        "File Masks:",
                        this.txtFileMasks
                    ),
                    new StandardFormField(
                        string.Empty,
                        this.chkRecursive
                    )
                ),
                new FormFieldGroup(
                    "Package Version",
                    "Specify the version to write to the matched version element.",
                    true,
                    new StandardFormField(
                        "Version:",
                        this.txtVersion
                    )
                )
            );
        }
    }
}
