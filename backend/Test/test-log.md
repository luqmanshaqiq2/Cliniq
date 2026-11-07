# Test Log

This document summarizes the test cases covered across the four test files in the backend test project.

## AuthTest.cs

Covers authentication and security behavior for the API.

- Register returns a public profile without exposing secrets
- Duplicate email registration is rejected regardless of case
- Register rejects invalid fields such as blank name, invalid email, short password, and null values
- Register rejects passwords over the supported length limit
- Login returns the same generic error for wrong password and unknown email
- Login does not expose password or password hash in the response
- Login rejects null or empty credentials
- Injection strings in login inputs are treated as data rather than executable payloads
- Register stores SQL and script injection strings as ordinary data
- Login enforces request rate limiting after the configured per-IP threshold

## DoctorManagementTests.cs

Covers doctor management and lookup behavior.

- A doctor can be created successfully
- A created doctor can be fetched by ID
- A doctor is returned in the available-doctors list when a matching availability slot exists
- Requesting a missing doctor returns 404 Not Found

## PatientWorkflowTests.cs

Covers patient lifecycle and data-handling behavior.

- A patient can be created successfully
- Search returns matching patients by searchable fields
- Possible duplicate detection identifies matching patient records
- Anonymize redacts personal information such as full name, contact number, and email

## AppointmentWorkflowTests.cs

Covers appointment booking, rescheduling, and conflict handling.

- A doctor appointment can be created and later rescheduled to a different slot
- Booking an appointment with a valid slot succeeds
- Rescheduling updates the appointment to the new time window
- Canceling the appointment succeeds with a valid reason
- Overlapping appointments are rejected to prevent double booking

## Overall Result

The test suite validates the primary flows of authentication, patient management, doctor lookup, and appointment scheduling in the Cliniq API.
