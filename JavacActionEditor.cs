using System;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Java
{
    public class JavacActionEditor : OldActionEditorBase
    {
        public override bool DisplaySourceDirectory { get { return true; } }
        public override bool DisplayTargetDirectory { get { return true; } }

        ValidatingTextBox txtExtensionDirectories;
        ValidatingTextBox txtAdditionalArguments;
        // TODO: Add support for Debug_, JavaSourceVersion

        protected override void CreateChildControls()
        {
            // txtExtensionDirectories
            txtExtensionDirectories = new ValidatingTextBox();
            txtExtensionDirectories.TextMode = TextBoxMode.MultiLine;
            txtExtensionDirectories.Rows = 4;
            txtExtensionDirectories.Columns = 100;
            txtExtensionDirectories.Width = Unit.Pixel(300);

            // txtAdditionalArguments
            txtAdditionalArguments = new ValidatingTextBox();
            txtAdditionalArguments.TextMode = TextBoxMode.MultiLine;
            txtAdditionalArguments.Rows = 4;
            txtAdditionalArguments.Columns = 100;
            txtAdditionalArguments.Width = Unit.Pixel(300);


            CUtil.Add(this,
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

            base.CreateChildControls();
        }

        public override void BindActionToForm(ActionBase action)
        {
            EnsureChildControls();

            var javac = (JavacAction)action;
            if (javac.AdditionalArguments != null) txtAdditionalArguments.Text = string.Join(Environment.NewLine, javac.AdditionalArguments);
            //javac.Debug_lines;
            //javac.Debug_source;
            //javac.Debug_vars;
            if (javac.ExtensionDirectories != null) txtExtensionDirectories.Text = string.Join(Environment.NewLine, javac.ExtensionDirectories);
            //javac.JavaSourceVersion;
        }

        public override ActionBase CreateActionFromForm()
        {
            EnsureChildControls();

            var javac = new JavacAction();
            javac.AdditionalArguments = Regex.Split(txtAdditionalArguments.Text, "\r?\n");
            //javac.Debug_lines;
            //javac.Debug_source;
            //javac.Debug_vars;
            javac.ExtensionDirectories = Regex.Split(txtExtensionDirectories.Text, "\r?\n"); 
            //javac.JavaSourceVersion;
            return javac;
        }
    }
}
