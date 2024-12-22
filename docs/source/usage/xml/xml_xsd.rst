********************
     XSD Schema
********************

Generating XSD Schema
=====================

You can use :ref:`compiler` to generate XSD schema from the set of Igor files using the following command:

.. code-block:: batch

   igorc.exe -xsd *.igor

This command will generate ``schema.xsd`` file in the current folder. You can override file name with ``-output-file``
command line option. See more information on available command line options here: :ref:`cli_options`.

XSD Schema Attributes
=====================

See :ref:`xml_attributes` for the list of XML related attributes.

Here's the additional list of attributes that affect XSD Schema generation.

+-------------------+-----------------+-------------+------------------+----------------------------------+----------------+
| Attribute         | Type            | Inheritance | Target           | Description                      | Default        |
+===================+=================+=============+==================+==================================+================+
| xsd.xs_type       | **string**      |             | type             | | Provide existing xs type name. |                |
|                   |                 |             |                  | | E.g. ``xs:date``               |                |
+-------------------+-----------------+-------------+------------------+----------------------------------+----------------+
| xsd.name          | **string**      |             | type             | | Override name for generated    |                |
|                   |                 |             |                  | | XSD type                       |                |
+-------------------+-----------------+-------------+------------------+----------------------------------+----------------+
| xsd.notation      | :ref:`Notation` | scope       | type             | | Notation used for name         |                |
|                   |                 |             |                  | | translation of XSD type names  |                |
+-------------------+-----------------+-------------+------------------+----------------------------------+----------------+
