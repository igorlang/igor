using System.Collections.Generic;

namespace Igor.UE4.Model
{
    /// <summary>
    /// Base class for UE4 C++ declarations that support macro attributes (UPROPERTY, UFUNCTION, UCLASS, USTRUCT or UENUM).
    /// </summary>
    public class UeMacroHost
    {
        /// <summary>
        /// UE4 macro specifiers (for UPROPERTY, UFUNCTION, UCLASS, USTRUCT or UENUM macros).
        /// Set null value for keys that don't require value.
        /// </summary>
        public Dictionary<string, string> Specifiers { get; } = new Dictionary<string, string>();

        /// <summary>
        /// UE4 metadata specifiers. 
        /// Set null value for keys that don't require value.
        /// See <a href="https://docs.unrealengine.com/en-US/Programming/UnrealArchitecture/Reference/Metadata/index.html">UE4 documentation</a> for reference.
        /// </summary>
        public Dictionary<string, string> Meta { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Add UE4 macro specifier (for UPROPERTY, UFUNCTION, UCLASS, USTRUCT or UENUM macros).
        /// Use null value for keys that don't require value.
        /// Don't put values in quotes.
        /// </summary>
        /// <param name="spec">Specifier</param>
        /// <param name="value">Value. Use null (default) if no value is required.</param>
        public void Specifier(string spec, string value = null)
        {
            Specifiers[spec] = value;
        }
    }
}
