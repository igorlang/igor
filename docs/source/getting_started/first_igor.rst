**********************************
   Writing The First Igor File
**********************************

JSONPlaceholder (https://jsonplaceholder.typicode.com/) is a free online REST API that just returns fake data, and exists for testing purposes. 
Let's create an igor file that describes the JSONPlaceholder *posts* API.

Creating Igor File
====================

Use your favourite text editor to create a text file ``json_placeholder.igor`` with the following content:

.. code-block:: igor

   module JSONPlaceholder
   {
      [* json.enabled]
      record Post
      {
         int userId;
         int id;
         string title;
         string body;
      }

      webservice JSONPlaceholderService
      {
         GetPosts => GET /posts?userId={?int userId} -> list<Post>;
         GetPost => GET /posts/{int id} -> Post;
         PutPost => PUT /posts/{int id} Post -> Post;
      }
   }

Here we defined the JSONPlaceholder module. This module contains two definitions: *record* ``Post`` and *webservice* ``JSONPlaceholderService``.

A *record* is a set of named fields of different types. In this example record has integer and string fields. *int* and *string* are primitive built-in types.

A record declaration starts with an attribute ``[* json.enabled]``. ``*`` means that the attribute affects all possible targets (e.g. different target languages, not only C#),
and ``json.enabled`` attribute enables code generation for JSON serialization for the record ``Post``. ``Post`` is now a user defined type.

The next definition is a *webservice* ``JSONPlaceholderService``, which describes HTTP resources. Each resource is defined by a name (``GetPosts``, ``GetPost``, ``PutPost``), 
and declaration contains HTTP verb, request URI with variables, request content (if available) and response content.

Variables are defined inside curly brackets. Variable ``userId`` in GetPosts request is optional (type name starts with ``?``), and will be omitted if unset. 
User defined ``Post`` record is used for content type, as well as ``list<Post>`` type (list is built-in generic type matching JSON array). 

.. note::

   By default content encoding is JSON, but it can be overriden. For example ``Post as xml`` would use XML serialization instead, but then you need to enable xml serialization 
   for ``Post`` record using ``xml.enabled`` attribute.

Testing Igor Syntax
===================

We can test the correctness of our file by running the following command:

.. code-block:: batch

   igorc.exe json_placeholder.igor

If there're any errors, error messages will be displayed in the console output.





