using System;
using System.IO;
using System.Reflection;

namespace MasterMemory.GeneratorCore
{
    public class AssemblyLoader
    {
        public string filePath;

        public AssemblyLoader(string filePath)
        {
            this.filePath = filePath;
        }

        public void Load()
        {
            ResolveEventHandler handler = CurrentDomain_ReflectionOnlyAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += handler;
            try
            {
                

                // new MetadataLoadContext()

                var assembly = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(filePath));

                var types = assembly.GetTypes();


            }
            finally
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= handler;
            }
        }

        private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                if (File.Exists(args.Name))
                {
                    return Assembly.ReflectionOnlyLoad(File.ReadAllBytes(args.Name));
                }
                else
                {
                    var path = Path.Combine(Path.GetDirectoryName(filePath), args.Name);
                    if (File.Exists(path))
                    {
                        return Assembly.ReflectionOnlyLoad(File.ReadAllBytes(path));
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}
