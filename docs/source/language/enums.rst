.. _enums:

****************
   Enum Types
****************

Enum is the enumeration of named options (fields). Each enum field has an implicitly or explicitly assigned integer value. If value is not specified, the incremented previous field value is taken. By default values start from 1.

Enum Declaration
================

::

    enum EnumName
    {
        FieldName1 = FieldValue1;
        FieldName2 = FieldValue2;
        ...
    }

where

* ``EnumName`` is a valid identifier of the declared type name
* ``FieldName`` is a valid identifier of the field name
* ``FieldValue`` is an integer constant (optional)

*Example:*

.. code-block:: igor

    enum TestEnum
    {  
        first = 1;
        second = 2;
        third = 3;
        last; // the value of 4 is assigned
    }

Text serialization formats should store string field names, while compact binary formats should store integer field values.

Record fields of enum type may be initialized with the default value:

.. code-block:: igor

    TestEnum enum_value = second;

.. _enum_integer_type:

Underlying Integer Type
=======================

Underlying integer type used for binary serialization can be specified with ``int_type`` attribute. 

*Example:*

.. code-block:: igor

    [* int_type=int]
    enum TestEnum
    {
        ...
    }

Supported attribute values are: **byte**, **sbyte**, **short**, **ushort**, **int**, **uint**, **long**.

If attribute is not specified, the minimum type containing all field values is chosen.

.. _flag_enums:

Flag Enums
==========

Flag enums may be used for bit masks. Field values are expected to be powers of two.

**flags<T>** type represents the collection of enum flags. Compact binary formats should store integer bit masks. Text serialization formats may use list of names or another text representation.

*Example:*

.. code-block:: igor

    enum Days
    {  
        none = 0;
        monday = 1;
        tuesday = 2;
        wednesday = 4;
        thursday = 8;
        friday = 16;
        saturday = 32;
        sonday = 64;
    }
    
    Days day = monday; // single value
    flags<Days> days = saturday; // mask combined of several values

At the moment flags cannot be initialized with the mask of several values, this feature will be added in future versions.

