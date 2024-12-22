.. _serialization:

******************************
     Serialization Overview
******************************

Igor is designed to support multiple serialization formats.

Currently supported serialization formats are:

* :ref:`binary`
* :ref:`json`
* :ref:`xml`

Serialization for each format is controlled by attributes in the respective domain.

.. _enable_serialization:

Enabling Serialization Format
=============================

By default, only data types are generated, without any serialization code. To enable serialization, use ``_domain_.enabled`` attribute
on data type or the whole module.

*Example:* enabling C# code generation for binary and JSON serialization for the whole module

.. code-block:: igor

    [csharp binary.enabled json.enabled]
    module Types
    {
    }

Excluding Fields
----------------

You can exclude a record field from serialization with ``_domain_.ignore`` attribute on the field:

.. code-block:: igor

    [csharp binary.enabled json.enabled]
    module Types
    {
        record ExampleRecord
        {
            string always_serialized; // Both JSON and binary serialization are on
            [* binary.ignore]
            string json_only_comment;  // JSON serialization is on, but binary serialization is off
        }
    }

.. warning::

   To ensure that different languages can share the same format, make sure that record field is included or excluded for all targets.

*Example:*

.. code-block:: igor

   // DO:
   [* binary.ignore]
   string value;

   // DO NOT:
   [csharp binary.ignore]
   string value;

.. _custom_serialization:

Custom Serialization
====================

In some cases the default serialization code is not what user wants. There may be several reasons for that, for example:

* Igor data is modelled for existing JSON or XML data layout, and this layout is not compatible with Igor serialization rules
* Igor type is an alias for the existing target platform type, and Igor does not know enough details of that target type to be able
  to generate the correct code

In such cases the user is responsible for providing custom serialization methods via attributes. The set and meaning of custom 
serialization attributes differ for different target languages. See documentation for the specific target for details.

.. _patch_records:

Patch Records
====================

Sometimes it is useful to define records that are used to describe changes of a certain state.

*Example:*

.. code-block:: igor

    record State
    {
        int value;
        ?int opt_value;
    }

    record StateUpdate
    {
        ?int value_update;
        ?int opt_value_update;
    }

In the example above ``StateUpdate`` can be used to transfer ``State`` changes over network. We could use ``State`` directly, but it may be very large,
so we introduce ``StateUpdate`` record where all fields are optional, and we can transfer only values that have been changed.

The problem with this example is that ``State.opt_value`` is optional, so its value range includes **null** value. So ``StateUpdate.opt_value_update`` 
valid range should add another **null** to this range, to represent absence of changes (unset value). But ``??int`` type is not supported.

Patch records allow to distinguish between explicit **null** and unset values:

.. code-block:: igor

    [* patch_record]
    record StateUpdate
    {
        int value_update;
        ?int opt_value_update;
    }

All patch record fields are optional, so we don't use optional type. If a value is of optional type (e.g. ``?int``), that means that its **null** value, if set,
should be explicitly present in the serialized data. Target implementations should provide a way to distinguish between **null** and unset values (e.g. by using 
a special **undefined** value in TypeScript or Erlang).

