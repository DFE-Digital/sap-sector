# 005 - Content Management Strategy: GitHub-Based Editing with JSON/HTML Files

**Status**: Accepted  
**Deciders**: Dan Murfitt  
**Date**: 2025-10-15  


## Context and Problem Statement
DfE services require a way for editors to manage and update content efficiently. The challenge is to provide a system that is fast, low-cost, and simple, while still enabling non-developers to contribute. Should we build a custom CMS, use a third-party service like Contentful, or enable editing directly via GitHub?

## Decision Drivers
- Speed of implementation  
- Low operational and licensing cost  
- Simplicity and transparency  
- Editor empowerment and flexibility  
- Avoiding vendor lock-in

## Considered Options
- GitHub-based editing with JSON and HTML files  
- Custom-built CMS  
- Contentful (headless CMS)

## Decision Outcome
Chosen option: "GitHub-based editing", because it is the simplest, fastest, and most cost-effective solution. Editors can manage content directly in version-controlled files, and the system avoids the complexity and overhead of a full CMS.

## Positive Consequences
- Immediate availability with no additional infrastructure  
- Transparent version control and audit trail  
- Low cost and minimal maintenance  
- Editors can work directly with structured content

## Negative Consequences
- Requires basic GitHub familiarity for editors  
- Limited UI/UX compared to traditional CMS platforms  
- May not scale well for large editorial teams or complex workflows

## Pros and Cons of the Options

### GitHub-Based Editing
Good, because  
- Fast to implement  
- No licensing or hosting costs  
- Full control over content structure and workflow  

Bad, because  
- Editors need to learn GitHub basics  
- No WYSIWYG interface  
- Risk of accidental edits without validation

### Custom CMS
Good, because  
- Tailored to specific editorial needs  
- Can provide intuitive UI for non-technical users  

Bad, because  
- High development and maintenance cost  
- Longer time to deliver  
- Requires ongoing support and updates

### Contentful
Good, because  
- Rich editor interface and API  
- Scalable and well-supported  

Bad, because  
- Licensing costs  
- Vendor lock-in  
- Requires integration and setup time
