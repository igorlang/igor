*******************************
TypeScript Generator Attributes
*******************************

Here's the list of attributes that affect TypeScript code generation.

.. seealso::
  - :ref:`json_attributes`

+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| Attribute      | Type            | Inheritance | Target           | Description                      | Default        |
+================+=================+=============+==================+==================================+================+
| enabled        | **bool**        | scope       | | module,        | | Enables code generation for    | ``true``       |
|                |                 |             | | type           | | the module or type             |                |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| file           | **string**      |             | | module         | Override generated file name     |                |
|                |                 |             | | webservice     |                                  |                |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| name           | **string**      |             | | module,        | | Override generated name        |                |
|                |                 |             | | type,          |                                  |                |
|                |                 |             | | record field   |                                  |                |
|                |                 |             | | enum field     |                                  |                |
|                |                 |             | | webservice     |                                  |                |
|                |                 |             | | web resource   |                                  |                |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| namespace      | **string**      |             | module           | | Override namespace that is     |                |
|                |                 |             |                  | | used when importing module     |                |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| field_notation | :ref:`Notation` | scope       | record field     | | Notation used for name         | ``lowerCamel`` |
|                |                 |             |                  | | translation of record          |                |
|                |                 |             |                  | | field names.                   |                |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| setup_ctor     | **bool**        | scope       | record           | | Generate setup constructor     | ``false``      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| parameter      | **bool**        | scope       | record field     | | Declare field as the           | ``false``      |
|                |                 |             |                  | | constructor parameter          |                |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| public         | **bool**        | scope       | record field     | Field is declared as public      | ``false``      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| private        | **bool**        | scope       | record field     | Field is declared as private     | ``false``      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| readonly       | **bool**        | scope       | record field     | Field is declared as readonly    | ``false``      |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| alias          | **string**      |             | type             | | Provides the alias type name.  |                |
|                |                 |             |                  | | Type declaration won't be      |                |
|                |                 |             |                  | | generated and alias type is    |                |
|                |                 |             |                  | | used instead.                  |                |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| json.serializer| **string**      |             | type             | | Provides the user-provided JSON|                |
|                |                 |             |                  | | serializer name. Serializer    |                |
|                |                 |             |                  | | won't be generated.            |                |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
| error_message  | **bool**        |             | exception field  | | This field contains error      | ``false``      |
|                |                 |             |                  | | message.                       |                |
|                |                 |             |                  | | See :ref:`ts_error_message`    |                |
+----------------+-----------------+-------------+------------------+----------------------------------+----------------+
