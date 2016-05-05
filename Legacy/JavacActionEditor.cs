using System;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Java
{
    internal sealed class JavacActionEditor : ActionEditorBase
    {
        private ValidatingTextBox txtExtensionDirectories;
        private ValidatingTextBox txtAdditionalArguments;

        public override bool DisplaySourceDirectory { get { return true; } }
        public override bool DisplayTargetDirectory { get { return true; } }

        protected override void CreateChildControls()
        {
            txtExtensionDirectories = new ValidatingTextBox();
            txtExtensionDirectories.TextMode = TextBoxMode.MultiLine;
            txtExtensionDirectories.Rows = 4;
            txtExtensionDirectories.Columns = 100;
            txtExtensionDirectories.Width = Unit.Pixel(300);

            txtAdditionalArguments = new ValidatingTextBox();
            txtAdditionalArguments.TextMode = TextBoxMode.MultiLine;
            txtAdditionalArguments.Rows = 4;
            txtAdditionalArguments.Columns = 100;
            txtAdditionalArguments.Width = Unit.Pixel(300);


            this.Controls.Add(
                new FormFieldGroup(
                    "Extensions Paths",
                    "The relative path of Java extensions required for compilation.",
                    false,
                    new StandardFormField("Extension Paths:", txtExtensionDirectories)),
                new FormFieldGroup(
                    "Additional Arguments",
                    "Any additional arguments for javac, entered one per line.",
                    true,
                    new StandardFormField("Additional Arguments:", txtAdditionalArguments)));
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var javac = (JavacAction)extension;
            if (javac.AdditionalArguments != null)
                txtAdditionalArguments.Text = string.Join(Environment.NewLine, javac.AdditionalArguments);
            if (javac.ExtensionDirectories != null)
                txtExtensionDirectories.Text = string.Join(Environment.NewLine, javac.ExtensionDirectories);
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            var javac = new JavacAction();
            javac.AdditionalArguments = Regex.Split(txtAdditionalArguments.Text, "\r?\n");
            javac.ExtensionDirectories = Regex.Split(txtExtensionDirectories.Text, "\r?\n");
            return javac;
        }
    }
}
