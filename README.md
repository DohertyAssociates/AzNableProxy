## Overview
This is a proxy service that allows user to retrieve information from N-Central without having complete API access.

### Backwards Compatibility
This is a complete re-write in C# with the original having been built in PowerShell. For backwards compatibility, the `Get` command can still be used and functions the same.

## API
### `GET` /api/sites
This will retrieve all sites in N-Central. Note that it does not return the Azure Tenant GUID even though it is returned as a property. The response will be a JSON object that looks like the following: 

```json
[
    {
        "name": "Example 1",
        "id": 2,
        "parentId": 1,
        "registrationToken": "21fcf0e6-5a38-fb4c-46dc-3a0fab76dfb5",
        "azureTenantId": ""
    },
    {
        "name": "Example 2",
        "id": 3,
        "parentId": 1,
        "registrationToken": "4dbc0e52-aed8-5a68-5006-584d7b5eaff9",
        "azureTenantId": ""
    },
]
```

### `GET` /api/sites/{id}
This will retrieve a specific site from N-Central. You must replace with `{id}` with an actual site id. This can be retrieved using the `/api/sites` or `/api/search/customer` functions. The response from this will be a JSON object including the Azure Tenant GUID if one is set. A 404 is returned if the site cannot be found.

```json
{
    "name": "Example 1",
    "id": 2,
    "parentId": 1,
    "registrationToken": "21fcf0e6-5a38-fb4c-46dc-3a0fab76dfb5",
    "azureTenantId": "a19f37b4-34ca-4526-b84c-b53a3815ba0c"
}
```

### `GET` /api/search/azuretenantguid
This will return an N-Central site object where the Azure Tenant GUID matches the one provided. This is an HTTP GET operation and expects a JSON object as its body. The body should be in the following format:
```json
{
	"azureTenantGuid":  "a19f37b4-34ca-4526-b84c-b53a3815ba0c",
	"customerWildcard":  "Bowmark",
	"excludedSites":  [
		"Barrick",
		"Doherty"
	]
}
```
- `azureTenantGuid`: This is required as is the Azure Tenant GUID you are looking for.
- `customerWildcard`: If you know the name of the site, or the rough name, you can provide a string to get a quicker response. If a site is not found it will retry the search without the wildcard automatically. This is parameter is optional. 
- `excludedSites`: You can optionally pass a string array to explicitly exclude sites to search for. Doing this will result in a quick response. 

The response is a JSON site object or a 404 if no site could be find with the provided Azure Tenant GUID.
```json
{
    "name": "Example 1",
    "id": 2,
    "parentId": 1,
    "registrationToken": "21fcf0e6-5a38-fb4c-46dc-3a0fab76dfb5",
    "azureTenantId": "a19f37b4-34ca-4526-b84c-b53a3815ba0c"
}
```

### `GET` /api/search/customer?name={siteName}
This will return N-Central site objects where the supplied name matches a site in N-Central.The response is a JSON site array or a 404 if no sites could be found. The Azure Tenant Id will be returned if one has been assigned to a site. 

```json
[
    {
        "name": "Example 1",
        "id": 2,
        "parentId": 1,
        "registrationToken": "21fcf0e6-5a38-fb4c-46dc-3a0fab76dfb5",
        "azureTenantId": "a19f37b4-34ca-4526-b84c-b53a3815bb0c"
    },
    {
        "name": "Example 2",
        "id": 3,
        "parentId": 1,
        "registrationToken": "4dbc0e52-aed8-5a68-5006-584d7b5eaff9",
        "azureTenantId": "c19f37b4-34fa-4526-b84c-b53a3815ba0c"
    },
]
```