using System.Globalization;
using Igor.Core.AST;
using Igor.Text;
using System.Linq;

namespace Igor.TestRunner
{
    public abstract class Accessor
    {
        public abstract object Access(object host);
    }

    public class PropertyAccessor : Accessor
    {
        public string Property { get; set; }

        public override object Access(object host)
        {
            var propInfo = host.GetType().GetProperty(Property);
            if (propInfo == null)
                return null;
            else
                return propInfo.GetValue(host);
        }

        public override string ToString() => $".{Property}";

        public PropertyAccessor(string property) => Property = property;

        public override bool Equals(object obj) => obj is PropertyAccessor pa && Property == pa.Property;

        public override int GetHashCode() => Property.GetHashCode();
    }

    public class IndexAccessor : Accessor
    {
        public int Index { get; set; }

        public override object Access(object host)
        {
            var indexProperty = host.GetType().GetProperties().FirstOrDefault(p => p.GetIndexParameters().Length == 1);
            if (indexProperty == null)
                return null;
            else
                return indexProperty.GetValue(host, new object[] { Index });
        }

        public override string ToString() => $"[{Index}]";

        public IndexAccessor(int index) => Index = index;

        public override bool Equals(object obj) => obj is IndexAccessor ia && Index == ia.Index;

        public override int GetHashCode() => Index.GetHashCode();
    }

    public abstract class Expression
    {
        public abstract object Evaluate(Module file);
    }

    public class IntegerExpression : Expression
    {
        public int Value { get; }

        public override object Evaluate(Module module) => Value;

        public override string ToString() => Value.ToString();

        public IntegerExpression(int value) => Value = value;

        public override bool Equals(object obj) => obj is IntegerExpression exp && Value == exp.Value;

        public override int GetHashCode() => Value.GetHashCode();
    }

    public class FloatExpression : Expression
    {
        public double Value { get; }

        public override object Evaluate(Module module) => Value;

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

        public FloatExpression(double value) => Value = value;

        public override bool Equals(object obj) => obj is FloatExpression exp && Value == exp.Value;

        public override int GetHashCode() => Value.GetHashCode();
    }

    public class StringExpression : Expression
    {
        public string Value { get; }

        public override object Evaluate(Module module) => Value;

        public override string ToString() => Value.Quoted();

        public StringExpression(string value) => Value = value;

        public override bool Equals(object obj) => obj is StringExpression exp && Value == exp.Value;

        public override int GetHashCode() => Value.GetHashCode();
    }

    public class BoolExpression : Expression
    {
        public bool Value { get; }

        public override object Evaluate(Module module) => Value;

        public override string ToString() => Value.ToString();

        public BoolExpression(bool value) => Value = value;

        public override bool Equals(object obj) => obj is BoolExpression exp && Value == exp.Value;

        public override int GetHashCode() => Value.GetHashCode();
    }

    public class NameExpression : Expression
    {
        public int Line { get; }
        public string Name { get; }

        public override object Evaluate(Module module)
        {
            return FindName(module, Name);
        }

        private static IAttributeHost FindName(IAttributeHost host, string name)
        {
            if (host is Statement statement && statement.Name == name)
                return host;
            foreach (var nested in host.NestedHosts)
            {
                var nestedResult = FindName(nested, name);
                if (nestedResult != null)
                    return nestedResult;
            }
            return null;
        }

        public override string ToString() => $"@{Name}";

        public NameExpression(int line, string name)
        {
            Line = line;
            Name = name;
        }

        public override bool Equals(object obj) => obj is NameExpression exp && Line == exp.Line && Name == exp.Name;

        public override int GetHashCode() => Name.GetHashCode();
    }

    public class AccessExpression : Expression
    {
        public Expression Host { get; }
        public Accessor Accessor { get; }

        public override object Evaluate(Module module)
        {
            var hostVal = Host.Evaluate(module);
            if (hostVal == null)
                return null;
            else
                try
                {
                    return Accessor.Access(hostVal);
                }
                catch
                {
                    return null;
                }
        }

        public override string ToString() => $"{Host}{Accessor}";

        public AccessExpression(Expression host, Accessor accessor)
        {
            Host = host;
            Accessor = accessor;
        }

        public override bool Equals(object obj) => obj is AccessExpression exp && Equals(Host, exp.Host) && Equals(Accessor, exp.Accessor);

        public override int GetHashCode() => Host.GetHashCode() + Accessor.GetHashCode() * 17;
    }

    public abstract class Expectation
    {
    }

    public class ErrorExpectation : Expectation
    {
        public int Line { get; }
        public string Spec { get; }

        public override string ToString() => $"Error {Spec} at line {Line}";

        public ErrorExpectation(int line, string spec)
        {
            Line = line;
            Spec = spec;
        }

        public override bool Equals(object obj) => obj is ErrorExpectation exp && Line == exp.Line && Spec == exp.Spec;

        public override int GetHashCode() => Spec.GetHashCode();
    }

    public class MatchExpectation : Expectation
    {
        public Expression Left { get; }
        public Expression Right { get; }

        public override string ToString() => $"{Left} = {Right}";

        public MatchExpectation(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        public override bool Equals(object obj) => obj is MatchExpectation exp && Equals(Left, exp.Left) && Equals(Right, exp.Right);

        public override int GetHashCode() => Left.GetHashCode() + Right.GetHashCode() * 17;
    }
}
