*******************************
   UE4 HTTP Client
*******************************

Overview
=========

Generated HTTP client uses standard UE4 HTTP module.

.. _http_client_setup:

HTTP Client Request Setup
==========================

There's a number of attributes that control how a user can set up HTTP requests.

``http.cilent.lazy`` attribute controls if HTTP request is processed immediately after creation, or a user is given a chance to setup request before processing.

If ``http.client.lazy`` is ``true``, the user should call ``ProcessRequest`` when a request is ready.

**Igor**:

.. code-block:: Igor
    
    [ue4 http.client.lazy http.client.query_setup=request]
    Search => GET /assets?lang={?string lang}&start={?int start}&count={?int count}&sortBy={?string effectiveDate}&sortDir={?SortDir sortDir}&category={?string category} -> TResult<SearchResult>;

**C++**:

.. code-block:: c++

   	auto SearchRequest = MarketplaceService->Search();
	SearchRequest->Count = 50;
	SearchRequest->OnComplete().AddUObject(this, &AIgorHttpGameMode::OnSearchResponse);
	SearchRequest->ProcessRequest();

The example above also uses ``http.client.query_setup=request`` attribute to specify that query variables are supposed to be set up 
using a request object rather than HTTP service client function arguments. The following attribute values are supported:

* ``args`` is the default. Variables are passed using service client function arguments (``Search()`` in the example above).
* ``request`` means that variables are set up using a request object. It must be used together with ``http.client.lazy``.
  
The same way ``http.client.query_setup`` attribute is used for query parameters, ``http.client.content_setup``, ``http.client.path_setup`` and ``http.client.header_setup`` 
may be used to control setup of content, path and header variables. All those attributes use scope inheritance and provide default behaviour for all resource variables.

``http.client.setup`` may be used to override default behaviour for a single variable.






