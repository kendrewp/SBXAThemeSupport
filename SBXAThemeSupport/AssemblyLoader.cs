using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SBXA.Shared;
using SBXA.UI.Client;

namespace SBXAThemeSupport
{
    public class AssemblyLoader : IAssemblyResolver
    {
        static readonly StringCollection _LoadedAssemblies = new StringCollection();


        public AssemblyLoader(string resourceNameSpace)
        {
            ResourceNameSpace = resourceNameSpace.EndsWith(".") ? resourceNameSpace : resourceNameSpace+".";
        }

        public static string ResourceNameSpace { get; set; }

        private static Assembly LoadAssembly(Assembly resourceAssembly, string assemblyName, string resourceNameSpace)
        {
            SBPlusClient.LogInformation("Assembly requested " + assemblyName);

            string rns = resourceNameSpace.EndsWith(".") ? resourceNameSpace : resourceNameSpace + ".";

            string resourceName = rns + new AssemblyName(assemblyName).Name;
            resourceName = resourceName.ToLower().EndsWith(".dll") ? resourceName : resourceName + ".dll";

            if (_LoadedAssemblies.Contains(assemblyName))
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    if (!assembly.FullName.Equals(assemblyName)) continue;

                    SBPlusClient.LogInformation(assemblyName + " already Loaded, returning assembly from AppDomain.");
                    Debug.WriteLine("[AssemblyLoader.LoadAssembly(31)] " + assemblyName + " already Loaded, returning assembly from AppDomain.");
                    return (assembly);
                }
            }
            using (var stream = resourceAssembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    try
                    {
                        Byte[] assemblyData = new Byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);

                        Assembly assembly = Assembly.Load(assemblyData);
                        // Add the name to the cache.
                        if (!_LoadedAssemblies.Contains(assemblyName))
                        {
                            _LoadedAssemblies.Add(assemblyName);
                        }

                        if (assembly != null)
                        {
                            SBPlusClient.LogInformation("Loaded assembly " + assembly.FullName);
                            Debug.WriteLine("[AssemblyLoader.LoadAssembly(56)] " + assembly.FullName);
                        }
                        return (assembly);
                    }
                    catch (Exception exception)
                    {
                        SBPlusClient.LogError("Could not create stream for " + assemblyName, exception);
                        Debug.WriteLine("[AssemblyLoader.LoadAssembly(54)] Exception loading assembly " + assemblyName);
                    }
                }
            }
            return (null);
        }

        public Assembly LoadAssembly(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return null;

            string fullPath = ExtensibilityManager.GetAssemblyInfo(fullName).FullPath;
            if (string.IsNullOrEmpty(fullPath)) return null;

            Assembly assembly = Assembly.LoadFrom(fullPath);

            return (assembly);
        }

        public static void WriteAssembliesToCurrentDirectory(Assembly resourceAssembly, string[] assemblyNames, string assNamespace)
        {
            string assemblyNamespace = assNamespace.EndsWith(".") ? assNamespace : assNamespace + ".";
            try
            {

                foreach (string assemblyName in assemblyNames)
                {
                    string resourceName = assemblyNamespace + new AssemblyName(assemblyName).Name;
                    using (var stream = resourceAssembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            try
                            {
                                Byte[] assemblyData = new Byte[stream.Length];
                                stream.Read(assemblyData, 0, assemblyData.Length);
                                SBPlusClient.LogInformation("Writing " + assemblyName + " to " + Environment.CurrentDirectory);
                                File.WriteAllBytes(assemblyName, assemblyData);
                            }
                            catch (Exception exception)
                            {
                                SBPlusClient.LogError("Failed to write " + assemblyName, exception);
                                Debug.WriteLine("[AssemblyLoader.LoadAssembly(54)] Exception writing assembly " +
                                                assemblyName);
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

        public virtual Assembly Resolve(string fullName)
        {
            try
            {
                Assembly assembly = fullName.StartsWith(ResourceNameSpace) ? GetType().Assembly : null;
                if (assembly == null && (GetType().Assembly.FullName.Equals(fullName) || GetType().Assembly.FullName.StartsWith(fullName + ",")))
                {
                    assembly = GetType().Assembly;
                }
                if (assembly == null)
                {
                    string[] parts = fullName.Split(GenericConstants.CHAR_COMMA);
                    if (parts[0].EndsWith(".resources") || parts[0].EndsWith(".XmlSerializers")) return (null);
                }
                return assembly ?? (assembly = LoadAssembly(typeof (AssemblyLoader).Assembly, fullName, ResourceNameSpace));
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Did not locate " + fullName + ".", exception);
                return (null);
            }
        }

    }
}
  
