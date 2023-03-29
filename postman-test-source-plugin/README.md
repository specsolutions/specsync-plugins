# Postman Test Source SpecSync Plugin: SpecSync.Plugin.PostmanTestSource

This plugin can be used to synchronize test from a Postman collection and publish results executed with Newman. 

*You can find more information about the SpecSync sample plugins in the [repository page](https://github.com/specsolutions/specsync-sample-plugins#readme).*

* Plugin package: [SpecSync.Plugin.PostmanTestSource](https://www.nuget.org/packages/SpecSync.Plugin.PostmanTestSource)
* Plugin source: [SpecSync.Plugin.PostmanTestSource](SpecSync.Plugin.PostmanTestSource)
* Sample project: [SampleProject](SampleProject)

The plugin require a SpecSync Enterprise license to run. Please [contact us](https://specsolutions.gitbook.io/specsync/contact/specsync-support) to get an evaluation license that you can use to try out this plugin.

## Synchronizing Postman Tests

The plugin connects to the Postman cloud server and loads the configured Postman collection. From the collection it detects tests as:

* Folders or requests that have been already linked to Test Cases by SpecSync (via documentation section)
* Optional: Folders or requests that have a name matches to the configured regex (e.g. name starts with `TEST`). Use the `testNameRegex` parameter for this. 
* Optional: Folders or requests that have a documentation matches to the configured regex (e.g. documentation contains `ADO Test Case = 1234`). User the `testDocumentationRegex` parameter for this.
* Requests where their folder is not a test

The Postman tests can be enhanced with specific additional details that are required or useful for Test Case syncrhonization.
These details are stored in the "documentation" of the Postman folder or request item in a special section with heading `SpecSync` (you can configure the heading name to different values).
The section can contain a bulleted list with different values:

* The ID (and link) to the connected Test Case: `- tc: 1234`. The `tc` prefix can be changed by configuring a different prefix in `synchronization/testCaseTagPrefix`.
* Tags that should be synchronized as Test Case tags: Use a `- tags:` bullet point and put the tags as separate sub bullet points.
* Links to other work items: Use a `- links:` bullet point and put the links as separate sub bullet points as `  - story: 1234`, where `story` is a prefix configured in `synchronization/links`. The ID value can be a link, eg. `[1234](https://...)`.

SpecSync will synchronzie the request(s) inside the test and the `pm.test` tests as individial test steps in the Test Case.

For example a sample documentation in Postman might be like:

```
This endpoint echoes the HTTP headers, request parameters and the complete  
URI requested.

## SpecSync

- tc: [201](https://dev.azure.com/specsync-demo/specsync-plugins-demo/_workitems/edit/201)
- tags:
    - tag1
    - tag2
- links:
    - story: [131](https://dev.azure.com/specsync-demo/specsync-plugins-demo/_workitems/edit/131)
```

## Plugin Parameters

* `collectionId`: Specify the ID of the Postman Collection.
* `postmanApiKey`: Specify your Postman API key or set the POSTMAN_API_KEY environment variable.
* `metadataHeading`: Set the heading name of the section in the Postman item documentation that contains the SpecSync settings. Optional, `SpecSync` is used by default.
* `testNameRegex`: A regular expression that matches to folders names that should be treated as tests. Optional. E.g. `^TEST-`, if all test-folders name starts with `TEST-`.
* `testDocumentationRegex`: A regular expression that matches to folders documentation that should be treated as tests. Optional. E.g. to use `## Azure DevOps` as heading, you have to set the parameter to `Azure DevOps`.
* `testCaseLinkTemplate`: Specify the URL template of the Test Case. Optional, it has to be specified for Jira only.

## Publishing Test Results

The plugin can also publish test execution results to the synchronized Test Cases. For that first you need to run the Postman tests using "Newman" using the `junit` reporter. E.g.

```
newman run "https://api.getpostman.com/collections/26495037-5c8d6f15-b2a3-4b47-ae1d-72dd3bc1b49b?apikey=${env:POSTMAN_API_KEY}" --reporters "cli,junit" --reporter-junit-export TestResults\result.xml
```

Once the tests have executed and the result XML file has been created, you can use the SpecSync `publish-test-results` command with the `NewmanJUnitXml` format to publish the results to the remote server.

```
dotnet specsync publish-test-results -r .\TestResults\result.xml -f NewmanJUnitXml
```