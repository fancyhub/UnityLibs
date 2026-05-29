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

Android is split into three steps/tabs so each stage has a single job:

- `Android AAR -> Local Maven`: publish an existing `.aar` into `ProjectRoot/LocalMaven`, then write POM and `maven-metadata.xml`.
- `Android JAR/SO -> AAR`: wrap `.jar`, `.so`, or old Android binary/content folders into an AAR only.
- `Android Source -> AAR`: generate an inspectable Android Library Gradle project, then optionally run Gradle to compile Java/C/C++ source into an AAR only.

Use the first tab as the final publishing step for AARs produced by the other Android tabs.
The AAR tab has a `Detect` button for Maven coordinates. It tries `META-INF/maven/**/pom.properties`, then `META-INF/maven/**/pom.xml`, then falls back to the file name pattern `artifact-version.aar`. Most Gradle-built AARs do not store `groupId`, so that field may still need to be filled manually.
Publishing also writes `.md5` and `.sha1` files for the AAR, POM, and metadata files.

Binary/content input can be any of these:

- Android library directory with `AndroidManifest.xml`
- parent folder containing a single supported Android binary/content input
- `.jar`
- `.so` file under an ABI folder, such as `arm64-v8a/libfoo.so`
- loose directory with jars, ABI `.so` folders, `res`, `assets`, or `aidl`

Source input can be a Java/C/C++ source directory. The source tab can first write a Gradle project into the selected `Gradle project` folder for inspection. `Build AAR` regenerates that project and runs `:lib:assembleRelease`; if no project folder is selected, it uses a temporary folder and deletes it after the build.
Set `Unity root/Data` when source imports `com.unity3d.player.UnityPlayer`; the generated Gradle project uses Unity `classes.jar` as a direct `compileOnly` absolute-path dependency.
When `Unity root/Data` is set, the generated project also copies Unity's `gradlew.bat` and `gradle-wrapper.jar` into the standard wrapper layout, and writes a `gradle.bat` helper that runs Unity's embedded Gradle launcher.
Kotlin source is detected and logged, but this tab does not add the Kotlin Gradle plugin automatically.

The AAR maps Unity/old Android Library layout as follows:

- `AndroidManifest.xml` -> `AndroidManifest.xml`
- `libs/*.jar` -> `classes.jar` or `libs/*.jar`
- `libs/<abi>/*.so` / `jniLibs/<abi>/*.so` -> `jni/<abi>/*.so`
- `res`, `assets`, `aidl` -> same AAR folders
- `proguard-project.txt` / `consumer-proguard-rules.pro` -> `proguard.txt`
- `.jar` -> wrapped as `classes.jar` in a minimal AAR
- `.so` -> wrapped under `jni/<abi>/` in a minimal AAR

The source tab maps source folders as follows:

- `src/main/java`, `java`, or old Unity `src` -> temporary `src/main/java`
- `src/main/cpp` or `cpp` -> temporary `src/main/cpp`
- `res`, `assets`, `aidl`, `jniLibs` -> temporary `src/main`
- `libs/*.jar` -> temporary Gradle module `libs`
- `libs/<abi>/*.so` -> temporary `src/main/jniLibs/<abi>`

The generated Gradle project includes:

- `settings.gradle`
- root `build.gradle`
- `lib/build.gradle`
- `lib/src/main/...`
- `gradle.bat` pointing to Unity's embedded Gradle launcher
- `gradlew.bat` and `gradle/wrapper/gradle-wrapper.jar` copied from the Unity installation when available

Unity `classes.jar` is resolved from `Unity root/Data` using paths such as:

```text
<UnityRoot>/Data/Data/PlaybackEngines/AndroidPlayer/Variations/il2cpp/Release/Classes/classes.jar
<UnityData>/PlaybackEngines/AndroidPlayer/Variations/il2cpp/Release/Classes/classes.jar
<UnityData>/Data/PlaybackEngines/AndroidPlayer/Variations/il2cpp/Release/Classes/classes.jar
```

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
- pod dependencies

The podspec uses internal defaults for metadata:

- `summary`: `IosSummaryFormat`
- `homepage`: `IosHomepageFormat`
- `license`: `IosLicenseType`
- `author`: `IosAuthorName` / `IosAuthorEmail`
- version directory: `IosGenerateVersionDirectory`
- `static_framework`: `IosStaticFramework`

Input can be either the exact native SDK directory or its parent folder. The tool detects a directory containing `.framework`, `.xcframework`, `.a`, `.bundle`, `.xcprivacy`, or Objective-C/C/C++ source files (`.m`, `.mm`, `.c`, `.cc`, `.cpp`, `.cxx`, `.h`, `.hh`, `.hpp`), then generates a Local Pod under the selected Unity project root:

- `PodName.podspec`
- `Vendor/` copied from the source directory, excluding Unity `.meta` files

The podspec maps iOS contents as follows:

- `*.framework` / `*.xcframework` -> `s.vendored_frameworks`
- `*.a` -> `s.vendored_libraries`
- `*.m`, `*.mm`, `*.c`, `*.cc`, `*.cpp`, `*.cxx`, headers -> `s.source_files`
- headers -> `s.public_header_files`
- `.bundle`, `.xcassets`, `.plist`, `.storyboard`, `.xib`, `.xcprivacy` -> `s.resources`

Use `Dependencies` when the generated Local Pod source imports another CocoaPod's headers. The field accepts one dependency per line or semicolon-separated:

```text
SolarEngineSDK, >=0
pod 'SolarEngineSDK', '>=0'
s.dependency 'SolarEngineSDK', '>=0'
```

All three forms generate:

```ruby
s.dependency 'SolarEngineSDK', '>=0'
```

Podfile example:

```ruby
pod 'CrashSightCN', :path => 'D:/fancyHub/UnityLibs/LocalPods/CrashSightCN/1.0.0'
```

For Unity, add this pod only in CN iOS builds during `PostProcessBuild` or through the iOS resolver flow.

Use the iOS `Collect` button to write:

```text
LocalPods/custom.podfile
```

It scans every `.podspec` under `LocalPods` and writes a custom Podfile fragment with two targets:

```ruby
target 'Add' do
  pod 'CrashSightCN', :path => '<UnityProject>/LocalPods/CrashSightCN/1.0.0'
end

target 'Remove' do
end
```
