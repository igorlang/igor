.. _binary_attributes:

*******************************
Binary Igor Protocol Attributes
*******************************

Here's the list of attributes that affect binary igor protocol serialization. They should be properly supported by all target languages.

+----------------+---------------+-------------+------------------+----------------------------------+--------------+
| Attribute      | Type          | Inheritance | Target           | Description                      | Default      |
+================+===============+=============+==================+==================================+==============+
| int_type       | | integer     |             | enum             | See :ref:`enum_integer_type`     |              |
|                | | type        |             |                  |                                  |              |
+----------------+---------------+-------------+------------------+----------------------------------+--------------+
| binary.header  | **bool**      |             | record           | | See :ref:`record_encoding`     | ``true``     |
+----------------+---------------+-------------+------------------+----------------------------------+--------------+
| binary.enabled | **bool**      | scope       | | module,        | | generate binary protocol       | ``false``    |
|                |               |             | | type           | | code for target language       |              |
|                |               |             |                  | | See :ref:`enable_serialization`|              |
+----------------+---------------+-------------+------------------+----------------------------------+--------------+
| binary.ignore  | **bool**      |             | | record field   | | exclude record field from      | ``false``    |
|                |               |             |                  | | binary serialization           |              |
+----------------+---------------+-------------+------------------+----------------------------------+--------------+
