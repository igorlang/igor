***************************
        Attributes
***************************

Overview
========

Any statement can have a list of assigned attributes. Attibutes are used to specify additional code generator options.

The list of attributes is specified before the statement. Each attribute has the following syntax:

::

    [Target Name1=Value1 Name2=Value2 ...]

where

* ``Target`` is the target language, e.g. ``csharp`` or ``erlang``. It can be set to ``*`` to apply attribute for all targets.

* ``Name`` is the attribute name identifier. It may consist from alpha characters, numbers, ``_`` and dots (``.``).

* ``Value`` is the optional attribute value.

By :ref:`naming_convention` attribute names should use ``lower_underscore`` notation, with optional domain prefix concatenated via dot symbol. For example, all attributes controlling json serialization should have ``json.`` prefix, e.g. ``json.enabled``.

Value type is name-dependent. Omitting value is identical to setting it to ``true``, and makes sense only for boolean attributes.

You can specify several name-value pairs, all of them will share the same target and side.

*Example:*

.. code-block:: igor

    [* json.enabled]
    [csharp struct alias="System.Numerics.Vector3" equality]
    [erlang tuple alias="vector:vector"]
    record Vector3
    {
        float x;
        float y;
        float z;
    }

Some attributes may allow multiple usage, by specifying the same attribute several times with different values.

Attribute Types
===============

Attribute value type depends on the attribute meaning. The following types are supported:

* **bool** values with ``true`` and ``false`` values (``true`` may be omitted)

* **string** attributes with quoted (``"``) string values are supposed to be inserted into generated code without any processing or validation

* **integer** attributes can hold integer values

* **enum** attributes can be set to one of predefined values (see documentation for specific attribute for details)

* **object** attributes allow to specify nested attributes.

*Example:*

.. code-block:: igor

    [diagram connector=(name="IN" type=in position="0.5,0")]


.. _attribute_inheritance:

Attribute Inheritance
=====================

Most attributes are applicable only to the statement they are declared for. However, some attributes expand there effect for other statements, or vice versa - statements can inherit some attributes from other statements, unless explicitly overriden.

Igor supports 3 models of attribute inheritance:

* Scope inheritance

* Type inheritance

* Variant inheritance

Scope Inheritance
-----------------

With scope inheritance, if a parent statement contains nested statements, all nested statements inherit scope attributes from their parent. 

*Example:* enabling json for the whole module

.. code-block:: igor

    // All nested definitions will have json enabled
    [* json.enabled]
    module MyModule
    {
        // Json support is on for MyRecord1 
        record MyRecord1
        {
            ...
        }

        // Explicitly disabling json for MyRecord2
        [* json.enabled=false]
        record MyRecord2
        {
            ....
        }    
    }

.. _attribute_type_inheritance:

Type Inheritance
----------------

With type inheritance statements inherit attribute values from their type statements:

* For record fields, attributes are inherited from type statement (if any)

* For define statements, attributes are inherited from synonim type statement (if any)

*Example:*

.. code-block:: igor

    module MyModule
    {
        [schema editor=key]
        define Key atom;

        // editor=key attribute is inherited from Key type
        [schema category="item"]
        define ItemKey Key;

        record MyRecord
        {
            // editor=key attribute is inherited from Key type
            [schema category="object"]
            Key object_key;

            // editor=key attribute is inherited from Key type
            // category="item" attribute is inherited from ItemKey type
            ItemKey item_key;

            // ERROR: THIS WON'T WORK, as keys is instance of list<Key>, not Key.
            // Use list<ItemKey> instead.
            [schema category="item"]
            list<Key> keys;
        }
    }

Variant Inheritance
-------------------

Variant inheritance allow variant records to inherit attributes from their ancestors.

*Example:*

.. code-block:: igor

    using Vectors;

    module Shapes
    {
        enum ShapeType
        {
            rectangle;
            circle;
        }

        [csharp equals]
        variant Shape
        {
            tag ShapeType type;
            Vector2 center;
        }
        
        // ShapeRectangle inherits equals attribute from Shape
        record Shape.ShapeRectangle[rectangle]
        {
            float width;
            float height;
        }
        
        // ShapeCircle inherits equals attribute from Shape
        record Shape.ShapeCircle[circle]
        {
            float radius;
        }
    }
