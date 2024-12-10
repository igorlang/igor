using Igor.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Sql.AST
{
    /// <summary>
    /// Maps Igor.Declarations namespace classes to AST namespace classes.
    /// </summary>
    public class AstMapper : MapperBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public AstMapper()
        {
            RegisterAuto<Declarations.Module, Module>();
            RegisterAuto<Declarations.DefineForm, DefineForm>();
            RegisterAuto<Declarations.EnumForm, EnumForm>();
            RegisterAuto<Declarations.EnumField, EnumField>();
            RegisterAuto<Declarations.RecordField, RecordField>();
            RegisterAuto<Declarations.RecordForm, RecordForm>();
            RegisterAuto<Declarations.VariantForm, VariantForm>();
            RegisterAuto<Declarations.UnionClause, UnionClause>();
            RegisterAuto<Declarations.UnionForm, UnionForm>();
            RegisterAuto<Declarations.InterfaceForm, InterfaceForm>();
            RegisterAuto<Declarations.TableForm, TableForm>();
            RegisterAuto<Declarations.TableField, TableField>();
            RegisterAuto<Declarations.ServiceForm, ServiceForm>();
            RegisterAuto<Declarations.ServiceFunction, ServiceFunction>();
            RegisterAuto<Declarations.FunctionArgument, FunctionArgument>();
            RegisterAuto<Declarations.FunctionThrow, FunctionThrow>();

            RegisterAuto<Declarations.WebServiceForm, WebServiceForm>();
            RegisterAuto<Declarations.WebResource, WebResource>();
            RegisterAuto<Declarations.WebStatusCode, WebStatusCode>();
            RegisterAuto<Declarations.WebResponse, WebResponse>();
            RegisterAuto<Declarations.WebHeader, WebHeader>();
            RegisterAuto<Declarations.WebContent, WebContent>();
            RegisterAuto<Declarations.WebPathSegment, WebPathSegment>();
            RegisterAuto<Declarations.WebQueryParameter, WebQueryParameter>();
            RegisterAuto<Declarations.WebVariable, WebVariable>();
            Register<Declarations.WebParameterType, WebParameterType>(p => (WebParameterType)p);

            RegisterAuto<Declarations.GenericTypeVariable, GenericArgument>();
            RegisterAuto<Declarations.GenericInterfaceInstance, GenericInterface>();
            Register<Declarations.SymbolName, string>(name => name.Name);

            Register<Declarations.BuiltInType.Bool, BuiltInType>(new BuiltInType.Bool());
            Register<Declarations.BuiltInType.Integer, BuiltInType>(type => new BuiltInType.Integer(type.Type));
            Register<Declarations.BuiltInType.Float, BuiltInType>(type => new BuiltInType.Float(type.Type));
            Register<Declarations.BuiltInType.String, BuiltInType>(new BuiltInType.String());
            Register<Declarations.BuiltInType.Atom, BuiltInType>(new BuiltInType.Atom());
            Register<Declarations.BuiltInType.Binary, BuiltInType>(new BuiltInType.Binary());
            Register<Declarations.BuiltInType.Json, BuiltInType>(new BuiltInType.Json());
            Register<Declarations.GenericTypeInstance, IType>(MapGenericInstance);

            RegisterAuto<Declarations.Value.Bool, Value.Bool>();
            RegisterAuto<Declarations.Value.Integer, Value.Integer>();
            RegisterAuto<Declarations.Value.Float, Value.Float>();
            RegisterAuto<Declarations.Value.String, Value.String>();
            Register<Declarations.Value.EmptyObject, Value.EmptyObject>(Value.EmptyObject.Empty);
            Register<Declarations.Value.EmptyList, Value.List>(Value.List.Empty);
            Register<Declarations.Value.EmptyDict, Value.Dict>(Value.Dict.Empty);
            RegisterAuto<Declarations.Value.Enum, Value.Enum>();
        }

        /// <summary>
        /// Maps Igor.Declarations namespace classes to AST namespace classes.
        /// </summary>
        /// <param name="modules">Module declarations parse tree</param>
        /// <returns>Modules AST</returns>
        public static IReadOnlyList<Module> Map(IReadOnlyList<Igor.Declarations.Module> modules)
        {
            var mapper = new AstMapper();
            return modules.Select(mapper.Map<Declarations.Module, Module>).ToList();
        }

        private IType MapGenericInstance(Declarations.GenericTypeInstance genericType)
        {
            var args = genericType.Args.Select(Map<Declarations.IType, IType>).ToList();
            switch (genericType.Prototype)
            {
                case Declarations.BuiltInType.List _: return new BuiltInType.List(args[0]);
                case Declarations.BuiltInType.Dict _: return new BuiltInType.Dict(args[0], args[1]);
                case Declarations.BuiltInType.Flags _: return new BuiltInType.Flags(args[0]);
                case Declarations.BuiltInType.Optional _: return new BuiltInType.Optional(args[0]);
                case Declarations.BuiltInType.OneOf _: return new BuiltInType.OneOf(args);
                case Declarations.TypeForm type: return new GenericType() { Prototype = Map<Declarations.TypeForm, TypeForm>(type), Args = args };
                default: throw new InvalidOperationException();
            }
        }
    }
}
