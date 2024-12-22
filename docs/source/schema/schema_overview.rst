**************************
    Schema Overview
**************************

What is Igor Schema?
====================

*Igor Schema* was initially created for describing game design data. Game design data describes 
rules and content of the game. This data is later used by the game code. 
 
For the successful data-driven development it's important to isolate data from code. Designers
use some kind of editor to edit data, and code loads this data. It is quite common that data
is stored in XML or JSON files, and designers use special editors (sometimes just simple text
editors) to edit data. It helps a lot if content they create is validated against schema, e.g.
XSD schema for XML files.

When Igor is used to describe types of design data, so that it can be deserialized from JSON or XML,
it is natural to use Igor to generate the schema as well. This schema can be used by the data 
editor to enforce data validation and provide the best user experience for designers.

This is exactly what *Igor Schema* does - it describes the JSON serialization format of Igor data types,
and provides details on data validation and editor types to be used by data editors. In particular,
*Igor Schema* is used by :ref:`hercules` tool that can edit *CouchDB* JSON documents containing 
JSON-serialized Igor records.

Igor Schema Format
==================
 
Technically Igor Schema is a JSON document of a certain layout. This layout can be described with Igor language itself.
This description is available here: https://raw.githubusercontent.com/igorlang/igor/refs/heads/main/source/Igor.Schema/schema.igor .

The main data type used by Igor Schema is ``Descriptor``. Descriptor describes how a value should be edited and
validated. Descriptor is a variant type, each record describes values of different types. Some types are built-in,
and some types are user defined custom types described with ``CustomType`` variant. In particular, *enum*, *record* 
and *variant* custom types are supported.

Enabling Schema
===============

By default, no type information at all is included into generated *Igor Schema*. You have to enable schema for modules
or types via ``enabled`` attribute for ``schema`` target:

.. code-block:: igor

   [schema enabled]
   module ProtocolCards
   {
       ...
   }

.. note::

   All schema related attributes should use ``schema`` target.

``schema enabled`` attribute has scope inheritance, which means that it is enough to set it on module and it will enable
schema generation for all nested types.

.. _cards:

Cards and Schema Root
=====================

We call the single isolated piece of data, that is usually distinguished by unique name or id, a *card*. A *card* describes
a single game content instance (e.g. character, item) or ruleset (e.g. game mode, mission).

Different classes of *cards* are described with different Igor types. *Igor Schema* expects that all cards are records
descending from the single **variant** type. This type is called the **root** type. In order to select a proper editor for
the JSON document describing a *card*, the variant *tag* field is used which we call a *category*.

By convention, the **root** variant is usually named ``Card``, the *category* field is named ``category`` and *category* enum is 
named ``CardCategory``. However this convention is not mandatory.

The **root** variant nust be marked with ``schema root`` attribute. The set of Igor files used to generate the schema should
have one and only one **root** variant.

*Example:*

.. code-block:: igor

    [schema enabled]
    module ProtocolCards
    {
        enum CardCategory
        {
            item;
            character;
            game_mode;
        }

        [schema root]
        variant Card
        {
            tag CardCategory category;
            ...
        }

        record Card.CardItem[item]
        {
            ...
        }

        ...
    }

Providing More Type Information
===============================

It's desirable to provide as much information as possible on how types and values should be edited and validated for better
editor validation and user experience. This can be achieved with schema attributes. 

The list of supported schema attributes is located here: :ref:`schema_attributes`.

Generating Schema
=================

You can use :ref:`compiler` to generate schema from the set of Igor files using the following command:

.. code-block:: batch

   igorc.exe -schema *.igor

This command will generate ``schema.json`` file in the current folder. You can override file name with ``-output-file``
command line option. See more information on available command line options here: :ref:`cli_options`.

.. seealso:: :ref:`upload_schema`

