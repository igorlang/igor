*******************************
   UE4 Generator Attributes
*******************************

Here's the list of attributes that affect UE4 code generation.

.. seealso::
  - :ref:`json_attributes`
  - :ref:`binary_attributes`

+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| Attribute                 | Type            | Inheritance | Target           | Description                      | Default              |
+===========================+=================+=============+==================+==================================+======================+
| enabled                   | **bool**        | scope       | | module,        | | Enables code generation for    | ``true``             |
|                           |                 |             | | type           | | the module or type             |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| igor_path                 | **string**      | scope       | module           | | Path to the Igor runtime       |                      |
|                           |                 |             |                  | | library                        |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| h_file                    | **string**      |             | module           | Override generated h file name   |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| cpp_file                  | **string**      |             | module           | Override generated cpp file name |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| h_path                    | **string**      | scope       | module           | Header output folder             |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| cpp_path                  | **string**      | scope       | module           | Cpp output folder                |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| namespace                 | **string**      | scope       | | module,        | | C++ namespace                  |                      |
|                           |                 |             | | type           | | Nested namespaces should be    |                      |
|                           |                 |             |                  | | separated with ``::``          |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| prefix                    | **string**      | scope       | type             | Type name project prefix         |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| name                      | **string**      |             | | module,        | Override generated name.         |                      |
|                           |                 |             | | type,          |                                  |                      |
|                           |                 |             | | record field   |                                  |                      |
|                           |                 |             | | enum field     |                                  |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| base_type                 | **string**      |             | | record,        | | Ancestor type for the struct   |                      |
|                           |                 |             | | variant        | | or class                       |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| alias                     | **string**      |             | type             | | Provide existing target type.  |                      |
|                           |                 |             |                  | | Do not generate type           |                      |
|                           |                 |             |                  | | definition                     |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| typedef                   | **string**      |             | alias type       | | Generate typedef declaration   | ``true``             |
|                           |                 |             |                  | | for the alias type             |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| ptr                       | **bool**        |             | record           | | Use ``TSharedPtr`` to store    | ``true``             |
|                           |                 |             |                  | | values (e.g. record fields)    |                      |
|                           |                 |             |                  | | of this type.                  |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| interfaces                | **bool**        | scope       | | module         | | Generate abstract classes for  | ``false``            |
|                           |                 |             | | interface      | | interfaces (not compatible     |                      |
|                           |                 |             |                  | | with ustructs)                 |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| uenum                     | **bool**        |             | enum             | Define enum as ``UENUM``         | ``false``            |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| ustruct                   | **bool**        |             | record           | Define record as ``USTRUCT``     | ``false``            |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| uclass                    | **bool**        |             | record           | Define record as ``UCLASS``      | ``false``            |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| blueprint_type            | **bool**        |             | | record         | | ``BlueprintType`` specifier    | ``false``            |
|                           |                 |             | | enum           | | for ``USTRUCT``, ``UCLASS`` or |                      |
|                           |                 |             |                  | | ``UENUM``.                     |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| uproperty                 | **bool**        | scope       | record field     | | Define record field  as        | ``false``            |
|                           |                 |             |                  | | ``UPROPERTY``.                 |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| h_include                 | **string**      |             | module           | | Include an extra header to     |                      |
|                           |                 |             |                  | | a generated h file. Multiple   |                      |
|                           |                 |             |                  | | attribute values can be set.   |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| cpp_include               | **string**      |             | module           | | Include an extra header to     |                      |
|                           |                 |             |                  | | a generated cpp file. Multiple |                      |
|                           |                 |             |                  | | attribute values can be set.   |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| log_category              | **string**      | scope       | module           | Log category for UE_LOG macros   | ``"LogTemp"``        |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| http.base_url             | **string**      |             | webservice       | Default HTTP client base url     |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| http.client.lazy          | **bool**        | scope       | | webservice     | | Don't process HTTP request     | ``false``            |
|                           |                 |             | | webresource    | | immediately but allow a user   |                      |
|                           |                 |             |                  | | to setup request first.        |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| http.client.content_setup | | ``args``      | scope       | | webservice     | | How HTTP client request        | ``args``             |
|                           | | ``request``   |             | | webresource    | | content is set up by a user.   |                      |
|                           |                 |             |                  | | See :ref:`http_client_setup`.  |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| http.client.path_setup    | | ``args``      | scope       | | webservice     | | How HTTP client request path   | ``args``             |
|                           | | ``request``   |             | | webresource    | | params are set up by a  user.  |                      |
|                           |                 |             |                  | | See :ref:`http_client_setup`.  |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| http.client.query_setup   | | ``args``      | scope       | | webservice     | | How HTTP client request query  | ``args``             |
|                           | | ``request``   |             | | webresource    | | params are set up by a user.   |                      |
|                           |                 |             |                  | | See :ref:`http_client_setup`.  |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| http.client.header_setup  | | ``args``      | scope       | | webservice     | | How HTTP client request        | ``args``             |
|                           | | ``request``   |             | | webresource    | | headers are set up by a user.  |                      |
|                           |                 |             |                  | | See :ref:`http_client_setup`.  |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+
| http.client.setup         | | ``args``      |             | web variable     | | Override how HTTP client       | ``args``             |
|                           | | ``request``   |             |                  | | request variable is set up.    |                      |
|                           |                 |             |                  | | See :ref:`http_client_setup`.  |                      |
+---------------------------+-----------------+-------------+------------------+----------------------------------+----------------------+

The following attributes enable generation of UE4 ``UPROPERTY`` specifiers. 
They are supported for record fields and have **scope** inheritance.

+-----------------------+-----------------+-------------------------------+
| Attribute             | Type            | UE4 UPROPERTY specifier       |
+=======================+=================+===============================+
| blueprint_read_write  | **bool**        | BlueprintReadWrite            |
+-----------------------+-----------------+-------------------------------+
| blueprint_read_only   | **bool**        | BlueprintReadOnly             |
+-----------------------+-----------------+-------------------------------+
| edit_anywhere         | **bool**        | EditAnywhere                  |
+-----------------------+-----------------+-------------------------------+
| edit_defaults_only    | **bool**        | EditDefaultsOnly              |
+-----------------------+-----------------+-------------------------------+
| visible_anywhere      | **bool**        | VisibleAnywhere               |
+-----------------------+-----------------+-------------------------------+
| visible_defaults_only | **bool**        | VisibleDefaultsOnly           |
+-----------------------+-----------------+-------------------------------+
| category              | **string**      | Category                      |
+-----------------------+-----------------+-------------------------------+
