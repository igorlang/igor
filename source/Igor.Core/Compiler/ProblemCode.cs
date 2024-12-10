namespace Igor.Compiler
{
    public enum ProblemCode
    {
        None = 0,
        InternalError = 1,
        // Syntax
        SyntaxError = 100,
        IntegralParseError = 101,
        FloatParseError = 102,
        InvalidIdentifier = 103,
        UnterminatedString = 104,
        UnrecognizedEscapeSequence = 105,
        UnknownHttpMethod = 106,
        UnknownDataFormat = 107,

        // Compile
        UnresolvedReference = 200,
        DuplicateDefinition = 201,

        CircularInterfaceDependency = 210,
        CircularVariantDependency = 211,

        CannotConvertValueToType = 220,
        IntegerRangeViolation = 221,

        InheritedFieldTypeMismatch = 230,
        InterfaceFieldTypeMismatch = 231,

        NonGenericTypeWithTypeArgs = 250,
        GenericArityMismatch = 251,
        GenericArgsRequired = 252,
        EmptyTypeParameterList = 253,

        ThrownTypeMustBeException = 260,

        TagFieldRequired = 290,
        TagFieldNotAllowedInInterfaces = 291,
        TagFieldNotAllowedInRecords = 292,
        TagFieldNotAllowedInVariantDescendents = 293,
        TagTypeMustBeEnum = 294,
        TooManyTagFields = 295,
        RecordTagRequired = 296,
        RecordTagNotAllowedInNonVariantRecords = 297,
        // Attributes
        UnknownAttribute = 300,
        InvalidAttributeValue = 301,
        DeprecatedAttribute = 302,
        // Targets
        UnknownTarget = 400,
        UnknownCommand = 401,
        // Script
        ScriptFileNotFound = 500,
        ScriptCompilationError = 501,
        ScriptCompilationWarning = 502,
        ScriptRuntimeError = 503,

        // Target specific
        TargetSpecificProblem = 1000,

        // Compiler
        SourceFileNotFound = 2000,
        FailedToWriteFile = 2100,
        TargetFileIsReadonly = 2101,
    }
}
