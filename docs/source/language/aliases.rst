.. _aliases:

***************
  Alias Types
***************

Alias types are used to define the new type that is an alias of existing type.

Alias Declaration
==================

*Syntax:*

::

    define AliasTypeName ExistingType;

    define GenericAliasTypeName<T1, T2, ...> ExistingType;

where

* ``AliasTypeName`` or ``GenericAliasTypeName`` is the valid identifier of the alias type being declared
* ``ExistingType`` is some existing type
* ``T1``, ``T2``, etc.  are optional generic type arguments

Alias Usage
===========

There're multiple reasons to use alias types. Some of them are described below with examples.

Self-documenting alias types
----------------------------

Alias types may be defined to give the reader better understanding what a certain type is used for.

*Example:*

.. code-block:: igor

   define ItemTag string;

   record Item
   {
      string name;
      list<ItemTag> tags;
   }

Generic type reduction
----------------------

Alias types may be used to simplify the type name of a commonly used generic type.

In the following example the type ``ordered_map`` is introduced as an alternative to built-in generic **dict** type, which does not provide guarantees on order persistence.

*Example:*

.. code-block:: igor

    record KeyValuePair<TKey, TValue>
    {
        TKey key;
        TValue value;
    }

    define ordered_map<TKey, TValue> list<KeyValuePair<TKey, TValue>>;

    record TestRecord
    {
        ordered_map<int, string> values;
    }

Providing attributes
--------------------

Very common reason to use alias types is to provide attributes modifying various aspects of the type behaviour. 
It may be handy to provide custom serialization attributes, schema attributes, or target language-specific attributes.

*Example:*

.. code-block:: igor

    [csharp alias="DateTime" struct json.serializer="DateTimeJsonSerializer.Instance" binary.serializer="DateTimeBinarySerializer.Instance"]
    [erlang alias="calendar:datetime"]
    [erlang binary.parser="igor_custom:datetime_from_binary" binary.packer="igor_custom:datetime_to_binary"]
    [erlang json.parser="igor_custom:datetime_from_json" json.packer="igor_custom:datetime_to_json"]
    [schema editor=datetime]
    define DateTime string;

In this example we define DateTime as an alias of the string type, but we use attributes to adjust code generation aspects for C# and Erlang languages, and setup schema information.

.. seealso::  :ref:`attribute_type_inheritance`.
