using System;
using System.IO;
using System.Reflection;

namespace Youtube_Sync
{
    internal class AssemblyHelper
    {        
        public static string AssemblyTitle
        {
            get
            {
                var customAttributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (customAttributes.Length != 0)
                {
                    var assemblyTitleAttribute = (AssemblyTitleAttribute)customAttributes[0];
                    if (assemblyTitleAttribute.Title != "")
                        return assemblyTitleAttribute.Title;
                }

                return Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().CodeBase);
            }
        }
        public static string AssemblyVersion => Assembly.GetCallingAssembly().GetName().Version.ToString();
        public static Assembly CurrentAssembly => Assembly.GetCallingAssembly();
    }
}