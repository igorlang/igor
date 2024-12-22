.. _hercules:

******************
    Hercules
******************

*Hercules* is the data editor tool that can edit JSON documents stored in CouchDB database. Hercules uses :ref:`schema` to
display typed editors and validate data. The schema is stored in CouchDB as well, in the document with ``schema`` id.

Document Id
===========

In CouchDB, all JSON documents have the string ``_id`` field, that contains the unique name of the document. In Hercules
users are encouraged to give documents meaningful names. These ids are also used as :ref:`schema_keys`.

*Example:*

.. code-block:: igor

    [schema root]
    variant Card
    {
        tag CardCategory category;
        [* json.key="_id"]
        [schema ignore]
        atom id;
    }

.. note::

    ``[schema ignore]`` attribute on ``id`` field disallows user to directly edit document id. Instead, *Hercules* provides the
    **Rename Document** menu command that can correctly edit document references (keys) stored by other documents.

.. _upload_schema:

Uploading Schema
================

After generating ``schema.json`` file it can be uploaded to CouchDB using *curl* command line utility.

.. code-block:: batch

    bin\curl -X POST --data-urlencode schema@schema.json https://%USERNAME%:%PASSWORD%@%HOST%/%DATABASE%/_design/tools/_update/update_schema/schema

Similarly, ``diagram_schema.json`` can be uploaded using the following command:

.. code-block:: batch

    bin\curl -X POST --data-urlencode schema@diagram_schema.json https://%USERNAME%:%PASSWORD%@%HOST%/%DATABASE%/_design/tools/_update/update_schema/diagram_schema

These *POST* requests relies on the presense of ``_design/tools`` CouchDB tool document. You have to create this document first:

.. code-block:: json

    {
        "_id": "_design/tools",
        "language": "javascript",
        "updates": {
            "update_schema": "function(doc, req) { var schema=JSON.parse(req.form.schema); for(var attr in schema) {doc[attr] = schema[attr];} return [doc, 'ok']; }"
        }
    }

Hercules Manual
===============

More information on *Hercules* may be found in the Hercules Manual, available from *Hercules* **Help** menu.



