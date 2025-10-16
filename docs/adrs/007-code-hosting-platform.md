# 007 - Source Control Strategy: Prefer GitHub over Azure DevOps
**Status**: Accepted  
**Deciders**: Dan Murfitt  
**Date**: 2025-10-15  



### Context and Problem Statement

DfE teams require a source control platform for managing code repositories and collaboration. The choice is between Azure DevOps and GitHub. The decision must support transparency, community engagement, and alignment with the 'coding in the open' principle.



### Decision Drivers

- Alignment with 'coding in the open' principle  
- Public visibility and community contribution  
- Integration with modern CI/CD workflows  
- Cost and licensing considerations  
- Developer familiarity and ecosystem support



### Considered Options

- GitHub  
- Azure DevOps



### Decision Outcome

**Chosen option**: "GitHub", because it best supports open development practices, offers strong community integration, and aligns with DfE’s transparency goals.



### Positive Consequences

- Enables public visibility of code and collaboration  
- Supports open-source contributions and reuse  
- Integrates well with modern CI/CD tools and workflows  
- Reduces barriers to entry for external developers



### Negative Consequences

- May require additional controls for sensitive/private repositories  
- Less integrated with some Microsoft enterprise tools  
- Migration effort from existing Azure DevOps projects



### Pros and Cons of the Options

#### GitHub

**Good, because**  
- Supports public repositories and open collaboration  
- Widely adopted and familiar to developers  
- Rich ecosystem of integrations and automation tools  

**Bad, because**  
- Requires governance for managing public/private boundaries  
- May lack some enterprise features of Azure DevOps



#### Azure DevOps

**Good, because**  
- Deep integration with Microsoft enterprise stack  
- Built-in boards, pipelines, and test plans  

**Bad, because**  
- Limited support for public/open repositories  
- Less alignment with open development principles  
- Higher complexity for external collaboration

