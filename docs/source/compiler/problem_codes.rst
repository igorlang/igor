***************************
Problem Codes
***************************

The following is the list of problem codes Igor can return.

When compiling scripts, C# compiler warnings and errors are also reported.

======= =========== =========== ===================================================
Code    Category    Severity    Description
======= =========== =========== ===================================================
IG0000  General     Error       Unknown error       
IG0001  General     Error       Internal error (a bug in Igor Compiler)

IG0100  Syntax      Error       Unrecognized syntax error
IG0101  Syntax      Error       Failed to parse integral value
IG0102  Syntax      Error       Failed to parse float value
IG0103  Syntax      Error       Invalid identifier
IG0104  Syntax      Error       Unterminated string
IG0105  Syntax      Warning     Unrecognized escape sequence
IG0106  Syntax      Error       Unknown HTTP method
IG0107  Syntax      Error       Unknown data format

IGO200  Compile     Error       Unresolved reference
IGO201  Compile     Error       Duplicate definition
IGO210  Compile     Error       Circular interface dependency
IGO211  Compile     Error       Circular variant dependency
IG0220  Compile     Error       Cannot convert value to type
IG0221  Compile     Error       Integer range violation
IG0230  Compile     Error       Inherited field type mismatch
IG0231  Compile     Error       Interface field type mismatch
IG0250  Compile     Error       Type arguments provided for non-generic type
IG0251  Compile     Error       Generic type arity mismatch
IG0252  Compile     Error       Type arguments are required for generic type
IG0253  Compile     Error       Empty generic type parameter list
IG0260  Compile     Error       Thrown type must be exception
IG0290  Compile     Error       Tag field is required for variant types
IG0291  Compile     Error       Tag fields are not allowed in interfaces
IG0292  Compile     Error       Tag fields are not allowed in records
IG0293  Compile     Error       Tag fields are not allowed in variant descendants
IG0294  Compile     Error       Tag fields must be of enum type
IG0295  Compile     Error       Too many tag fields
IG0296  Compile     Error       Tag value should be provided for variant descendant record
IG0297  Compile     Error       Tag values are not allowed in non-variant records

IG0300  Attribute   Warning     Unknown attribute
IG0301  Attribute   Warning     Invalid attribute value
IG0302  Attribute   Warning     Deprecated attribute

IGO400  Target      Error       Unknown target
IGO401  Target      Error       Unknown command

IG0400  Script      Error       Script file not found
IG0500  Script      Error       Script compilation error
IG0501  Script      Warning     Script compilation warning
IG0502  Script      Error       Script runtime warning
IG0503  Script      Error       Script runtime error

IG1000  Target      Error       Target-specific problem

IG2000  Compiler    Error       Source file not found
IG2100  Compiler    Error       Failed to write output file
IG2101  Compiler    Warning     Output file is read-only
======= =========== =========== ===================================================
