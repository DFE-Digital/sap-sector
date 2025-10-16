# 003 - Adopt .NET as the Project's Coding Language

**Status**: Accepted  
**Deciders**: Dan Murfitt  
**Date**: 2025-10-15  




## Context and Problem Statement

DfE digital services have historically used a mix of technologies including Ruby on Rails and C#.net. As the department scales its digital infrastructure and seeks to improve maintainability, security, and developer productivity, a decision is needed on a preferred backend language and framework to standardise future development.



## Decision Drivers

- Alignment with existing Microsoft ecosystem (e.g., Azure, Active Directory)  
- Long-term maintainability and support  
- Security and compliance requirements  
- Availability of skilled developers  
- Performance and scalability  
- Integration with legacy systems  



## Considered Options

- .NET (C#)  
- Ruby on Rails  
- Node.js  



## Decision Outcome

**Chosen option**: ".NET", because it aligns best with DfE’s existing infrastructure, offers strong security and performance characteristics, and benefits from long-term vendor support and a large talent pool.



## Positive Consequences

- Easier integration with Microsoft services (e.g., Azure, Office 365)  
- Improved performance and scalability for enterprise-grade applications  
- Strong support for security and compliance standards  
- Better tooling and developer experience with Visual Studio and GitHub integration  



## Negative Consequences

- Reduced flexibility in choosing open-source frameworks  
- Risk of vendor lock-in with Microsoft technologies  



## Pros and Cons of the Options

### .NET (C#)

**Good, because**  
- Strong enterprise support and long-term viability  
- Excellent performance and scalability  
- Seamless integration with Azure and other Microsoft services  
- Rich tooling ecosystem and developer productivity  

**Bad, because**  
- Less flexible than some open-source alternatives  
- Requires Windows-based development environments for some legacy tools  
- May involve retraining for teams used to dynamic languages  



### Ruby on Rails

**Good, because**  
- Rapid development and prototyping  
- Convention over configuration simplifies onboarding  
- Strong community and open-source ecosystem  

**Bad, because**  
- Performance limitations at scale  
- Smaller talent pool in the UK public sector  
- Less alignment with DfE’s infrastructure and security needs  



### Node.js

**Good, because**  
- Non-blocking I/O and event-driven architecture  
- Large ecosystem of packages via npm  
- JavaScript ubiquity across frontend and backend  

**Bad, because**  
- Callback complexity and maintainability issues  
- Security concerns with third-party packages  
- Less mature tooling compared to .NET  


