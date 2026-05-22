# 5.4 Validation Overview

This page summarizes, in a single page, at which point in the pipeline Sdp's validation operates. For the details of each individual Attribute, see [5.2 Attribute Catalog](./02-attributes.md), and for the entire data flow, see [1. Introduction](../01-introduction.md).

## Three validation points

|Point|Performed by|Target|
|-|-|-|
|**Declaration check**|`ExcelColumnExtractor` · `StaticDataHeaderGenerator`|Defects in the Record/Attribute **declaration** itself|
|**Extraction**|`ExcelColumnExtractor`|Whether the Excel **cell value** is compatible with the schema and Attributes|
|**Load**|`StaticDataManager.LoadAsync`|Structure, value, and reference checks at runtime load time|

The declaration check and extraction operate in the build pipeline (offline), and load operates at application runtime. Because the declaration check analyzes `.cs` sources, it is performed whenever either `ExcelColumnExtractor` or `StaticDataHeaderGenerator` runs, and it does not operate at runtime where there is no source.

</br></br></br>

## Operating point per check item

`O` means the check is performed at that point, and `—` means it is not performed.

### A. Type and structure schema

|Check item|Declaration check|Extraction|Load|Note|
|-|-|-|-|-|
|Whether the type is supported|O|—|—|Rejects unsupported types|
|No nullable on the collection itself|O|—|—|`ImmutableArray<T>?`·`FrozenSet<T>?`·`FrozenDictionary<,>?`|
|No nullable Record elements|O|—|—|Nullable in a Record array/set/Map Value|
|No circular references|O|—|—|A Record directly or indirectly contains itself|
|Map Key non-nullable|O|—|—||
|Map Key↔Value `[Key]` type match|O|—|—||

### B. Attribute consistency

|Check item|Declaration check|Extraction|Load|Note|
|-|-|-|-|-|
|`[Length]` required|O|—|—|Multi-column collection|
|`[NullString]` required / misuse|O|—|—|Required for nullable, forbidden for non-nullable|
|`[DateTimeFormat]` required / misuse|O|—|—|Required for `DateTime`, forbidden for non-DateTime|
|`[TimeSpanFormat]` required / misuse|O|—|—|Required for `TimeSpan`, forbidden for non-TimeSpan|
|`[RegularExpression]` type|O|—|—|`string` / `string?` only|
|`[Range]` applicable type|O|—|—|`bool`·collection·record forbidden|
|`[CountRange]` consistency|O|—|—|`[SingleColumnCollection]` required, `[Length]` exclusive, minCount>0|
|`[SingleColumnCollection]` consistency|O|—|—|Not allowed on Map, elements primitive only|
|`[Key]` consistency|O|—|O|At most 1 per Record and non-nullable is the declaration check / the presence of `[Key]` in a Map Value is both the declaration check and load|
|`[StaticDataRecord]` presence|O|O|O|Identifies the declaration check target / terminates when there are 0 extraction targets / required at load|

### C. Cell value

|Check item|Declaration check|Extraction|Load|Note|
|-|-|-|-|-|
|Header presence|—|O|O||
|Cell value-type compatibility|—|O|O|Load detects this through a conversion (`Convert.ChangeType`) failure|
|enum member name validity|—|O|O|Load uses `Enum.IsDefined` — a `[Key]` enum is omitted|
|`DateTime`/`TimeSpan` format match|—|O|O|`ParseExact` with the same format on both sides|
|`[Range]` value range|—|O|O||
|`[RegularExpression]` pattern match|—|O|O||
|`[CountRange]` split element count|—|O|O|Single-column collection|
|Primary Key duplication within the sheet|—|O|—|`[Key]` column. Load has no automatic check — `UniqueIndex` guarantees it opt-in|

### D. Foreign keys

|Check item|Declaration check|Extraction|Load|Note|
|-|-|-|-|-|
|No simultaneous `[ForeignKey]`·`[SwitchForeignKey]` attachment|O|—|O|Checked on both sides|
|No duplicate `[SwitchForeignKey]` conditions|O|—|O|Checked on both sides|
|FK target TableSet exists|—|—|O|A name typo is detected by the type check before load, and a reference to a table dropped via `disabledTables` is detected by the reference validation after load (both `FkTargetNotFound`)|
|FK target is not a `[SingleColumnCollection]`|—|—|O||
|FK target column exists|—|—|O||
|`[SwitchForeignKey]` condition column exists|—|—|O||
|`[SwitchForeignKey]` condition value branch match|—|—|O||
|FK reference value exists|—|—|O|Actual referential integrity|

### E. Load structure

|Check item|Declaration check|Extraction|Load|Note|
|-|-|-|-|-|
|TableSet single constructor|—|—|O||
|Table parameter type|—|—|O|Whether it is `StaticDataTable<,>`|
|Table constructor (`ImmutableArray`) exists|—|—|O||
|ViewSet single constructor|—|—|O||
|View parameter type and non-nullable|—|—|O||
|View constructor (`TableSet`) exists|—|—|O||
|No concurrent entry into `LoadAsync`|—|—|O||
|`UniqueIndex` key duplication|—|—|O|At table/view creation, opt-in|

### F. User-defined validation

|Check item|Declaration check|Extraction|Load|Note|
|-|-|-|-|-|
|Table self-validation|—|—|O|`StaticDataTable.Validate()` override, immediately after table instantiation|
|Manager cross-validation|—|—|O|`StaticDataManager.Validate(TTableSet)` override, after all FK validation|
|View self-validation|—|—|O|`StaticDataView.Validate()` override, immediately after view build|

</br></br></br>

## Only CSVs that have passed extraction go to production

Among the cell value checks (C), `[Range]`·`[RegularExpression]`·`[CountRange]` operate by the same rules in both extraction and load. However, Primary Key duplication and the declaration consistency checks (A·B) are performed only at the extraction stage (including the declaration check). If you feed a CSV that has not gone through the extraction stage directly into the runtime, these checks are skipped, so a healthy pipeline always puts only CSVs that have passed the extractor into production.

---

[← Previous: 5.3 Type Branding Pattern](./03-type-branding.md) | [Table of Contents](../README.md) | [Next: 6. License →](../06-license.md)
