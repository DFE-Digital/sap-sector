# Branching strategy

* Status: accepted
* Deciders: Dan Murfitt, Vipin Reddy, Georgina Newcomb
* Date: 2025-09-18

## Context and Problem Statement

To ensure consistency, a strategy is needed for branching, merging, and deploying code.

## Decision Drivers <!-- optional -->

* Code needs to be reviewed
* Code needs to be deployed for non-devs to view outcomes
* Code needs to be managed up to environments

## Considered Options

* Trunk-based 

## Decision Outcome

Trunk based flow following the the diagram

![SAPSec Flow](/adrs/assets/002-flow.png "Flow diagram showing branching and deployment process")

Branches will be formatted in the following way

* feature/{trello-id}-{trello-description}
    * e.g. feature/1001-let-users-login-with-azure-credentials
* bug/{trello-id}-{trello-description}
    * e.g. bug/1002-azure-login-not-working-for-scunthorpe-la
