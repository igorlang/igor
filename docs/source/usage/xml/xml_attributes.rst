.. _xml_attributes:

*******************************
 XML Serialization Attributes
*******************************

Here's the list of attributes that affect XML serialization. They should be properly supported by all target languages.

+------------------------+-----------------+-------------+------------------+------------------------------------+--------------+
| Attribute              | Type            | Inheritance | Target           | Description                        | Default      |
+========================+=================+=============+==================+====================================+==============+
| xml.enabled            | **bool**        | scope       | | module,        | | generate XML serialization       | ``false``    |
|                        |                 |             | | type           | | code for target language         |              |
|                        |                 |             |                  | | See :ref:`enable_serialization`  |              |
+------------------------+-----------------+-------------+------------------+------------------------------------+--------------+
| xml.ordered            | **bool**        | scope       | | record         | | whether serialized record fields | ``false``    |
|                        |                 |             |                  | | are required to be in strict     |              |
|                        |                 |             |                  | | order or not                     |              |
+------------------------+-----------------+-------------+------------------+------------------------------------+--------------+
| xml.ignore             | **bool**        |             | | record field   | | exclude record field from XML    | ``false``    |
|                        |                 |             |                  | | serialization                    |              |
+------------------------+-----------------+-------------+------------------+------------------------------------+--------------+
| xml.attribute          | **bool**        |             | | record field   | serialize field as XML attribute   | ``false``    |
+------------------------+-----------------+-------------+------------------+------------------------------------+--------------+
| xml.text               | **bool**        |             | | record field   | | serialize value as XML element   | ``false``    |
|                        |                 |             | | union clause   | | text                             |              |
+------------------------+-----------------+-------------+------------------+------------------------------------+--------------+
| xml.content            | **bool**        |             | | record field   | | serialize value as XML element   | ``false``    |
|                        |                 |             | | union clause   | | content (without enclosing tag)  |              |
+------------------------+-----------------+-------------+------------------+------------------------------------+--------------+
| xml.notation           | :ref:`Notation` | scope       | | record field   | | Notation used for name           | ``none``     |
|                        |                 |             | | enum field     | | translation of record and enum   |              |
|                        |                 |             |                  | | field names into XML strings     |              |
+------------------------+-----------------+-------------+------------------+------------------------------------+--------------+
| xml.enum_notation      | :ref:`Notation` | scope       | enum field       | | Notation used for name           | xml.notation |
|                        |                 |             |                  | | translation of enum values       |              |
+------------------------+-----------------+-------------+------------------+------------------------------------+--------------+
| xml.element_notation   | :ref:`Notation` | scope       | any              | | Notation used for name           | xml.notation |
|                        |                 |             |                  | | translation of element names     |              |
+------------------------+-----------------+-------------+------------------+------------------------------------+--------------+
| xml.attribute_notation | :ref:`Notation` | scope       | any              | | Notation used for name           | xml.notation |
|                        |                 |             |                  | | translation of attribute names   |              |
+------------------------+-----------------+-------------+------------------+------------------------------------+--------------+
