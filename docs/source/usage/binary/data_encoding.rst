********************
    Data Encoding
********************

Overview
=======================

All values occupy the integer number of bytes. That means that 1 byte is the minimum size of the serialized value.

Little endianness is always used.

Variable size values - strings, binaries, lists and dicts - encode size with variable-length encoding (supported range is ``0..0x0FFFFFFF``).

Built-in Type Encoding
=======================

=============== ======================== ========= ====================================================
Igor Type       Description              Byte size Comment
=============== ======================== ========= ====================================================
**bool**       	boolean value            1         **1** for **true**, **0** for **false**
**byte**        unsigned 8bit integer    1    
**sbyte**       signed 8bit integer      1    
**short**       signed 16bit integer     2    
**ushort**      unsigned 16bit integer   2    
**int**         signed 32bit integer     4    
**uint**        unsigned 32bit integer   4    
**long**        signed 64bit integer     8    
**ulong**       unsigned 64bit integer   8    
**float**       32bit floating-point     4
**double**      64bit floating-point     8 
=============== ======================== ========= ====================================================

**string** encoding: variable-length byte size (1-4 bytes) | string content (utf8).

**atom** encoding: variable-length byte size (1-4 bytes) | content.

**binary** encoding: variable-length byte size (1-4 bytes) | content.

**list<T>** encoding: variable-length item count (1-4 bytes) | items (encoding depends on type T).

**dict<K,V>** encoding: variable-length item count (1-4 bytes) | pairs (key | value).

**?T** encoding: presence byte is used, ``0`` if value is unset, ``1`` | value if value is set. 

.. note::

   The presence of optional record fields is encoded via the header bit mask when header mode is used. See :ref:`record_encoding` for details.

.. _binary_enum_encoding:

Enum Encoding
=============

Enums are encoded as integer values, using the underlying integer type (see :ref:`enum_integer_type`).

Enum flags (bit masks) are encoded as integer values.

.. _record_encoding:

Record Encoding
===============

Records are encoded by encoding record fields one by one, in order they are defined.

There're two supported modes - headerless and with header. They differ in how they encode optional fields, and produce the same result for records without optional fields (**?T**).

In headerless mode optional fields are encoded as usual, each with the presence byte prefix (``0`` if value is unset, ``1`` | value if value is set).

In header mode the presence of optional field values is encoded in the record header, one bit per field, zero padded to byte boundary. Values itself are encoded, if present, in usual order, without presence byte prefix.

*Example:*

.. code-block:: igor

    record Record
    {
        int required_value; // set to 0x12345678
        ?int optional_value1; // unset
        ?int optional_value2; // set to 0xabcdef12
    }

In headerless mode, this record is encoded as following: ``0x12345678 | 0 | 1 | 0xabcdef12``.

In header mode, this record is encoded as following:  ``0b00000010 | 0x12345678 | 0xabcdef12``.

Header mode is more efficient when encoding records with many optional values.

``binary.header`` boolean attribute is used to toggle header mode. Header mode is used by default.

Variant Encoding
================

Variant tag is encoded first. Then the variant record is encoded as usual, with optional header and all fields except the tag field.

When deserializing, tag is decoded first to determine which variant descendent record is to be decoded after that.

Exception Encoding
==================

Exceptions are encoded the same way as common records.
