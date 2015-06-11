# Elvanto API .NET Library

This library is all set to go with version 1 of the <a href="https://www.elvanto.com/api/" target="_blank">Elvanto API</a>.


## Authenticating

The Elvanto API supports authentication using either <a href="https://www.elvanto.com/api/getting-started/#oauth" target="_blank">OAuth 2</a> or an <a href="https://www.elvanto.com/api/getting-started/#api_key" target="_blank">API key</a>.

### What is This For?

* Quick summary
This is an API wrapper to use in conjunction with an Elvanto account. This wrapper can be used by developers to develop programs for their own churches, or to design integrations to share to other churches using OAuth authentication.
* Version 1.0.0

### Installation

#### Via Package Manager Console

Enter the following command in the Package Manager Console

```
Install-Package ElvantoAPI
```

#### Via Nuget

Enter the following command in command prompt

```
nuget install ElvantoAPI
```

#### Downloading

* Download
* Navigate to folder you downloaded the source
* `csc /target:library ElvantoAPI.cs`
This will create a .DLL file for you to include in your projects.

## Usage

### For Authentication using an API Key:

First get your API key from Settings > Account Settings, then in the program:

```csharp
ElvantoAPI api = new ElvantoAPI(api_key);
```

### For Authentication via OAuth

Create an ElvantoAPI object

```csharp
ElvantoAPI api = new ElvantoAPI(client_id,client_secret);
```

#### For Webapps

Get the Authorization URL. For WebApps, the value of `isWebApp` will be set to True.

```csharp
string URL = api.AuthorizeUrl(redirect_uri,scope, isWebApp);
```

You will then want to point your users to this URL. After they have logged in, they will be redirected to the RedirectURL, with the following code in the URL, as follows.

```csharp
http://mywebapp.com/login/?code=string
```

The next step is to take this code, and get your access tokens. The code here is the code in the above URL.

```csharp
string json = api.GetTokens(code,redirect_uri);
```

This will return a dict object of the form:

```csharp
{
	"access_token": "e1e8422f68d8cf3c44b6e3d4beb065722abf",
	"expires_in": 1209600,
	"refresh_token": "6d59273f6fb7671bf1bb79ac81c63c12bc73633421"
}
```

You now want to store these tokens within the object.

```csharp
api.SetTokens(access_token,refresh_token,expires_in);
```

#### For Non-Webapps

Get the Authorization URL. For Non-WebApps, the value of `isWebApp` will be set to False.

```csharp
string URL = api.AuthorizeUrl(redirect_uri,scope, isWebApp);
```

Direct your users to this URL.

After the user has logged in, they will be sent back to the specified redirect_uri, with a code. Unlike the WebApp method
this code will be behind a hash.

```csharp
http://mynonwebapp.com/login/#code=string&expiresin=int
```

Once you have the code, you can set the tokens in the object.

```csharp
api.SetTokens(access_token,expires_in);
```

### Performing Calls

To perform a call, simply use the end point and any arguments required. The basic call is as follows:

```csharp
api.Call("endpoint","json_parameters")
```

All calls return a JSON string, with the results of the call.

Calls require the parameters of the arguments to be sent as a JSON string. You will need to serialize the information to send. You can use JavaScriptSeralizer to achieve this.

For example:
```csharp
JavaScriptSerializer jss = new JavaScriptSerializer();
string json = jss.Serialize(new { firstname = "Test", preferred_name = "Test", lastname = "User" });
api.Call("people/create", json);
```

An example `people/search` API call.

```csharp
JavaScriptSerializer jss = new JavaScriptSerializer();
string json = jss.Serialize(new { page = "1", page_size = "100", search = new { date_entered = "2015-01-01", volunteer = "yes" }, fields= new string[]{"gender", "birthday", "locations"}});
api.Call("people/search", json);

```

```csharp
{
    "status": "ok", 
    "generated_in": "0.035", 
    "people": {
        "on_this_page": 2, 
        "per_page": 1000, 
        "total": 2, 
        "page": 1, 
        "person": [
            {
                "username": "john.feeney", 
                "preferred_name": "", 
                "timezone": "", 
                "id": "7a411238-6fbc-11e0-bda8-de12be825216", 
                "archived": 0, 
                "family_id": "", 
                "family_relationship": "Other", 
                "last_login": "", 
                "email": "fee-ney-john@syllables.com", 
                "status": "Active", 
                "picture": "https://d5w68ic4qi8w5.cloudfront.net/assets/logo.png", 
                "school_grade": "", 
                "firstname": "John", 
                "lastname": "Feeney", 
                "phone": "", 
                "birthday": "1984-08-23", 
                "date_added": "2011-04-26 04:20:08", 
                "volunteer": 1, 
                "date_modified": "2015-02-26 05:07:06", 
                "admin": 0, 
                "country": "", 
                "mobile": "0456833923", 
                "contact": 0, 
                "category_id": "c37482a8-eb06-11e0-9229-ea942707ad51", 
                "deceased": 0
            }, 
            {
                "username": "john.hua", 
                "preferred_name": "", 
                "timezone": "", 
                "id": "7bcc31fa-6fbc-11e0-bda8-de12be825216", 
                "archived": 0, 
                "family_id": "", 
                "family_relationship": "Other", 
                "last_login": "", 
                "email": "johnhua@example.com", 
                "status": "Active", 
                "picture": "https://d5w68ic4qi8w5.cloudfront.net/assets/logo.png", 
                "school_grade": "12", 
                "firstname": "John", 
                "lastname": "Hua", 
                "phone": "", 
                "birthday": "1998-11-23", 
                "date_added": "2011-04-26 04:20:08", 
                "volunteer": 1, 
                "date_modified": "2015-02-26 05:07:06", 
                "admin": 0, 
                "country": "", 
                "mobile": "0451783968", 
                "contact": 0, 
                "category_id": "c37482a8-eb06-11e0-9229-ea942707ad51", 
                "deceased": 0
            }
        ]
    }
}
```

### Refreshing Tokens (Oauth)

Oauth tokens expire in time. As the response will throw an error when the tokens have expired, you can use the `.RefreshTokens()` to get new tokens as needed.

An example of how to do this, is as follows.

```csharp
try
{
	elvanto.Call("people/currentUser","");
}
	catch (WebException ex)
{
	HttpWebResponse webResponse = (HttpWebResponse)ex.Response;
	if (webResponse.StatusCode == HttpStatusCode.Unauthorized)
	{
		string json = elvanto.RefreshTokens();
		JavaScriptSerializer jss = new JavaScriptSerializer();
		Dictionary<string, string> tokens = jss.Deserialize<Dictionary<string, string>>(json);
		elvanto.SetTokens(tokens["access_token"], tokens["refresh_token"], tokens["expires_in"]);
		elvanto.Call("people/currentUser","");
	}
}
```


## Documentation

Documentation can be found on the <a href="https://www.elvanto.com/api/" target="_blank">Elvanto API website</a>.

## Updates

Follow our <a href="http://twitter.com/ElvantoAPI" target="_blank">Twitter</a> to keep up-to-date with changes in the API.

## Support

For bugs with the .NET API Wrapper please use the <a href="https://github.com/elvanto/api-dotnet/issues">Issue Tracker</a>.

For suggestions on the API itself, please <a href="http://support.elvanto.com/support/discussions/forums/1000123316" target="_blank">post in the forum</a> or contact us <a href="http://support.elvanto.com/support/tickets/new/" target="_blank">via our website</a>.