# SkillBoost — Learning Management System (LMS)

A learning-management prototype built with **ASP.NET Core MVC (.NET 8)**, **Entity Framework Core**, **SQL Server**, and Razor views.

> **Security / portfolio status:** This is a learning project, **not yet production-ready**. Review the security notes below before deployment or entering real personal data.

## What it includes

- Course catalog, categories, and search.
- User registration and login, with Student and Teacher signup roles.
- Course modules, lessons, preview lessons, and student progress.
- Course enrollment and learning dashboard.
- Events, news, and contact pages.
- Admin user-management screen.

## Technology

| Layer | Technology |
| --- | --- |
| Backend | C#, ASP.NET Core MVC, .NET 8 |
| Data | EF Core, SQL Server, migrations |
| Frontend | Razor views, HTML/CSS/JavaScript |
| Authentication | Session-based student/teacher/admin roles (custom prototype) |

## Local setup

**Requirements:** .NET SDK 8, running SQL Server or SQL Server Express, and a development environment configured for HTTPS.

1. Clone the repository.
2. Set `ConnectionStrings:DefaultConnection` via user secrets or environment variables. Avoid committing real credentials.
3. Apply EF Core migrations for the `LMS/LMS.csproj` project.
4. Start the application:

```bash
dotnet restore LMS.sln
dotnet ef database update --project LMS/LMS.csproj --startup-project LMS/LMS.csproj
dotnet run --project LMS/LMS.csproj
```

If the `dotnet ef` command is unavailable, install the appropriate .NET 8-compatible `dotnet-ef` tool first. Startup URLs will be shown in the terminal.

Example of **setting a local development connection without committing a secret**:

```bash
dotnet user-secrets init --project LMS/LMS.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_LOCAL_SQL_SERVER_CONNECTION_STRING" --project LMS/LMS.csproj
```

The existing `appsettings.json` includes a development-style connection string. Replace it with your own local configuration if needed. Never commit real passwords.

## Security improvements proposed in this branch

- New passwords are hashed with ASP.NET Core `PasswordHasher<User>`, using the existing `Users.Password` database column (no schema migration needed).
- Registration uses an explicit input model and accepts **only Student or Teacher**; it does not bind `Id` or `Admin` from requests.
- Registration, login, logout, and admin promotions are protected against CSRF with anti-forgery tokens.
- `MakeAdmin` is POST-only, and admin access rechecks the actual role in the database.
- Lesson completion uses the signed-in student instead of a hard-coded ID, validates lesson/course membership, and checks enrollment.
- Lesson viewing permits preview content; full content requires enrollment, ownership, or an Admin account.
- A session cache is registered explicitly.

### Existing accounts: password migration warning

**Important: do not merge into a deployed project with existing users without a migration plan.** Earlier versions saved passwords in plaintext. New logins only accept password hashes by default; existing plaintext users will be unable to sign in until migrated.

For a **brief, private local migration window only**, an administrator may opt in with the configuration key `Authentication:AllowLegacyPlaintextLogin=true`. A successful legacy login then re-hashes that user's password. This setting is **disabled by default** and must never be enabled on a public server. Prefer a controlled one-time migration or forced password reset for real deployments. In either case, treat historical plaintext passwords and backups as compromised, and never publish them.

### Known security work still required

- Review and protect course/content mutation endpoints in `SubjectsController`, `EventsController`, and `NewsController`: authorization, ownership checks, anti-forgery, and upload validation.
- Add a real authentication/authorization framework and secure credential-recovery flow for a deployable application.
- Review session-cookie settings, session fixation, rate limiting, and account-lockout controls.
- Add database-level uniqueness for emails and test concurrent registrations.
- Confirm payment flow semantics: the existing `ConfirmPayment` action records enrollment, but no payment provider is integrated.
- Review sensitive data, database configuration, and sample data before publishing a demo.

**Do not treat the presence of this README as a completed security audit.**

## Validation and tests

A .NET 8 build runs in GitHub Actions for pull requests, followed by a small automated authentication-safety test project. Run them locally with:

```bash
dotnet build LMS.sln --configuration Release
dotnet test LMS.Tests/LMS.Tests.csproj --configuration Release
```

The tests cover public signup role validation, password-strength input validation, and password hashing. **These are unit tests, not end-to-end security tests.** Full SQL Server/database-backed integration tests and manual browser testing are still required before merging or deploying.

Suggested manual checks:

1. New Student and Teacher registrations succeed, but posting `Role=Admin` is rejected.
2. New passwords are hashed in the database and logins work after restart.
3. Existing accounts follow a deliberate migration/reset path.
4. CSRF-less signup, login, promotion, or logout POST requests are rejected.
5. Non-admin users cannot invoke the admin promotion route.
6. Students cannot mark someone else's lesson progress or mark a lesson from a different course.
7. An enrolled student can watch full lessons, while unauthenticated users can only see previews.
8. Course management features are still usable for authorized teachers.

## Portfolio presentation

Once the app has been tested using **synthetic data**, add real screenshots of the landing page, course catalog, lesson player, and teacher/admin views. A brief screen recording can demonstrate role-specific flows. Do not record real credentials or students' data.

Suggested repository description: *ASP.NET Core MVC learning-management prototype with course content, enrollments, role-based flows, and SQL Server.*

Suggested topics: `aspnetcore`, `dotnet`, `csharp`, `entity-framework-core`, `sql-server`, `lms`, `mvc`.
