.. _variants:

******************
     Variants
******************

Variant types are used to create inheritance tree. Variant is an abstract type that other variants and records can derive from, inheriting all ancestor fields.

Root variants have one special enum field, called **tag** field. All descendent records are bound to one of possible tag values. 

Generic variants are not supported.

.. warning::

   Variant type and all its descendents must be declared in the same module.

Variant Declaration
===================

*Syntax:*

::

    variant AncestorVariant.VariantName : ImplementsInterface1, ImplementsInterface2, ...
    {
        tag TagFieldType TagFieldName = TagDefaultValue;
        FieldType1 FieldName1 = DefaultValue1;
        FieldType2 FieldName2 = DefaultValue2;
        ...
    }

where

* ``VariantName`` is the valid identifier of the variant type being declared
* ``AncestorVariant`` (optional)  is the existing name of parent variant type
* ``ImplementsInterfaceX`` (otional) is the existing interface name, see :ref:`interfaces`
* ``FieldType``, ``FieldName`` and ``DefaultValue`` (optional) have the same meaning as for records, see :ref:`Records`. 

*Example:*

.. code-block:: igor

    enum ItemType
    {
        sword;
        bow;
        shield;
    }

    variant Item
    {
        int id;
        tag ItemType type; // Tag field
        string name;
    }

    // Nested variant
    variant Item.Weapon
    {
        float damage;
    }

    record Weapon.Sword[sword]
    {
        float arc;
    }

    record Weapon.Bow[bow]
    {
        float range;
    }

    record Item.Shield[shield]
    {
        float armor;
    }

See :ref:`record_field_order` to understand order in which records are serialized (but **tag** field always goes first).

.. _tag_field:

Tag Field
=========

.. note::

    All records deriving from variants should have **tag** value (e.g. ``[sword]`` in example above). 

.. warning::

    It is an error to define a variant record without **tag** value, or to define a non-variant record with **tag** value.
    It is also an error to have two variant records derived from the same variant with the same **tag** value.

Only enum **tag** field types are supported.

When deserializing, **tag** value is read first, and the relevant descendent record is instantiated and deserialized. When serializing, **tag** value is serialized first.

Field Redeclaration
===================

The parent variant field may be redeclared in ancestor variant or record by declaring the field with the same name and type. 
It is an error to use another type for the redeclared field. But the default value and attributes may be overriden.

.. seealso:: :ref:`interface_field_redeclaration` for interfaces.
