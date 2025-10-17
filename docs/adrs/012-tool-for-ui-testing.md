# 012 - Tool For UI Testing

ADR taken from https://github.com/DFE-Digital/sts-plan-technology-for-your-school

* **Status**: accepted

## Context and Problem Statement

What tool should be adopted within the SAP project to facilitate UI testing?

## Decision Drivers

* Within DfE’s Technical Guidance
* DfE projects using Cypress
	* [find-a-tuition-partner](https://github.com/DFE-Digital/find-a-tuition-partner)
	* [trams-data-api](https://github.com/DFE-Digital/trams-data-api)
  
## Considered Options

* Cypress
* Selenium / specflow
* Puppeteer

## Decision Outcome

Using [Cypress](https://cypress.io) as it is the most commonly used UI testing application/framework across DFE.