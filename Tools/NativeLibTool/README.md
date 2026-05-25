# NativeLibTool

WinForms tool for converting Unity native plugins into region-specific dependencies.

The output path is the Unity project root. The tool creates these folders automatically:

- `LocalMaven`
- `LocalPods`

Defaults are loaded from:

```text
NativeLibTool.config.json
```

If the file is missing, the tool creates it with internal defaults.

User-entered UI values are cached separately at:

```text
%APPDATA%/NativeLibTool/NativeLibTool.cache.json
```

On startup, cached values override the shared defaults.

## Android

Keep only these inputs in the UI:

- Android source path
- Maven `GroupId`, `ArtifactId`, `Version`

Input can be any of these:

- Android library directory with `AndroidManifest.xml`
- parent folder containing a single supported Android input
- existing `.aar`
- `.jar`
- `.so` file under an ABI folder, such as `arm64-v8a/libfoo.so`
- loose directory with jars, ABI `.so` folders, `res`, `assets`, or `aidl`

The tool generates:

- `*.aar`
- `*.pom`
- `maven-metadata.xml`

The AAR maps Unity/old Android Library layout as follows:

- `AndroidManifest.xml` -> `AndroidManifest.xml`
- `libs/*.jar` -> `classes.jar` or `libs/*.jar`
- `libs/<abi>/*.so` / `jniLibs/<abi>/*.so` -> `jni/<abi>/*.so`
- `res`, `assets`, `aidl` -> same AAR folders
- `proguard-project.txt` / `consumer-proguard-rules.pro` -> `proguard.txt`
- existing `.aar` -> copied into `LocalMaven` as-is, with generated POM/metadata
- `.jar` -> wrapped as `classes.jar` in a minimal AAR
- `.so` -> wrapped under `jni/<abi>/` in a minimal AAR

Gradle example:

```gradle
// settingsTemplate.gradle, inside dependencyResolutionManagement.repositories
def unityProjectPath = $/file:///**DIR_UNITYPROJECT**/$.replace("\\", "/")

maven {
    url (unityProjectPath + "/LocalMaven")
}

// launcherTemplate.gradle, inside dependencies
dependencies {
    implementation "com.uqm.gcloud:crashsight:1.0.0"
}
```

Use the Android `Collect` button to write:

```text
LocalMaven/temp.gradle
```

It scans every `.pom` under `LocalMaven` and writes a copy-friendly Maven repository block plus dependency lines for all collected packages.

## iOS

Keep only these inputs in the UI:

- iOS source directory
- Pod name and version
- minimum iOS version
- system frameworks and libraries

The podspec uses internal defaults for metadata:

- `summary`: `IosSummaryFormat`
- `homepage`: `IosHomepageFormat`
- `license`: `IosLicenseType`
- `author`: `IosAuthorName` / `IosAuthorEmail`
- version directory: `IosGenerateVersionDirectory`
- `static_framework`: `IosStaticFramework`

Input can be either the exact native SDK directory or its parent folder. The tool detects a directory containing `.framework`, `.xcframework`, `.a`, `.bundle`, or `.xcprivacy`, then generates a Local Pod under the selected Unity project root:

- `PodName.podspec`
- `Vendor/` copied from the source directory, excluding Unity `.meta` files

Podfile example:

```ruby
pod 'CrashSightCN', :path => 'D:/fancyHub/UnityLibs/LocalPods/CrashSightCN/1.0.0'
```

For Unity, add this pod only in CN iOS builds during `PostProcessBuild` or through the iOS resolver flow.

Use the iOS `Collect` button to write:

```text
LocalPods/podfile-patch_temp.json
```

It scans every `.podspec` under `LocalPods` and writes all collected pods, with `onlyWhenDefinesContain` set to `UNITY_IOS`.
