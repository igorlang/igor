.. _schema_meta:

*******************************
      Schema Metadata
*******************************

You can use ``meta`` attribute to provide custom metadata values for Igor entities (types and fields). 
Those values may be used by programs that use the generated schema.

*Example:*

.. code-block:: igor

    [schema meta=(unreal_class_path record_caption)]
    string string_value;

.. warning::

      Igor doesn't try to validate metadata and pastes it as is into generated schema.

Hercules Metadata
=================

Consult Hercules documentation for the list of meta attributes supported by Hercules.
