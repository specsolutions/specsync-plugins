Feature: Complex Addition
 
Rules for complex addition:
- Is able to calculate the result of 42
- Works with zero
- Commutative

@tc:180
Scenario Outline: Add two numbers (outline)
	Given I have entered <a> into the calculator
	And I have entered <b> into the calculator
	When I choose add
	Then the result should be <result>

Examples:
	| description   | a  | b  | result |
	| classic       | 50 | 70 | 120    |
	| commutativity | 70 | 50 | 120    |
	| zero          | 0  | 42 | 42     |
	