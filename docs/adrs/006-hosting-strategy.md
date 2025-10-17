# 006 - ADR: Hosting Strategy - Prefer AKS over Azure App Service

**Status**: Accepted  
**Deciders**: Dan Murfitt  
**Date**: 2025-10-15  



### Context and Problem Statement

DfE applications require a hosting platform that supports scalability, flexibility, and operational control. The team must decide between using Azure Kubernetes Service (AKS) or Azure App Service for hosting containerised workloads and microservices. Which platform best supports long-term infrastructure goals?



### Decision Drivers

- Flexibility in deployment and orchestration  
- Support for containerised workloads  
- Scalability and resilience  
- Infrastructure control and customisation  
- Cost-effectiveness for complex applications



### Considered Options

- Azure Kubernetes Service (AKS)  
- Azure App Service



### Decision Outcome

**Chosen option**: "AKS", because it provides greater flexibility, control, and scalability for containerised applications. It aligns with modern DevOps practices and supports complex workloads better than App Service.



### Positive Consequences

- Full control over container orchestration and networking  
- Better support for microservices and distributed systems  
- Easier integration with CI/CD pipelines and infrastructure-as-code  
- Scalable and resilient architecture



### Negative Consequences

- Higher operational complexity and learning curve  
- Requires Kubernetes expertise  
- More setup and maintenance overhead compared to App Service



### Pros and Cons of the Options

#### AKS

**Good, because**  
- Full Kubernetes support for orchestration  
- Highly scalable and configurable  
- Ideal for microservices and container-first architecture  

**Bad, because**  
- Requires more infrastructure knowledge  
- Higher setup and operational overhead  
- More complex monitoring and debugging



#### Azure App Service

**Good, because**  
- Simple to set up and manage  
- Integrated with Azure ecosystem  
- Suitable for monolithic or low-complexity apps  

**Bad, because**  
- Limited control over infrastructure  
- Less suitable for container orchestration  
- Scaling and customisation options are constrained
