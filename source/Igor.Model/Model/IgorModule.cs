using System;
using System.Collections.Generic;
using System.Text;

namespace Igor.Model
{
    public class IgorAttribute
    {
        public string Target { get; }

        public string Name { get; }
        public string Value { get; }

        public IgorAttribute(string target, string name, string value)
        {
            Target = target;
            Name = name;
            Value = value;
        }

    }

    public abstract class IgorDeclaration
    {
        public string Name { get; }
        public string Annotation { get; set; }
        
        public IReadOnlyList<IgorAttribute> Attributes => attributes;

        private readonly List<IgorAttribute> attributes = new List<IgorAttribute>();

        protected IgorDeclaration(string name)
        {
            Name = name;
        }

        public void Attribute(string name, string value = null, string target = "*")
        {
            attributes.Add(new IgorAttribute(target, name, value));
        }
    }

    public class IgorDefine : IgorDeclaration
    {
        public string Type { get; set; }

        public IgorDefine(string name) : base(name)
        {
        }
    }

    public class IgorRawDeclaration : IgorDeclaration
    {
        public string Content { get; set; }

        public IgorRawDeclaration(string name) : base(name)
        {
        }
    }

    public class IgorModule : IgorDeclaration
    {
        public IReadOnlyList<IgorDeclaration> Declarations => declarations;

        private readonly List<IgorDeclaration> declarations = new List<IgorDeclaration>();

        public IgorModule(string name) : base(name)
        {
        }

        public IgorEnum Enum(string name) => (IgorEnum)declarations.GetOrAdd(name, d => d.Name, () => new IgorEnum(name));
        public IgorStruct Struct(string name, IgorStructType? type = null)
        {
            var result = (IgorStruct)declarations.GetOrAdd(name, d => d.Name, () => new IgorStruct(name));
            if (type.HasValue)
                result.StructType = type.Value;
            return result;
        }
        public IgorStruct Record(string name) => Struct(name, IgorStructType.Record);
        public IgorStruct Variant(string name) => Struct(name, IgorStructType.Variant);
        public IgorStruct Interface(string name) => Struct(name, IgorStructType.Interface);

        public IgorWebService WebService(string name)
        {
            return (IgorWebService)declarations.GetOrAdd(name, d => d.Name, () => new IgorWebService(name));
        }

        public IgorRawDeclaration RawDeclaration(string name, string content = null)
        {
            var result = (IgorRawDeclaration)declarations.GetOrAdd(name, d => d.Name, () => new IgorRawDeclaration(name));
            if (content != null)
                result.Content = content;
            return result;
        }

        public IgorDefine Define(string name) => (IgorDefine)declarations.GetOrAdd(name, d => d.Name, () => new IgorDefine(name));
    }
}
