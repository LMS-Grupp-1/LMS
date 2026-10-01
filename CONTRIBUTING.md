# Contributing

## Language

All code, including names and comments, is written in English.

## Branches

| Branch | Purpose |
|---|---|
| `main` | Stable, released code. Updated at the end of each sprint. |
| `Dev` | Integration branch. All features are merged here. |
| `feature/<issue>-<short-name>` | New functionality, e.g. `feature/us04-modules` |
| `bugfix/<short-name>` | Bug fixes, e.g. `bugfix/submission-deadline` |

`Dev` is the repository's default branch, so new pull requests target `Dev` automatically and `Closes #<number>` closes issues when a pull request is merged.

Direct pushes to `master` and `Dev` are blocked. All changes go through a pull request.

```mermaid
%%{init: {'gitGraph': {'mainBranchName': 'main'}}}%%
gitGraph
    commit id: "init"
    branch Dev
    checkout Dev
    branch feature/us02-courses
    checkout feature/us02-courses
    commit id: "course entity"
    commit id: "course endpoints"
    checkout Dev
    merge feature/us02-courses id: "PR reviewed"
    branch feature/us01-users
    checkout feature/us01-users
    commit id: "user admin"
    checkout Dev
    merge feature/us01-users id: "PR reviewed "
    checkout master
    merge Dev id: "end of sprint 1"
```

## Workflow

1. Pick a sub-issue in GitHub Projects, assign yourself and move it to **Ready**.
2. Create a feature branch from `Dev`.
3. Implement and verify the change locally. One branch may cover several closely related sub-issues.
4. Push the branch and open a pull request against `Dev`. Use a draft PR while work is in progress.
5. Mark the PR as **Ready for review** when the solution builds and the feature can be tested. Move the sub-issues to **In review**.
6. Another team member reviews the code, tests the feature and approves or leaves comments.
7. Reference the sub-issues the PR completes with `Closes #<number>`.
8. Merge into `Dev` after approval.
9. At the end of each sprint, `Dev` is merged into `master` through a pull request.

A parent issue (user story) is closed only when all of its acceptance criteria are met.

### Avoid a review queue

No more than 2–3 pull requests should be **Ready for review** at the same time. When the queue grows, review before opening new PRs.

## Project board statuses

| Status | Meaning |
|---|---|
| Backlog | The requirement exists but is not selected for active work. |
| Sprint backlog | Understood, assigned and planned for the current sprint. |
| In progress | Someone is actively working on it. |
| In review | Implemented in a pull request that is ready for review. |
| Done | Reviewed, merged and verified. |

## Definition of Done

- [ ] The implementation meets the issue description and the related acceptance criteria.
- [ ] The solution builds without errors and starts.
- [ ] Server-side validation and authorization are in place.
- [ ] Database changes have a working migration.
- [ ] Client API calls go through the existing BFF/YARP flow.
- [ ] Relevant manual test scenarios are checked and automated tests pass.
- [ ] The pull request is reviewed by at least one other team member.
- [ ] Review comments are resolved.
- [ ] The change is merged into `Dev` and the related sub-issues are closed.
- [ ] The README is updated if installation, configuration or testing has changed.
