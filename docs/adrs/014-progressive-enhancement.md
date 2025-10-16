# 014 - Progressive Enhancement vs Graceful Degradation

**Status**: Accepted  
**Deciders**: Dan Murfitt  
**Date**: 2025-10-15  


### Context and Problem Statement

DfE web applications must be accessible, resilient, and performant across a wide range of devices and browsers. The team must choose between progressive enhancement and graceful degradation as the guiding principle for frontend development. Which approach better supports inclusivity and long-term maintainability?



### Decision Drivers

- Accessibility and inclusivity  
- Support for legacy and low-capability environments  
- Maintainability and simplicity  
- Alignment with GOV.UK design principles  
- Resilience to failure and partial loading



### Considered Options

- Progressive Enhancement  
- Graceful Degradation



### Decision Outcome

**Chosen option**: "Progressive Enhancement", because it ensures core functionality is always available and aligns with the principle of designing for the most constrained environments first.



### Positive Consequences

- Improved accessibility for users with limited devices or connectivity  
- Better alignment with GOV.UK and DfE design standards  
- Resilient applications that degrade gracefully when advanced features fail  
- Encourages modular and maintainable code



### Negative Consequences

- May require more upfront planning and testing  
- Advanced features may be harder to implement across all layers  
- Developers must be disciplined in separating core and enhanced functionality



### Pros and Cons of the Options

#### Progressive Enhancement

**Good, because**  
- Prioritises core functionality  
- Works well in low-bandwidth or older browsers  
- Encourages semantic HTML and modular design  

**Bad, because**  
- Requires careful layering of enhancements  
- May limit use of cutting-edge features without fallbacks  
- Testing across layers can be complex



#### Graceful Degradation

**Good, because**  
- Allows rapid development with modern features  
- Focuses on full-feature experience first  

**Bad, because**  
- Core functionality may be lost in degraded environments  
- Less inclusive for users with limited capabilities  
- Harder to maintain fallback paths


