# 004 - Architecture Decision Record: Data Storage Strategy

**Title**: Data Storage Strategy: Prefer In-Memory, Fallback to Redis, then Postgres  
**Status**: Accepted  
**Deciders**: Dan Murfitt  
**Date**: 2025-10-15  



### Context and Problem Statement

DfE services require fast and reliable access to frequently used data. The challenge is to determine the optimal data storage strategy that balances performance, complexity, and cost. The question is: which data layer should be used by default, and under what conditions should we fall back to alternatives?

Both SAP projects, public and sector will host a large amount of data for fast retrieval, this data is public data and may change periodically. Metadata for the school could change daily, whereas Measures data could be quarterly. 

SAP Sector may also store data around user preferences and login data. This data will be stored in Postgres only. 

For the other data, (GIAS and EES sourced) this ADR covers the strategic approach.



### Decision Drivers

- Performance requirements for real-time access  
- Simplicity and speed of implementation  
- Cost and infrastructure overhead  
- Scalability and fault tolerance  
- Operational complexity and maintainability



### Considered Options

- In-Memory (e.g., application-level caching)  
- Redis (external in-memory cache)  
- Postgres (persistent relational database)



### Decision Outcome

**Chosen option**: "In-Memory", because it offers the fastest access with minimal infrastructure overhead. Redis will be used if In-Memory is insufficient for performance or scalability, and Postgres will be used if persistence or durability is required.



### Positive Consequences

- Fastest possible data access for high-performance use cases  
- Minimal latency for frequently accessed data  
- Reduced infrastructure complexity when In-Memory is sufficient  
- Clear fallback strategy improves system resilience



### Negative Consequences

- In-Memory data is volatile and not shared across instances  
- Redis introduces external dependencies and operational overhead  
- Postgres may not meet latency requirements for real-time access



### Pros and Cons of the Options

#### In-Memory

**Good, because**  
- Fastest access speed  
- No external dependencies  
- Simple to implement within application lifecycle  

**Bad, because**  
- Data is lost on restart  
- Not suitable for distributed systems without coordination  
- Limited by application memory footprint  



#### Redis

**Good, because**  
- Shared cache across services  
- Fast access with persistence options  
- Scalable and fault-tolerant  

**Bad, because**  
- Requires external infrastructure  
- Adds operational complexity  
- Slightly slower than in-process memory  



#### Postgres

**Good, because**  
- Durable and reliable storage  
- Supports complex queries and relationships  
- Well-understood and widely supported  

**Bad, because**  
- Slower access times  
- Higher latency for frequent reads  
- Overkill for transient or frequently changing data  
