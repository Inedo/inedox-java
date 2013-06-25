using System;
using System.Collections.Generic;
using System.Text;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster;
using System.IO;
using Inedo.BuildMaster.Files;
using Inedo.BuildMaster.Web;
using System.Text.RegularExpressions;

namespace Inedo.BuildMasterExtensions.Java
{
    /// <summary>
    /// Represents an action that wraps javac
    /// </summary>
    /// <remarks>
    /// This is based off of JSE1.6: 
    /// http://java.sun.com/javase/6/docs/technotes/tools/windows/javac.html
    /// </remarks>
    [ActionProperties("Javac",
        "Compiles a Java source tree.",
        "Java")]
    [CustomEditor(typeof(JavacActionEditor))]
    public sealed class JavacAction : CommandLineActionBase
    {
        public JavacAction()
        {
            // By default, only line number and source file information is generated. 
            Debug_lines = true;
            Debug_source = true;
            Debug_vars = false;

            //1.6 : This is the default value
            JavaSourceVersion = JavaVersions.v6;
        }

        public enum JavaVersions
        {
            v1_3,
            v1_4,
            v5,
            v6
        }

        #region -g (Debug)
        /// <summary>
        /// Indicates that source file debugging information be generated
        /// </summary>
        [Persistent]
        public bool Debug_source { get; set; }

        /// <summary>
        /// Indicates that line number debugging information be generated
        /// </summary>
        [Persistent]
        public bool Debug_lines { get; set; }
        
        /// <summary>
        /// Indicates that local variable debugging information be generated
        /// </summary>
        [Persistent]   
        public bool Debug_vars { get; set; }
        #endregion

        #region -source (JavaSourceVersion)
        /// <summary>
        /// Specifies the version of source code accepted
        /// </summary>
        public JavaVersions JavaSourceVersion { get; set; }
        #endregion

        #region -extdirs (Extension Dirs)
        /// <summary>
        /// Gets or sets the extdirs
        /// </summary>
        /// <remarks>
        /// Cross-compile against the specified extension directories. Directories is a semicolon-separated list of directories. Each JAR archive in the specified directories is searched for class files. 
        /// </remarks>
        [Persistent]
        public string[] ExtensionDirectories { get; set; }
        #endregion

        /// <summary>
        /// Gets or sets any additional arguments to be passed to javac
        /// </summary>
        [Persistent]
        public string[] AdditionalArguments { get; set; }

        protected override void Execute()
        {
            LogInformation("Executing javac...");
            ExecuteRemoteCommand(null);
            LogInformation("javac execution complete.");
        }
        
        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            //// Verify Javac exists
            //if (!File.Exists(JavacPath)) LogError(JavacPath + " does not exist.");

            // Build file list
            var sourceFiles = Directory.GetFiles(
                RemoteConfiguration.SourceDirectory,
                "*.java",
                SearchOption.AllDirectories);

           
            // Build args list
            var argList = new List<string>();
            argList.Add("-s \"" + RemoteConfiguration.TargetDirectory.Replace("\\","\\\\") + "\"");
            argList.Add("-d \"" + RemoteConfiguration.TargetDirectory.Replace("\\", "\\\\") + "\"");
            argList.Add("-g:{" + Util.CoalesceStr(
                Debug_lines ? "lines" : "",
                Debug_vars ? "vars" : "",
                Debug_source ? "source" : "")
                + "}");
            argList.Add("-source " + JavaSourceVersion
                .ToString()
                .Substring(1) //remove "v"
                .Replace("_", "."));
            if (ExtensionDirectories != null && ExtensionDirectories.Length > 0)
                argList.Add("-extdirs " + string.Join(";", ExtensionDirectories));
            if (AdditionalArguments != null && AdditionalArguments.Length > 0)
                argList.AddRange(AdditionalArguments);
            
            // Ouput temps
            string FILE_sourceFiles = Path.GetTempFileName(),
                FILE_argList = Path.GetTempFileName();
            File.WriteAllLines(FILE_sourceFiles, sourceFiles);
            File.WriteAllLines(FILE_argList, argList.ToArray());

            // Create Output (Javac doesn't create it)
            if (!Directory.Exists(RemoteConfiguration.TargetDirectory))
                Directory.CreateDirectory(RemoteConfiguration.TargetDirectory);

            string javacPath = Path.Combine(((JavaExtensionConfigurer)GetExtensionConfigurer()).JdkPath, "bin" + Path.DirectorySeparatorChar +  "javac.exe");

            // Exec
            var retCde = ExecuteCommandLine(
                javacPath,
                string.Format(
                    "@\"{0}\" @\"{1}\"",
                    FILE_argList,
                    FILE_sourceFiles),
                RemoteConfiguration.SourceDirectory);

            // Done
            return retCde;
        }

        bool isWarning = false;
        protected override void LogProcessErrorData(string data)
        {
            // Ignore "Note: Some input..." and "23 warnings"
            if (data.StartsWith("Note:") || Regex.IsMatch(data, "[0-9]+ warning"))
            {
                LogWarning(data);
            }
            else
            {
                // It might be a warning. Warnings look like:
                //    path\to\file.java:82: warning: you suck
                //    Confirm: Yes, you do
                //    Hello wor"ld = new Hello();
                //             ^
                // So, we want to look for "warning" in first line and "^" in trailing
                
                //are we already in a warning block?
                if (!isWarning)
                {
                    //look for warning header
                    string[] dary = data.Split(':');
                    if (dary.Length >= 3 && dary[3].Trim() == "warning")
                    {
                        isWarning = true;
                        LogWarning(data);
                    }
                    else
                    {
                        //delegate to base
                        base.LogProcessErrorData(data);
                    }
                }
                else
                {
                    //already in a warning
                    LogWarning(data);
                }

                //maybe we can terminate block?
                if (data.Trim() == "^") isWarning = false;
            }
                
        }

        public override string ToString()
        {
            return string.Format(
                "Compile {0} to {1}.",
                Util.CoalesceStr(OverriddenSourceDirectory, "default directory"),
                Util.CoalesceStr(OverriddenTargetDirectory, "default directory"));
        }
    }
}
