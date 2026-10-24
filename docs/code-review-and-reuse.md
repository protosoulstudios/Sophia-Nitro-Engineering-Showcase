# Code review and reuse policy

[Portfolio](../README.md) · [Example coverage](coverage.md)

## Purpose and scope

This is a **technical portfolio for professional review**, not a publicly licensed Unity game or an open-source software package. Its three C# examples were authored separately to illustrate architecture choices and failure handling; they are **not extracted production scripts**, not drop-in components, and not assertions that every sample safeguard shipped in *Sophia & Nitro: Sweetland Adventures*.

The case studies describe source-inspected production behavior and limitations. The 41 executable checks reported in the documentation exercise the **independent .NET 8 examples only**. The developer separately reports completing Android release QA and delivering v1.0.0 APK/AAB builds to the client.

## Public visibility does not grant a general code license

**Current release choice: allow professional inspection when approved for publication, without granting an open-source or general reuse license.** No MIT, Apache, GPL, or other blanket license is added to this candidate.

Absent a separate license from the applicable rights holder, reviewers should not assume permission to reproduce, redistribute, incorporate, or modify this code for their own projects. GitHub's Terms of Service permit platform-related viewing and forking of public repositories; applicable legal exceptions may also apply. A public repository is **not automatically open source**.

For reuse or redistribution beyond what applicable law and platform terms already allow, request explicit permission from the appropriate rights holder. The right to authorize particular uses must be confirmed before permission is offered.

Reference: [GitHub Docs — Licensing a repository](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository).

## Provenance and content boundaries

- The examples are independently prepared portfolio models. Their technical similarity to inspected gameplay responsibilities **does not make them production code**.
- No playable game, original Unity source, client save data, signing keys, purchased raw assets or private audit ledgers are distributed in this showcase.
- Third-party game models, environments and audio may be visible **in future approved footage**, but depicting a game is distinct from redistributing its underlying asset files. Clip-specific terms still require review.
- The client's general permission to show the project publicly has **already been reported by the developer**; it is not a substitute for an applicable third-party asset license or for a decision to grant code reuse rights.
- No contributions or pull-request-based collaboration program is offered by this portfolio policy. If that changes, decide contribution terms separately.

## Private-release gates

This repository remains **PRIVATE**. The owner has specifically chosen to include gameplay videos **before** any public release. Therefore:

1. Keep the prepared code/case studies private until the selected actual gameplay clips have been recorded, reviewed and added or linked.
2. Confirm authority to publicly display the exact independently prepared samples, plus attribution and any contractual restrictions. Do not infer entitlement solely from separate authorship or use of AI-assisted tooling.
3. Review the *final* public file inventory, Git history, confidential data patterns, links, current captions and any newly added media.
4. Obtain the owner's **explicit final go-live instruction** for the exact repository and revision. Do not change visibility, merge default branches, or publish material just because the documentation is ready.

This policy expresses the planned portfolio presentation and non-open-source default; it is **not a substitute for legal review or proof of code ownership**. It may be revisited if the owner later decides to license individual examples.
