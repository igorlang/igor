.. _records:

******************
     Records
******************

Records are the ordered sequences of named fields of different types.

Record Declaration
==================

*Syntax:*

::

    record RecordName : ImplementsInterface1, ImplementsInterface2, ...
    {
        FieldType1 FieldName1 = DefaultValue1;
        FieldType2 FieldName2 = DefaultValue2;
        ...
    }

where

* ``RecordName`` is the valid identifier of the record type being declared
* ``ImplementsInterfaceX`` (optional) is the existing interface name, see :ref:`interfaces`
* ``FieldTypeX`` is the existing built-in or user-defined type
* ``FieldNameX`` is the existing identifier of field name
* ``DefaultValueX`` (optional) is the value of type ``FieldType`` which is the default value of the field. When record type is instantiated in target language, default values should be assigned in constructor. If not set, there's no default value. 

*Example:*

.. code-block:: igor

    record UserAccount
    {
        int id;
        string name;
        bool active = true;
        flags<Role> roles; // previously defined enum type Role is used here
        ?string comment;
    }

Variant Record Declaration
==========================

Records may inherit from variants.

*Syntax:*

.. code-block:: igor

    record VariantAncestor.RecordName[TagValue] : ImplementsInterface1, ImplementsInterface2, ...
    {
        FieldType1 FieldName1 = DefaultValue1;
        FieldType2 FieldName2 = DefaultValue2;
        ...
    }

where

* ``VariantAncestor`` is the existing name of parent variant type
* ``TagValue`` is the variant tag value, see :ref:`tag_field`

See :ref:`variants` for examples and more information about variants.

.. _record_field_order:

Field Order
===========

When serializing record types, the following field order is used:

1. Parent variant fields, starting from the root of derivation tree
2. Interface fields, in order interfaces are declared in record definition
3. Fields defined in record itself

Generic Records
===============

Generic records has one or more generic type arguments, that may be used as field types.

*Example:*

.. code-block:: igor

    record Pair<T1,T2>
    {
        T1 item1;
        T2 item2;
    }

When using generic records they should be instantiated with actual types:

.. code-block:: igor

    record PairUsageExample
    {
        list<Pair<int, string>> pairs = [];
    }
