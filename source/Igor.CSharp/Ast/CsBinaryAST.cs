using System.Collections.Generic;
using System.Linq;

namespace Igor.CSharp.AST
{
    public partial class Form
    {
        public string csBinaryNamespace => Attribute(CsAttributes.BinaryNamespace, csNamespace);
    }

    public partial class GenericArgument
    {
        public string csBinarySerializerType => $"Igor.Serialization.IBinarySerializer<{csName}>";
    }

    public partial class TypeForm
    {
        public string csBinaryCustomSerializerInstance => Attribute(CsAttributes.BinarySerializer, null);
        public string csBinaryGeneratedSerializerClass => $"{csName}BinarySerializer";

        public bool csBinaryGenerateSerializer => csEnabled && binaryEnabled && csBinaryCustomSerializerInstance == null;

        public string csBinarySerializerInstance(string ns)
        {
            if (!binaryEnabled)
                throw new EFormatDisabled(Location, Name, "igor");
            else if (csBinaryCustomSerializerInstance != null)
                return CsName.RelativeName(csBinaryCustomSerializerInstance, ns);
            else if (IsGeneric)
                return $"new {CsName.RelativeName(csBinaryNamespace, csBinaryGeneratedSerializerClass, ns)}";
            else
                return $"{CsName.RelativeName(csBinaryNamespace, csBinaryGeneratedSerializerClass, ns)}.Instance";
        }
    }

    public partial class RecordField
    {
        public string csBinarySerializer => Struct.binaryHeader ? csType.nonOptType.binarySerializer(Struct.csBinaryNamespace) : csType.binarySerializer(Struct.csBinaryNamespace);
        public string csBinaryWriteValue => Struct.binaryHeader ? csType.writeValue(csName) : csName;

        public int csBinaryId => Struct.csBinaryBitFields.ToList().IndexOf(this);

        public string csBinaryRequire(string paramName)
        {
            if (csCanBeNull && !IsOptional)
                return
$@"    if (value.{csName} == null)
        throw new System.ArgumentException(""Required property {csName} is null"", {CsVersion.NameOf(paramName)});";
            else
                return null;
        }

        public string csBinaryWrite
        {
            get
            {
                if (IsOptional && csCanBeNull)
                    return
$@"    if (value.{csNotNull}) {csBinarySerializer}.Serialize(writer, value.{csBinaryWriteValue});";
                else
                    return
$@"    {csBinarySerializer}.Serialize(writer, value.{csBinaryWriteValue});";
            }
        }

        public string csBinaryWriteNoHeader => $"    {csBinarySerializer}.Serialize(writer, value.{csBinaryWriteValue});";

        public string csBinaryRead
        {
            get
            {
                if (IsOptional)
                {
                    if (!csCanBeNull)
                        return
$@"    if (bits[{csBinaryId}])
        value.{csName} = {csBinarySerializer}.Deserialize(reader);";
                    else
                        return
$@"    if (bits[{csBinaryId}])
        value.{csName} = {csBinarySerializer}.Deserialize(reader);
    else
        value.{csName} = null;";
                }
                else
                    return
$@"    value.{csName} = {csBinarySerializer}.Deserialize(reader);";
            }
        }

        public string csBinaryReadNoHeader => $"    value.{csName} = {csBinarySerializer}.Deserialize(reader);";
    }

    public partial class StructForm
    {
        public IEnumerable<RecordField> csBinaryBitFields => binarySerializedFields.Where(f => f.IsOptional);
    }
}
