using System.Reflection;

namespace Domain.Constants
{
    public static class PermissionExtensions
    {
        public static IReadOnlyList<string> GetPermissionValues(this Type permissionGroupType)
        {
            return permissionGroupType
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue()!)
                .ToList();
        }

        public static IReadOnlyList<string> GetAllPermissionValues(this Type permissionsRootType)
        {
            return permissionsRootType
                .GetNestedTypes(BindingFlags.Public)
                .SelectMany(groupType => groupType.GetPermissionValues())
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }
    }
}
