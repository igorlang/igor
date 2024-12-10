using Igor.Compiler;
using Igor.Declarations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WebResponse = Igor.Declarations.WebResponse;

namespace Igor.Parser
{
    public class IgorParser : IgorScanner
    {
        private readonly Term attributeTerm = new OneOfTerm(LineAnnotationTerm.Default, BlockAnnotationTerm.Default, Punctuation.LeftBracket);

        private readonly Term formTerm = new OneOfTerm(Keywords.Record, Keywords.Interface, Keywords.Variant, Keywords.Exception,
            Keywords.Table, Keywords.Enum, Keywords.Define, Keywords.Union, Keywords.Service, Keywords.Webservice);

        public IgorParser(string source, string filename, CompilerOutput output) : base(source, filename, output)
        {
            AutoSkipWhitespaceAndComments = true;
        }

        public CompilationUnit ParseCompilationUnit()
        {
            var location = GetCurrentLocation();
            var usings = new List<SymbolReference<Module>>();
            var modules = new List<Module>();
            try
            {
                while (!TestEof())
                {
                    if (Test(Keywords.Using))
                        usings.AddIfNotNull(ParseUsing());
                    else if (Test(Keywords.Module / attributeTerm))
                        modules.AddIfNotNull(ParseModule());
                    else
                    {
                        var expected = modules.Any() ? "module" : "'using' or 'module'";
                        Output.Error(GetCurrentLocation(), $"Expected: {expected}", ProblemCode.SyntaxError);
                        Recover(Keywords.Using / Keywords.Module / attributeTerm, null);
                    }
                }
            }
            catch (ParserNonRecoverableException)
            {
            }
            return new CompilationUnit(location, usings, modules);
        }

        public SymbolReference<Module> ParseUsing()
        {
            try
            {
                Match(Keywords.Using);
                var moduleReference = ParseSymbolReference<Module>();
                Match(Punctuation.Semi);
                return moduleReference;
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.Semi, Keywords.Using / Keywords.Module / attributeTerm);
            }
            return null;
        }

        public Module ParseModule()
        {
            IReadOnlyList<AttributeDefinition> attributes = Array.Empty<AttributeDefinition>();
            Location location = null;
            SymbolName moduleName = null;
            var definitions = new List<Form>();
            try
            {
                attributes = ParseAttributes();
                location = GetLocation(Match(Keywords.Module));
                moduleName = ParseSymbolName();
                Match(Punctuation.LeftBrace);

                while (!TryMatch(Punctuation.RightBrace))
                {
                    IReadOnlyList<AttributeDefinition> formAttributes = Array.Empty<AttributeDefinition>();
                    if (Test(attributeTerm))
                        formAttributes = ParseAttributes();
                    if (TestAny(Keywords.Record, Keywords.Variant, Keywords.Exception))
                        definitions.AddIfNotNull(ParseRecord(formAttributes));
                    else if (Test(Keywords.Interface))
                        definitions.AddIfNotNull(ParseInterface(formAttributes));
                    else if (Test(Keywords.Enum))
                        definitions.AddIfNotNull(ParseEnum(formAttributes));
                    else if (Test(Keywords.Define))
                        definitions.AddIfNotNull(ParseDefine(formAttributes));
                    else if (Test(Keywords.Union))
                        definitions.AddIfNotNull(ParseUnion(formAttributes));
                    else if (Test(Keywords.Table))
                        definitions.AddIfNotNull(ParseTable(formAttributes));
                    else if (Test(Keywords.Service))
                        definitions.AddIfNotNull(ParseService(formAttributes));
                    else if (Test(Keywords.Webservice))
                        definitions.AddIfNotNull(ParseWebservice(formAttributes));
                    else
                    {
                        var expected = formAttributes.Any() ? "Expected: type declaration" : "Expected: '}' or type declaration";
                        Output.Error(GetCurrentLocation(), expected, ProblemCode.SyntaxError);
                        Recover(null, Punctuation.RightBrace / formTerm / attributeTerm);
                    }
                }
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.RightBrace, Keywords.Using / Keywords.Module / attributeTerm);
            }
            if (location != null && moduleName != null)
                return new Module(location, attributes, moduleName, definitions);
            return null;
        }

        public Form ParseUnion(IReadOnlyList<AttributeDefinition> attributes)
        {
            var token = Match(Keywords.Union);
            SymbolName name = null;
            IReadOnlyList<GenericTypeVariable> genericTypeVars = Array.Empty<GenericTypeVariable>();
            var clauses = new List<UnionClause>();
            try
            {
                name = ParseSymbolName();
                genericTypeVars = ParseTypeVariables();
                Match(Punctuation.LeftBrace);
                while (!TryMatch(Punctuation.RightBrace))
                    clauses.AddIfNotNull(ParseUnionClause());
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.RightBrace, null);
            }

            if (name != null)
                return new UnionForm(GetLocation(token), attributes, name, genericTypeVars, clauses);
            return null;
        }

        public UnionClause ParseUnionClause()
        {
            try
            {
                var attributes = ParseAttributes();
                var name = ParseSymbolName();
                ITypeReference type = null;
                if (TryMatch(Punctuation.Arrow))
                    type = ParseTypeReference();
                Match(Punctuation.Semi);
                return new UnionClause(name.Location, attributes, name, type);
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.Semi, Punctuation.RightBrace);
            }

            return null;
        }

        public Form ParseRecord(IReadOnlyList<AttributeDefinition> attributes)
        {
            var token = MatchAny(Keywords.Record, Keywords.Variant, Keywords.Exception);
            bool isException = token.Term == Keywords.Exception;
            bool isVariant = token.Term == Keywords.Variant || isException && TryMatch(Keywords.Variant);

            SymbolName name = null;
            SymbolReference<VariantForm> ancestor = null;
            SymbolReference<EnumField> tagValue = null;
            IReadOnlyList<GenericTypeVariable> genericTypeVars = Array.Empty<GenericTypeVariable>();
            IReadOnlyList<InterfaceReference> interfaces = Array.Empty<InterfaceReference>();
            var fields = new List<RecordFieldDeclaration>();
            try
            {
                var nameToken = Match(IdentifierTerm.MaybeInvalidIdentifier);
                if (TryMatch(Punctuation.Dot))
                {
                    var nameToken2 = Match(IdentifierTerm.MaybeInvalidIdentifier);
                    name = TokenToSymbolName(nameToken2);
                    ancestor = TokenToSymbolReference<VariantForm>(nameToken);
                }
                else
                {
                    name = TokenToSymbolName(nameToken);
                }

                if (!isVariant && TryMatch(Punctuation.LeftBracket))
                {
                    tagValue = ParseSymbolReference<EnumField>();
                    Match(Punctuation.RightBracket);
                }

                genericTypeVars = ParseTypeVariables();
                if (TryMatch(Punctuation.Colon))
                    interfaces = ParseInterfaceList();
                Match(Punctuation.LeftBrace);
                while (!TryMatch(Punctuation.RightBrace))
                    fields.AddIfNotNull(ParseRecordField());
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.RightBrace, null);
            }

            if (name != null)
            {
                if (isVariant)
                    return new VariantForm(GetLocation(token), attributes, name, genericTypeVars, fields, ancestor, interfaces, isException);
                else
                    return new RecordForm(GetLocation(token), attributes, name, genericTypeVars, fields, ancestor, interfaces, isException, tagValue);
            }
            else
                return null;
        }

        public Form ParseInterface(IReadOnlyList<AttributeDefinition> attributes)
        {
            var interfaceToken = Match(Keywords.Interface);
            SymbolName interfaceName = null;
            IReadOnlyList<GenericTypeVariable> genericTypeVars = Array.Empty<GenericTypeVariable>();
            IReadOnlyList<InterfaceReference> interfaces = Array.Empty<InterfaceReference>();
            var fields = new List<RecordFieldDeclaration>();
            try
            {
                interfaceName = ParseSymbolName();
                genericTypeVars = ParseTypeVariables();
                if (TryMatch(Punctuation.Colon))
                    interfaces = ParseInterfaceList();
                Match(Punctuation.LeftBrace);
                while (!TryMatch(Punctuation.RightBrace))
                    fields.AddIfNotNull(ParseRecordField());
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.RightBrace, null);
            }

            if (interfaceName != null)
                return new InterfaceForm(GetLocation(interfaceToken), attributes, interfaceName, genericTypeVars, fields, interfaces);
            else
                return null;
        }

        public RecordFieldDeclaration ParseRecordField()
        {
            try
            {
                var attributes = ParseAttributes();
                var isTag = TryMatch(Keywords.Tag);
                var typeReference = ParseTypeReference();
                var fieldName = ParseSymbolName();
                ValueReference defaultValue = null;
                if (TryMatch(Punctuation.Assign))
                    defaultValue = ParseValueReference();
                Match(Punctuation.Semi, "';'");
                return new RecordFieldDeclaration(fieldName.Location, attributes, fieldName, typeReference, defaultValue, isTag);
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.Semi, Punctuation.RightBrace);
                return null;
            }
        }

        public ITypeReference ParseTypeReference()
        {
            var isOptional = TryMatch(Punctuation.Question);

            ITypeReference ParseSingleTypeReference(bool optional)
            {
                var reference = ParseSymbolReference<IType>();
                var args = Test(Punctuation.Less) ? ParseGenericArgs() : null;
                return new TypeReference(reference.Location, reference, args, optional);
            }

            ITypeReference ParseTypeReferenceNonOpt(bool optional)
            {
                var typeRef = ParseSingleTypeReference(false);
                if (Test(Punctuation.Or))
                {
                    var types = new List<ITypeReference> { typeRef };
                    while (TryMatch(Punctuation.Or))
                    {
                        types.Add(ParseSingleTypeReference(false));
                    }
                    return new OneOfTypeReference(typeRef.Location, types, optional);
                }
                else
                    return typeRef;
            }

            if (isOptional && TryMatch(Punctuation.LeftParen))
            {
                var reference = ParseTypeReferenceNonOpt(true);
                Match(Punctuation.RightParen, ")");
                return reference;
            }
            else if (isOptional)
            {
                return ParseSingleTypeReference(true);
            }
            else
            {
                return ParseTypeReferenceNonOpt(false);
            }
        }

        public InterfaceReference ParseInterfaceReference()
        {
            var reference = ParseSymbolReference<InterfaceForm>();
            var args = Test(Punctuation.Less) ? ParseGenericArgs() : null;
            return new InterfaceReference(reference.Location, reference, args);
        }

        public ValueReference ParseValueReference()
        {
            if (TryMatch(Keywords.True, out var trueToken))
                return new ValueReference.Bool(GetLocation(trueToken), true);
            if (TryMatch(Keywords.False, out var falseToken))
                return new ValueReference.Bool(GetLocation(falseToken), false);
            if (TryMatch(StringLiteralTerm.Default, out var stringToken))
                return new ValueReference.String(GetLocation(stringToken), stringToken.Text);
            if (TryMatch(NumberTerm.Default, out var numberToken))
            {
                var location = GetLocation(numberToken);
                if (numberToken.Text.Contains("."))
                {
                    if (double.TryParse(numberToken.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                        return new ValueReference.Float(location, value);
                    else
                    {
                        Output.Error(location, $"Invalid float constant {numberToken.Text}", ProblemCode.FloatParseError);
                        return null;
                    }
                }
                else
                {
                    if (NumberTerm.TryParseIntegral(numberToken, out var value, out var numberBase))
                        return new ValueReference.Integer(location, value, numberBase);
                    else
                    {
                        Output.Error(location, $"Invalid integral constant {numberToken.Text}", ProblemCode.IntegralParseError);
                        return null;
                    }
                }
            }

            if (TryMatch(IdentifierTerm.MaybeInvalidIdentifier, out var identifier))
            {
                var location = GetLocation(identifier);
                return new ValueReference.Enum(location, new SymbolReference<EnumField>(location, identifier.Text, identifier.IsValid));
            }

            if (TryMatch(Punctuation.LeftBracket, out var emptyListToken))
            {
                Match(Punctuation.RightBracket);
                return new ValueReference.EmptyList(GetLocation(emptyListToken));
            }

            if (TryMatch(Punctuation.LeftBrace, out var emptyObjectToken))
            {
                Match(Punctuation.RightBrace);
                return new ValueReference.EmptyObject(GetLocation(emptyObjectToken));
            }

            throw new ParserException(GetCurrentLocation(), "Expected: value", ProblemCode.SyntaxError);
        }

        public IReadOnlyList<GenericTypeVariable> ParseTypeVariables()
        {
            if (TryMatch(Punctuation.Less, out var openToken))
            {
                if (TryMatch(Punctuation.Greater))
                {
                    Output.Error(GetLocation(openToken), "Empty type parameter list", ProblemCode.EmptyTypeParameterList);
                    return Array.Empty<GenericTypeVariable>();
                }

                var result = new List<GenericTypeVariable>();
                try
                {
                    while (true)
                    {
                        var typeVar = ParseSymbolName();
                        result.Add(new GenericTypeVariable(typeVar.Location, typeVar));
                        if (MatchAny(Punctuation.Comma, Punctuation.Greater).Term == Punctuation.Greater)
                            break;
                    }
                }
                catch (ParserException exception)
                {
                    ReportException(exception);
                    Recover(Punctuation.Greater, Punctuation.LeftBrace / Punctuation.Colon / Punctuation.RightBrace);
                }

                return result;
            }
            else
                return Array.Empty<GenericTypeVariable>();
        }

        public IReadOnlyList<ITypeReference> ParseGenericArgs()
        {
            var openToken = Match(Punctuation.Less);
            if (TryMatch(Punctuation.Greater))
            {
                Output.Error(GetLocation(openToken), "Empty type parameter list", ProblemCode.EmptyTypeParameterList);
                return Array.Empty<ITypeReference>();
            }

            var result = new List<ITypeReference>();
            while (true)
            {
                var typeReference = ParseTypeReference();
                result.AddIfNotNull(typeReference);
                if (MatchAny(Punctuation.Comma, Punctuation.Greater).Term == Punctuation.Greater)
                    break;
            }
            return result;
        }

        public IReadOnlyList<InterfaceReference> ParseInterfaceList()
        {
            var result = new List<InterfaceReference>();
            do
            {
                result.AddIfNotNull(ParseInterfaceReference());
            } while (TryMatch(Punctuation.Comma));

            return result;
        }

        public Form ParseEnum(IReadOnlyList<AttributeDefinition> attributes)
        {
            var enumToken = Match(Keywords.Enum);
            var location = GetLocation(enumToken);
            List<EnumField> fields = new List<EnumField>();
            SymbolName name = null;
            try
            {
                name = ParseSymbolName();
                Match(Punctuation.LeftBrace);
                while (!TryMatch(Punctuation.RightBrace))
                    fields.AddIfNotNull(ParseEnumField());
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.RightBrace, null);
            }

            if (name != null && location != null)
                return new EnumForm(location, attributes, name, fields);
            else
                return null;
        }

        public EnumField ParseEnumField()
        {
            try
            {
                var attributes = ParseAttributes();
                var enumName = ParseSymbolName();
                long? enumValue = null;
                if (TryMatch(Punctuation.Assign))
                {
                    var numberToken = Match(NumberTerm.Default, "integer");
                    if (NumberTerm.TryParseIntegral(numberToken, out var value, out var numberBase))
                        enumValue = value;
                    else
                        Output.Error(GetLocation(numberToken), $"Invalid integral constant {numberToken.Text}", ProblemCode.IntegralParseError);
                }

                Match(Punctuation.Semi, "';'");
                return new EnumField(enumName.Location, attributes, enumName, enumValue);
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.Semi, Punctuation.RightBrace);
                return null;
            }
        }

        public Form ParseDefine(IReadOnlyList<AttributeDefinition> attributes)
        {
            DefineForm result = null;
            try
            {
                var defineToken = Match(Keywords.Define);
                var name = ParseSymbolName();
                var typeVars = ParseTypeVariables();
                var reference = ParseTypeReference();
                result = new DefineForm(GetLocation(defineToken), attributes, name, typeVars, reference);
                Match(Punctuation.Semi);
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.Semi, Punctuation.RightBrace / formTerm);
            }

            return result;
        }

        public Form ParseTable(IReadOnlyList<AttributeDefinition> attributes)
        {
            var tableToken = Match(Keywords.Table);
            SymbolName name = null;
            var fields = new List<TableField>();
            try
            {
                name = ParseSymbolName();
                Match(Punctuation.LeftBrace);
                while (!TryMatch(Punctuation.RightBrace))
                    fields.AddIfNotNull(ParseTableField());
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.RightBrace, null);
            }

            if (name != null)
                return new TableForm(GetLocation(tableToken), attributes, name, fields);
            else
                return null;
        }

        public TableField ParseTableField()
        {
            try
            {
                var attributes = ParseAttributes();
                var type = ParseTypeReference();
                var name = ParseSymbolName();
                ValueReference value = null;
                if (TryMatch(Punctuation.Assign))
                    value = ParseValueReference();
                TableFieldReference foreignKey = null;
                if (TryMatch(Punctuation.To))
                {
                    var foreignKeyTable = ParseSymbolReference<TableForm>();
                    Match(Punctuation.Dot);
                    var foreignKeyField = ParseSymbolReference<TableField>();
                    foreignKey = new TableFieldReference(foreignKeyTable.Location, foreignKeyTable, foreignKeyField);
                }

                Match(Punctuation.Semi);
                return new TableField(type.Location, attributes, name, type, value, foreignKey);
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.Semi, Punctuation.RightBrace);
                throw;
            }
        }

        public Form ParseService(IReadOnlyList<AttributeDefinition> attributes)
        {
            var serviceToken = Match(Keywords.Service);
            SymbolName name = null;
            var functions = new List<ServiceFunction>();
            try
            {
                name = ParseSymbolName();
                Match(Punctuation.LeftBrace);
                while (!TryMatch(Punctuation.RightBrace))
                {
                    functions.AddIfNotNull(ParseServiceFunction());
                }
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.RightBrace, null);
            }

            if (name != null)
                return new ServiceForm(GetLocation(serviceToken), attributes, name, functions);
            return null;
        }

        public ServiceFunction ParseServiceFunction()
        {
            try
            {
                var attributes = ParseAttributes();
                var directionToken = MatchAny(Keywords.CToS, Keywords.SToC);
                var direction = directionToken.Term == Keywords.CToS ? Direction.ClientToServer : Direction.ServerToClient;
                var name = ParseSymbolName();
                var arguments = ParseFunctionArguments();
                bool rpc = false;
                IReadOnlyList<FunctionArgument> returnArguments = Array.Empty<FunctionArgument>();
                if (TryMatch(Keywords.Returns))
                {
                    rpc = true;
                    returnArguments = ParseFunctionArguments();
                }
                List<FunctionThrow> throws = new List<FunctionThrow>();
                if (TryMatch(Keywords.Throws))
                {
                    rpc = true;
                    do
                    {
                        var throwReference = ParseTypeReference();
                        throws.Add(new FunctionThrow(throwReference.Location, throwReference));
                    } while (TryMatch(Punctuation.Comma));
                }
                Match(Punctuation.Semi);
                return new ServiceFunction(GetLocation(directionToken), attributes, name, direction, arguments, returnArguments, throws, rpc);
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.Semi, Punctuation.RightBrace);
            }
            return null;
        }

        private IReadOnlyList<FunctionArgument> ParseFunctionArguments()
        {
            Match(Punctuation.LeftParen);
            if (TryMatch(Punctuation.RightParen))
                return Array.Empty<FunctionArgument>();
            var result = new List<FunctionArgument>();
            do
            {
                var attributes = ParseAttributes();
                var reference = ParseTypeReference();
                var name = ParseSymbolName();
                var argument = new FunctionArgument(name.Location, attributes, name, reference);
                result.Add(argument);
            } while (TryMatch(Punctuation.Comma));

            Match(Punctuation.RightParen);
            return result;
        }

        public Form ParseWebservice(IReadOnlyList<AttributeDefinition> attributes)
        {
            var webserviceToken = Match(Keywords.Webservice);
            SymbolName name = null;
            var resources = new List<WebResource>();
            try
            {
                name = ParseSymbolName();
                Match(Punctuation.LeftBrace);
                while (!TryMatch(Punctuation.RightBrace))
                {
                    resources.AddIfNotNull(ParseWebResource());
                }
            }
            catch (ParserException exception)
            {
                ReportException(exception);
                Recover(Punctuation.RightBrace, null);
            }

            if (name != null)
                return new WebServiceForm(GetLocation(webserviceToken), attributes, name, resources);
            return null;
        }

        public WebResource ParseWebResource()
        {
            DataFormat ParseDataFormat()
            {
                DataFormat format = DataFormat.Default;
                if (TryMatch(Keywords.As))
                {
                    var formatToken = Match(IdentifierTerm.MaybeInvalidIdentifier, "'json' or 'text' or 'binary' or 'xml' or 'form'");
                    if (!WebUtils.TryParseDataFormat(formatToken.Text, out format))
                        Output.Error(GetLocation(formatToken), $"Invalid data format {formatToken.Text}", ProblemCode.UnknownDataFormat);
                }

                return format;
            }

            IReadOnlyList<WebHeader> ParseHeaders()
            {
                List<WebHeader> result = null;
                while (TryMatch(UriPart.WebHeaderName, out var headerName))
                {
                    result = result ?? new List<WebHeader>();
                    Match(Punctuation.Colon);
                    if (Test(Punctuation.LeftBrace))
                    {
                        result.Add(new WebHeader(headerName.Text, ParseWebVariable(WebParameterType.Header)));
                    }
                    else
                    {
                        var value = Match(StringLiteralTerm.Default, "'{' or quoted string");
                        result.Add(new WebHeader(headerName.Text, value.Text));
                    }
                }

                if (result != null)
                    return result;
                return Array.Empty<WebHeader>();
            }

            WebVariable ParseWebVariable(WebParameterType parameterType)
            {
                var oldAutoSkipWhitespaceAndComments = AutoSkipWhitespaceAndComments;
                AutoSkipWhitespaceAndComments = true;
                var lbToken = Match(Punctuation.LeftBrace);
                try
                {
                    var attributes = ParseAttributes();
                    var type = ParseTypeReference();
                    var name = ParseSymbolName();
                    var value = TryMatch(Punctuation.Assign) ? ParseValueReference() : null;
                    var dataFormat = ParseDataFormat();
                    Match(Punctuation.RightBrace);
                    return new WebVariable(GetLocation(lbToken), attributes, name, type, value, dataFormat, parameterType);
                }
                catch (ParserException exception)
                {
                    ReportException(exception);
                    Recover(Punctuation.RightBrace, Punctuation.Semi);
                    return null;
                }
                finally
                {
                    AutoSkipWhitespaceAndComments = oldAutoSkipWhitespaceAndComments;
                }
            }

            WebContent ParseContent()
            {
                if (Test(Punctuation.LeftBrace))
                {
                    var contentVar = ParseWebVariable(WebParameterType.Content);
                    return new WebContent(contentVar.Location, contentVar.Format, contentVar);
                }
                else
                {
                    var type = ParseTypeReference();
                    var format = ParseDataFormat();
                    return new WebContent(type.Location, format, type);
                }
            }

            try
            {
                var attributes = ParseAttributes();
                var name = ParseSymbolName();
                Match(Punctuation.Arrow);
                var methodToken = Match(IdentifierTerm.MaybeInvalidIdentifier);
                if (!WebUtils.TryParseHttpMethod(methodToken.Text, out var method))
                    Output.Error(GetLocation(methodToken), $"Invalid HTTP method {methodToken.Text}", ProblemCode.UnknownHttpMethod);
                AutoSkipWhitespaceAndComments = false;
                SkipWhitespaceAndComments();
                var path = new List<WebPathSegment>();
                do
                {
                    var pathPart = Match(UriPart.PathSegment);
                    if (pathPart.Text == "" && Test(Punctuation.LeftBrace))
                    {
                        path.Add(new WebPathSegment(ParseWebVariable(WebParameterType.Path)));
                    }
                    else
                    {
                        path.Add(new WebPathSegment(pathPart.Text));
                    }
                    SkipWhitespaceAndComments();
                } while (TestChar('/'));

                var query = new List<WebQueryParameter>();
                if (TestChar('?'))
                {
                    bool isFirst = true;
                    do
                    {
                        var queryParameterName = Match(isFirst ? UriPart.PathQuery : UriPart.PathQueryPart);
                        if (queryParameterName.Text == "")
                            Output.Error(GetCurrentLocation(), "Missing query parameter name", ProblemCode.SyntaxError);
                        isFirst = false;
                        var queryParameterValue = Match(UriPart.PathQueryValue);
                        if (queryParameterValue.Text == "" && Test(Punctuation.LeftBrace))
                        {
                            query.Add(new WebQueryParameter(queryParameterName.Text, ParseWebVariable(WebParameterType.Query)));
                        }
                        else
                        {
                            if (queryParameterValue.Text == "")
                                Output.Error(GetCurrentLocation(), "Missing query parameter value", ProblemCode.SyntaxError);
                            query.Add(new WebQueryParameter(queryParameterName.Text, queryParameterValue.Text));
                        }
                        SkipWhitespaceAndComments();
                    } while (TestChar('&'));
                }

                AutoSkipWhitespaceAndComments = true;
                var requestHeaders = ParseHeaders();

                WebContent requestContent = null;
                if (!TryMatch(Punctuation.To))
                {
                    requestContent = ParseContent();
                    Match(Punctuation.To);
                }

                var responses = new List<WebResponse>();

                do
                {
                    WebStatusCode status = null;
                    SkipWhitespaceAndComments();
                    var location = GetCurrentLocation();

                    var responseAttributes = ParseAttributes();

                    if (TryMatch(NumberTerm.Default, out var nextToken))
                    {
                        if (!int.TryParse(nextToken.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var statusCode))
                            Output.Error(GetLocation(nextToken), $"Invalid HTTP status code '{nextToken.Text}'", ProblemCode.SyntaxError);
                        nextToken = MatchAny(ReasonPhraseTerm.Default, Punctuation.Colon, Punctuation.Semi, Punctuation.Comma);
                        string reasonPhrase = null;
                        if (nextToken.Term == ReasonPhraseTerm.Default)
                        {
                            reasonPhrase = nextToken.Text;
                            nextToken = MatchAny(Punctuation.Colon, Punctuation.Semi, Punctuation.Comma);
                        }

                        status = new WebStatusCode(GetLocation(nextToken), statusCode, reasonPhrase);
                        if (nextToken.Term == Punctuation.Semi || nextToken.Term == Punctuation.Comma)
                        {
                            CurrentPosition = nextToken.Position;
                            responses.Add(new WebResponse(location,  responseAttributes,Array.Empty<WebHeader>(), null, status));
                            continue;
                        }
                    }

                    var headers = ParseHeaders();
                    SkipWhitespaceAndComments();
                    WebContent content = null;
                    if (!TestChar(',') && !TestChar(';'))
                        content = ParseContent();

                    var response = new WebResponse(location, responseAttributes, headers, content, status);
                    responses.Add(response);
                } while (TryMatch(Punctuation.Comma));

                Match(Punctuation.Semi, "',' or ';'");

                return new WebResource(name.Location, attributes, name, method, path, query, requestHeaders, requestContent, responses);
            }
            catch (ParserException exception)
            {
                AutoSkipWhitespaceAndComments = true;
                ReportException(exception);
                Recover(Punctuation.Semi, null);
            }
            finally
            {
                AutoSkipWhitespaceAndComments = true;
            }

            return null;
        }

        public IReadOnlyList<AttributeDefinition> ParseAttributes()
        {
            var result = new List<AttributeDefinition>();
            while (true)
            {
                if (TryMatch(LineAnnotationTerm.Default, out var annotation) || TryMatch(BlockAnnotationTerm.Default, out annotation))
                {
                    var location = GetLocation(annotation);
                    result.Add(new AttributeDefinition(location, "*", "annotation", new AttributeValue.String(location, annotation.Text)));
                }
                else if (TryMatch(Punctuation.LeftBracket))
                {
                    try
                    {
                        var target = MatchAny(Punctuation.Asterisk, AttributeTargetTerm.Default).Text;
                        while (TryMatch(AttributeNameTerm.Default, out var name))
                        {
                            var location = GetLocation(name);
                            AttributeValue value = TryMatch(Punctuation.Assign) ? ParseAttributeValue() : new AttributeValue.Bool(location, true);
                            result.Add(new AttributeDefinition(location, target, name.Text, value));
                        }

                        Match(Punctuation.RightBracket, "attribute or ']'");
                    }
                    catch (ParserException exception)
                    {
                        ReportException(exception);
                        Recover(Punctuation.RightBracket / Punctuation.Semi / Punctuation.RightBrace, attributeTerm);
                    }
                }
                else
                    break;
            }

            return result;
        }

        public AttributeValue ParseAttributeValue()
        {
            if (TryMatch(Keywords.True, out var trueToken))
                return new AttributeValue.Bool(GetLocation(trueToken), true);
            if (TryMatch(Keywords.False, out var falseToken))
                return new AttributeValue.Bool(GetLocation(falseToken), false);
            if (TryMatch(StringLiteralTerm.Default, out var stringToken))
                return new AttributeValue.String(GetLocation(stringToken), stringToken.Text);
            if (TryMatch(NumberTerm.Default, out var numberToken))
            {
                var location = GetLocation(numberToken);
                if (numberToken.Text.Contains("."))
                {
                    if (double.TryParse(numberToken.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                        return new AttributeValue.Float(location, value);
                    else
                    {
                        Output.Error(location, $"Invalid float constant {numberToken.Text}", ProblemCode.FloatParseError);
                        return new AttributeValue.Float(location, 0f);
                    }
                }
                else
                {
                    if (NumberTerm.TryParseIntegral(numberToken, out var value, out var numberBase))
                        return new AttributeValue.Integer(location, value);
                    else
                    {
                        Output.Error(location, $"Invalid integral constant {numberToken.Text}", ProblemCode.IntegralParseError);
                        return new AttributeValue.Integer(location, 0);
                    }
                }
            }
            if (TryMatch(IdentifierTerm.MaybeInvalidIdentifier, out var identifier))
                return new AttributeValue.Enum(GetLocation(identifier), identifier.Text);
            if (TryMatch(Punctuation.LeftParen, out var objectToken))
            {
                var properties = new List<AttributeObjectProperty>();
                try
                {
                    while (TryMatch(AttributeNameTerm.Default, out var name))
                    {
                        var location = GetLocation(name);
                        AttributeValue value = TryMatch(Punctuation.Assign) ? ParseAttributeValue() : new AttributeValue.Bool(location, true);
                        properties.Add(new AttributeObjectProperty(location, name.Text, value));
                    }

                    Match(Punctuation.RightParen, "attribute or ')'");
                }
                catch (ParserException exception)
                {
                    ReportException(exception);
                    Recover(Punctuation.RightParen, Punctuation.RightBracket);
                }
                return new AttributeValue.Object(GetLocation(objectToken), properties);
            }
            throw new ParserException(GetCurrentLocation(), "Expected: attribute value", ProblemCode.SyntaxError);
        }

        public SymbolName ParseSymbolName()
        {
            return TokenToSymbolName(Match(IdentifierTerm.MaybeInvalidIdentifier));
        }

        private SymbolName TokenToSymbolName(Token token)
        {
            return new SymbolName(GetLocation(token), token.Text);
        }

        public SymbolReference<T> ParseSymbolReference<T>() where T : ILocated
        {
            return TokenToSymbolReference<T>(Match(IdentifierTerm.MaybeInvalidIdentifier));
        }

        private SymbolReference<T> TokenToSymbolReference<T>(Token token) where T : ILocated
        {
            return new SymbolReference<T>(GetLocation(token), token.Text, token.IsValid);
        }

        protected override bool SkipToken()
        {
            if (IsEof())
                return false;
            var token = TryMatchAny(IdentifierTerm.SkipInvalidIdentifier, NumberTerm.Default, BlockAnnotationTerm.Default, LineAnnotationTerm.Default, StringLiteralTerm.Default);
            if (!token.IsNone)
                return true;
            return base.SkipToken();
        }
    }
}
