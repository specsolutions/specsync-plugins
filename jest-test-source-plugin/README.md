# Jest Test Source SpecSync Plugin: `SpecSync.Plugin.JestTestSource`

Allows synchronizing TypeScript Jest tests and publishing Jest JSON test result files.

*You can find more information about the SpecSync sample plugins in the [repository page](https://github.com/specsolutions/specsync-sample-plugins#readme).* 

* Plugin package: [`SpecSync.Plugin.JestTestSource`](https://www.nuget.org/packages/SpecSync.Plugin.JestTestSource)
* Plugin source: [`SpecSync.Plugin.JestTestSource`](SpecSync.Plugin.JestTestSource)
* Sample project: [`SampleProject`](SampleProject)

The plugin requires a SpecSync Enterprise license to run. Please [contact us](https://specsolutions.gitbook.io/specsync/contact/specsync-support) to get an evaluation license.

## Synchronizing TypeScript Jest tests

The plugin scans configured TypeScript source files (for example `src/**/*.test.ts`) and parses Jest test declarations.

It supports:
- JavaScript (.js), TypeScript (.ts), JSX (.jsx) and TSX (.tsx) files
- `test(...)` and `it(...)`
- describe blocks including nested describes
- common Jest extensions, e.g. `describe.each`, `describe.only`, `test.concurrent`, `test.only`, `test.todo`
- parameterized tests with `.each(...)`

Example:

```ts
import { sum } from "./sum";

describe("sum utility [@describeTag]", () => {
  test("adds two numbers [@tc:251 @testTag]", () => {
    expect(sum(2, 3)).toBe(5);
  });

  test.each([
    [1, 2, 3],
    [0, 0, 0],
    [-3, 7, 4],
  ])("adds %i and %i to equal %i [@tc:262]", (left: number, right: number, expected: number) => {
    expect(sum(left, right)).toBe(expected);
  });
});

// top-level test is also supported
test("adds two numbers (top level) [@tc:264]", () => {
  expect(sum(2, 3)).toBe(5);
});
```

### SpecSync tags in Jest test titles

SpecSync tags are read from tag blocks in test and describe titles, for example:

- `[@tc:251]` to link to a Test Case
- `[@story:123]` to link to another work item
- custom tags like `[@important]`

Describe tags are inherited by nested tests.

## Example configuration

Sample `specsync.json` highlights:

```json
{
  "toolSettings": {
    "plugins": [
      {
        "packageId": "SpecSync.Plugin.JestTestSource",
        "packageVersion": "5.0.0"
      }
    ]
  },
  "local": {
    "folder": "src",
    "sourceFiles": [
      "src/**/*.test.ts"
    ]
  },
  "synchronization": {
    "links": [
      {
        "tagPrefix": "story"
      }
    ],
    "automation": {
      "enabled": true,
      "automatedTestType": "Jest Unit Test"
    },
    "fieldUpdates": {
      "Description": "<b>describe: {rule-name}</b>{br:HTML}<pre>{local-test-case-container-source-file-path:HtmlEncode}:{br}{local-test-case-source:HtmlEncode}</pre>"
    }
  }
}
```

## Publishing test results

The plugin publishes Jest execution results from Jest JSON output.

Supported format:
- `JestJson`

Generate Jest JSON, e.g.:

```bash
npx jest --json --outputFile=jest-results.json
```

Publish results with SpecSync:

```bash
dotnet specsync publish-test-results -r .\jest-results.json -f JestJson
```

The sample project contains a result file example at `SampleProject/jest-results.json`.
