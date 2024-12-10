using System.Collections.Generic;
using System.Linq;

namespace Igor.UE4.Model
{
    /// <summary>
    /// C++ member access modifier
    /// </summary>
    public enum AccessModifier
    {
        Public,
        Protected,
        Private,
    }

    /// <summary>
    /// C++ class or struct field model
    /// </summary>
    public class UeStructField : UeMacroHost
    {
        /// <summary>
        /// Field name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Access modifier
        /// </summary>
        public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

        /// <summary>
        /// C++ type name
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Default value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Whether this field is an UPROPERTY
        /// </summary>
        public bool UProperty { get; set; }

        /// <summary>
        /// Documentation comment
        /// </summary>
        public string Comment { get; set; }

        internal UeStructField(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// C++ class or struct function (method) model
    /// </summary>
    public class UeFunction : UeMacroHost
    {
        /// <summary>
        /// Access modifier
        /// </summary>
        public AccessModifier AccessModifier { get; }

        /// <summary>
        /// Function code text
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Whether this function is UFUNCTION
        /// </summary>
        public bool UFunction { get; set; }

        /// <summary>
        /// Documentation comment
        /// </summary>
        public string Comment { get; set; }

        internal UeFunction(string text, AccessModifier accessModifier)
        {
            Text = text;
            AccessModifier = accessModifier;
        }
    }

    /// <summary>
    /// Class or struct type
    /// </summary>
    public enum StructType
    {
        Struct,
        Class,
    }

    /// <summary>
    /// C++ class or struct model
    /// </summary>
    public class UeStruct : UeMacroHost
    {
        /// <summary>
        /// C++ declaration name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Documentation comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// API macro
        /// </summary>
        public string ApiMacro { get; set; }

        /// <summary>
        /// Whether this is a struct or class
        /// </summary>
        public StructType Type { get; set; }

        /// <summary>
        /// Whether this is an UCLASS
        /// </summary>
        public bool UClass { get; set; }

        /// <summary>
        /// Whether this is an USTRUCT
        /// </summary>
        public bool UStruct { get; set; }

        internal readonly List<string> BaseTypes = new List<string>();
        public string[] GenericArguments { get; set; }
        internal readonly List<UeStructField> Fields = new List<UeStructField>();
        internal readonly List<UeFunction> Functions = new List<UeFunction>();
        internal readonly List<(UeStruct, AccessModifier)> Structs = new List<(UeStruct, AccessModifier)>();
        internal readonly List<(UeTypedef, AccessModifier)> Typedefs = new List<(UeTypedef, AccessModifier)>();
        internal readonly List<string> Friends = new List<string>();

        internal UeStruct(string name)
        {
            this.Name = name;
        }

        internal IEnumerable<UeStruct> GetStructs(AccessModifier accessModifier)
        {
            foreach (var sa in Structs)
                if (sa.Item2 == accessModifier)
                    yield return sa.Item1;
        }

        internal IEnumerable<UeTypedef> GetTypedefs(AccessModifier accessModifier)
        {
            foreach (var sa in Typedefs)
                if (sa.Item2 == accessModifier)
                    yield return sa.Item1;
        }

        internal IEnumerable<UeFunction> GetFunctions(AccessModifier accessModifier) => Functions.Where(f => f.AccessModifier == accessModifier);
        internal IEnumerable<UeStructField> GetFields(AccessModifier accessModifier) => Fields.Where(f => f.AccessModifier == accessModifier);

        /// <summary>
        /// Find or create a field by name. Same as Field
        /// </summary>
        /// <param name="name">Name of the field to be created or found</param>
        /// <returns>C++ field model</returns>
        public UeStructField Field(string name)
        {
            return Fields.GetOrAdd(name, p => p.Name, () => new UeStructField(name));
        }

        /// <summary>
        /// Find or create a new field with the specified name, type and access modifier
        /// </summary>
        /// <param name="name">Name of the field to be created or found</param>
        /// <param name="type">C++ field type string</param>
        /// <param name="accessModifier">Access modifier</param>
        /// <returns>C++ field model</returns>
        public UeStructField Field(string name, string type, AccessModifier accessModifier)
        {
            var prop = Field(name);
            prop.Type = type;
            prop.AccessModifier = accessModifier;
            return prop;
        }

        /// <summary>
        /// Provide a base type for the inherited struct or class
        /// </summary>
        /// <param name="baseType">Base type name</param>
        public void BaseType(string baseType)
        {
            if (!BaseTypes.Contains(baseType))
                BaseTypes.Add(baseType);
        }

        public UeFunction Function(string text, AccessModifier accessModifier)
        {
            var function = new UeFunction(text, accessModifier);
            Functions.Add(function);
            return function;
        }

        /// <summary>
        /// Find or create a nested struct or class declaration with a given name and access modifier
        /// </summary>
        /// <param name="name">A name of a struct or class to be found or created</param>
        /// <param name="accessModifier">Access modifier</param>
        /// <returns>Struct or class model</returns>
        public UeStruct StructOrClass(string name, AccessModifier accessModifier = AccessModifier.Public)
        {
            (UeStruct s, AccessModifier _) = Structs.FirstOrDefault(sa => sa.Item1.Name == name);
            if (s == null)
            {
                s = new UeStruct(name);
                Structs.Add((s, accessModifier));
            }
            return s;
        }

        /// <summary>
        /// Find or create a nested struct declaration with a given name and access modifier
        /// </summary>
        /// <param name="name">A name of a struct to be found or created</param>
        /// <param name="accessModifier">Access modifier</param>
        /// <returns>Struct or class model</returns>
        public UeStruct Struct(string name, AccessModifier accessModifier = AccessModifier.Public)
        {
            var result = StructOrClass(name, accessModifier);
            result.Type = StructType.Struct;
            return result;
        }

        /// <summary>
        /// Find or create a nested class declaration with a given name and access modifier
        /// </summary>
        /// <param name="name">A name of a class to be found or created</param>
        /// <param name="accessModifier">Access modifier</param>
        /// <returns>Struct or class model</returns>
        public UeStruct Class(string name, AccessModifier accessModifier = AccessModifier.Public)
        {
            var result = StructOrClass(name, accessModifier);
            result.Type = StructType.Class;
            return result;
        }

        public UeTypedef Typedef(string name, string declaration, AccessModifier accessModifier)
        {
            var typedef = new UeTypedef(name, declaration);
            Typedefs.Add((typedef, accessModifier));
            return typedef;
        }

        /// <summary>
        /// Provide a friend type name
        /// </summary>
        /// <param name="name">Friend type name</param>
        public void Friend(string name)
        {
            if (!Friends.Contains(name))
                Friends.Add(name);
        }
    }
}
