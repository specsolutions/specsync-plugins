Feature: Complex Addition
 
Rules for complex addition:
- Is able to calculate the result of 42
- Works with zero
- Commutative

@tc:183
@basic
Scenario: Add two numbers
	Given I have entered 5 into the calculator
	And I have entered 7 into the calculator
	When I choose add
	Then the result should be 12

@tc:184
@important
Scenario Outline: Add two numbers (outline)
	Given I have entered <a> into the calculator
	And I have entered <b> into the calculator
	When I choose add
	Then the result should be <result>
Examples: 
	| case          | a  | b  | result |
	| classic       | 50 | 70 | 120    |
	| commutativity | 70 | 50 | 120    |
	| zero          | 0  | 42 | 42     |
