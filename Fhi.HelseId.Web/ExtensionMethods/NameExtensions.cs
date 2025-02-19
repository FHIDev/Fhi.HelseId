using System;

namespace Fhi.HelseId.Web.ExtensionMethods
{
    public static class NameExtensions
    {
        public static string ObfuscateName(this string? name)
            => name == null ? "(null)" : name.Substring(0, Math.Min(3, name.Length)) + "*******";
    }
}
