# Remaining Work Checklist – Ballast Lane .NET Interview Project

**Purpose**: This checklist captures the remaining gaps between the current state of the project and the official requirements + evaluation criteria in the Ballast Lane PDF ("Net - BLA - Technical Interview Exercise - V5.pdf").

**Last Updated**: 2026-05-28  
**Current Overall Readiness**: 85–88% (Strong, but has 2–3 noticeable gaps that a good interviewer will likely probe)

---

## 1. Must Fix Before Interview (Highest Risk Items)

These items directly address explicit requirements or high-probability interview questions.

| # | Task | Why It Matters (PDF Link) | Estimated Effort | Priority | Acceptance Criteria |
|---|------|---------------------------|------------------|----------|---------------------|
| 1 | **Add Controller / API Integration Tests** | PDF explicitly requires "unit tests for ... and API endpoints". Currently we only test Application + Infrastructure layers. | 4–6 hours | **Critical** | - Create `tests/TaskManagement.API.Tests/` project<br>- Use `WebApplicationFactory` + `TestServer`<br>- Cover at least: Auth endpoints, CRUD happy paths, ownership/unauthorized cases, validation errors<br>- Minimum 6–8 meaningful tests<br>- All tests pass in CI |
| 2 | **Make GenAI Documentation Presentation-Ready** | PDF has a dedicated "Generative AI tools" section with specific deliverables: show prompt, show AI output sample, describe validation/corrections/edge cases. | 3–4 hours | **Critical** | - Update `docs/genai-usage.md` with 1–2 concrete prompt examples<br>- Include real (or realistic) "before" AI-generated code snippets<br>- Show what was changed and why<br>- Add a 1-page "GenAI Story" summary suitable for screen sharing during presentation<br>- Practice verbal delivery |
| 3 | **Fix / Document DueDate Partial Update Limitation** | Current logic in `TaskService.UpdateTaskAsync` cannot reliably distinguish "field omitted" vs "clear the due date". This is a subtle but real business logic gap. | 1–2 hours | High | - Decide on final behavior (recommend: only update when `HasValue`)<br>- Add clear comment + unit test<br>- Update OpenAPI spec or README if behavior is non-obvious<br>- Test via Scalar UI |

**Subtotal for Must-Fix section**: **8–12 hours** (1–1.5 developer days)

---

## 2. Strong Recommendations (Medium Risk)

These will make the project feel more complete and professional during code review.

| # | Task | Why It Matters | Estimated Effort | Priority | Acceptance Criteria |
|---|------|----------------|------------------|----------|---------------------|
| 4 | Add 2–3 more Frontend Component Tests | Current frontend test coverage is minimal (only 2 basic app tests). Interviewers may ask about testing strategy on the frontend. | 2–3 hours | Medium | - Add tests for `tasks.component.ts` (create, status update, delete flows using Vitest)<br>- Mock `TaskService` and `AuthService`<br>- Tests pass with `npm test -- --watch=false` |
| 5 | Address Pre-existing Code Quality Issues | Several low-severity but visible issues exist that a good reviewer may point out. | 2–3 hours | Medium | - Resolve or suppress the 3 nullability warnings in repositories<br>- Decide on `LiteDbContext` lifetime (currently Singleton + IDisposable)<br>- Add `[ApiExplorerSettings]` or route attribute to normalize `/api/Auth` casing if desired |
| 6 | Improve Runtime OpenAPI Quality | The hand-written `docs/openapi.yaml` is richer than what Scalar currently shows. Interviewers may compare the two. | 3–4 hours | Medium | - Add `[ProducesResponseType]` + XML comments for all error cases<br>- Implement one or two OpenAPI transformers in `Program.cs` (e.g. standardize error responses)<br>- Consider generating `docs/openapi.yaml` from the running app at build time |

**Subtotal for Recommendations**: **7–10 hours**

---

## 3. Nice-to-Have / Polish (Low Risk)

Do these only if you have extra time or want to stand out.

| # | Task | Benefit | Estimated Effort | Priority | Notes |
|---|------|---------|------------------|----------|-------|
| 7 | Update the ironic seeded task | The task "Write unit tests for services" is now funny because we actually did it. | 15 min | Low | Change the title/description of the third seeded task to something more neutral. |
| 8 | Create a simple Postman collection or `.http` file examples | Makes demoing the API easier during presentation. | 1 hour | Low | Export current Scalar usage or create a small `docs/TaskManagement-API.http` file with auth + CRUD examples. |
| 9 | Add basic e2e test skeleton (Playwright or Cypress) | Shows full-stack testing awareness (not required by PDF). | 3–4 hours | Low | Only if you want to go above and beyond. |
| 10 | Create a short "Interview Demo Script" (1–2 pages) | Helps you stay calm and hit all the important points during the 15–20 min presentation. | 1–2 hours | Low | Include: user story, architecture tour, live demo steps, GenAI story, testing approach. |

**Subtotal for Polish**: **5–8 hours** (optional)

---

## Summary – Total Estimated Remaining Effort

| Category | Estimated Time | Recommendation |
|----------|----------------|----------------|
| **Must Fix (Critical)** | 8–12 hours | Do this first. Aim to complete before the interview. |
| **Strong Recommendations** | 7–10 hours | Highly recommended if time allows. |
| **Nice to Have** | 5–8 hours | Only if you have buffer time. |
| **Total Realistic** | **15–22 hours** (2–3 full days) | Comfortable pace with good coverage |
| **Minimum Viable for Interview** | **8–10 hours** | Focus only on items #1 and #2 |

---

## Suggested Order of Execution

1. **Today / Tomorrow** — Items 1 + 2 (Controller tests + GenAI presentation material). These give the biggest risk reduction.
2. **Next** — Item 3 (DueDate logic) + Item 5 (code quality warnings).
3. **If time remains** — Items 4, 6, and any polish items.

---

## How to Use This Checklist

- Treat this as your personal backlog for the final stretch.
- Mark items as you complete them.
- For each item, the "Acceptance Criteria" column is the definition of done.
- Before the interview, re-read the original PDF and do one final honest pass against this list.

---

**Note**: The core requirements (Clean Architecture, no forbidden packages, full CRUD, auth, seeded demo, frontend, README, user story) are already well satisfied. The remaining work is mostly about **closing explicit gaps** the PDF calls out and **reducing interview risk**.

Good luck — you're in a strong position. Completing items 1 and 2 will make this project interview-ready at a high level.