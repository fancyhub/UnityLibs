# Fancyhub Protobuf

This package contains a small protobuf runtime plus two code-generation modes:

- `ForceGenerate`: reads `.proto` files and writes message data plus serialization code together in `.pb.cs`.
- `CompileTime`: reads `.proto` files and writes attributed message data classes to `.pb.cs`; the Roslyn generator emits serialization code during compilation.

Both modes use the same serialization method generator from `FH.Protobuf.SourceGenerator.dll`.

## Attribute-based generation

The Roslyn source generator is shipped as `Runtime/RoslynAnalyzers/FH.Protobuf.SourceGenerator.dll` and is labeled `RoslynAnalyzer` for Unity. Any assembly that references `fancyhub.protobuf` can use the attributes below.

```csharp
using System.Collections.Generic;
using FH;

[PBMessage]
public partial class LoginReq
{
    [PBField(1)]
    public long AccountId;

    [PBField(2)]
    public string Token { get; set; } = string.Empty;

    [PBField(3, Type = EPBFieldType.SInt32)]
    public int ScoreDelta;

    [PBField(4)]
    public List<int> Items = new List<int>();

    [PBField(5)]
    public Dictionary<int, string> Labels = new Dictionary<int, string>();
}
```

The generator emits `Serialize(PBWriter writer)` and `Unserialize(PBReader reader)` into a generated partial class and adds `IPBMessage` through that partial declaration.

Requirements:

- The message type and containing types must be non-generic `partial class` declarations.
- Generated members must be writable fields or properties.
- Repeated fields use `List<T>`.
- Map fields use `Dictionary<TKey, TValue>`.
- Scalar defaults are inferred from the C# type. Use `PBFieldAttribute.Type` for protobuf-specific encodings such as `SInt32`, `Fixed64`, or `SFixed32`.
- Map fields can use `PBFieldAttribute.KeyType` and `PBFieldAttribute.ValueType` when the key or value uses a protobuf-specific encoding.

To disable attribute generation for a project that supports analyzer config options:

```ini
build_property.FHProtobufAttributeGeneration = false
```

## Proto generation

Use `Tools/Protobuf/Generate C#` and choose one of the two modes:

- `ForceGenerate` writes data fields and codec methods into the same generated class. The data classes do not carry `[PBMessage]` or `[PBField]` attributes.
- `CompileTime` writes data classes with `[PBMessage]`, `[PBField]`, and `IPBMessage`; Unity's Roslyn analyzer compiles the codec methods later.

`PBProtoCodeGenerator` still dispatches through `IPBProtoCodeGenBackend`. The built-in backend is `csharp`; future languages can be added by implementing the interface and calling:

```csharp
PBProtoCodeGenerator.RegisterBackend(myBackend);
```

Backends receive parsed `PBProtoFile` models and return a list of relative output files, so language-specific layout stays outside the generator coordinator.

## Rebuilding the Roslyn analyzer

The analyzer source lives under `Roslyn~/FH.Protobuf.SourceGenerator` so Unity does not import the source project.

```powershell
dotnet build Packages\com.fancyhub.unitylibs.protobuf\Roslyn~\FH.Protobuf.SourceGenerator\FH.Protobuf.SourceGenerator.csproj -c Release
```

After rebuilding, copy the generated DLL from `bin\Release\netstandard2.0` to `Runtime\RoslynAnalyzers\FH.Protobuf.SourceGenerator.dll`.
