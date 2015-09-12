// --------------------------------------------------
// ReiPatcher - AssemblyInfo.cs
// --------------------------------------------------

#region Usings

using System.Reflection;
using System.Runtime.InteropServices;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

#if X86
[assembly: AssemblyTitle("ReiPatcher x86")]
#elif X64
[assembly: AssemblyTitle("ReiPatcher x64")]
#else

[assembly: AssemblyTitle("ReiPatcher")]
#endif

[assembly: AssemblyDescription(".NET Assembly Patcher - Powered by Mono.Cecil")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ReiPatcher")]
[assembly: AssemblyCopyright("Copyright © Usagirei 2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("12ffe4db-d7e4-4937-af44-b90c25bcf28f")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

#if !GIT

[assembly: AssemblyVersion("0.0.0.0")]
[assembly: AssemblyFileVersion("0.0.0.0")]
[assembly: AssemblyInformationalVersion("development build - internal use only")]

#endif
