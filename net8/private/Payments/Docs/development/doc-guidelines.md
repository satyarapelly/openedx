# Documentation Guidelines

## Target audience
Developers and PMs contributing code or documentation to PX

## Overview
If you are contributing to PX code, we request you to make corresponding doc changes as well (pls. see 
[contribution guidelines](../contribution-guidelines.md)). Also, you may 
be contributing to just docs as well. In either case, this doc has a set of established rules and 
guidelines to follow.

## Prerequisites
- [Basics of markdown syntax](https://www.markdownguide.org/basic-syntax/)
- [Documentation Philosophy](doc-philosophy.md)
- [Documentation Setup](doc-setup.md)

## To start with
For sure, there will be situations where these rules/guides will not be optimal. Also, some style 
choices below, which have been made with a goal of consistency, may not always be right. If you have 
a situations like that, pls. let us know (contact info is at the bottom).

## Files and folders
1.  Use [doc-template.md](doc-template.md) to create a new file. This helps 
    with consistency of file structure

1.  Use lowercase filenames and `-` as separators. Example: `doc-guideline.md`. This helps 
    with consistency and readability.

1. Use the right folder
    1. `development` - Docs intended to help with code contributions to PX. Readers are typically 
        developers working on PX. Authors are typically the PX Eng team.
    2. `scenarios` - Docs that explain customer scenarios. Readers are a wide range. Authors are
        typically PMs or Devs from either PX or contributing teams.
    3. `engineering` - Docs related to service fundamental, agility, tools-development and 
        process-development. Authors and readers are typically the PX Eng team.
    4. `operations` - Docs related to service operations - Tasks, activities, processes and procedures
        needed to keep the service running. Authors and readers are typically PX Eng team.

## Markdown guidelines

### Headings
1.  Don't skip heading levels; e.g., to make the html look better, I used to skip one level and go 
    `#` -> `###` -> `#####`. But this is unnecessary complexity. It's better to use css for that 
    and use md headings to indicate hierarchical relationship.

2.  Avoid trailing `:` in headings. Being a heading, its understood that content related to it are
    below it.

3.  Have only one top level heading `#` and use it for the doc's title

4.  For the title (top level heading `#`), use title case. For all other headings, use sentence case.
    E.g., `# This is Title Case`<sup>[1](#ShortPrepositions)</sup> and `## This is sentence case`.

### Lists
1.  Use `-` for bullet lists. Even though `*` also works, using just one type helps with 
    consistency. Also, `**` is used for bold. So, using `-` helps distinguish it a bit better 
    when reading raw .md 

2.  When using nested lists, indent by 4 spaces. This helps readability of raw .md

        - Item 1
            - Sub-item 1
        - Item 2

3.  When adding code block or other content along with lists, indent them align with text. This 
    helps md and html readability.

        - Item 1
            `some code content related to Item 1`

4.  Use lazy numbering for long lists (10 or more items). When such lists change, this avoid 
    having to re-number by hand

        1.  Item 1
        1.  Item 2
            <!--Many Items-->
        1.  Item 11

5.  Use actual numbering for short lists (less than 10). Actual numbers make raw .md more readable.

        1.  Item 1
        2.  Item 2
        3.  Item 3

### Links

1.  When referencing a source code file (which is outside the Docs folder), use absolute path like 
    "https://microsoft.visualstudio.com/Universal%20Store/_git/SC.CSPayments.PX?path=/private/Payments/Payments.sln"
    instead of "../Payments.sln"

2.  When referencing a heading inside another file, use lower case and replace spaces with dashes

### Tables
1.  If tables start to get complex (e.g. list / links inside cells), use lists instead. Complex 
    tables are hard to maintain/read in raw .md.

2.  When using tables, format them to look like tables in raw .md. This improves readability of
    raw .md.

        |Column 1        |Column 2        | Column 3        |
        |----------------|----------------|-----------------|
        |Value 1         |                |                 |

### Spacing
1.  Headings - Give one blank space after hash/es

2.  Numbered lists - Indent to 4 spaces. So, if it's a bullet list give 3 spaces after the `-`. If it's a
    numbered list, give two spaces after `1.`.  

3.  List line break - After a line break within a list, indent the new line to match the previous

        1.  There is enough text in this list item that it goes one and continues in the 
            next line. Notice "There" and "next" are aligned. This helps with readability
            of raw .md.

4.  Use a single space after period, comma, question mark, and other punctuations.

### Portability
1.  Try using generic markdown as much as possible and avoid or reduce DocFX specific syntax. This
    ensures that our docs stay portable and render correctly on various different viewers/editors.

2.  Use markdown and not html when markdown support exists. When markdown support does not exits, use
    html sparingly. An example is the footnote in this doc.

## Style and voice
1.  Use contractions (let's, it's, etc.) as you normally would in casual office conversations.
    Docs with casual (but respectful) tone are fun to read. 

1.  Use abbreviations (PX, IcM etc) as you normally would in casual office conversations
    1.  This helps keep docs concise
    2.  Helps readers get familiar with abbreviations used by the team (by adding them to the 
        [glossary page](../glossary.md))
    3.  An exception to consider is the doc title.  Here, its better to use the full form. E.g.,
        The title for this doc "Documentation Guideline" indicates that it's a guideline for the 
        process of documentation.  An alternate title, "Doc Guide", could be interpreted as
        a guidebook that helps readers navigate through documents, which, this doc is not.

## References
- [Links and Cross References](https://dotnet.github.io/docfx/tutorial/links_and_cross_references.html)
- [DocFX Flavored Markdown](https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html?tabs=tabid-1%2Ctabid-a)
- [Table of Contents](https://dotnet.github.io/docfx/tutorial/intro_toc.html)
- [Microsoft Docs - Style and Voice Quick Start](https://docs.microsoft.com/en-us/contribute/style-quick-start)
- [Microsoft Docs - Bias-free Communication](https://docs.microsoft.com/en-us/style-guide/bias-free-communication)

<a name="ShortPrepositions">1</a>: In the title `# This is Title Case`, notice that `is` is still lower 
case. That's because its a short preposition (less than 4 characters). Don't worry if you get this wrong 
at first. A different style (Chicago-style) recommends lower-casing all prepositions. We are choosing to 
go with AP-style for this one.

---
For questions/clarifications, email 
[author/s of this doc and PX support](mailto:kowshikpfte@microsoft.com?cc=PXSupport@microsoft.com&subject=Docs%20-%20development/documentation-template.md).
---