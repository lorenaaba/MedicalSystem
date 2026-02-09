namespace SimpleMedicalORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; }
        public string? SqlType { get; set; }
        public bool IsNullable { get; set; } = true;
        public bool IsUnique { get; set; } = false;
        public string? DefaultValue { get; set; }

        public ColumnAttribute(string name)
        {
            Name = name;
        }
    }
}