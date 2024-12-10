using System;

namespace Igor
{
    /// <summary>
    /// Base class for exceptions that are bound to a certain Igor source code location.
    /// </summary>
    /// <remarks>
    /// Exceptions immediately stop compilation process, and so should be avoided in favour of compiler messages, 
    /// because it's a better user experience to get a list of errors at once rather than get them one by one.
    /// </remarks>
    /// <seealso cref="Igor.Context.CompilerOutput"/>
    public class CodeException : Exception
    {
        public Location Location { get; }

        /// <summary>
        /// Create an exception that is bound to a certain Igor source code location.
        /// </summary>
        /// <remarks>
        /// Exceptions immediately stop compilation process, and so should be avoided in favour of compiler messages, 
        /// because it's a better user experience to get a list of errors at once rather than get them one by one.
        /// </remarks>
        /// <seealso cref="Igor.Context.CompilerOutput"/>
        public CodeException(Location location, string message)
            : base(message)
        {
            this.Location = location;
        }
    }

    // TODO: make code exceptions from the following
    public class EInternal : Exception
    {
        public EInternal(string message)
            : base($"Internal error: {message}")
        {
        }
    }

    public class EUnknownType : EInternal
    {
        public EUnknownType(string type)
            : base($"Unknown type: '{type}'")
        {
        }
    }

    public class EFormatDisabled : CodeException
    {
        public EFormatDisabled(Location location, string target, string format)
            : base(location, $"Format {format} for {target} is disabled but required")
        {
        }
    }
}
