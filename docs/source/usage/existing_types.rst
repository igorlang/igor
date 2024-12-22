===========================
  Mapping Existing Types
===========================

Sometimes your Igor data types use other types that already exist on the target platform. In that case you need to
define Igor type that is mapped to existing taret language type.

There're two edge cases:

1) You define Igor type that is structuraly equivalent to the existing target type
2) Igor type and existing target type are structurally different

Mapping to Structurally Equivalent Type
=======================================

As an example we will define ``Vector3`` record and map it to the existing C# ``System.Numerics.Vector3`` type.

Those types are structurally equivalent: they have the same set of properties.

Using alias Attribute
---------------------

*Example:*

.. code-block:: igor

    [csharp struct alias="System.Numerics.Vector3" equality]
    record Vector3
    {
        float x;
        float y;
        float z;
    }

We used an **alias** attribute to denote that ``Vector3`` type is an alias for C#'s ``System.Numerics.Vector3`` type.
It has the following consequences:

* C# type declaration for ``Vector3`` is not generated
* Whenever ``Vector3`` type is referenced, ``System.Numerics.Vector3`` type name is used.

.. note::

    ``System.Numerics.Vector3`` properties are called ``X``, ``Y`` and ``Z``. Name translation is automatically handled
    by the ``field_notation`` attribute, which defaults to **upper_camel** (so we can omit it in this case).

The details of how Igor type is mapped to the existing type are target language specific. Most targets would use **alias** attribute for 
that, but refer to the target documentation for details.

Providing details about existing type
-------------------------------------

In the example abobe we also used attributes **struct** and **alias** to let C# code generator know that
``System.Numerics.Vector3`` is a *struct* type and implements equality operators (``==`` and ``!=``). This information
is used for code generation.

*Example:*

.. code-block:: igor

    [csharp struct equality]
    record Transform
    {
        Vector3 position;
        Vector3 euler;
    }

And this is the fragment of generated code. C# code generator knows that ``==`` operator may be used for ``Position`` and  ``Euler`` properties.

.. code-block:: C#

    public bool Equals(Transform other)
    {
        return Position == other.Position && Euler == other.Euler;
    }

Refer to target code generator documentation for details.

Using default serialization code
--------------------------------

In the ``Vector3`` example above there's no extra work required to support serialization. Igor will generate the default serialization code
to all enabled formats, as it has full and correct information about its structure.

Structurally Different Types
============================

Suppose you need a DateTime type. We want it to be stored in *ISO 8601* in JSON or XML, so we define it as string:

.. code-block:: igor

    define DataTime string;

However, target languages may have native implementation of DateTime, which stores data very differently from *ISO 8601* string.

As we already know, we need to provide an **alias** attribute (and other attributes to provide more details about the target type):

.. code-block:: igor

    [csharp struct alias="System.DateTime"]
    [erlang alias="calendar:datetime"]
    define DateTime string;

The default serialization code won't work, cause neither C# ``System.DateTime`` nor Erlang ``calendar:datetime()`` types are 
actually strings. That is why user has to provide custom serialization code.

.. note::

   Custom serialization code is different for different formats. That allows you to choose more compact binary encoding 
   for DateTime type, rather than *ISO 8601* string.

*Example:*

.. code-block:: igor

    [csharp alias="DateTime" struct json.serializer="DateTimeJsonSerializer.Instance" binary.serializer="DateTimeBinarySerializer.Instance"]
    [erlang alias="calendar:datetime"]
    [erlang binary.parser="igor_custom:datetime_from_binary" binary.packer="igor_custom:datetime_to_binary"]
    [erlang json.parser="igor_custom:datetime_from_json" json.packer="igor_custom:datetime_to_json"]
    [schema editor=datetime]
    define DateTime string;

See :ref:`custom_serialization` and target language documentation for more details.

.. note::

   In the DateTime example above, all other target generated code except C# and Erlang will store ``DateTime`` values as strings
   and serialize them as strings.


