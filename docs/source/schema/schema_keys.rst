.. _schema_keys:

**********************
     Schema Keys
**********************

Key Type
========

It is very often that JSON documents, or *cards*, refer to other documents. We call these references *keys*. Technically keys are 
*string* values storing the id of another document.

To define the *key* type, use ``editor=key`` attribute:

.. code-block:: igor

    [schema editor=key]
    define Key atom;

And here's the usage example:

.. code-block:: igor

    record Card.CardCharacter[character]
    {
        Key weapon;  // the reference to the weapon document
        ...
    }

Restricting keys to a single category
=====================================

The ``Key`` type defined above would accept the reference to any document at all. Most often this is not desirable, 
and you'd like to restrict accepted documents to a single category. This may be done using ``category`` attribute.

*Example:*

.. code-block:: igor

    enum CardCategory
    {
        character;
        weapon;
    }

    [schema root]
    variant Card
    {
        tag CardCategory category;
    }

    [schema editor=key]
    define Key atom;

    [schema category="weapon"]
    define WeaponKey Key;

    record Card.CardCharacter[character]
    {
        WeaponKey weapon;
        ...
    }

    record Card.CardWeapon[weapon]
    {
        ...
    }

Restricting keys to several document types
==========================================

Sometimes there're multiple categories the key would accept. The possible approach is to use an interface marker and to
accept only documents implementing a certain interface.

*Example:*

.. code-block:: igor

    enum CardCategory
    {
        weapon;
        gear;
        character;
    }

    [schema root]
    variant Card
    {
        tag CardCategory category;
    }

    [schema editor=key]
    define Key atom;

    interface IItem
    {
        ...
    }

    [schema interface="IItem"]
    define ItemKey Key;

    record Card.CardWeapon[weapon] : IItem
    {
        ...
    }

    record Card.CardGear[gear] : IItem
    {
        ...
    }

    record Card.CardCharacter[character] // does not implement IItem
    {
        list<ItemKey> default_inventory;
    }
