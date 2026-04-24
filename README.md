# SwiftAPI — MT103 Reader API

SwiftAPI is a minimal ASP.NET Core Web API that reads SWIFT MT103 message files, parses transactions, stores them in a local SQLite database (without Entity Framework), and exposes simple HTTP endpoints for upload and retrieval. Logging is included across service and data layers.

---

## Tech Stack

- **.NET 10** — ASP.NET Core Minimal API  
- **SQLite** — via `Microsoft.Data.Sqlite`  
- **No ORM** — manual SQL only  
- **Swagger** — enabled in Development  
- **Logging** — `ILogger<T>`

---

## Features

- Upload `.txt` or `.mt` files containing MT103 messages
- Parse MT103 fields into structured entities
- Persist transactions to SQLite
- Retrieve single or all MT103 records via HTTP

---

## API Endpoints

- **POST `/mt103/upload`**  
  Upload MT103 file (`multipart/form-data`)

- **GET `/mt103/{id}`**  
  Retrieve a single MT103 by ID

- **GET `/allMt103`**  
  Retrieve all MT103 records (ordered by newest first)

---

## MT103 Processing Flow

1. File uploaded via controller
2. File read fully into memory
3. Messages split using `{1:` boundaries
4. MT103 transactions identified by presence of `:20:` tag
5. Fields parsed using line-based tag detection:
   - `:20:` Transaction Reference Number  
   - `:23B:` Bank Operation Code  
   - `:32A:` Value Date, Currency, Amount  
   - `:50K:` Ordering Customer (multi-line)  
   - `:59:` Beneficiary Customer  
   - `:71A:` Details of Charges  
6. Parsed MT103 objects stored in SQLite

---

## Data Storage

- SQLite database file: `swift.db`
- Created automatically on startup
- Manual SQL (`CREATE TABLE`, `INSERT`, `SELECT`)
- Each record includes:
  - Parsed MT103 fields
  - Raw message
  - UTC receive timestamp

---

## Logging

- Uses built-in ASP.NET Core logging
- Logs parsing steps, inserts, and query operations
- Output goes to console/host only (no file sink configured)

---

## Running the Application

```bash
dotnet run
```
- Files used for testing Example.mt && Example.txt
