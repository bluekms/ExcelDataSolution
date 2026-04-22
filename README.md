# StaticDataPipeline (Sdp)

Sdp is a C# pipeline library for static (master) data. It reads the schema
from your C# record definitions, extracts only the columns those records need
from Excel sheets, and loads the data into memory as immutable collections for
fast, thread-safe lookup.

1. Read the schema from C# record definitions.
2. Extract only the required columns from Excel sheets into CSV.
3. Load the CSV as immutable collections and query it in memory.

## Documentation

- [한국어](./Docs/ko/README.md)
