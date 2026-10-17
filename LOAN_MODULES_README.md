# FINCORE Loan Modules – Integration Notes

This delivery updates only the assigned loan workflow and its supporting frontend shell:

- Apply Loan: options, EMI calculation, application submission and LoanDeal creation.
- Loan Deal: paginated pending deals, approve/reject review actions.
- Sanction Letter: paginated approved deals, PDF generation and download.
- Disbursement: paginated pending/history lists, release validation and LoanAccount creation.

## Backend setup

Open a terminal in the `Loan` folder (the folder containing `Loan.slnx`):

```powershell
dotnet restore Loan.slnx
dotnet test Loan.Tests/Loan.Tests.csproj
dotnet run --project Loan/Loan.csproj
```

Swagger should be available at `http://localhost:5087/swagger` with the current launch profile.

### API conventions

- Responses use `{ success, message, data, error }`.
- List endpoints accept `page` (default `1`) and `pageSize` (default `10`, maximum `100`). The `data` value contains `items`, `page`, `pageSize`, `totalCount`, `totalPages`, `hasPreviousPage`, and `hasNextPage`.
- Assigned workflow controllers are rate limited to 120 requests per minute per client IP. Excess requests return HTTP 429 and the same response wrapper.
- AutoMapper profiles are in `Loan.Application/Mapping/LoanMappingProfile.cs`.
- Mapping tests are in `Loan.Tests`.

### Authentication and authorization

The Angular HTTP interceptor adds a Bearer token from browser local storage if a key named `token`, `accessToken`, or `jwtToken` exists. Use your existing login flow to obtain and store a valid JWT. Customer application actions accept the `User` or `Customer` role. Loan deal review, sanction, and disbursement actions accept `Office`, `Admin`, or `LoanOfficer`.

The loan options endpoint is public because it contains only the static loan type, tenure, interest-rate and EMI-day choices. Other customer/office operations remain protected.

### Email configuration

The SMTP password was removed from `appsettings.json` rather than repackaged as a plaintext secret. If your email service needs it, set the environment variable `EmailSettings__SenderPassword` in the environment running the API. Rotate any SMTP app password that was previously stored in a source file.

## Angular setup

Open a terminal in `Front-end-Loan`:

```powershell
npm install
ng serve --open
```

The frontend API base URL is set in `src/app/services/api-config.ts` to `http://localhost:5087/api`.

The shell uses a responsive blue sidebar inspired by the supplied LoanApp example, but contains only the four assigned workflow pages (no dashboard). The TestApp upload was used as an Angular project-structure/reference, not copied as business logic.

## Important integration notes

- A valid JWT with the correct role is required for the protected actions. This package does not add a separate login page because authentication is outside the four assigned modules.
- The current API keeps the existing SQL Server schema and models. No migration was added for pagination, response wrappers, AutoMapper or rate limiting because those are application-layer/API concerns.
- The existing eligibility rule (`MonthlyIncome * 20`) and interest/tenure options are preserved from the uploaded project.
- The project could not be built/tested in this execution environment because the .NET SDK was not installed and npm dependency installation could not complete here. Run the commands above locally and share the first new error if anything fails.
