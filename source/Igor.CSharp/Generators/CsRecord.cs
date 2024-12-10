using Igor.CSharp.Model;
using Igor.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Igor.CSharp
{
    [Flags]
    public enum RecordFeatures
    {
        None = 0,
        Reference = 1,
        Abstract = 2,
        IsInherited = 4,
        DefaultCtor = 8,
        SetupCtor = 16,
        EqualsAndGetHashCode = 32,
        InheritedGetHashCode = 64,
        Equality = 128,
        Immutable = 256,
    }

    public enum PropertyType
    {
        None,
        Abstract,
        Virtual,
        Override,
    }

    public class CsPropertyInfo
    {
        public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
        public IReadOnlyList<string> Attributes { get; set; }
        public CsType Type { get; set; }
        public string Name { get; set; }
        public string VarName => Helper.ShadowName(Name.Format(Notation.LowerCamel));
        public bool IsReadOnly { get; set; } = false;
        public PropertyType PropertyType { get; set; } = PropertyType.None;
        public string Value { get; set; }
        public string Expression { get; set; }
        public bool IgnoreEquals { get; set; } = false;
        public bool IgnoreSetupCtor { get; set; } = false;
        public bool IsInherited { get; set; } = false;
        public bool IsParentSetup { get; set; }
        public string Summary { get; set; }

        public bool IsAbstract => PropertyType == PropertyType.Abstract;
    }

    public static class CsRecord
    {
        public static void Record(this CsClass c, IReadOnlyList<CsPropertyInfo> properties, string ns, RecordFeatures features)
        {
            var isReference = features.HasFlag(RecordFeatures.Reference);
            var isInherited = features.HasFlag(RecordFeatures.IsInherited);
            var isAbstract = features.HasFlag(RecordFeatures.Abstract);

            if (features.HasFlag(RecordFeatures.DefaultCtor))
            {
                var isTrivial = !features.HasFlag(RecordFeatures.SetupCtor) && (CsVersion.SupportsPropertyDefaults || !properties.Any(f => f.IsInherited && f.Value != null)) && !isAbstract;
                if (!isTrivial)
                    c.DefaultConstructor(properties.Where(f => !f.IsInherited), isInherited, isAbstract);
            }

            if (features.HasFlag(RecordFeatures.SetupCtor) || features.HasFlag(RecordFeatures.Immutable))
            {
                c.SetupConstructor(properties, ns);
            }

            if (features.HasFlag(RecordFeatures.EqualsAndGetHashCode))
            {
                var hashCodeFields = isInherited ? properties.Where(f => !f.IsInherited && !f.IgnoreEquals) : properties.Where(f => !f.IgnoreEquals);
                var csEqualsFields = properties.Where(f => !f.IgnoreEquals).ToArray();

                c.DefineGetHashCode(hashCodeFields, ns, isInherited && features.HasFlag(RecordFeatures.InheritedGetHashCode));
                c.DefineEquals(csEqualsFields, isReference, isAbstract);
            }

            if (features.HasFlag(RecordFeatures.Equality))
            {
                c.DefineEquality(isReference, isAbstract);
            }

            var csDefinedFields = properties.Where(field => (field.Expression != null) || !field.IsInherited);
            foreach (var f in csDefinedFields)
                c.Property(f, ns, features.HasFlag(RecordFeatures.DefaultCtor));
        }

        public static void Property(this CsClass c, CsPropertyInfo property, string ns, bool allowPropertyValues)
        {
            var sb = new StringBuilder();
            sb.Append(property.AccessModifier.ToString().ToLower());
            sb.Append(" ");
            if (property.PropertyType != PropertyType.None)
            {
                sb.Append(property.PropertyType.ToString().ToLower());
                sb.Append(" ");
            }
            sb.Append(property.Type.relativeName(ns));
            sb.Append(" ");
            sb.Append(property.Name);

            if (Context.Instance.TargetVersion >= CsVersion.Version60)
            {
                if (property.IsReadOnly)
                {
                    if (property.Expression != null)
                        sb.Append($" => {property.Expression};");
                    else
                        sb.Append(" { get; }");
                }
                else
                {
                    sb.Append(" { get; set; }");
                }

                if (allowPropertyValues && property.Value != null)
                {
                    sb.Append($" = {property.Value};");
                }
            }
            else
            {
                if (property.IsReadOnly)
                {
                    if (property.Expression != null)
                        sb.Append($@"
{{
    get {{ return {property.Expression}; }}
}}

");
                    else if (property.PropertyType == PropertyType.None)
                        sb.Append(" { get; private set; }");
                    else
                        sb.Append(" { get; }");
                }
                else
                {
                    sb.Append(" { get; set; }");
                }
            }

            var classProperty = c.Property(property.Name, sb.ToString());
            if (property.Attributes != null)
                classProperty.AddAttributes(property.Attributes);
            classProperty.Summary = property.Summary;
        }

        public static void DefaultConstructor(this CsClass c, IEnumerable<CsPropertyInfo> properties, bool isInherited, bool isProtected)
        {
            var csConstructorAccess = isProtected ? "protected" : "public";
            var r = new Renderer();
            r += $"{csConstructorAccess} {c.Name}()";
            r += "{";
            r++;
            foreach (var property in properties)
            {
                if (property.Value != null && !CsVersion.SupportsPropertyDefaults)
                {
                    r += $"{property.Name} = {property.Value};";
                }
            }
            r--;
            r += "}";
            c.Constructor(r.Build());
        }

        public static void SetupConstructor(this CsClass c, IEnumerable<CsPropertyInfo> properties, string ns)
        {
            string csRequire(CsPropertyInfo prop)
            {
                if (prop.Type.csNotNullRequired)
                    return
$@"    if ({prop.VarName} == null)
        throw new System.ArgumentNullException({CsVersion.NameOf(prop.VarName)});";
                else
                    return null;
            }

            var initFields = properties.Where(f => f.Expression == null && !f.IsAbstract && !f.IgnoreSetupCtor).ToList();
            var lastWithoutDefault = initFields.FindLastIndex(f => !f.Type.isOptional && (f.Value == null || !f.Type.isLiteral));
            var sb = new StringBuilder();
            for (int i = 0; i < initFields.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                var prop = initFields[i];
                sb.Append($"{prop.Type.relativeName(ns)} {prop.VarName}");
                if (i > lastWithoutDefault)
                {
                    if (prop.Value != null)
                        sb.Append($" = {prop.Value}");
                    else
                        sb.Append(" = null");
                }
            }
            var initStrings = initFields.Where(f => !f.IsParentSetup).JoinLines(field => $"    this.{field.Name} = {field.VarName};");
            var ctorAccess = c.Abstract ? "protected" : "public";
            string baseCtor = null;
            var inheritedFields = initFields.Where(f => f.IsParentSetup);
            if (inheritedFields.Any())
            {
                baseCtor += $"\n    : base({inheritedFields.JoinStrings(", ", prop => prop.VarName)})";
            }
            var ctor = $@"
{ctorAccess} {c.Name}({sb}){baseCtor}
{{
{initFields.JoinLines(csRequire)}
{initStrings}
}}";
            c.Constructor(ctor);
        }

        public static void DefineGetHashCode(this CsClass c, IEnumerable<CsPropertyInfo> properties, string ns, bool isInherited)
        {
            var baseHash = isInherited ? "base.GetHashCode()" : "17";
            string CsGetHashCode(CsPropertyInfo property) =>
                property.Type.getHashCode(property.Name, ns);
            var hashCodes = properties.JoinLines(field => $"        hash = hash * 23 + {CsGetHashCode(field)};");
            var csGetHashCodeImpl =
$@"public override int GetHashCode()
{{
    unchecked
    {{
        int hash = {baseHash};
{hashCodes}
        return hash;
    }}
}}
";
            c.Method(csGetHashCodeImpl);
        }

        public static void DefineEquals(this CsClass c, IReadOnlyList<CsPropertyInfo> properties, bool isReference, bool isAbstract)
        {
            string csEquals(CsPropertyInfo prop, string other) => prop.Type.equals(prop.Name, $"{other}.{prop.Name}");

            var objectType = CsVersion.NullableReferenceTypes ? "object?" : "object";

            string csEqualsImpl;
            if (isAbstract)
            {
                var csEqualsComparison = properties.Count == 0 ? "true" : properties.JoinStrings(" && ", f => csEquals(f, "that"));
                csEqualsImpl = $@"public override bool Equals({objectType} obj)
{{
    if (ReferenceEquals(this, obj))
        return true;
    var that = obj as {c.Name};
    if ({CsVersion.IsNull("that")})
        return false;
    return {csEqualsComparison};
}}
";
            }
            else if (isReference)
            {
                csEqualsImpl =
$@"public override bool Equals({objectType} obj)
{{
    return Equals(obj as {c.Name});
}}
";
            }
            else
            {
                csEqualsImpl =
$@"public override bool Equals({objectType} obj)
{{
    if (!(obj is {c.Name}))
        return false;
    return Equals(({c.Name})obj);
}}
";
            }

            c.Method(csEqualsImpl);

            if (!isAbstract)
            {
                var csEqualsNullCheck = isReference ? $"    if ({CsVersion.IsNull("other")})\n        return false;" : null;
                var csEqualsReferenceCheck = isReference ? "    if (ReferenceEquals(this, other))\n        return true;" : null;
                var csEqualsComparison = properties.Count == 0 ? "true" : properties.JoinStrings(" && ", f => csEquals(f, "other"));

                var csEquatableType = isReference && CsVersion.NullableReferenceTypes ? c.Name + "?" : c.Name;

                var csIEquatable =
$@"public bool Equals({csEquatableType} other)
{{
{csEqualsNullCheck}
{csEqualsReferenceCheck}
    return {csEqualsComparison};
}}
";
                c.Interface($"System.IEquatable<{csEquatableType}>");
                c.Method(csIEquatable);
            }
        }

        public static void DefineEquality(this CsClass c, bool isReference, bool isAbstract)
        {
            string csEquality;
            if (isAbstract)
                csEquality =
$@"public static bool operator ==({c.Name} left, {c.Name} right)
{{
    return object.Equals(left, right);
}}
";
            else if (isReference)
                csEquality =
$@"public static bool operator ==({c.Name} left, {c.Name} right)
{{
    if (ReferenceEquals(left, right))
        return true;
    if ({CsVersion.IsNull("left")} || {CsVersion.IsNull("right")})
        return false;
    return left.Equals(right);
}}
";
            else
                csEquality =
$@"public static bool operator ==({c.Name} left, {c.Name} right)
{{
    return left.Equals(right);
}}
";

            var csInequality =
$@"public static bool operator !=({c.Name} left, {c.Name} right)
{{
    return !(left == right);
}}
";

            c.Method(csEquality);
            c.Method(csInequality);
        }
    }
}
