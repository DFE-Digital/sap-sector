# 001 - Build the service, without a GDS-aligned name

* Status: accepted
* Deciders: Dan Murfitt
* Date: 2025-09-10

## Context and Problem Statement

Whilst in early development phases, the service needs a name, for reference to but also for creating github/analytics/platform artefacts. 
The project team does not currently have a best case name but waiting for this decision holds up building based on decisions that *have* been made.

## Decision Drivers

* Need to get on with development to meet expected timescales
* Need to name the service in-line with GDS best practice

## Considered Options

* Build based on working title and move/rename for later
* Wait for name to be firmed up
* Pick a name and go with it

## Decision Outcome

Chosen option: Build based on working title and move/rename for later, because the service can be built without it and using IaaC etc it shouldn't be difficult to rename later.

### Positive Consequences

* Development can begin

### Negative Consequences

* Requests will need to be re-raised and services migrated


## Pros and Cons of the Options

### Build based on working title and move/rename for later

* Good, because gets us started
* Bad, because have to move later


### Wait for name to be firmed up

* Good, because service gets build with the correct name
* Bad, because development is paused until the decision is made

### Pick a name and go with it

* Good, because development can begin
* Good, because service doesn't need renaming
* Bad, because the name could be wrong and promoted as wrong. E.g. "The service is called Wibble School Wobble, and then this is taken and promoted around the department"