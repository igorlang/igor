using Igor.Elixir.AST;
using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Elixir
{
    public abstract class StructType
    {
        public bool IsMap => this is Map;
        public bool IsStruct => this is Struct;
        public bool IsTuple => this is Tuple;
        public bool IsRecord => this is Record;
        public string StructName => ((Struct)this).Name;
        public string RecordName => ((Record)this).Name;
        public abstract string Empty { get; }

        public class Map : StructType
        {
            public static readonly StructType Instance = new Map();

            public override string Empty => "%{}";
        }

        public class Struct : StructType
        {
            public string Name { get; }

            public Struct(string name) => Name = name;

            public override string Empty => $"%{Name}{{}}";
        }

        public class Tuple : StructType
        {
            public static readonly StructType Instance = new Tuple();

            public override string Empty => "{}";
        }

        public class Record : StructType
        {
            public string Name { get; }

            public Record(string name) => Name = name;

            public override string Empty => $"%{Name}{{}}";
        }
    }

    public static class CodeUtils
    {
        private static void Require(Renderer r, IEnumerable<RecordField> fields)
        {
            // TODO: asserts
            // r.Blocks(fields.Select(f => $"?assert({f.exVarName} =/= undefined),"));
        }

        public static void DeconstructStruct(Renderer r, StructType structType, IEnumerable<RecordField> fields, string value)
        {
            var requiredFields = fields.Where(f => !f.IsOptional);
            if (structType.IsTuple)
            {
                SetupStructParams(r, structType, fields, false, 100);
                r *= $" = {value}\n";
                Require(r, requiredFields);
            }
            else if (structType.IsMap)
            {
                if (requiredFields.Any())
                {
                    SetupStructParams(r, structType, requiredFields, false, 3);
                    r *= $" = {value}\n";
                }
                r.Blocks(fields.Where(f => f.IsOptional).Select(f => $"{f.exVarName} = Map.get({f.exName}, {value}),"));
            }
            else
            {
                SetupStructParams(r, structType, fields, false, 3);
                r *= $" = {value}\n";
                Require(r, requiredFields);
            }
        }

        public static void Group(Renderer r, string prefix, IEnumerable<string> items, string postfix, int maxSingleLine)
        {
            var c = items.Count();
            if (c == 0)
            {
                r *= prefix + postfix;
            }
            else if (c <= maxSingleLine)
            {
                r *= prefix + items.JoinStrings(", ") + postfix;
            }
            else
            {
                r *= prefix;
                r++;
                r.Blocks(items, delimiter: ",");
                r--;
                r *= postfix;
            }
        }

        private static void ConstructStruct(Renderer r, StructType structType, string prefix, IEnumerable<RecordField> fields, string postfix, int maxSingleLine)
        {
            if (structType.IsMap && fields.Any(f => f.IsOptional))
            {
                ConstructStructVar(r, structType, fields.Where(f => !f.IsOptional), "Result0", maxSingleLine);
                var optFields = fields.Where(f => f.IsOptional).ToList();
                int i = 0;
                foreach (var optField in optFields)
                {
                    if (i == optFields.Count - 1)
                    {
                        r += $"{prefix}if {optField.exVarName} =/= undefined -> maps:put({optField.exName}, {optField.exVarName}, Result{i}); true -> Result{i} end{postfix}";
                    }
                    else
                    {
                        r += $"Result{i + 1} = if {optField.exVarName} =/= undefined -> maps:put({optField.exName}, {optField.exVarName}, Result{i}); true -> Result{i} end,";
                    }
                    i++;
                }
            }
            else
            {
                r *= prefix;
                SetupStructParams(r, structType, fields, true, maxSingleLine);
                r *= postfix;
            }
        }

        public static void ReturnStruct(Renderer r, StructType structForm, IEnumerable<RecordField> fields, int maxSingleLine)
        {
            ConstructStruct(r, structForm, null, fields, "", maxSingleLine);
        }

        public static void ConstructStructVar(Renderer r, StructType structForm, IEnumerable<RecordField> fields, string varName, int maxSingleLine)
        {
            ConstructStruct(r, structForm, $"{varName} = ", fields, ",", maxSingleLine);
        }

        private static void SetupStructParams(Renderer r, StructType structType, IEnumerable<RecordField> fields, bool pack, int maxSingleLine)
        {
            if (structType.IsTuple)
            {
                Group(r, "{", fields.Select(field => field.exVarName), "}", maxSingleLine);
            }
            else if (structType.IsMap)
            {
                Group(r, "%{", fields.Select(f => $"{f.exName} => {f.exVarName}"), "}", maxSingleLine);
            }
            else if (structType.IsStruct)
            {
                if (pack)
                    Group(r, $"%{structType.StructName}{{", fields.Select(f => $"{f.exName}: {f.exVarName}"), "}", maxSingleLine);
                else
                    Group(r, $"%{{", fields.Select(f => $"{f.exName}: {f.exVarName}"), "}", maxSingleLine);
            }
            else
            {
                Group(r, $"{structType.RecordName}(", fields.Select(f => $"{f.exName}: {f.exVarName}"), ")", maxSingleLine);
            }
        }
    }
}
