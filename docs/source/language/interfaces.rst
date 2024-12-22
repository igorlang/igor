.. _interfaces:

******************
    Interfaces
******************

Interfaces allow different records or variants to reuse the same subset of fields. This is useful inside Igor language, 
but if target language has the concept of interface code may be generated as well.

.. note::

   Interfaces are not real data types. You cannot use interface as the record field or function argument type. 

Interface Declaration
=====================

::

    interface InterfaceName<TypeParameters> : InterfaceAncestor1, InterfaceAncestor2, ...
    {
        FieldType1 FieldName1 = DefaultValue1;
        FieldType2 FieldName2 = DefaultValue2;
        ...
    }

where

    * ``InterfaceName`` is the valid identifier of interface being declared
    * ``<TypeParameters>`` is the optional comma separated-list of generic type parameters
    * ``InterfaceAncestorX`` are the names of ancestor interfaces
    * ``FieldTypeX``, ``FieldNameX`` and ``DefaultValueX`` have the same meaning as in record declaration, see :ref:`records`

*Example:*

.. code-block:: igor

    interface IGui
    {
        string name;
        int icon_index;
    }

    interface IItem : IGui
    {
        int price;
        float weight;
    }

    record Weapon : IItem
    {
        WeaponType type;
        float damage;
    }

.. warning::

   Circular interface inheritance is not allowed.

.. _interface_field_redeclaration:

Field Redeclaration
===================

When declaring a record, variant or interface that implements another interface, the field may be redeclared by declaring the field with the same name and type. 
It is an error to use another type for the redeclared field. But the default value and attributes may be overriden.

*Example:*

.. code-block:: igor

    interface IGui
    {
        string name;
        [schema editor_key="editor_atlas_items"]
        int icon_index = 0;
    }

    record Ability : IGui
    {
        // ERROR: type name can't be overriden!
        Text name;

        // OK: attributes and default value can be overriden
        [schema editor_key="editor_atlas_items"]
        int icon_index = 1;

        ...
    }

If record, variant or interface implement two interfaces which both define (directly or via interface inheritance) the field with the same name, it is not the error if the type is the same. The latter field's default value and attributes are picked. If types are different, it is an error.

*Example:*

.. code-block:: igor

    interface IGui
    {
        string name;
        int icon_index;
    }

    interface IItem
    {
        string name;
        int price;
    }

    // field 'name' is declared in both interfaces, but as the type is the same,
    // it is ok, and record Weapon will just have one 'name' field.
    record Weapon : IGui, IItem
    {
        ...
    }

Interfaces vs Variants
======================

While both interfaces and variants support inheritance, there're important differences:

* Interfaces are not real data types. You cannot use interface as the record field or function argument type. 
* Interfaces support multiple inheritance, variants do not.
* Generic interfaces are supported, generic variants are not.
