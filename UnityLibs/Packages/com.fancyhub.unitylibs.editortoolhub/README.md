# Editor Tool Hub

`com.fancyhub.unitylibs.editortoolhub` provides an editor-only hub window that can host small tool modules in one editable workspace.

Open it from `Tools/FancyHub/Editor Tool Hub`.

## Code Layers

- `Editor/Core`: stable APIs, module registration, ids, layout data, and layout asset storage.
- `Editor/Window`: the main hub window UI.
- `Editor/Builtin`: optional built-in modules and samples.
- `Editor/Adapters`: generic adapters, including the reflected `EditorWindow` module adapter.

## Layout Assets

The hub can work with a transient `EditorPrefs` layout or a `EditorToolHubLayoutAsset`.

Use the toolbar object field to open an existing layout asset. Use `Save Asset` or `Save As` to persist the current edited layout into an asset under `Assets`.

In edit mode, the workspace is a split tree. Each panel can choose or replace its module, split itself vertically or horizontally, or be removed. The saved asset stores that split tree directly.

## Add A UI Toolkit Module

```csharp
using FH.EditorToolHub;
using UnityEngine.UIElements;

[EditorToolModule("my.company.asset-checker", "Asset Checker", Category = "Project")]
public sealed class AssetCheckerModule : IEditorToolModule
{
    public VisualElement CreateGUI(EditorToolModuleContext context)
    {
        VisualElement root = new VisualElement();
        root.Add(new Label("Asset Checker"));
        return root;
    }
}
```

## Wrap An Existing IMGUI Tool

```csharp
using FH.EditorToolHub;
using UnityEditor;

[EditorToolModule("my.company.old-tool", "Old Tool", Category = "Legacy")]
public sealed class OldToolModule : IMGUIEditorToolModule
{
    protected override void OnGUI(EditorToolModuleContext context)
    {
        if (GUILayout.Button("Run"))
        {
            // Existing EditorWindow OnGUI logic can live here.
        }
    }
}
```

## Replace A Reflected EditorWindow Module

Existing project `EditorWindow` types are exposed as reflected modules when they define `CreateGUI` or `OnGUI`. The reflected module renders the window content directly inside the panel. If you later write a real module for that window, mark it as the replacement:

Unity built-in windows are exposed conservatively through a whitelist. The current list includes Project, Inspector, Hierarchy, Console, Scene, Game, Animation, and Profiler.

```csharp
using FH.EditorToolHub;
using UnityEngine.UIElements;

[EditorToolModule("my.company.asset-checker", "Asset Checker", ReplacesEditorWindowType = typeof(AssetCheckerWindow))]
public sealed class AssetCheckerModule : IEditorToolModule
{
    public VisualElement CreateGUI(EditorToolModuleContext context)
    {
        return new Label("Custom module wins over the reflected window adapter.");
    }
}
```

The hub discovers modules through `TypeCache`, so modules can live in this package or in other editor assemblies. If the module is compiled by another asmdef, add an asmdef reference to `fancyhub.editortoolhub.editor`.
