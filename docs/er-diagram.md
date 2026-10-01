# ER diagram

> **Status:** Draft – pending team review and approval.
> Source file: [`er-diagram.mmd`](er-diagram.mmd)

```mermaid
erDiagram
  Course ||--o{ Module : "has"
  Course |o--o{ ApplicationUser : "enrolls (students)"
  Module ||--o{ Activity : "has"
  ActivityType ||--o{ Activity : "classifies"
  Activity ||--o{ Submission : "receives"
  ApplicationUser ||--o{ Submission : "submits"
  Submission ||--|| Document : "file"
  Submission ||--o| Feedback : "gets"
  ApplicationUser ||--o{ Feedback : "writes (teacher)"
  ApplicationUser ||--o{ Document : "uploads"
  Course |o--o{ Document : "holds"
  Module |o--o{ Document : "holds"
  Activity |o--o{ Document : "holds"
  ApplicationUser ||--o{ Notification : "receives"

  ApplicationUser {
    string Id PK "Identity"
    string FirstName
    string LastName
    string Email "Identity, unique"
    int CourseId FK "nullable, students only"
  }
  Course {
    int Id PK
    string Name
    string Description
    datetime StartDate
    datetime EndDate
  }
  Module {
    int Id PK
    int CourseId FK
    string Name
    string Description
    datetime StartDate
    datetime EndDate
  }
  ActivityType {
    int Id PK
    string Name "Lecture, E-learning, Exercise, Assignment, Other"
  }
  Activity {
    int Id PK
    int ModuleId FK
    int ActivityTypeId FK
    string Name
    string Description
    datetime StartTime
    datetime EndTime "deadline when Assignment"
  }
  Document {
    int Id PK
    string Name
    string Description
    string FileName "original file name"
    string ContentType
    long SizeBytes
    string BlobPath "path in Blob Storage"
    datetime UploadedAt
    string UploadedById FK
    int CourseId FK "nullable"
    int ModuleId FK "nullable"
    int ActivityId FK "nullable"
  }
  Submission {
    int Id PK
    int ActivityId FK
    string StudentId FK
    int DocumentId FK "unique"
    datetime SubmittedAt "set by server"
  }
  Feedback {
    int Id PK
    int SubmissionId FK "unique"
    string TeacherId FK
    string Comment
    datetime CreatedAt
  }
  Notification {
    int Id PK
    string UserId FK
    string Type "NewDocument, NewFeedback"
    string Message
    string Link "nullable"
    datetime CreatedAt
    bool IsRead
  }
```

**Notation:** `||` exactly one · `|o` zero or one · `o{` zero or more · `PK` primary key · `FK` foreign key

## Business rules

| Rule | Enforced in |
|---|---|
| Start date is before end date (course, module, activity) | DTO + service |
| A module is within the course period and does not overlap other modules in the course | `ModuleService` |
| An activity is within the module period and does not overlap other activities in the module | `ActivityService` |
| A student belongs to exactly one course | `UserService` |
| Access to course, document and submission data depends on role and course membership, even if an id or URL is manipulated | Services |
| File uploads validate file type and file size | `DocumentService` |

Invalid dates and overlaps are always rejected server-side, not only in the Blazor UI.
