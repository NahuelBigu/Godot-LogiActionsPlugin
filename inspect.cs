using System;
using System.Reflection;
using System.Linq;
using Loupedeck;

namespace Inspector
{
    class Program
    {
        static void Main()
        {
            var type = typeof(PluginDynamicAdjustment);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.IsVirtual)
                .Select(m => m.Name)
                .OrderBy(n => n);

            Console.WriteLine("PluginDynamicAdjustment Virtuals:");
            foreach (var m in methods) Console.WriteLine(" - " + m);

            var type2 = typeof(PluginDynamicCommand);
            var methods2 = type2.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.IsVirtual)
                .Select(m => m.Name)
                .OrderBy(n => n);

            Console.WriteLine("\nPluginDynamicCommand Virtuals:");
            foreach (var m in methods2) Console.WriteLine(" - " + m);
        }
    }
}
