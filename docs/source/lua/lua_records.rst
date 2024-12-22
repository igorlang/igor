.. _lua_records:

*****************
     Records
*****************

.. _lua_record_style:

Record Style
============

Igor Lua generator supports different ways to generate record tables, controlled by ``record_style`` attribute.

*Example:*

.. code-block:: igor

    [lua record_style=class]
    record Item
    {
        Key name;
        int id;
        int level = 1;
    }

With ``[lua record_style=class]`` attribute, which is the default, no type declaration is generated. Only serialization code is generated.

.. code-block:: lua

     Item = class(Item)

     function Item:init()
          self.level = 1
     end

     function Item:get_name()
          return self.name
     end

     function Item:set_name(name)
          self.name = name
     end

     function Item:get_id()
          return self.id
     end

     function Item:set_id(id)
          self.id = id
     end

     function Item:get_level()
          return self.level
     end

     function Item:set_level(level)
          self.level = level
     end

**class** function is user-defined. Getters and setters are generated both for documentation purpose and to allow fast failure.
Default values are properly initialized.

*Usage example:*

.. code-block:: lua

     local item = Item:new()
     item:set_name("rifle")

With ``[lua record_style=data]`` attribute no type declaration is generated. Only serialization code is generated (if enabled), 
which expects and produces structurally compatible data.

.. note::

     By default table keys are generated as *lower_underscore*. You can control name translation with ``field_notation`` attribute. See :ref:`notation` for details.

Integer Indices
===============

Sometimes it is desirable to store items as array with integer keys instead of string keys. This can be achieved with ``index`` attribute.

*Example:*

.. code-block:: igor

    [lua record_style=data]
    record Vector3
    {
        [lua index=1]
        float x;
        [lua index=2]
        float y;
        [lua index=3]
        float z;
    }

In Lua this definition is compatible with any ``{x, y, z}`` table where ``x``, ``y`` and ``z`` are integer numbers.

You can mix integer and string keys:

.. code-block:: igor

    record Item
    {
        [lua index=1]
        Key key;        // accessed via item[1]
        int level;      // accessed via item.level
    }
