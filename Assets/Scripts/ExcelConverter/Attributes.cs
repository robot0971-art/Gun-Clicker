using System;

namespace ExcelConverter
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SheetAttribute : Attribute
    {
        public string Name { get; }
        
        public SheetAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class IgnoreAttribute : Attribute
    {
    }
}