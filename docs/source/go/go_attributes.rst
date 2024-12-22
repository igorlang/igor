*******************************
   Go Generator Attributes
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
| package        | **string**      |             | module           | Go package name.                 |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| name           | **string**      |             | | module,        | Override generated name.         |                      |
|                |                 |             | | type,          |                                  |                      |
|                |                 |             | | record field   |                                  |                      |
|                |                 |             | | enum field     |                                  |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| field_notation | :ref:`Notation` | scope       | | record field   | | Notation used for name         | ``upper_camel``      |
|                |                 |             | | enum field     | | translation of record or enum  |                      |
|                |                 |             |                  | | field names.                   |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| prefix         | **string**      | scope       | | enum field     | | Provides the common prefix     |                      |
|                |                 |             |                  | | for enum field names.          |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| string_enum    | **string**      | scope       | enum             | | Generate enum as set of string | ``false``            |
|                |                 |             |                  | | constants, not integers        |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| interface      | **string**      |             | variant          | | Generate common interface type | ``table``            |
|                |                 |             |                  | | for variant and its            |                      |
|                |                 |             |                  | | descendents with the given     |                      |
|                |                 |             |                  | | name                           |                      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------------+
