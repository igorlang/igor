.. _unions:

******************************
  Union Types (Experimental)
******************************

.. warning::

   This feature is experimental and can be changed in future versions of Igor.

Union types allow to define tagged unions. They are useful when the value can belong to one of several fixed types.

Union Declaration
==================

*Syntax:*

::

    union UnionTypeName<T1, T2, ...>
    {
        Name1 => Type1;
        Name2 => Type2;
        ...
    }

where

* ``UnionTypeName`` is the valid identifier of the union type being declared
* ``TypeX`` (optional) are existing types
* ``NameX`` are union clause names

Union clauses may be typed values or single (*unit*) values, if type is not provided.

*Example:*

.. code-block:: igor

    record Option<T>
    {
        value => T;
        none;
    }

This example defines the generic ``Option`` type (the alternative to built-in optional ``?`` type). Values of ``Option<T>`` type can hold 
values of type ``T`` or a special *unit* value ``none``.

*Example:*

.. code-block:: igor

    union MarkupContent
    {
        strong => Strong;
        emphasis => Emphasis;
        style => Style;
        a => A;
        strikethrough => Strikethrough;
        sub => Sub;
        sup => Sup;
        code => Code;
        image => Image;
    }

This example is taken from **FB2** (FictionBook) XML format definition, and describes some XML content tags available for markup content.

Unions vs Variants
==================

Both variants and unions provide the type that is the algebraic sum of other types. However, there're many differences:

* Variant is the sum of records, but there're no limitations on union clause types (they may be primitive types or even records descending from different variants).
* Variant and its descendents must be defined in the same module. Types used in union clauses may be defined in different modules.
* Unions may have special (unit) values (e.g. ``none`` in ``Option<T>`` example above).
* Generic variants are not supported at the moment, while generic unions are supported.
