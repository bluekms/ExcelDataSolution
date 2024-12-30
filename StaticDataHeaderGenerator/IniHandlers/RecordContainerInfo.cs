using SchemaInfoScanner.NameObjects;

namespace StaticDataHeaderGenerator.IniHandlers;

public sealed record RecordContainerInfo(RecordName RecordName, HashSet<string> LengthRequiredHeaderNames);
