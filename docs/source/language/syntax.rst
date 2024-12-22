****************
Igor Syntax
****************

File Format
===========

Igor Language is a text code stored in a plain text utf8 files. By convention Igor file should (but not must) have ``.igor`` extension.

Whitespaces
===========

Whitespaces are used to separate tokens. Whitespaces are allowed between any two tokens but required only to separate keywords and identifiers.

Any non-empty combination of the following elements is a valid whitespace:

    * Tab (``\t``)
    * Linebreaks (``\r``, ``\n``)
    * Spaces
    * Comments

All whitespaces are indistinuguishable. Indentation is not enforced. 

Comments
========

C-style comments are supported:

::

    // Single-line comment

    /* 
        Multi-line comment
    */

Comments are treated as regular whitespaces. Their content is ignored by compiler. 

Annotations
===========

There's a special syntax for documentation comments (annotations):

::

    # Single-line annotation

    <#
        Multi-line annotation
    #>

Unlike normal comments, documentation comments are used to generate annotations, comments and documentation summaries for the target code.

Identifiers
============

Identifier is a non-empty sequence of latin letters, decimal digits and underscore character, starting from the letter or underscore character.

Identifiers are case-sensitive. 

Numbers
========

There're two types of numeric literals: integers and floats. The conventional notation is used.

*Examples:*

::

    42
    -10.5
    1.5e7

Strings
========

Strings are enclosed in double quotes (``"``).

*Example:*

::

    "This is a string"
    "" // empty string


