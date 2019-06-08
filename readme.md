# Cloud Cache Owin Example

This example shows how to use Pivotal Cloud Cache using C#.   I have tried to make it as simple and as few moving parts as possible.   Hopefully this makes easier to understand how to get started with Cloud Cache and .Net.

You can watch a video walkthrough including pushing to Cloud Foundary over on you tube:  https://youtu.be/0wSOVdNdQOc
# What does the service do?

I used Microsoft OWIN to make a simple Book CRUD Book Rest service.    That Rest service is backed by Cloud Cache.   

For serializing the Book domain model we are using a Cloud Cache technology called PDX.   PDX used Reflection to serialze the obejct binary format.   The added bonus is the server can read elements with in the PDX structire.  This becomes helpful when querying the data, the server doesn't have to deserialize the entire object just the elements needed to satify the query.


## Rest API
List all of the ISDNs in the database:
```
curl -X GET \
  https://cloudcache-owin-example.apps.pcfone.io/api/book \
  -H 'Content-Type: application/json' 
```
Add a book to the database:
```
curl -X PUT \
  https://cloudcache-owin-example.apps.pcfone.io/api/book \
  -H 'Content-Type: application/json' \
  -d '{"FullTitle":	"The Shining",
"ISBN":	"0525565329",
"MSRP":	"9.99",
"Publisher":	"Anchor",
"Authors":	"Stephen King"
}'
```
Get a book by ISBN:
```
curl -X GET \
  'https://cloudcache-owin-example.apps.pcfone.io/api/book?isbn=0525565329' \
  -H 'Content-Type: application/json' 
  ```
Bulk add books to the database:
```
curl -X POST \
  https://cloudcache-owin-example.apps.pcfone.io/api/book \
  -H 'Content-Type: application/json' \
  -d '[{"FullTitle":	"The Shining",
"ISBN":	"0525565329",
"MSRP":	"9.99",
"Publisher":	"Anchor",
"Authors":	"Stephen King"
}, 
{"Authors": "Ernest Cline",
"FullTitle": "Ready Player One",
"ISBN": "3596296595",
"MSRP": "0.00",
"Publisher":	"	Fischer Tor"}
]
'
```
Delete a book by ISBN:
```
curl -X DELETE \
  'https://cloudcache-owin-example.apps.pcfone.io/api/book?isbn=3596296595' 
````
# Project Dependancies:

## NuGet
* Microsoft.AspNet.WebApi.OwinSelfHost
* JSONPath

## Download from Pivotal Network
* Pivotal GemFire Native Client 10.X

# Building

Pivotal GemFire only supports 64 bit applications.   So ensure when building the application that you are targeting a 64-bit platform.

The provided cloud foundary manifest assumes that the application will also be built as a release configuration.

# Deploy

1 -  Create a cloud cache instance in Pivotal Cloud Foundary.   The manifest assumes that the cloud cache instance is called `pcc-dev`.
```
$ cf create-service p-cloudcache dev-plan pcc-dev
```
2 - Create the service key 
```
$ cf create-service-key pcc-dev pcc-dev_service_key
```
3 - Using the GemFire command line tool ``gfsh`` create the region with the policy that we want for the data.

```
voltron:gemfire cblack$ gfsh
    _________________________     __
   / _____/ ______/ ______/ /____/ /
  / /  __/ /___  /_____  / _____  / 
 / /__/ / ____/  _____/ / /    / /  
/______/_/      /______/_/    /_/  

Monitor and Manage Pivotal GemFire
gfsh>connect --use-http=true --url=https://somehost/gemfire/v1 --user=cluster_operator --password=*****

Successfully connected to: GemFire Manager HTTP service @ https://somehost/gemfire/v1
gfsh>create region --name=owinexample --type=PARTITION
                     Member                      | Status
------------------------------------------------ | -----------------------------------------------------------------------------------
cacheserver-7541bb25-71b2-4ae7-ad80-9d518e18facd | Region "/owinexample" created on "cacheserver-7541bb25-71b2-4ae7-ad80-9d518e18facd"

Cluster-0 gfsh>exit
```
Note: The version of Cloud Cache I am using is backed by GemFire version 9.7.1 - make sure you are using the same versions of GemFire as your cloud cache instance.

4 -`cf push` the application and give it a try using postman or curl.
