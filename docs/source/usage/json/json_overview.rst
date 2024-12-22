********************
    Overview
********************

Enable JSON serialization
=========================

JSON serialization is controlled by attributes, starting with ``json.`` namespace.

For example, JSON serialization may be enabled for module or type using ``json.enabled`` attribute.
See :ref:`enable_serialization` for more details.

See :ref:`json_attributes` for the full list of JSON attributes that are not target-specific.

.. warning::

   JSON imposes limitations on types it can serialize. In particular, **dict** types can be serialized to JSON only if
   **integer**, **string** and **enum** types (or other types that have bulit-in or custom string serialization)
   are used as dict keys.

JSON Data
=========

JSON serialization in generated code is performed from Igor data objects to target-specific JSON data, not to JSON string.
It is up to the user to decide how to format JSON to string, and to parse string to JSON data for further deserialization.

.. note::
    
    That said, in some cases, e.g. for HTTP resources with ``application/json`` content type, JSON data <-> string conversion may 
    be performed by generated code.

Modelling Existing JSON Layout
==============================

JSON value may belong to one of following types: *string*, *number*, *object*, *array*, and special values: *true*, *false* and *null*. 
The following table illustrates which Igor types you can choose when modelling existing JSON layout.

+----------------+---------------------+-----------------------------------------------------------------------+
| JSON Type      | Igor Type           | Comment                                                               |
+================+=====================+=======================================================================+
| string         | | **string**        | | **enum** may be a prefered choice when string belongs to the fixed  |
|                | | **atom**          | | set of known values                                                 |
|                | | **enum**          |                                                                       |
+----------------+---------------------+-----------------------------------------------------------------------+
| number         | | **float**         | | Although JSON standard doesn't distingush integer and float types,  |
|                | | **double**        | | many real world implementations do so. You may use integer types    |
|                | | **int** and other | | to limit constraints                                                |
|                | | integer types     |                                                                       |
+----------------+---------------------+-----------------------------------------------------------------------+
| | **true**     | | **bool**          |                                                                       |
| | **false**    |                     |                                                                       |
+----------------+---------------------+-----------------------------------------------------------------------+
| array          | | **list<T>**       | | **flags** may be used if T is **enum**                              |
|                | | **flags<T>**      |                                                                       |
+----------------+---------------------+-----------------------------------------------------------------------+
| object         | | **dict<K,V>**     | | Use **dict** if object keys are all of the same type and records    |
|                | | **record**        | | if they are of different types, but the set of names if fixed       |
|                | | **variant**       | | and known.                                                          |
|                |                     | | Use variants if there's a single tag field that defines             |
|                |                     | | which set of other fields is used.                                  |
+----------------+---------------------+-----------------------------------------------------------------------+
| **null**       | **?T**              | If null is a valid value, use optional type to denote that.           |
+----------------+---------------------+-----------------------------------------------------------------------+

Unset Values vs null
=====================

Optional record fields
----------------------

For optional record fields there's no difference if they are not included into JSON object at all or set to **null**.

When deserializing, optional field value is set to **null** no matter if JSON object contained **null** value for the field key,
ot the key was absent.

.. note::

   There's one common scenario when it's convenient to distinguish between **null** and absent values - *update* records, 
   which contain requested changes to some data source. Then absent value would mean lack of update information, and
   **null** value would mean reset/removal request. This scenario is not supported at the moment.

Collections
-----------

If optional type is used as generic collection type argument (e.g. **?T** in **list<?T>** or **?TValue** in **dict<TKey,?TValue>**)
then **null** value should be present in JSON to be included into collection. **null**  is both properly serialized and deserialized.

Optional **dict** keys are not supported.
