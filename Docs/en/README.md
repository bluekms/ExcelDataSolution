# Sdp Documentation

Documentation for **StaticDataPipeline (Sdp)**, a pipeline library that validates and loads Excel data into C# Records and provides high-speed in-memory lookups.

> This documentation is machine-translated by AI from the Korean original, which is the source of truth.

## Quick Start

If this is your first time, start with the **[Quick Start](./quickstart.md)** — a single page that takes you from defining a Record to loading and querying in five minutes. For what problems Sdp solves and how its flow works, see [1. Introduction](./01-introduction.md).

</br></br></br>

## Table of Contents

### 1. [Introduction](./01-introduction.md)
The problems Sdp solves, its key benefits, and the data flow.

### 2. [Installation](./02-installation.md)
Requirements and how to install.

### 3. Usage
The pipeline taught through examples. Some chapters guide the **data author** when filling in Excel, while the rest guide the **record author** when composing Records, Tables, and Managers. Once the data structure is agreed upon, the two tracks can proceed in parallel without waiting for each other.
- [3.1 Working with Excel](./03-usage/01-record-to-excel.md) — data author perspective
- [3.2 Standard Header Generator](./03-usage/02-header-generator.md) — data author perspective
- [3.3 Defining Your First Record](./03-usage/03-first-record.md) — record author perspective
- [3.4 Implementing StaticDataTable](./03-usage/04-static-data-table.md)
- [3.5 Managing Multiple Tables with StaticDataManager](./03-usage/05-static-data-manager.md)
- [3.6 Foreign Keys (ForeignKey, SwitchForeignKey)](./03-usage/06-foreign-keys.md)
- [3.7 StaticDataView Pre-Generated Views](./03-usage/07-static-data-view.md)

### 4. CLI Tools
How to use the two CLI tools invoked from the build pipeline — command forms, the full set of options, and bat examples.
- [4.1 StaticDataHeaderGenerator](./04-cli-tools/01-header-generator.md)
- [4.2 ExcelColumnExtractor](./04-cli-tools/02-column-extractor.md)

### 5. Advanced
- [5.1 Supported Types (Schemata)](./05-advanced/01-schemata.md)
- [5.2 Attribute Catalog](./05-advanced/02-attributes.md)
- [5.3 Type Branding Pattern](./05-advanced/03-type-branding.md)
- [5.4 Validation Overview](./05-advanced/04-validation.md)

### 6. [License](./06-license.md)

---

We recommend running through the [Quick Start](./quickstart.md) once from start to finish, then returning to the relevant chapter wherever you get stuck. If you prefer to read thoroughly, it is natural to follow along according to your role:

- **Data author**: 1 → 3.1 → 3.2 — these two chapters cover everything you need to author Excel.
- **Record author**: 1 → 3.3 through the end — it is worth going through the Record/Table/Manager/View composition as well as the CLI Tools (Chapter 4) and Advanced (Chapter 5).
