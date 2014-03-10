// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyLoader.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="AssemblyLoader.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="AssemblyLoader.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport
{
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    using SBXA.Shared;
    using SBXA.UI.Client;

    /// <summary>
    ///     This class will load assemblies on demand for SB/XA themes.
    /// </summary>
    public class AssemblyLoader : IAssemblyResolver
    {
        #region Static Fields

        private static readonly StringCollection LoadedAssemblies = new StringCollection();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyLoader"/> class.
        /// </summary>
        /// <param name="resourceNameSpace">
        /// The namespace of the assemblies where the resources reside.
        /// </param>
        public AssemblyLoader(string resourceNameSpace)
        {
            ResourceNameSpace = resourceNameSpace.EndsWith(".") ? resourceNameSpace : resourceNameSpace + ".";
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets resource namespace.
        /// </summary>
        public static string ResourceNameSpace { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// If this method is called, it will cause the assemblies to bewritten to the current directory.
        /// </summary>
        /// <param name="resourceAssembly">
        /// The source of the assemblies.
        /// </param>
        /// <param name="assemblyNames">
        /// A list of assemblies to write out.
        /// </param>
        /// <param name="assNamespace">
        /// The namespace of the resource containing the assemblies.
        /// </param>
        public static void WriteAssembliesToCurrentDirectory(Assembly resourceAssembly, string[] assemblyNames, string assNamespace)
        {
            string assemblyNamespace = assNamespace.EndsWith(".") ? assNamespace : assNamespace + ".";
            try
            {
                foreach (var assemblyName in assemblyNames)
                {
                    string resourceName = assemblyNamespace + new AssemblyName(assemblyName).Name;
                    using (var stream = resourceAssembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            try
                            {
                                var assemblyData = new byte[stream.Length];
                                stream.Read(assemblyData, 0, assemblyData.Length);
                                SBPlusClient.LogInformation("Writing " + assemblyName + " to " + Environment.CurrentDirectory);
                                File.WriteAllBytes(assemblyName, assemblyData);
                            }
                            catch (Exception exception)
                            {
                                SBPlusClient.LogError("Failed to write " + assemblyName, exception);
                                Debug.WriteLine("[AssemblyLoader.LoadAssembly(54)] Exception writing assembly " + assemblyName);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Failed to write assebmlies.", exception);
            }
        }

        /// <summary>
        /// This method will return the assembly if it is located.
        /// </summary>
        /// <param name="fullName">
        /// The full name of the assembly to load.
        /// </param>
        /// <returns>
        /// The assembly or null
        /// </returns>
        public Assembly LoadAssembly(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return null;
            }

            string fullPath = ExtensibilityManager.GetAssemblyInfo(fullName).FullPath;
            if (string.IsNullOrEmpty(fullPath))
            {
                return null;
            }

            Assembly assembly = Assembly.LoadFrom(fullPath);

            return assembly;
        }

        /// <summary>
        /// This method will return the assembly if it is located.
        /// </summary>
        /// <param name="fullName">
        /// Full name of the assembly
        /// </param>
        /// <returns>
        /// The assembly or null.
        /// </returns>
        public virtual Assembly Resolve(string fullName)
        {
            try
            {
                Assembly assembly = fullName.StartsWith(ResourceNameSpace) ? this.GetType().Assembly : null;
                if (assembly == null
                    && (this.GetType().Assembly.FullName.Equals(fullName) || this.GetType().Assembly.FullName.StartsWith(fullName + ",")))
                {
                    assembly = this.GetType().Assembly;
                }

                if (assembly == null)
                {
                    string[] parts = fullName.Split(GenericConstants.CHAR_COMMA);
                    if (parts[0].EndsWith(".resources") || parts[0].EndsWith(".XmlSerializers"))
                    {
                        return null;
                    }
                }

                return assembly ?? LoadAssembly(typeof(AssemblyLoader).Assembly, fullName, ResourceNameSpace);
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Did not locate " + fullName + ".", exception);
                return null;
            }
        }

        #endregion

        #region Methods

        private static Assembly LoadAssembly(Assembly resourceAssembly, string assemblyName, string resourceNameSpace)
        {
            SBPlusClient.LogInformation("Assembly requested " + assemblyName);

            string rns = resourceNameSpace.EndsWith(".") ? resourceNameSpace : resourceNameSpace + ".";

            string resourceName = rns + new AssemblyName(assemblyName).Name;
            resourceName = resourceName.ToLower().EndsWith(".dll") ? resourceName : resourceName + ".dll";

            if (LoadedAssemblies.Contains(assemblyName))
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (!assembly.FullName.Equals(assemblyName))
                    {
                        continue;
                    }

                    SBPlusClient.LogInformation(assemblyName + " already Loaded, returning assembly from AppDomain.");
                    Debug.WriteLine(
                        "[AssemblyLoader.LoadAssembly(31)] " + assemblyName + " already Loaded, returning assembly from AppDomain.");
                    return assembly;
                }
            }

            using (var stream = resourceAssembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    try
                    {
                        var assemblyData = new byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);

                        Assembly assembly = Assembly.Load(assemblyData);
                        // Add the name to the cache.
                        if (!LoadedAssemblies.Contains(assemblyName))
                        {
                            LoadedAssemblies.Add(assemblyName);
                        }

                        if (assembly != null)
                        {
                            SBPlusClient.LogInformation("Loaded assembly " + assembly.FullName);
                            Debug.WriteLine("[AssemblyLoader.LoadAssembly(56)] " + assembly.FullName);
                        }

                        return assembly;
                    }
                    catch (Exception exception)
                    {
                        SBPlusClient.LogError("Could not create stream for " + assemblyName, exception);
                        Debug.WriteLine("[AssemblyLoader.LoadAssembly(54)] Exception loading assembly " + assemblyName);
                    }
                }
            }

            return null;
        }

        #endregion
    }
}