using System.Collections.Generic;

namespace Igor.TypeScript.Model
{
    /// <summary>
    /// TypeScript enum field model
    /// </summary>
    public class TsEnumField
    {
        /// <summary>
        /// Enum field name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Enum field integer value
        /// </summary>
        public long Value { get; set; }

        /// <summary>
        /// Enum field TSDoc annotation
        /// </summary>
        public string Annotation { get; set; }

        public TsEnumField(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// TypeScript enum model
    /// </summary>
    public class TsEnum : TsDeclaration
    {
        /// <summary>
        /// Is enum exported?
        /// </summary>
        public bool Export { get; set; } = true;

        internal List<TsEnumField> Fields { get; } = new List<TsEnumField>();

        internal TsEnum(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Create or find existing enum field by name
        /// </summary>
        /// <param name="name">Enum field name</param>
        /// <returns>Enum field model</returns>
        public TsEnumField Field(string name)
        {
            return Fields.GetOrAdd(name, f => f.Name, () => new TsEnumField(name));
        }
    }
}
