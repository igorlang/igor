*******************************
 Elixir Generator Attributes
*******************************

Here's the list of attributes that affect Elixir code generation.

.. seealso::
  - :ref:`json_attributes`
  - :ref:`binary_attributes`
  - :ref:`xml_attributes`

+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| Attribute                 | Type            | Inheritance | Target           | Description                      | Default              |
+===========================+=================+=============+==================+==================================+======================+
| enabled                   | **bool**        | scope       | | module,        | | Enables code generation for    | ``true``             |
|                           |                 |             | | type           | | the module or type             |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| file                      | **string**      |             | module           | Override generated file name     |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| name                      | **string**      |             | | module         | Override generated type name     |                      |
|                           |                 |             | | type           |                                  |                      |
|                           |                 |             | | service        |                                  |                      |
|                           |                 |             | | record field   |                                  |                      |
|                           |                 |             | | enum field     |                                  |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| alias                     | **string**      |             | type             | | Do not generate a type but use |                      |
|                           |                 |             |                  | | existing type definition       |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| tuple                     | **bool**        |             | record           | | Generate Elixir tuple type     | ``false``            |
|                           |                 |             |                  | | for the record                 |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| record                    | **bool**        |             | record           | | Generate Elixir record type    | ``false``            |
|                           |                 |             |                  | | for the record                 |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| map                       | **bool**        |             | record           | | Generate Elixir map type       | ``false``            |
|                           |                 |             |                  | | for the record                 |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| tagged                    | **bool**        | scope       | | union          | | Represent union clauses as     | ``true``             |
|                           |                 |             | | union clause   | | ``{:clause, value}`` tuples    |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| exception_message         | **string**      |             | exception        | Override exception message       |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| json.custom               | **string**      |             | type             | | Custom JSON serializer module  |                      |
|                           |                 |             |                  | | exporting ``from_json!`` and   |                      |
|                           |                 |             |                  | | ``to_json!`` functions         |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| json.compatible           | **bool**        |             | type             | | Skip JSON serialization as     | ``false``            |
|                           |                 |             |                  | | type's JSON representation is  |                      |
|                           |                 |             |                  | | identical with Elixir values   |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| string.custom             | **string**      |             | type             | | Custom string serializer module|                      |
|                           |                 |             |                  | | exporting ``from_string!`` and |                      |
|                           |                 |             |                  | | ``to_string!`` functions       |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
