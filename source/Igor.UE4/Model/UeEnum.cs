using System.Collections.Generic;

namespace Igor.UE4.Model
{
    /// <summary>
    /// C++ enum field model
    /// </summary>
    public class UeEnumField
    {
        /// <summary>
        /// Enum field name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Integer value for this field
        /// </summary>
        public long Value { get; set; }

        /// <summary>
        /// Documentation comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// UE4 metadata specifiers. 
        /// Set null value for keys that don't require value.
        /// See <a href="https://docs.unrealengine.com/en-US/Programming/UnrealArchitecture/Reference/Metadata/index.html">UE4 documentation</a> for reference.
        /// </summary>
        public Dictionary<string, string> Meta { get; } = new Dictionary<string, string>();

        internal UeEnumField(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// C++ enum model
    /// </summary>
    public class UeEnum : UeMacroHost
    {
        /// <summary>
        /// Enum name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Base integer type (may be null)
        /// </summary>
        public string IntType { get; set; }

        /// <summary>
        /// Documentation comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Whether this enum is UENUM
        /// </summary>
        public bool UEnum { get; set; }

        internal readonly List<UeEnumField> Fields = new List<UeEnumField>();

        internal UeEnum(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Find or add a new enum field
        /// </summary>
        /// <param name="name">Name of the field to be found or created</param>
        /// <returns>Enum field model</returns>
        public UeEnumField Field(string name)
        {
            return Fields.GetOrAdd(name, f => f.Name, () => new UeEnumField(name));
        }
    }
}
