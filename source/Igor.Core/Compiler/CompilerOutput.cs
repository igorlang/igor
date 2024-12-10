namespace Igor.Compiler
{
    /// <summary>
    /// Problem type
    /// </summary>
    public enum CompilerMessageType
    {
        /// <summary>
        /// Problem is an error
        /// </summary>
        Error = 0,

        /// <summary>
        /// Problem is a warning
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Problem is a hint
        /// </summary>
        Hint = 2
    }

    /// <summary>
    /// Base class for Igor compiler output log implementations.
    /// CompilerOutput is responsible for reporting problems and simple text messages.
    /// </summary>
    public abstract class CompilerOutput
    {
        /// <summary>
        /// Whether errors have been reported
        /// </summary>
        public bool HasErrors { get; private set; }

        /// <summary>
        /// Log simple text message
        /// </summary>
        /// <param name="text">Message text</param>
        public abstract void Log(string text);

        /// <summary>
        /// Implementation of logging compiler messages
        /// </summary>
        /// <param name="type">Problem type</param>
        /// <param name="location">Problem location</param>
        /// <param name="text">Problem description</param>
        /// <param name="code">Problem code identifier</param>
        protected abstract void WriteMessage(CompilerMessageType type, Location location, string text, string code);

        /// <summary>
        /// Report a problem
        /// </summary>
        /// <param name="type">Problem type</param>
        /// <param name="location">Problem location (use Location.NoLocation if location is unknown or not applicable)</param>
        /// <param name="text">Problem description</param>
        /// <param name="code">Problem code identifier</param>
        public void ReportMessage(CompilerMessageType type, Location location, string text, string code)
        {
            WriteMessage(type, location, text, code);
            if (type == CompilerMessageType.Error)
                HasErrors = true;
        }

        /// <summary>
        /// Report a problem
        /// </summary>
        /// <param name="type">Problem type</param>
        /// <param name="location">Location (use CompilerOutput.NoLocation if location is unknown or not applicable)</param>
        /// <param name="text">Problem description</param>
        /// <param name="code">Problem code identifier</param>
        public void ReportMessage(CompilerMessageType type, Location location, string text, ProblemCode code)
        {
            ReportMessage(type, location, text, $"IG{(int)code:0000}");
        }

        /// <summary>
        /// Report error
        /// </summary>
        /// <param name="location">Location (use CompilerOutput.NoLocation if location is unknown or not applicable)</param>
        /// <param name="text">Problem description</param>
        /// <param name="code">Problem code identifier</param>
        public void Error(Location location, string text, ProblemCode code) => ReportMessage(CompilerMessageType.Error, location, text, code);

        /// <summary>
        /// Report warning
        /// </summary>
        /// <param name="location">Location (use CompilerOutput.NoLocation if location is unknown or not applicable)</param>
        /// <param name="text">Problem description</param>
        /// <param name="code">Problem code identifier</param>
        public void Warning(Location location, string text, ProblemCode code) => ReportMessage(CompilerMessageType.Warning, location, text, code);

        /// <summary>
        /// Report hint
        /// </summary>
        /// <param name="location">Location (use CompilerOutput.NoLocation if location is unknown or not applicable)</param>
        /// <param name="text">Problem description</param>
        /// <param name="code">Problem code identifier</param>
        public void Hint(Location location, string text, ProblemCode code) => ReportMessage(CompilerMessageType.Hint, location, text, code);
    }
}
