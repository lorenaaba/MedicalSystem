using System.Reflection;
using SimpleMedicalORM.Attributes;

namespace SimpleMedicalORM.Mapping
{
    public class EntityMapper
    {
        public static string GetTableName(Type entityType)
        {
            var tableAttr = entityType.GetCustomAttribute<TableAttribute>();
            return tableAttr?.Name ?? entityType.Name.ToLower() + "s";
        }

        public static string GetColumnName(PropertyInfo property)
        {
            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
            return columnAttr?.Name ?? property.Name.ToLower();
        }

        public static PropertyInfo? GetPrimaryKeyProperty(Type entityType)
        {
            return entityType.GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
        }

        public static IEnumerable<PropertyInfo> GetColumnProperties(Type entityType)
        {
            return entityType.GetProperties()
                .Where(p => p.GetCustomAttribute<NavigationPropertyAttribute>() == null)
                .Where(p => !p.PropertyType.IsClass || p.PropertyType == typeof(string));
        }

        public static IEnumerable<PropertyInfo> GetNavigationProperties(Type entityType)
        {
            return entityType.GetProperties()
                .Where(p => p.GetCustomAttribute<NavigationPropertyAttribute>() != null);
        }

        public static string GetSqlType(PropertyInfo property)
        {
            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();

            if (!string.IsNullOrEmpty(columnAttr?.SqlType))
                return columnAttr.SqlType;

            return SqlTypeMapper.GetSqlType(property.PropertyType);
        }

        public static bool IsNullable(PropertyInfo property)
        {
            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();

            if (columnAttr != null)
                return columnAttr.IsNullable;

            return Nullable.GetUnderlyingType(property.PropertyType) != null ||
                   property.PropertyType == typeof(string);
        }

        public static bool IsUnique(PropertyInfo property)
        {
            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
            return columnAttr?.IsUnique ?? false;
        }

        public static bool IsPrimaryKey(PropertyInfo property)
        {
            return property.GetCustomAttribute<PrimaryKeyAttribute>() != null;
        }

        public static bool IsAutoIncrement(PropertyInfo property)
        {
            var pkAttr = property.GetCustomAttribute<PrimaryKeyAttribute>();
            return pkAttr?.AutoIncrement ?? false;
        }
    }
}