namespace SimpleMedicalORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NavigationPropertyAttribute : Attribute
    {
        public string ForeignKeyProperty { get; }

        public NavigationPropertyAttribute(string foreignKeyProperty)
        {
            ForeignKeyProperty = foreignKeyProperty;
        }
    }
}