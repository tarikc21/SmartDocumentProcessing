# Smart Document Processing System

## Overview

This application is built using ASP.NET Core Blazor Server and is designed to process business documents such as invoices.

The system focuses on extracting structured data from imperfect inputs, validating it, and allowing users to review and correct the results.

---

## Features

### Document Ingestion

* Upload support for:

  * TXT (semi-structured documents)
  * CSV (structured documents with line items)
  * PDF (text-based documents)

### Data Extraction

* Extracts key fields:

  * Document type
  * Supplier name
  * Document number
  * Currency
  * Total amount
  * Line items (CSV)
  * Issue date
  * Due date
  * Subtotal and tax

### Validation Engine

* Automatically detects:

  * Missing required fields
  * Duplicate document numbers
  * Invalid totals
  * Data inconsistencies
  * Line item total validation
  * Subtotal and total consistency check

### Review Interface

* Manual correction of extracted data
* Re-validation after edits
* Ability to reject invalid documents

### Dashboard

* Overview of all processed documents
* Status tracking:

  * Uploaded
  * Needs Review
  * Validated
  * Rejected
* Display of validation issues

### Data Storage

* SQLite database
* Entity Framework Core (Code First approach)

---

## Technologies

* ASP.NET Core Blazor Server
* Entity Framework Core
* SQLite
* C#
* Bootstrap

---

## Running the Application

1. Clone the repository
2. Open the project in Visual Studio 2022
3. Run the application
4. Navigate to:

   * `/` – Home page
   * `/upload` – Upload documents
   * `/dashboard` – View and manage documents

---

## Screenshots

### Home

![Home](screenshots/home.png)

### Upload

![Upload](screenshots/upload.png)

### Dashboard

![Dashboard](screenshots/dashboard.png)

### Review

![Review](screenshots/review.png)

---

## AI Usage

AI tools were used as assistance for:

* project structure guidance
* debugging
* UI improvements

All implemented logic, validation rules, and functionality are fully understood and manually integrated.

---

## Future Improvements

* PDF document support
* OCR integration for image-based documents
* Improved UI/UX
* Advanced validation rules
* REST API support

---

## Notes

The system is designed to work with incomplete and semi-structured data, prioritizing validation and user correction over perfect extraction.

PDF extraction works for text-based PDFs. OCR (image-based PDFs) is not supported.
