# BDD Specs

## Target Audience
PX Engineering

## Overview
B.D.D. stands for behavior driven development, but it's really a synonym for user-focused functional testing, an in some sense,
an evolution of T.D.D. that addresses the deficiencies of that practice.

Issues with TDD (or testing in general):
- Focuses too much on unit testing (test per method), therefore it focuses on the how, not on the what.
- Focuses on test coverage.
- **Tests can become difficult and expensive to maintain.** i.e., refactoring can be expensive.
- **When tests break, it's hard to understand their intent.**
- Test code can become larger and harder to maintain than implementation code.


BDD was conceived as a way of teaching TDD better:
- Moves away from tests to **Specifications/requirements/behaviors**
- Focuses on getting the words right:
    - Spec/feature not test.
    - Scenario not test case.
    - Given, when, then.
    - Start specs with "should".
- Builds on top of other ideas:
    - Domain Driven Design
    - Ubiquitous language. Common and consistent language from business to technical implementation to users.
    - Spec by example. By using ubiquitous language as a common language, capture business requirements as BDD scenarios in the same
    language so anybody can ready them.
    - Acceptance test driven development. Once you have automated the acceptance tests, you're testing the business acceptance criteria.

### How to implement BDD?
- Establish a ubiquitous language (and implementations).
- Drive development with executable specifications from the perspective of a external user.
- Separate the What from How.
    - Avoid testing how the system works
    - Focus on what the system does.

### How to implement a feature?
- Write spec, even if it has steps that need to be implemented and test is ignored.
- Implement feature so the spec tests pass.
- Refactor to sanitize code smells, aka implement patterns.
- Tests should pass after refactor.

## Feature Structure

## Best Practices

## Helpful Links

## F.A.Q.

## Notes:
- How do we create more maintainable mocks that only exist during the test duration?

---
For questions/clarifications, email [author/s of this doc and PX support](mailto:holugo@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20engineering/tests/bdd.md).

---