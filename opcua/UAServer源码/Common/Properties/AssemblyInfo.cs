﻿// Copyright (c) 2014 Converter Systems LLC

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("ConverterSystems.Common")]
[assembly: AssemblyDescription("Common Infrastructure")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Converter Systems LLC")]
[assembly: AssemblyProduct("Workstation")]
[assembly: AssemblyCopyright("Copyright ©  2011 Converter Systems LLC")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("5691be8c-c499-491f-9ee6-9f65f1378408")]

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

[assembly: AssemblyVersion("4.5.4.0")]
[assembly: AssemblyFileVersion("4.5.4.0")]
[assembly: XmlnsDefinition("http://schemas.convertersystems.com/workstation", "ConverterSystems.Workstation.Behaviors")]
[assembly: XmlnsDefinition("http://schemas.convertersystems.com/workstation", "ConverterSystems.Workstation.Controls")]
[assembly: XmlnsDefinition("http://schemas.convertersystems.com/workstation", "ConverterSystems.Workstation.Data")]
[assembly: XmlnsDefinition("http://schemas.convertersystems.com/workstation", "ConverterSystems.Workstation.Input")]
[assembly: XmlnsDefinition("http://schemas.convertersystems.com/workstation", "ConverterSystems.Workstation")]
[assembly: XmlnsPrefix("http://schemas.convertersystems.com/workstation", "cs")]

//In order to begin building localizable applications, set 
//<UICulture>en-US</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.
//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]

[assembly: NeutralResourcesLanguage("en-US")]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
    )]