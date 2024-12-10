using Igor.Compiler;
using System.Collections.Generic;

namespace Igor
{
    /// <summary>
    /// Generation context singleton
    /// </summary>
    public class Context
    {
        /// <summary>
        /// Current target name
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Target version provided by user (or default target version if not provided by user).
        /// May be null if target doesn't support versions.
        /// </summary>
        public System.Version TargetVersion { get; set; }

        /// <summary>
        /// List of environment attributes (provided via command line)
        /// </summary>
        public IReadOnlyDictionary<string, AttributeValue> Attributes { get; set; }

        /// <summary>
        /// Get a single attribute value set for this AST statement or inherited using inheritance type defined by attribute descriptor
        /// (or default value if value is unset in Igor source or environment)
        /// </summary>
        /// <typeparam name="T">Attribute value type argument</typeparam>
        /// <param name="attribute">Attribute descriptor</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Attribute value or default</returns>
        public T Attribute<T>(AttributeDescriptor<T> attribute, T defaultValue)
        {
            if (Attributes.TryGetValue(attribute.Name, out var attributeValue) && attribute.Convert(attributeValue, out T val))
                return val;
            else
                return defaultValue;
        }

        /// <summary>
        /// Enable verbose logging
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Enable Unicode BOM file header
        /// </summary>
        public bool Bom { get; set; }

        /// <summary>
        /// Compiler output for logging and problem reporting
        /// </summary>
        public CompilerOutput CompilerOutput { get; set; }

        private Context()
        {
        }

        private static Context instance;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static Context Instance
        {
            get
            {
                if (instance == null)
                    instance = new Context();
                return instance;
            }
        }
    }
}
