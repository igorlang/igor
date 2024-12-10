namespace Igor.TypeScript.Model
{
    /// <summary>
    /// Base class for TypeScript top-level declarations
    /// </summary>
    public abstract class TsDeclaration
    {
        /// <summary>
        /// TypeScript name of declared entity
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// TSDoc annotation
        /// </summary>
        public string Annotation { get; set; }
    }
}
