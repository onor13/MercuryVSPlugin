﻿//------------------------------------------------------------------------------
// <copyright file="MercuryVSPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using EnvDTE;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using System.Collections.Generic;
using MercuryLangPlugin.Text;

namespace MercuryLangPlugin
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(MercuryVSPackage.PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideOptionPage(typeof(Menu.MOptionPage), "Mercury", "General", 101, 102, true)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class MercuryVSPackage : Package
    {
        /// <summary>
        /// MercuryVSPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "4123774A-2A0B-4D4A-8FCB-B9E72F750600";
        public static string MercuryProjectDir;
        public static SourceTextCache TextCache;
        public static ParsedTreeCache ParsedCache;
        public static SyntaxAnalysis.MercuryLibraries Libraries = new SyntaxAnalysis.MercuryLibraries();


        /// <summary>
        /// Initializes a new instance of the <see cref="MercuryVSPackage"/> class.
        /// </summary>
        public MercuryVSPackage()
        {
            System.Console.Write("dfd");
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            DTE dte = (DTE)this.GetService(typeof(DTE));
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            TextCache = new SourceTextCache();
            ParsedCache = new ParsedTreeCache();

            var options = MOptions;
            if (!System.IO.Directory.Exists(options.LibrariesFolder) || !System.IO.Directory.Exists(options.ProjectDir))
            {
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                0,
                ref clsid,
                "MercuryPackage",
                 string.Format(System.Globalization.CultureInfo.CurrentCulture, "Either project directory or mercury libraries directory is not set." +
                 "Go to Tools-Options-Mercury-General to set them up", this.GetType().FullName),
                string.Empty,
                0,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_INFO,
                0,
                out result));
                return;
            }
            Libraries = new SyntaxAnalysis.MercuryLibraries(options.LibrariesFolder);
            MercuryProjectDir = options.ProjectDir;
        }

        private Menu.MOptionPage MOptions
        {
            get { return ((Menu.MOptionPage)GetDialogPage(typeof(Menu.MOptionPage))); }
        }
        #endregion
    }
}
