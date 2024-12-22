*******************************
   Lua Generator Attributes
*******************************

Here's the list of attributes that affect Lua code generation.

.. seealso::
  - :ref:`json_attributes`

+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| Attribute      | Type            | Inheritance | Target           | Description                      | Default              |
+================+=================+=============+==================+==================================+======================+
| enabled        | **bool**        | scope       | | module,        | | Enables code generation for    | ``true``             |
|                |                 |             | | type           | | the module or type.            |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| file           | **string**      |             | module           | Override generated file name.    |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| name           | **string**      |             | | module,        | Override generated name.         |                      |
|                |                 |             | | type,          |                                  |                      |
|                |                 |             | | record field   |                                  |                      |
|                |                 |             | | enum field     |                                  |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| field_notation | :ref:`Notation` | scope       | record field     | | Notation used for name         | ``lower_underscore`` |
|                |                 |             |                  | | translation of record          |                      |
|                |                 |             |                  | | field names.                   |                      |
|                |                 |             |                  | |                                |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| field_notation | :ref:`Notation` | scope       | enum field       | | Notation used for name         | ``upper_camel``      |
|                |                 |             |                  | | translation of enum            |                      |
|                |                 |             |                  | | field names.                   |                      |
|                |                 |             |                  | |                                |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| json.serializer| **string**      |             | type             | | Provides the user-provided JSON|                      |
|                |                 |             |                  | | serializer name. Serializer    |                      |
|                |                 |             |                  | | won't be generated.            |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| enum_alias     | **string**      |             | type             | | Provides the alias enum table  |                      |
|                |                 |             |                  | | name. See :ref:`lua_enum_alias`|                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| enum_style     | | ``table``     |             | enum             | | The style of generated enum.   | ``table``            |
|                | | ``enum``      |             |                  | | See :ref:`lua_enum_style`      |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| record_style   | | ``data``      |             | | record         | | The style of generated         | ``class``            |
|                | | ``class``     |             | | variant        | | record table.                  |                      |
|                |                 |             |                  | | See :ref:`lua_record_style`    |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
