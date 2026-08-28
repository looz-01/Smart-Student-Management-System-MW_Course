# Seeding Data Details

This document describes every record the application seeds at startup, how to
enable/disable it, and the exact login credentials for the seeded accounts.

## How seeding works

- Seeding runs automatically on every startup, right after EF Core migrations are applied (`Program.cs` -> `DbSeeder.SeedAsync`).
- It is **idempotent**: re-running the app never duplicates data (roles are created only if missing; demo data is skipped if it already exists).
- Three phases:
  1. **Roles** — `Admin`, `Teacher`, `Student` are created if missing.
  2. **Admin account** — read from the `SeedAdmin` section of configuration.
  3. **Demo data** — enabled with `Seeding:DemoData`. It is `true` only in the `Development` environment (`appsettings.Development.json`) and `false` by default (`appsettings.json`) so production stays clean.

## Enabling / disabling demo data

```json
{
  "Seeding": {
    "DemoData": true
  }
}
```

- `true` -> a large set of demo records is inserted on next startup.
- `false` -> only roles + the admin account are seeded.

> Tip: already-seeded demo data is left untouched when the flag is toggled off. To start fresh, delete the database (`dotnet ef database drop`) and run again.

## Seed data overview

| Resource | Count |
|---|---|
| AppUser accounts (linked to profiles) | 26 |
| Roles | 3 (`Admin`, `Teacher`, `Student`) |
| Teachers | 6 |
| Students | 20 |
| Courses | 12 |
| Enrollments | ~80 (each student in 4 courses) |
| Grades | ~80 (one per enrollment) |
| Attendance | ~800 (10 records per enrollment) |

## Accounts & credentials

### Admin

| Field | Value |
|---|---|
| Email | `admin@school.com` |
| Password | `Admin@123456` |
| Role | Admin |

> Configured under `SeedAdmin` in `appsettings.Development.json`.

### Teachers (role `Teacher`, password `Teacher@123`)

| # | Name | Email | Specialization | Phone | Age |
|---|---|---|---|---|---|
| 1 | Dr. Ahmed Hassan | teacher1@school.com | Mathematics | 01010000001 | 45 |
| 2 | Ms. Sara Mostafa | teacher2@school.com | Computer Science | 01010000002 | 34 |
| 3 | Mr. Omar El-Sayed | teacher3@school.com | Physics | 01010000003 | 41 |
| 4 | Mrs. Laila Mohamed | teacher4@school.com | English | 01010000004 | 38 |
| 5 | Mr. Karim Adel | teacher5@school.com | Chemistry | 01010000005 | 47 |
| 6 | Ms. Nour Ali | teacher6@school.com | Biology | 01010000006 | 29 |

### Students (role `Student`, password `Student@123`)

| # | Name | Email | Gender | Phone | Age |
|---|---|---|---|---|---|
| 1 | Adam Khaled | student1@school.com | Male | 01110000001 | 21 |
| 2 | Youssef Nabil | student2@school.com | Male | 01110000002 | 20 |
| 3 | Mariam Tarek | student3@school.com | Female | 01110000003 | 22 |
| 4 | Omar Sherif | student4@school.com | Male | 01110000004 | 19 |
| 5 | Salma Hany | student5@school.com | Female | 01110000005 | 20 |
| 6 | Mostafa Emad | student6@school.com | Male | 01110000006 | 23 |
| 7 | Hana Mahmoud | student7@school.com | Female | 01110000007 | 21 |
| 8 | Kareem Samir | student8@school.com | Male | 01110000008 | 22 |
| 9 | Farida Wael | student9@school.com | Female | 01110000009 | 18 |
| 10 | Ziad Ashraf | student10@school.com | Male | 01110000010 | 24 |
| 11 | Laila Gamal | student11@school.com | Female | 01110000011 | 20 |
| 12 | Hassan Ali | student12@school.com | Male | 01110000012 | 21 |
| 13 | Nada Fathy | student13@school.com | Female | 01110000013 | 19 |
| 14 | Mazen Hossam | student14@school.com | Male | 01110000014 | 22 |
| 15 | Rana Yasser | student15@school.com | Female | 01110000015 | 23 |
| 16 | Seif El-Din | student16@school.com | Male | 01110000016 | 20 |
| 17 | Malak Saeed | student17@school.com | Female | 01110000017 | 21 |
| 18 | Bishoy George | student18@school.com | Male | 01110000018 | 24 |
| 19 | Nourhan Adel | student19@school.com | Female | 01110000019 | 19 |
| 20 | Tarek Refaat | student20@school.com | Male | 01110000020 | 20 |

## Courses (assigned to teachers)

| # | Name | Hours | Teacher |
|---|---|---|---|
| 1 | Mathematics 101 | 60 | Dr. Ahmed Hassan |
| 2 | Calculus II | 45 | Dr. Ahmed Hassan |
| 3 | Introduction to Programming | 48 | Ms. Sara Mostafa |
| 4 | Data Structures | 40 | Ms. Sara Mostafa |
| 5 | Physics I | 55 | Mr. Omar El-Sayed |
| 6 | Thermodynamics | 40 | Mr. Omar El-Sayed |
| 7 | English Composition | 30 | Mrs. Laila Mohamed |
| 8 | Technical Writing | 30 | Mrs. Laila Mohamed |
| 9 | Organic Chemistry | 50 | Mr. Karim Adel |
| 10 | Analytical Chemistry | 35 | Mr. Karim Adel |
| 11 | Molecular Biology | 52 | Ms. Nour Ali |
| 12 | Genetics | 38 | Ms. Nour Ali |

## Enrollments

Each student is enrolled in **4 courses** chosen deterministically by course id
via the offsets `[0, 3, 7, 11]` (mod 12). Examples:

| Student | Enrolled in course ids |
|---|---|
| student1 (Adam) | 1, 4, 8, 12 |
| student2 (Youssef) | 2, 5, 9, 1 |
| student3 (Mariam) | 3, 6, 10, 2 |
| ... | ... |

- `EnrollmentDate` is set ~2 months in the past (staggered per student).
- The composite key `(StudentId, CourseId)` guarantees no duplicate enrollments.

## Grades

- Exactly **one grade per enrollment** (enforced by the unique index on `(StudentId, CourseId)`).
- `Score` is between **55 and 100** (deterministic pseudo-random formula).
- `CreatedDate` is set ~14 days ago.

## Attendance

- **10 records per enrollment**, one every other day (spanning ~20 days back from today).
- `Date` is stored date-only (UTC) to match the unique index `(StudentId, CourseId, Date)`.
- Roughly **90% present** (`IsPresent = true`), ~10% absent — deterministic so results are reproducible.

## Seed data vs. business rules

The seeded data intentionally satisfies every API validation rule so it is fully
usable from all endpoints:

- Password policy: min 8 chars + at least one digit (all seeded passwords comply).
- `Student.UserId` / `Teacher.UserId` unique-indexed — every profile has its own user.
- A grade only exists for a student enrolled in that course.
- One attendance record per student/course/day.
- Teachers only own grades/attendance for their own courses (per the teacher-restriction rules).
- `GET /api/Auth/login` works for every account listed above.
