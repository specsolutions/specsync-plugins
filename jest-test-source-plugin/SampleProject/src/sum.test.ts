import { isEven, sum } from "./sum";

describe("sum utility [@describeTag]", () => {
  test("adds two numbers [@tc:251 @testTag]", () => {
    expect(sum(2, 3)).toBe(5);
  });

  it("adds negative numbers [@tc:252 @important]", () => {
    expect(sum(-1, -4)).toBe(-5);
  });

  test("adds other numbers [@tc:253]", () => {
    expect(sum(10, 5)).toBe(15);
  });

  test.each([
    [1, 2, 3],
    [0, 0, 0],
    [-3, 7, 4],
  ])("adds %i and %i to equal %i [@tc:262]", (left: number, right: number, expected: number) => {
    expect(sum(left, right)).toBe(expected);
  });
});

describe("isEven utility", () => {
  test("returns true for even values [@tc:254]", () => {
    expect(isEven(6)).toBe(true);
  });

  it.each([1, 3, 5])("returns false for odd values like %i [@tc:263]", (value: number) => {
    expect(isEven(value)).toBe(false);
  });

  test("returns false for odd numbers (failing) [@tc:255]", () => {
    expect(isEven(3)).toBe(true);
  });
});
