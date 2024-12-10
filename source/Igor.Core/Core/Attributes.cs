using Igor.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Igor
{
    public interface IAttributeHost
    {
        IAttributeHost ScopeHost { get; }
        IAttributeHost ParentTypeHost { get; }
        IAttributeHost InheritedHost { get; }
        IEnumerable<IAttributeHost> NestedHosts { get; }
        IReadOnlyList<AttributeDefinition> Attributes { get; }
    }

    public class AttributeDefinition
    {
        public string Target { get; }
        public string Name { get; }
        public AttributeValue Value { get; }
        public Location Location { get; }

        public AttributeDefinition(Location location, string target, string name, AttributeValue value)
        {
            Location = location;
            Target = target;
            Name = name;
            Value = value;
        }
    }

    public class AttributeObjectProperty
    {
        public string Name { get; }
        public AttributeValue Value { get; }
        public Location Location { get; }

        public AttributeObjectProperty(Location location, string name, AttributeValue value)
        {
            Location = location;
            Name = name;
            Value = value;
        }
    }

    public abstract class AttributeValue : ILocated, System.IEquatable<AttributeValue>
    {
        public class Bool : AttributeValue
        {
            public bool Value { get; }

            public Bool(Location location, bool value) : base(location) => Value = value;

            public override bool Equals(AttributeValue other) => ReferenceEquals(this, other) || other is Bool v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value ? "true" : "false";
        }

        public class Integer : AttributeValue
        {
            public long Value { get; }

            public Integer(Location location, long value) : base(location) => Value = value;

            public override bool Equals(AttributeValue other) => ReferenceEquals(this, other) || other is Integer v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        }

        public class Float : AttributeValue
        {
            public double Value { get; }

            public Float(Location location, double value) : base(location) => Value = value;

            public override bool Equals(AttributeValue other) => ReferenceEquals(this, other) || other is Float v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        }

        public class String : AttributeValue
        {
            public string Value { get; }

            public String(Location location, string value) : base(location) => Value = value;

            public override bool Equals(AttributeValue other) => ReferenceEquals(this, other) || other is String v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value.Quoted();
        }

        public class Enum : AttributeValue
        {
            public string Value { get; }

            public Enum(Location location, string value) : base(location) => Value = value;

            public override bool Equals(AttributeValue other) => ReferenceEquals(this, other) || other is String v && v.Value == Value;

            public override int GetHashCode() => Value.GetHashCode();

            public override string ToString() => Value;
        }

        public class Object : AttributeValue
        {
            public IReadOnlyList<AttributeObjectProperty> Definitions { get; }

            public Object(Location location, IReadOnlyList<AttributeObjectProperty> definitions) : base(location) => this.Definitions = definitions;

            public override bool Equals(AttributeValue other) => ReferenceEquals(this, other) || other is Object v; // TODO: && v.Value == Value;

            public override int GetHashCode() => 1002;

            public override string ToString() => $"({Definitions.JoinStrings(def => $"{def.Name} = {def.Value.ToString()}")})";
        }

        public abstract bool Equals(AttributeValue other);

        public override bool Equals(object obj)
        {
            return obj is AttributeValue other && Equals(this, other);
        }

        protected AttributeValue(Location location)
        {
            Location = location;
        }

        public Location Location { get; }

        public override int GetHashCode() => base.GetHashCode();

        public static AttributeValue Parse(string val)
        {
            switch (val)
            {
                case "true": return new Bool(Location.NoLocation, true);
                case "false": return new Bool(Location.NoLocation, false);
                case var str when (str.Length >= 2 && str.First() == '"' && str.Last() == '"'): return new String(Location.NoLocation, str.Substring(1, str.Length - 2));
                case var str when (str.Length >= 1 && char.IsDigit(str.First()) && str.Contains('.')): return new Float(Location.NoLocation, double.Parse(str, CultureInfo.InvariantCulture));
                case var str when (str.Length >= 1 && char.IsDigit(str.First())): return new Integer(Location.NoLocation, long.Parse(str));
                default: return new Enum(Location.NoLocation, val);
            }
        }
    }
}
