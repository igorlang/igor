.. _diagram_schema_attributes:

*******************************
   Diagram Schema Attributes
*******************************

Here's the list of attributes that affect *Diagram Schema* generation. Use ``diagram`` target for all of them.

+----------------+-----------------+-------------+------------------+------------------------------------+----------------+
| Attribute      | Type            | Inheritance | Target           | Description                        | Default        |
+================+=================+=============+==================+====================================+================+
| enabled        | **bool**        |             | record (card)    | | Enables diagram view for         | ``false``      |
|                |                 |             |                  | | the record                       |                |
+----------------+-----------------+-------------+------------------+------------------------------------+----------------+
| name           | **string**      |             | record (block)   | | Override block name              |                |
|                |                 |             |                  | | See :ref:`diagram_customization` |                |
+----------------+-----------------+-------------+------------------+------------------------------------+----------------+
| caption        | **string**      |             | record (block)   | | Block caption                    |                |
|                |                 |             |                  | | See :ref:`diagram_customization` |                |
+----------------+-----------------+-------------+------------------+------------------------------------+----------------+
| archetype      | **string**      | ``variant`` | record (block)   | | Block archetype                  |                |
|                |                 |             |                  | | See :ref:`diagram_customization` |                |
+----------------+-----------------+-------------+------------------+------------------------------------+----------------+
| icon           | **string**      | ``variant`` | record (block)   | | Block icon                       |                |
|                |                 |             |                  | | See :ref:`diagram_customization` |                |
+----------------+-----------------+-------------+------------------+------------------------------------+----------------+
| show_icon      | **bool**        | ``variant`` | record (block)   | | Display icon on block            | ``true``       |
|                |                 |             |                  | | See :ref:`diagram_customization` |                |
+----------------+-----------------+-------------+------------------+------------------------------------+----------------+
| color          | **string**      | ``variant`` | record (block)   | | Block color                      |                |
|                |                 |             |                  | | See :ref:`diagram_customization` |                |
+----------------+-----------------+-------------+------------------+------------------------------------+----------------+
| special_field  | **string**      | ``type``    | record field     | | Mark block field as special      |                |
|                |                 |             |                  | | See :ref:`diagram_customization` |                |
+----------------+-----------------+-------------+------------------+------------------------------------+----------------+
| connector      | Connector       | ``variant`` | record (block)   | Define block connector             |                |
+----------------+-----------------+-------------+------------------+------------------------------------+----------------+
| connector      | Connector       |             | record field     | Mark field as asset connector      |                |
+----------------+-----------------+-------------+------------------+------------------------------------+----------------+

The following attributes are supporte by **connector** object attribute:

+----------------+-----------------+-------------------------------------+-----------------------------+
| Attribute      | Type            |  Description                        | Default                     |
+================+=================+===================================================================+
| type           | | ``in``        | | Connector type                    | | ``asset`` on records,     |
|                | | ``out``       | | See :ref:`diagram_connectors`     | | ``property`` on fields    |
|                | | ``property``  |                                     |                             |
|                | | ``asset``     |                                     |                             |
+----------------+-----------------+-------------------------------------+-----------------------------+
| name           | **string**      | | Name of the connector,            |                             |
|                |                 | | stored in  anchor links           |                             |
+----------------+-----------------+-------------------------------------+-----------------------------+
| caption        | **string**      | Connector name displayed in UI      | Connector name              |
+----------------+-----------------+-------------------------------------+-----------------------------+
| position       | **string**      | | Connector position on the block - |                             |
|                |                 | | comma separated coordinates.      |                             |
|                |                 | | See :ref:`diagram_connectors`     |                             |
+----------------+-----------------+-------------------------------------+-----------------------------+
| category       | **string**      | | Connector category,               |                             |
|                |                 | | stored in  anchor links           |                             |
|                |                 | | See :ref:`connector_categories`   |                             |
+----------------+-----------------+-------------------------------------+-----------------------------+
| color          | **string**      | | Connector color                   |                             |
|                |                 | | See :ref:`diagram_customization`  |                             |
+----------------+-----------------+-------------------------------------+-----------------------------+
