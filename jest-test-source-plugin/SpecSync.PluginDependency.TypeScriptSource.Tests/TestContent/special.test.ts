import { isEven, sum } from "./sum";

// top level test
test("adds two numbers (top level) [@tc:264]", () => {
  expect(sum(2, 3)).toBe(5);
});

// nested describes
describe("calculations", () => {
  describe("sum utility", () => {
    test("returns true for even values (nested describes) [@tc:265]", () => {
      expect(isEven(6)).toBe(true);
    });
  });
});
    