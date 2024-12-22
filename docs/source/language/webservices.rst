.. _webservices:

******************
  Web Services
******************

Igor *webservices* are capable of describing HTTP services. From this description, HTTP client or HTTP server (or both) can be generated for the target language.

Syntax
======

.. code-block:: igor

    webservice WebserviceName
    {
        // request
        ResourceName => HttpVerb /path/parts?query1=value1&query2=value2 ~Header1:Header1Value Content as ContentType ->
            // 200 response
            200 OK: ~Header1:Header1Value Content as ContentType,
            // 404 response
            404 Not Found: ~HeaderI:HeaderIValue 404Content as ContentType,
            ...;
        ...
    }

where

* ``WebserviceName`` is the webservice name identifier
* ``ResourceName``  is the name of resource. It is used for generating HTTP client API and HTTP server callback function names
* ``HttpVerb``: ``GET``, ``PUT``, ``POST``, ``DELETE`` or ``PATCH``
* ``/path/parts?query1=value1&query2=value2`` are the URI path and query parts, that can contain variables
* ``HeaderX``, ``HeaderXValue`` are the optional header names and values (values can be variables)
* ``Content`` is the type name or variable definition of request content
* ``as ContentType`` is the optional content type specifier, e.g. **json**, **binary**, **xml** (**json** by default)
* ``200 OK`` or similar are the optional status code & phease (there may be multiple status codes supported)


