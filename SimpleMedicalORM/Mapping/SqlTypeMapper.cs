namespace SimpleMedicalORM.Mapping
{
    public static class SqlTypeMapper
    {
        public static string GetSqlType(Type clrType)
        {
            if (clrType == typeof(int) || clrType == typeof(int?))
                return "INTEGER";

            if (clrType == typeof(long) || clrType == typeof(long?))
                return "BIGINT";

            if (clrType == typeof(decimal) || clrType == typeof(decimal?))
                return "DECIMAL(18,2)";

            if (clrType == typeof(float) || clrType == typeof(float?))
                return "FLOAT";

            if (clrType == typeof(double) || clrType == typeof(double?))
                return "DOUBLE PRECISION";

            if (clrType == typeof(string))
                return "VARCHAR(255)";

            if (clrType == typeof(DateTime) || clrType == typeof(DateTime?))
                return "TIMESTAMP WITHOUT TIME ZONE";

            if (clrType == typeof(DateTimeOffset) || clrType == typeof(DateTimeOffset?))
                return "TIMESTAMP WITH TIME ZONE";

            if (clrType == typeof(bool) || clrType == typeof(bool?))
                return "BOOLEAN";

            throw new NotSupportedException($"Type {clrType.Name} is not supported");
        }

        public static object? ConvertFromDb(object? value, Type targetType)
        {
            if (value == null || value is DBNull)
                return null;

            if (targetType == typeof(string))
                return value.ToString();

            if (targetType == typeof(int) || targetType == typeof(int?))
                return Convert.ToInt32(value);

            if (targetType == typeof(long) || targetType == typeof(long?))
                return Convert.ToInt64(value);

            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                return Convert.ToDecimal(value);

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                return Convert.ToDateTime(value);

            if (targetType == typeof(bool) || targetType == typeof(bool?))
                return Convert.ToBoolean(value);

            return value;
        }
    }
}