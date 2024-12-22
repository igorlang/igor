.. _json_attributes:

*******************************
JSON Serialization Attributes
*******************************

Here's the list of attributes that affect JSON serialization. They should be properly supported by all target languages.

+---------------------+-----------------+-------------+------------------+----------------------------------+--------------+
| Attribute           | Type            | Inheritance | Target           | Description                      | Default      |
+=====================+=================+=============+==================+==================================+==============+
| json.enabled        | **bool**        | scope       | | module,        | | generate JSON serialization    | ``false``    |
|                     |                 |             | | type           | | code for target language       |              |
|                     |                 |             |                  | | See :ref:`enable_serialization`|              |
+---------------------+-----------------+-------------+------------------+----------------------------------+--------------+
| json.ignore         | **bool**        |             | | record field   | | exclude record field from JSON | ``false``    |
|                     |                 |             |                  | | serialization                  |              |
+---------------------+-----------------+-------------+------------------+----------------------------------+--------------+
| json.field_notation | :ref:`Notation` | scope       | | module,        | | Notation used for name         | ``none``     |
|                     |                 |             | | type,          | | translation of record and      |              |
|                     |                 |             | | record field   | | enum field names into JSON     |              |
|                     |                 |             | | enum field     | | strings                        |              |
+---------------------+-----------------+-------------+------------------+----------------------------------+--------------+
| json.notation       | :ref:`Notation` | scope       | | module,        | | Notation used for name         | ``none``     |
|                     |                 |             | | type,          | | translation of service         |              |
|                     |                 |             | | service        | | functions and exception names  |              |
|                     |                 |             |                  | | into JSON strings              |              |
+---------------------+-----------------+-------------+------------------+----------------------------------+--------------+
| json.key            | **string**      |             | | enum field,    | | Override record and enum field |              |
|                     |                 |             | | record field   | | names (and ignore notation)    |              |
+---------------------+-----------------+-------------+------------------+----------------------------------+--------------+
| json.number         | **bool**        |             | | module         | | Serialize enums as numbers     | ``false``    |
|                     |                 |             | | enum           | | instead of strings             |              |
+---------------------+-----------------+-------------+------------------+----------------------------------+--------------+
| json.nulls          | **bool**        | scope       | | module         | | Include unset optional field   |              |
|                     |                 |             | | record         | | values as nulls                |              |
+---------------------+-----------------+-------------+------------------+----------------------------------+--------------+
| patch_record        | **bool**        |             | record           | See :ref:`patch_records`         | ``false``    |
+---------------------+-----------------+-------------+------------------+----------------------------------+--------------+
