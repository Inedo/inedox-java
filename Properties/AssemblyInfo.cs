using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Inedo.BuildMaster.Extensibility;

[assembly: AssemblyTitle("Java")]
[assembly: AssemblyDescription("Contains actions for Java compilation and testing.")]

[assembly: ComVisible(false)]
[assembly: AssemblyCompany("Inedo, LLC")]
[assembly: AssemblyProduct("BuildMaster")]
[assembly: AssemblyCopyright("Copyright © 2008 - 2012")]
[assembly: AssemblyVersion("0.0.0.0")]
[assembly: AssemblyFileVersion("0.0")]
[assembly: BuildMasterAssembly]
[assembly: CLSCompliant(false)]
[assembly: RequiredBuildMasterVersion("3.0.0")]

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Java.Tests")]