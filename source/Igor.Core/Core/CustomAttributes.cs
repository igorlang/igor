using System;

namespace Igor
{
    /// <summary>
    /// Mark class containing custom attributes with [CustomAttributes] to let Igor be aware of them.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomAttributesAttribute : Attribute
    {
    }
}
