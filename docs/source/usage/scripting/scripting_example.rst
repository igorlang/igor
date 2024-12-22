*************************
    Example Script
*************************

This chapter describes a simple example of an extension script. It is a TypeScript generator extension, that provides ``getDescription`` function for
enums, which returns an enum value text annotation. This function can be handy for UI.

Igor Example
============

.. code-block:: igor

    module ScriptSample
    {
        // the following attribute instructs extension script to generate Numbers.getDescription function
        [* enum_descriptions]
        enum Numbers
        {
            # First
            one = 1;
            # Second
            two = 2;
            # Third
            three = 3;
        }
    }

What we want to achieve:

.. code-block:: igor

    import * as Igor from './igor';

    export enum Numbers {
        /** First */
        One = 1,
        /** Second */
        Two = 2,
        /** Third */
        Three = 3,
    }

    export namespace Numbers {
        export function getDescription(value: Numbers): string {
            switch (value) {
                case Numbers.One: return 'First';
                case Numbers.Two: return 'Second';
                case Numbers.Three: return 'Third';
                default: return '';
            }
        }
    }

Extension Script
================

We will use C# 4.0. Here is the script that generates the ``getDescription`` function above.

.. code-block:: C#

    using Igor.Text;
    using Igor.TypeScript.AST;
    using Igor.TypeScript.Model;

    // Use Igor.TypeScript namespace, cause we're creating a TypeScript generator extension
    namespace Igor.TypeScript
    {
        // Let Igor know that this class defines custom attributes
        [CustomAttributes]
        public class EnumAnnotations : ITsGenerator  // Implement ITsGenerator - the base interface for TypeScript generators
        {
            // Define a custom bool attribute "enum_descriptions" that can be used for enums and modules 
            // (scope inheritance means that if attribute is provided for a module, all enums in this module inherit it)
            public static readonly BoolAttributeDescriptor EnumDescriptionsAttribute =
                new BoolAttributeDescriptor("enum_descriptions", IgorAttributeTargets.Enum, AttributeInheritance.Scope);

            // This function is called for each AST module
            // The first parameter is the TypeScript target module
            // The second parameter is the Igor module AST
            public void Generate(TsModel model, Module mod)
            {
                foreach (var e in mod.Enums)
                {
                    // We're only working with enums that have "enum_descriptions" attribute
                    if (e.Attribute(EnumDescriptionsAttribute))
                    {
                        // Define (or reuse if it's already defined) a namespace where our getDescription function will reside
                        var ns = model.FileOf(e).Namespace(e.tsName);
                        // Define a function inside a namespace
                        ns.Function(string.Format(@"
    export function getDescription(value: {0}): string {{
        switch (value) {{
    {1}
            default: return '';
        }}
    }}", e.tsName, e.Fields.JoinLines(CaseClause)));
                    }
                }
            }

            private string CaseClause(EnumField field)
            {
                return string.Format("        case {0}.{1}: return '{2}';", field.Enum.tsName, field.tsName, field.Annotation);
            }
        }
    }

See comments for better understanding on how this script works.

Now to run this script, save it as *EnumAnnotations.cs* and run the following compiler command:

.. code-block:: batch

   igorc.exe -t ts -x EnumAnnotations.cs enum_sample.igor

Make sure to provide relevant relative or absolute paths to script and igor files.

