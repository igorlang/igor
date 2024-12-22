==============
Built-in Types
==============

Primitive Types
===============

========================= ======================== ========================================================
Igor Type                 Description              Values
========================= ======================== ========================================================
**bool**       	          boolean value            true | false
**int8** or **sbyte**     signed 8bit integer      -128 .. 127
**uint8** or **byte**     unsigned 8bit integer    0 .. 255
**int16** or **short**    signed 16bit integer     -32 768 .. 32 767
**uint16** or **ushort**  unsigned 16bit integer   0 .. 65 535
**int32** or **int**      signed 32bit integer     -2 147 483 648 .. 2 147 483 647
**uint32** or **uint**    unsigned 32bit integer   0 .. 4 294 967 295
**int64** or **long**     signed 64bit integer     -9 223 372 036 854 775 808 .. 9 223 372 036 854 775 807
**uint64** or **ulong**   unsigned 64bit integer   0 .. 18 446 744 073 709 551 615
**float32** or **float**  32bit floating-point
**float64** or **double** 64bit floating-point
**string**                utf8 string
**binary**                binary (byte array)
**atom**                  utf8 string
**json**                  json value
========================= ======================== ========================================================

Boolean type
------------

Boolean types can have two values: ``true`` and ``false``.

*Example:*

::

    bool boolean_value = true;

Integer types
-------------

Eight integer types are supported: see the table of primitive types.

*Example:*

::

    int int_value = -10;

Floating point types
--------------------

Single and double precision floating-point types are supported.

*Example:*

::

    float float_value = 0; // Integer constant is automatically converted to the float
    double double_value = 1.2e5;

String type
-----------

String type represents utf8 string.

*Example:*

::

    string string_value = "test";

Binary type
-----------

Binary type represents the raw byte array. No binary literal values exist, so no default values can be set for binary fields.

Atom type
----------

Atom type represents the atom type. It is mapped to the  target language's atom type, if supported, or to string type. No default values are supported.

Json type
---------

Json type should be mapped to the target language's json type (built-in or library).

Generic Types
=============

List type
---------

**list<T>** is the list of items of type **T**.

The special literal ``[]`` is the possible value for list types, representing the empty list.

*Example:*

::

    list<int> list_of_ints = [];

Dict type
---------

**dict<K,V>** represents the unordered associative map with key of type **K** and value of type **V**.

The special literal ``[]`` is the possible value for list types, representing the empty dict.

*Example:*

::

    dict<int, string> map_of_ints_to_strings = [];

.. warning::

   While Igor does not put any restrictions on key type, the target language or serialization format may do that. 
   It is recommended to only use integer, string and enum types as dict keys for JSON compatibility.

.. warning::

   **dict** is unordered which means that implementations are not required to guarantee that pair order is maintained. 
   While some target implementations may still maintain pair order, you should not rely on that.

Optional type
-------------

**?T** represents the optional value of type **T**.

Nested optional types are not supported.

Flags type
----------

**flags<T>** is the bit mask of enum **T**. It is an error to use non-enum type as **T**.

See :ref:`flag_enums` for details.
