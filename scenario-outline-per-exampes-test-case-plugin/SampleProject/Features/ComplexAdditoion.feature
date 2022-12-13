Feature: Complex Addition
 
Rules for complex addition:
- Is able to calculate the result of 42
- Works with zero
- Commutative

@tc:135
@basic
Scenario: Add two numbers
	Given I have entered 5 into the calculator
	And I have entered 7 into the calculator
	When I choose add
	Then the result should be 12

@important
Scenario Outline: Add two numbers (outline)
	Given I have entered <a> into the calculator
	And I have entered <b> into the calculator
	When I choose add
	Then the result should be <result>

@tc:138
Examples: 
	| case          | a  | b  | result |
	| classic       | 50 | 70 | 120    |
	| commutativity | 70 | 50 | 120    |
	| zero          | 0  | 42 | 42     |

#@tc:137
@tc:139
@wrong
Examples: Wrong test cases
	| case          | a  | b  | result |
	| wrong!        | 0  | 42 | 43     |

@important
Scenario Outline: Add two numbers (outline with single examples)
	Given I have entered <a> into the calculator
	And I have entered <b> into the calculator
	When I choose add
	Then the result should be <result>

@tc:136
Examples: 
	| case          | a  | b  | result |
	| classic       | 50 | 70 | 120    |
	| commutativity | 70 | 50 | 120    |
	| zero          | 0  | 42 | 42     |
	