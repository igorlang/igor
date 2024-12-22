*************************
    Type Serialization
*************************

This section describes how JSON serialization is performed for Igor types by default. It can be overriden with 
custom serialization, see :ref:`custom_serialization`.

Primitive types
===============

=============== ======================== ===================== ==============
Igor Type       Description              JSON Type             Comment
=============== ======================== ===================== ==============
**bool**       	boolean value            **true** or **false**
**byte**        unsigned 8bit integer    number
**sbyte**       signed 8bit integer      number
**short**       signed 16bit integer     number
**ushort**      unsigned 16bit integer   number
**int**         signed 32bit integer     number
**uint**        unsigned 32bit integer   number
**long**        signed 64bit integer     number
**ulong**       unsigned 64bit integer   number
**float**       32bit floating-point     number
**double**      64bit floating-point     number
**string**      utf8 string              string
**atom**        atom                     string
**binary**      binary                   string                Base64 encoded
=============== ======================== ===================== ==============

Optional types
==============

Optional types (``?T``) are represented with ``T`` value serialized to JSON if the value is present, and **null** if it is not.

Lists
======

Lists (``list<T>``) are serialized as JSON arrays of serialized ``T`` values.

Dictionaries
============

Dictionaries (``dict<TKey, TValue>``) are serialized as JSON objects, where keys are serialized values of ``TKey`` type and
values are serialized corresponding values of ``TValue`` type. If ``TValue`` is optional type, including pairs with **null** values
is mandatory.

JSON object keys must be strings, not arbitrary object values. That is why only limited set of ``TKey`` types is supported. 
Those types are: **string**, **atom**, enums, all integer types (which are first converted to string), and their alias types
(declared with **define** keyword). For other ``TKey`` types user needs to provide the custom serializer that is capable to
serialize ``TKey`` values to JSON strings in order to use ``dict<TKey, TValue>`` type. The way to do it is target language specific.

Enums and Flags
===============

Enums are serialized as string values by default. By default enum names are used as JSON values, but the following attributes can be used to 
alter them:

* ``json.key`` allows to overwrite the name for the certain enum value
* ``json.field_notation`` (**none** by default) defines name translation for all fields

*Example:*

.. code-block:: igor

    [* json.field_notation=uppercase]
    enum LogLevel
    {
        fatal;      // "FATAL"
        error;      // "ERROR"
        [* json.key="WARN"]
        warning;    // "WARN"
        info;       // "INFO"
        debug;      // "DEBUG"
        trace;      // "TRACE"
    }

Enum flags are converted to JSON array of enum values. For example, ``flags<LogLevel>`` field set to ``error | warning`` mask would be
serialized as ``["ERROR", "WARN"]``.

Use ``json.number`` attribute to serialize enum values as integers rather than strings.

Records
=======

Records are serialized to objects, where object keys are field names and corresponding values are field values serialized to JSON.

The following attributes can be used to alter JSON keys:

* ``json.key`` allows to overwrite the name for the certain enum value
* ``json.field_notation`` (**none** by default) defines name translation for all fields

These attributes work similarly to how they work for enums.

If record field type is optional, and field value is unset, this field may be omited in the resulting JSON object or present as ``null`` value.
This behaviour can be configured with ``json.nulls`` attribute, which has no default value (so default behaviour is target-specific).
When deserializing, key with **null** value and absense of key in the JSON object are treated the same way.

Patch records should be able to distinguish between null and unset values. See :ref:`patch_records` for details.

.. note::

    When deserializing, unknown keys are silently ignored.

Exceptions are encoded the same way as normal records.

Variants
========

When serializing a variant record, the record is serialized normally as described above. The tag key is always included.

When deserializing, tag value is deserialized first to determine which variant descendent record is to be deserialized after that.

Unions
======

* Typed clauses are serialized to JSON object with a single key-value pair
* *Unit* clauses are serialized to a string

``json.key`` and ``json.field_notation`` attributes work the same way as for enums and records.
