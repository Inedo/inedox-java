using Inedo.BuildMaster.Extensibility.Configurers.Extension;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Java
{
    internal sealed class JavaExtensionConfigurerEditor : ExtensionConfigurerEditorBase
    {
        private ValidatingTextBox txtJdkPath;
        private ValidatingTextBox txtAntPath;

        protected override void CreateChildControls()
        {
            this.txtJdkPath = new ValidatingTextBox();
            this.txtAntPath = new ValidatingTextBox();

            this.Controls.Add(
                new SlimFormField("JDK path:", this.txtJdkPath),
                new SlimFormField("Ant path:", this.txtAntPath)
            );
        }

        public override void BindToForm(ExtensionConfigurerBase extension)
        {
            var configurer = (JavaExtensionConfigurer)extension;
            txtJdkPath.Text = configurer.JdkPath ?? string.Empty;
            txtAntPath.Text = configurer.AntPath ?? string.Empty;
        }

        public override ExtensionConfigurerBase CreateFromForm()
        {
            return new JavaExtensionConfigurer
            {
                JdkPath = txtJdkPath.Text,
                AntPath = txtAntPath.Text
            };
        }

        public override void InitializeDefaultValues()
        {
            this.BindToForm(new JavaExtensionConfigurer());
        }
    }
}
