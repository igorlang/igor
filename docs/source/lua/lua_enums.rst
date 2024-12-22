.. _lua_enums:

*****************
     Enums
*****************

.. _lua_enum_style:

Enum Style
==========

Igor Lua generator supports different ways to generate enums, controlled by ``enum_style`` attribute.

*Example:*

.. code-block:: igor

    [lua enum_style=table]
    enum Gender
    {
        male;
        female;
    }

With ``[lua enum_style=table]`` attribute, which is the default, the following declaration is generated:

.. code-block:: lua

     Gender = {
          Male = 1,
          Female = 2,
     }

With ``[lua enum_style=enum]`` attribute the following declaration is generated:

.. code-block:: lua

     Gender = enum(
          "Male",
          "Female"
     )

**enum** function is user-defined.

.. note::

     By default enum names are generated as *UpperCamel*. You can control name translation with ``field_notation`` attribute. See :ref:`notation` for details.

.. _lua_enum_alias:

Enum Aliases
============

If you have an enum defined in your own Lua code, you can use it in Igor files using ``enum_alias`` attribute.

*Lua enum definition example:*

.. code-block:: lua

     UnitStats = {
          Health = 1,
          Defense = 2,
          Armor = 3,
     }

*Igor enum alias alias:*

.. code-block:: igor

    [lua enum_alias="UnitStats"]
    define UnitStats atom;

The drawback is that when using :ref:`hercules` you won't get proper editor and validation. One solution would be to use Hercules based enums:

.. code-block:: igor
    
    [schema source="enums.UnitStats"]
    [lua enum_alias="UnitStats"]
    define UnitStats atom;

In this example *Hercules* expects to have **enums** document with **UnitStats** field containing the list of possible values. But you'll have to keep *Hercules* and Lua enums in sync.

*enums card definition example:*

.. code-block:: igor

    record Card.CardEnums[enums]
    {
        list<atom> UnitStats;
        ...
    }

