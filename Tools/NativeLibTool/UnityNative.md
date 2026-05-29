# Unity 里面增加 Native 的方法

这份文档按平台和文件后缀整理 Unity 接入 Native 能力的方式，重点回答两个问题：

- 这个文件或目录本质是什么。
- 要怎么放进 Unity，并在构建或运行时生效。

## 总览

Unity 里的 Native 插件本质上是“平台相关代码或平台相关资源”。Unity 会把特定后缀的文件或目录当成 Plugin Asset 管理，并通过 `.meta` 里的 Plugin Inspector 设置决定它在哪些平台、哪些 CPU 架构、哪些构建条件下生效。

常见调用方式有三类：

- C/C++/Objective-C 暴露 C ABI 函数，C# 用 `[DllImport]` 调用。
- Android Java/Kotlin 暴露类和方法，C# 用 `AndroidJavaClass` / `AndroidJavaObject` 调用。
- WebGL JavaScript 用 `.jslib` 暴露函数，C# 用 `[DllImport("__Internal")]` 调用。

推荐放置目录：

```text
Assets/Plugins/Android
Assets/Plugins/iOS
Assets/Plugins/x86
Assets/Plugins/x86_64
Assets/Plugins/WebGL
```

也可以放在 `Assets` 下的其他目录，再手动在 Plugin Inspector 里勾选平台。放在平台目录下的好处是 Unity 会自动给出更合理的默认平台设置。

## 通用规则

### Plugin Inspector 是入口

把插件文件放到 `Assets` 后，在 Unity Project 窗口选中文件，可以在 Inspector 里配置：

- `Select platforms for plugin`：插件在哪些平台进入构建。
- `Platform settings`：CPU 架构、OS、iOS framework dependency、是否加入 Embedded Binaries 等。
- `Load on startup`：部分 Native 插件可以在脚本调用前加载。

注意：Native 插件一旦被 Unity Editor 加载，通常不能在同一个 Editor session 里卸载。替换 dll/so/bundle 后，如果行为异常，重启 Unity 是很常见的排查步骤。

### C# 调用 Native C ABI

Native 函数最好暴露为 C ABI，避免 C++ name mangling：

```cpp
extern "C" {
    float FooPluginFunction();
}
```

C# 侧：

```csharp
using System.Runtime.InteropServices;

public static class NativeApi
{
#if UNITY_IOS && !UNITY_EDITOR
    private const string LibraryName = "__Internal";
#else
    private const string LibraryName = "FooPlugin";
#endif

    [DllImport(LibraryName)]
    private static extern float FooPluginFunction();
}
```

命名规则：

- Windows: `FooPlugin.dll` -> `[DllImport("FooPlugin")]`
- macOS: `FooPlugin.bundle` / `libFooPlugin.dylib` -> 通常 `[DllImport("FooPlugin")]`
- Linux: `libFooPlugin.so` -> `[DllImport("FooPlugin")]`
- Android: `libFooPlugin.so` -> `[DllImport("FooPlugin")]`
- iOS: 静态链接进最终 app -> `[DllImport("__Internal")]`

也就是说，`DllImport` 里一般不写 `lib` 前缀，也不写 `.dll` / `.so` / `.bundle` 后缀。

## Android

Android Native 接入方式比较多，核心区别是“已经编译好的包”还是“让 Unity/Gradle 在构建时编译”。

### `.aar`

本质：Android Archive。它是一个 zip 格式的 Android Library 成品包，可以包含：

- Java/Kotlin 编译后的字节码，通常在 `classes.jar`。
- Android 资源，例如 `res/`。
- AndroidManifest 片段，例如 `AndroidManifest.xml`。
- Native so，例如 `jni/arm64-v8a/libxxx.so`。
- assets、consumer ProGuard rules 等。

添加方式：

```text
Assets/Plugins/Android/libs/xxx.aar
```

或放在 `Assets/Plugins/Android` 下，再在 Inspector 里勾选 Android。

怎么调用：

- AAR 里的 Java/Kotlin 类：用 `AndroidJavaClass` / `AndroidJavaObject`。
- AAR 里的 `.so`：确保 so 在 AAR 的 `jni/<abi>/` 下，然后 C# 用 `[DllImport("xxx")]`。

适合场景：

- 第三方 Android SDK。
- 带 Android 资源、Manifest、ProGuard 配置的库。
- 希望作为一个成品库分发，不希望 Unity 项目直接维护 Android 源码。

常见坑：

- AAR 的依赖不会自动从 Maven 下载，除非额外配置 Gradle/EDM4U/自定义仓库。
- AAR 内部如果依赖别的 AAR/JAR，需要一起接入或通过 Gradle dependency 声明。
- ABI 要覆盖项目目标架构，例如 `arm64-v8a`。

### `.androidlib`

本质：Android Library Project 目录，不是单个文件。目录名必须以 `.androidlib` 结尾，Unity 会把它当作一个 Android Gradle 子工程参与构建。

典型结构：

```text
MySdk.androidlib/
  AndroidManifest.xml
  build.gradle
  src/main/java/...
  src/main/res/...
  libs/...
```

添加方式：

```text
Assets/Plugins/Android/MySdk.androidlib
```

怎么调用：

- Java/Kotlin 类：用 `AndroidJavaClass` / `AndroidJavaObject`。
- 里面编译或携带的 `.so`：用 `[DllImport("xxx")]`。

适合场景：

- 需要保留 Android Studio Library Project 结构。
- 有资源、Manifest、Java/Kotlin 源码、Gradle 配置一起构建。
- SDK 还没有打成 AAR，或者需要在 Unity 构建时根据 Gradle 配置编译。

常见坑：

- Gradle 配置要和 Unity 使用的 Android Gradle Plugin 版本兼容。
- 复杂依赖更适合发布成 Maven/AAR，再通过 Gradle dependency 引入。

### `.jar`

本质：Java Archive，只包含 JVM 字节码和普通资源，不包含 Android `res/` 资源表，也不适合携带 AndroidManifest 合并内容。

添加方式：

```text
Assets/Plugins/Android/xxx.jar
```

选中文件，在 Inspector 里勾选 Android。

怎么调用：

```csharp
using (var clazz = new AndroidJavaClass("com.company.sdk.Foo"))
{
    clazz.CallStatic("bar");
}
```

适合场景：

- 纯 Java 工具库。
- 不需要 Android `res/`、Manifest、native so。
- 复用少量 Java class。

常见坑：

- 需要 Android 资源时不要用 JAR，改用 AAR 或 `.androidlib`。
- Java 版本和 Unity 内置 JDK/Gradle 兼容性要注意。

### `.java` / `.kt`

本质：Android Java/Kotlin 源码文件。Unity 可以把单个 Java/Kotlin 源码文件当作 Android 插件，在 Android 构建时交给 Gradle 编译。

添加方式：

```text
Assets/Plugins/Android/src/com/company/sdk/Foo.java
Assets/Plugins/Android/src/com/company/sdk/Foo.kt
```

或放在 `Assets` 下其他普通目录，然后在 Inspector 里勾选 Android。

怎么调用：

```csharp
using (var clazz = new AndroidJavaClass("com.company.sdk.Foo"))
{
    clazz.CallStatic("bar");
}
```

适合场景：

- 项目内少量 Android 胶水代码。
- 不想为了几行 Java/Kotlin 单独建 Android Studio Library。

常见坑：

- 不要把源码放进 `StreamingAssets` 这类特殊目录，否则 Unity 不会按插件处理。
- Kotlin 可能需要额外 Gradle/Kotlin 插件配置，具体取决于 Unity 版本和工程模板。
- 复杂 Android SDK 仍建议用 AAR 或 `.androidlib`。

### `.so`

本质：Android/Linux ELF 动态库。Android 上通常来自 NDK 编译，文件名一般是 `libxxx.so`，每个 ABI 都要有对应产物。

添加方式：

```text
Assets/Plugins/Android/arm64-v8a/libfoo.so
Assets/Plugins/Android/armeabi-v7a/libfoo.so
Assets/Plugins/Android/x86_64/libfoo.so
```

也可以放到 `Assets/Plugins/Android` 下，然后在 Inspector 里手动设置 CPU 架构。

怎么调用：

```csharp
[DllImport("foo")]
private static extern int Foo_Init();
```

适合场景：

- 只有 C/C++ native 动态库，没有 Java 层和 Android 资源。
- SDK 的 Java 层由别的 AAR/JAR 提供，so 只是 native 依赖。

常见坑：

- `DllImport` 写 `foo`，不要写 `libfoo.so`。
- C++ 方法需要 `extern "C"` 暴露，或者提供 C wrapper。
- ABI 不匹配会在运行时 `DllNotFoundException` 或加载失败。
- so 依赖另一个 so 时，要确保依赖库也被打进同一 ABI 目录。

### `.a`

本质：C/C++ 静态库。Android 下只在特定构建方式里有意义，通常和 IL2CPP、C/C++ source plugin 一起链接。

添加方式：

```text
Assets/Plugins/Android/libfoo.a
```

选中文件，在 Inspector 里勾选 Android，并确认 CPU 架构。

怎么调用：

静态库最终会被链接进目标二进制，C# 侧通常按 `__Internal` 调用：

```csharp
[DllImport("__Internal")]
private static extern int Foo_Init();
```

适合场景：

- 已经有 Android NDK 静态库，并且项目使用 IL2CPP。
- 想把 native 代码静态链接进最终产物。

常见坑：

- 对 Mono/IL2CPP、Unity 版本、链接参数更敏感。
- 如果不是特别需要，Android SDK 分发通常优先 `.aar` + `.so`。

### `.c` / `.cc` / `.cpp` / `.h`

本质：C/C++ 源码。Android 下使用 IL2CPP 时，Unity 可以把这些源码和 IL2CPP 生成代码一起编译。

添加方式：

```text
Assets/Plugins/Android/foo.cpp
Assets/Plugins/Android/foo.h
```

选中文件，在 Inspector 里勾选 Android。

怎么调用：

```cpp
extern "C" int Foo_Init()
{
    return 1;
}
```

```csharp
[DllImport("__Internal")]
private static extern int Foo_Init();
```

适合场景：

- 少量 C/C++ 源码，想直接随 Unity 构建。
- 不想维护独立 NDK 工程。

常见坑：

- 依赖复杂、需要 CMake/NDK 配置时，建议独立编译成 `.so` 或打进 AAR。
- C++ 一定注意 `extern "C"`。

### Android 资源和 Manifest

如果 SDK 需要 Android `res/`、`assets/`、`AndroidManifest.xml`，推荐方式是：

- 用 `.aar`。
- 用 `.androidlib`。

不推荐长期维护裸的 `Assets/Plugins/Android/res` 资源目录。它在新 Unity/Gradle 构建链里更容易触发资源和 Manifest 合并问题。

### Maven / Gradle dependency

本质：不是 Unity 插件文件，而是 Android Gradle 依赖声明。Unity 构建 Android 时最终也是 Gradle 工程，所以可以通过 Gradle 仓库和 dependency 引入 native SDK。

添加方式常见有三种：

- EDM4U dependency XML，让 Resolver 下载 AAR/JAR 到项目。
- 修改 `settingsTemplate.gradle` / `mainTemplate.gradle` / `launcherTemplate.gradle`，加入 Maven 仓库和 dependency。
- 使用本工具生成 `LocalMaven`，再在 Gradle 模板里引用本地 Maven。

适合场景：

- SDK 本来就是 Maven 坐标，例如 `group:artifact:version`。
- 同一个依赖要给多个 flavor 或构建变体控制开关。
- 不希望把所有 AAR/JAR 直接提交到 `Assets/Plugins/Android`。

### 转成 `LocalMaven`

本质：`LocalMaven` 是一个放在 Unity 项目根目录下的本地 Maven 仓库。它不是 Unity 插件目录，而是给 Gradle 读取的依赖仓库。Unity 构建 Android 时，Gradle 像读取远程 Maven 一样读取这个本地目录。

目标形态：

```text
<UnityProjectRoot>/LocalMaven/
  com/company/foo/
    foo/
      1.0.0/
        foo-1.0.0.aar
        foo-1.0.0.pom
      maven-metadata.xml
```

Gradle 里引用的是 Maven 坐标：

```gradle
implementation "com.company.foo:foo:1.0.0"
```

也可以按项目 flavor 改成：

```gradle
chinesebaseImplementation "com.company.foo:foo:1.0.0"
```

转换判断表：

| 输入格式 | 能不能转成 `LocalMaven` | 怎么改 |
| --- | --- | --- |
| 旧 Unity Android 插件目录 | 可以，最适合本工具 | 用 NativeLibTool 打成 AAR，并生成 POM、`maven-metadata.xml` |
| `.aar` | 可以，工具已支持 | 工具会复制 AAR 到 Maven 目录，并生成 POM、`maven-metadata.xml` |
| `.jar` | 可以，工具已支持 | 工具会包成最小 AAR，把 JAR 放到 `classes.jar` |
| `.so` | 可以，工具已支持有条件转换 | `.so` 必须在 ABI 父目录下，例如 `arm64-v8a/libfoo.so`；工具会包进 AAR 的 `jni/<abi>/` |
| `.androidlib` | 不建议直接用本工具转 | 先用 Gradle/Android Studio 编译成 AAR，再发布到 `LocalMaven` |
| `.java` / `.kt` | 不适合直接转 | 先创建 Android Library，把源码编译成 AAR，再发布 |
| `.c` / `.cpp` / `.h` | 不适合直接转 | 先用 NDK/CMake 编译成 `.so`，再放进 AAR |
| `res/` / `assets` / `aidl` / `AndroidManifest.xml` | 可以，工具已支持目录打包 | 用本工具打成 AAR；没有 Manifest 时工具会生成最小 Manifest |
| 远程 Maven 坐标 | 可以镜像，但不是简单复制一个文件 | 下载 artifact、POM 和所有传递依赖，按 Maven 结构放进 `LocalMaven` |

容易直接转的格式：

- 已经是 `.aar` 文件。
- 纯 `.jar` 文件，或目录里有 `*.jar` / `libs/*.jar`。
- `.so` 文件在标准 ABI 目录下，例如 `arm64-v8a/libfoo.so`。
- 目录里已经有 `AndroidManifest.xml`，并且 Java 已经编译成 `libs/*.jar`。
- Native 库已经是 `libs/<abi>/*.so` 或 `jniLibs/<abi>/*.so`。
- 资源已经是标准 Android `res/`、`assets/`、`aidl/` 目录。
- 不需要在转换阶段编译 Java/Kotlin/C++。

不好直接转的格式：

- `.androidlib` 里还有 `src/main/java`、`src/main/kotlin`，需要 Gradle 编译。
- SDK 依赖 annotation processor、Kotlin plugin、DataBinding、AIDL 生成代码等 Gradle 构建能力。
- SDK 只是 `.java` / `.kt` / `.cpp` 源码，还没有形成 Android Library 产物。
- SDK 依赖远程 Maven 包，但本地没有把传递依赖一起镜像下来。

这些不好直接转的格式，要先变成标准 AAR：

```text
Android source / .androidlib / Java / Kotlin / C++ source
  -> Android Studio 或 Gradle 构建
  -> xxx.aar
  -> 发布到 LocalMaven
  -> Gradle dependency 引用
```

NativeLibTool 现在把 Android 转换拆成三个页签：

1. `Android AAR -> Local Maven`：只做一件事，把现成 `.aar` 发布到 `LocalMaven`，并生成 `.pom`、`maven-metadata.xml` 和 `temp.gradle`。
2. `Android JAR/SO -> AAR`：把 `.jar`、`.so`、旧 Unity Android 插件目录或 loose content 目录打成 `.aar`，不自动发布到 `LocalMaven`。
3. `Android Source -> AAR`：把 Java/C/C++ 源码目录生成可检查的 Android Library Gradle 工程，也可以继续调用 Gradle 编译成 `.aar`，不自动发布到 `LocalMaven`。

推荐流程：

```text
.aar
  -> Android AAR -> Local Maven

.jar / .so / old Android library directory
  -> Android JAR/SO -> AAR
  -> Android AAR -> Local Maven

Java/C/C++ source directory
  -> Android Source -> AAR
  -> Android AAR -> Local Maven
```

发布到 `LocalMaven` 时需要填 Maven 坐标：

- `GroupId`：例如 `com.company.foo`
- `ArtifactId`：例如 `foo`
- `Version`：例如 `1.0.0`

`Android AAR -> Local Maven` 页签有 `Detect` 按钮，会按顺序尝试：

1. 读取 AAR 内的 `META-INF/maven/**/pom.properties`。
2. 读取 AAR 内的 `META-INF/maven/**/pom.xml`。
3. 从文件名 `artifact-version.aar` 推断 `ArtifactId` 和 `Version`。

普通 Gradle 直接编译出来的 AAR 通常不包含 Maven 坐标，特别是 `GroupId`。这种情况下工具只能从文件名猜 `ArtifactId` / `Version`，`GroupId` 仍然需要手填。发布时工具会为 AAR、POM、`maven-metadata.xml` 同时生成 `.md5` 和 `.sha1`。

工具打 AAR 时的映射规则：

| 原目录内容 | AAR 内部位置 |
| --- | --- |
| `AndroidManifest.xml` | `AndroidManifest.xml` |
| `libs/*.jar` | 第一个/唯一 jar 变成 `classes.jar`，其他 jar 放到 `libs/*.jar` |
| `libs/<abi>/*.so` | `jni/<abi>/*.so` |
| `jniLibs/<abi>/*.so` | `jni/<abi>/*.so` |
| `res/` | `res/` |
| `assets/` | `assets/` |
| `aidl/` | `aidl/` |
| `proguard-project.txt` | `proguard.txt` |
| `consumer-proguard-rules.pro` | `proguard.txt` |
| single `.jar` file | `classes.jar` in a minimal AAR |
| single `.so` file | `jni/<abi>/<name>.so` in a minimal AAR |

源码页签的临时 Gradle 工程映射规则：

| 原目录内容 | 临时 Gradle 工程位置 |
| --- | --- |
| `src/main/java` / `java` / old Unity `src` | `lib/src/main/java` |
| `src/main/cpp` / `cpp` / loose C/C++ files | `lib/src/main/cpp` |
| `src/main/AndroidManifest.xml` / `AndroidManifest.xml` | `lib/src/main/AndroidManifest.xml` |
| `res/` | `lib/src/main/res` |
| `assets/` | `lib/src/main/assets` |
| `aidl/` | `lib/src/main/aidl` |
| `jniLibs/<abi>/*.so` | `lib/src/main/jniLibs/<abi>/*.so` |
| `libs/<abi>/*.so` | `lib/src/main/jniLibs/<abi>/*.so` |
| `libs/*.jar` | `lib/libs/*.jar` |

源码页签可以先单独生成类似这样的 Gradle 工程：

```text
TempGradleProject/
  settings.gradle
  build.gradle
  gradle.bat
  gradlew.bat
  gradle/
    wrapper/
      gradle-wrapper.jar
  lib/
    build.gradle
    src/main/
      AndroidManifest.xml
      java/
      cpp/
      res/
      assets/
      aidl/
      jniLibs/
```

`Gradle project` 输入框可以指定这个工程的输出目录。点击 `Generate Project` 只生成工程，方便检查 `build.gradle`、`CMakeLists.txt`、源码复制位置和依赖是否正确；点击 `Build AAR` 会生成/更新工程，然后执行：

```text
gradle.bat :lib:assembleRelease --stacktrace
```

如果没有指定 `Gradle project` 目录，`Build AAR` 会使用临时目录，编译完成后删除。

如果 Java 源码里引用了 `com.unity3d.player.UnityPlayer`，工具顶部需要填写 `Unity root/Data` 路径。可以填 Unity 安装根目录：

```text
c:\tools\Unity\Unity2022.3.47f1\
```

也可以填 Unity 的 `Data` 目录：

```text
c:\tools\Unity\Unity2022.3.47f1\Data\
```

工具会从这个路径下查找：

```text
Data/Data/PlaybackEngines/AndroidPlayer/Variations/il2cpp/Release/Classes/classes.jar
PlaybackEngines/AndroidPlayer/Variations/il2cpp/Release/Classes/classes.jar
Data/PlaybackEngines/AndroidPlayer/Variations/il2cpp/Release/Classes/classes.jar
```

并在临时 Gradle 工程里直接引用绝对路径：

```gradle
dependencies {
    compileOnly files('C:/tools/Unity/Unity2022.3.47f1/Data/Data/PlaybackEngines/AndroidPlayer/Variations/il2cpp/Release/Classes/classes.jar')
}
```

这里用 `compileOnly`，不会把 Unity 的 `classes.jar` 打进 AAR。

工具还会从 Unity 安装目录查找：

```text
PlaybackEngines/AndroidPlayer/Tools/VisualStudioGradleTemplates/gradlew.bat
PlaybackEngines/AndroidPlayer/Tools/VisualStudioGradleTemplates/gradle-wrapper.jar
PlaybackEngines/AndroidPlayer/Tools/gradle/lib/gradle-launcher-*.jar
```

生成工程时会把 `gradlew.bat` 和 `gradle-wrapper.jar` 复制进去，并额外写一个 `gradle.bat`，直接指向 Unity 内置 Gradle launcher。这样生成出来的工程可以不依赖系统全局 `gradle` 命令来检查 Gradle 版本或执行构建。

如果 C/C++ 源码目录没有 `CMakeLists.txt`，工具会生成一个最小 `CMakeLists.txt`，把 `.c`、`.cc`、`.cpp`、`.cxx` 编译成一个 `.so` 并打进 AAR。复杂 NDK 工程仍建议保留自己的 `CMakeLists.txt` 或先在 Android Studio 中整理为标准 Android Library。

Kotlin 源码会被检测并在日志里提示，但当前源码页签不会自动加入 Kotlin Gradle Plugin。包含 Kotlin、annotation processor、DataBinding 或复杂远程依赖的 SDK，建议先整理成标准 Android Library 工程再产出 AAR。

发布到 `LocalMaven` 后的文件：

```text
<UnityProjectRoot>/LocalMaven/<group path>/<artifact>/<version>/<artifact>-<version>.aar
<UnityProjectRoot>/LocalMaven/<group path>/<artifact>/<version>/<artifact>-<version>.pom
<UnityProjectRoot>/LocalMaven/<group path>/<artifact>/maven-metadata.xml
```

例如：

```text
LocalMaven/com/company/foo/foo/1.0.0/foo-1.0.0.aar
LocalMaven/com/company/foo/foo/1.0.0/foo-1.0.0.pom
LocalMaven/com/company/foo/foo/maven-metadata.xml
```

在 Unity Android Gradle 模板里接入：

```gradle
// settingsTemplate.gradle
// dependencyResolutionManagement { repositories { ... } } 里面
def unityProjectPath = $/file:///**DIR_UNITYPROJECT**/$.replace("\\", "/")

maven {
    url (unityProjectPath + "/LocalMaven")
    content {
        includeGroup "com.company.foo"
    }
}
```

然后在真正拥有 Android 依赖的 module 里加 dependency。大多数 Unity 项目是在 `launcherTemplate.gradle` 的 `dependencies` 里：

```gradle
dependencies {
    implementation "com.company.foo:foo:1.0.0"
}
```

如果项目使用 product flavor，并且只想在某个 flavor 里加入：

```gradle
dependencies {
    chinesebaseImplementation "com.company.foo:foo:1.0.0"
}
```

用工具生成依赖片段：

1. 点击 Android 页签的 `Collect`。
2. 工具会扫描 `LocalMaven` 下所有 `.pom`。
3. 生成：

```text
<UnityProjectRoot>/LocalMaven/temp.gradle
```

`temp.gradle` 只是复制用模板，不会自动修改 Unity 工程。里面会列出：

- `maven { url ... }`
- `includeGroup`
- `implementation "group:artifact:version"` 示例

如果输入本来就是 `.aar`，手工发布到 `LocalMaven` 的最小结构是：

```text
LocalMaven/com/company/foo/foo/1.0.0/foo-1.0.0.aar
LocalMaven/com/company/foo/foo/1.0.0/foo-1.0.0.pom
```

对应 `foo-1.0.0.pom` 至少需要：

```xml
<project>
  <modelVersion>4.0.0</modelVersion>
  <groupId>com.company.foo</groupId>
  <artifactId>foo</artifactId>
  <version>1.0.0</version>
  <packaging>aar</packaging>
</project>
```

建议仍然用第一个页签发布一次，因为它会同时生成 POM 和 `maven-metadata.xml`，目录结构也不容易写错。

常见坑：

- `GroupId`、`ArtifactId`、`Version` 必须和 Gradle dependency 完全一致。
- `settingsTemplate.gradle` 里只加仓库还不够，还要在 `launcherTemplate.gradle` 或正确 module 的 `dependencies` 里加依赖。
- 如果用 flavor dependency，例如 `chinesebaseImplementation`，这个 configuration 必须在同一个 module 里真实存在。
- AAR 里面如果还依赖别的 Maven 包，本地 POM 需要声明依赖，或者把依赖也放进 `LocalMaven` 并手动添加 dependency。
- 旧 Unity 插件里的 `.meta` 导入设置不会进入 AAR；转成 Maven 后，是否参与构建完全由 Gradle dependency 控制。

## iOS

iOS 的特点是：很多 Native 插件会在生成 Xcode 工程时复制进去，然后由 Xcode 编译或链接。C# 调用 iOS native 函数通常使用 `[DllImport("__Internal")]`。

### `.a`

本质：Apple 平台静态库，通常由 Objective-C/C/C++ 编译得到。

添加方式：

```text
Assets/Plugins/iOS/libfoo.a
```

选中文件，在 Inspector 里勾选 iOS，必要时设置 CPU 架构。

怎么调用：

如果 `.a` 暴露的是 C 函数，可以直接：

```csharp
[DllImport("__Internal")]
private static extern int Foo_Init();
```

如果 `.a` 暴露的是 Objective-C/C++ API，通常需要额外写一个 `.mm` C wrapper：

```objective-c
extern "C" int Foo_Init()
{
    return [FooSdk initSdk] ? 1 : 0;
}
```

适合场景：

- 老式 iOS SDK 只提供静态库。
- 不需要 Swift module 或动态 framework。

常见坑：

- 缺少系统 framework 时，需要在 Xcode 工程里添加依赖，例如 `Security.framework`、`SystemConfiguration.framework`。
- 可以用 `PostProcessBuild` 的 `PBXProject` API 自动补充 Xcode 配置。

### `.framework`

本质：Apple Framework Bundle。里面通常包含二进制、Headers、Modules、资源等。它可以是静态 framework，也可以是动态 framework。

添加方式：

```text
Assets/Plugins/iOS/Foo.framework
```

选中文件，在 Inspector 里勾选 iOS。动态 framework 或运行时需要加载的资源，通常还要勾选 `Add to Embedded Binaries`。

怎么调用：

- C ABI：C# 用 `[DllImport("__Internal")]`。
- Objective-C/Swift API：写 `.mm` 或 Swift bridge 暴露 C ABI，再由 C# 调用。

适合场景：

- 第三方 iOS SDK 提供 framework。
- SDK 自带资源、Headers、module map。

常见坑：

- 动态 framework 需要 embed，否则运行时可能找不到。
- 依赖的系统 frameworks/libraries 要补到 Xcode 工程。
- Swift framework 可能涉及 Swift runtime、module、`use_frameworks!` 等额外配置。

### `.xcframework`

本质：Apple 多平台、多架构 framework 容器。它可以同时包含 device/simulator、arm64/x86_64 等 slice，是现在 iOS SDK 常见分发格式。

添加方式：

```text
Assets/Plugins/iOS/Foo.xcframework
```

如果 Unity 版本或插件导入器对 `.xcframework` 支持不完整，可以用 CocoaPods 或 `PostProcessBuild` 手动加入 Xcode 工程。

怎么调用：

和 `.framework` 类似，最终仍建议通过 C ABI bridge 给 C# 调用：

```csharp
[DllImport("__Internal")]
private static extern int Foo_Init();
```

适合场景：

- 新版第三方 iOS SDK。
- 同时支持真机和模拟器。
- 需要规避传统 fat framework 在 Apple 平台上的限制。

常见坑：

- 不同 Unity 版本对 `.xcframework` 的直接导入体验不同。
- CocoaPods 管理 `.xcframework` 往往比直接拖入更稳定。

### `.m`

本质：Objective-C 源码文件。

添加方式：

```text
Assets/Plugins/iOS/FooBridge.m
```

放在 `Assets/Plugins/iOS` 下时，Unity 会默认按 iOS 插件处理，并复制到生成的 Xcode 工程。

怎么调用：

Objective-C 方法不能直接被 C# 调用。需要暴露 C 函数：

```objective-c
int Foo_Init()
{
    return 1;
}
```

```csharp
[DllImport("__Internal")]
private static extern int Foo_Init();
```

适合场景：

- 少量 Objective-C 原生能力，例如调用系统 API。
- 给 `.a` / `.framework` 写桥接层。

### `.mm`

本质：Objective-C++ 源码，可以同时写 Objective-C 和 C++。

添加方式：

```text
Assets/Plugins/iOS/FooBridge.mm
```

怎么调用：

`.mm` 经常作为 C# 和 Objective-C/C++ SDK 之间的桥：

```objective-c
extern "C" const char* Foo_GetVersion()
{
    return "1.0.0";
}
```

```csharp
[DllImport("__Internal")]
private static extern IntPtr Foo_GetVersion();
```

适合场景：

- iOS SDK 是 Objective-C，但底层还有 C++。
- 需要 `extern "C"` 包住 C++ 函数，避免 name mangling。

### `.c` / `.cpp` / `.h`

本质：C/C++ 源码和头文件。Unity 会把 iOS 插件源码复制到 Xcode 工程中，由 Xcode 编译。

添加方式：

```text
Assets/Plugins/iOS/foo.c
Assets/Plugins/iOS/foo.cpp
Assets/Plugins/iOS/foo.h
```

怎么调用：

```cpp
extern "C" int Foo_Add(int a, int b)
{
    return a + b;
}
```

```csharp
[DllImport("__Internal")]
private static extern int Foo_Add(int a, int b);
```

适合场景：

- 少量跨平台 C/C++ 代码。
- iOS 上希望由 Xcode 编译源码，而不是提前编译二进制。

### `.swift`

本质：Swift 源码。Unity 可以把 Swift 文件复制进 Xcode 工程，但 Swift 和 C# 不能直接互调，通常仍需要 Objective-C/C bridge。

添加方式：

```text
Assets/Plugins/iOS/Foo.swift
```

怎么调用：

常见方式是：

1. Swift 实现业务逻辑。
2. Objective-C/Objective-C++ bridge 调 Swift。
3. Bridge 暴露 C 函数。
4. C# 用 `[DllImport("__Internal")]` 调 C 函数。

适合场景：

- SDK 或系统能力已经用 Swift 写好。
- 需要接入 Swift-only API。

常见坑：

- Swift runtime、module、bridging header、Xcode Build Settings 可能需要 PostProcessBuild 处理。
- 对 Unity/Xcode 版本更敏感。

### `.bundle`

本质：Apple Bundle 目录，常用于资源包，也可能包含动态加载代码。

添加方式：

```text
Assets/Plugins/iOS/FooResources.bundle
```

选中文件，在 Inspector 里勾选 iOS。如果运行时要从 app bundle 读取资源，通常要确保被复制进最终 app。

适合场景：

- SDK 需要图片、模型、配置、隐私资源等。
- iOS framework 外挂资源包。

常见坑：

- 资源路径要按 iOS bundle 规则读取。
- 动态库和资源 bundle 的处理方式不同，不要把它们混为一谈。

### `.xcprivacy`

本质：Apple Privacy Manifest 文件，用于声明 SDK 或 App 使用的隐私相关 API、数据收集用途等。

添加方式：

通常跟随 SDK 放进 iOS 插件目录，或者作为 framework/xcframework 内部资源随 SDK 一起进入 Xcode 工程：

```text
Assets/Plugins/iOS/PrivacyInfo.xcprivacy
```

适合场景：

- 第三方 iOS SDK 要满足 App Store 隐私清单要求。

常见坑：

- 文件放进 Unity 不等于一定进入最终 Xcode target，必要时用 PostProcessBuild 检查 Copy Bundle Resources。

### CocoaPods / Podfile

本质：不是 Unity 插件后缀，而是 iOS 依赖管理方式。Podfile 决定 Xcode workspace 里拉取、编译和链接哪些 iOS SDK。

添加方式常见有三种：

- EDM4U 生成 iOS dependency XML，构建时修改 Podfile。
- 自己写 `PostProcessBuild`，在 Unity 导出的 Xcode 工程里 patch Podfile。
- 使用本工具生成 `LocalPods`，再按构建区域或宏条件写入 Podfile。

适合场景：

- 第三方 SDK 官方只提供 Pod。
- SDK 依赖链复杂，例如多个 framework、resource bundle、Swift 配置。
- 需要区域包或 flavor 级别控制，例如只在 CN iOS 构建加入某个 Pod。

### 转成 `LocalPods`

本质：`LocalPods` 是放在 Unity 项目根目录下的一组本地 CocoaPods。它不是 Unity 插件目录，而是给 Xcode/CocoaPods 读取的本地 Pod 源码或本地 Pod SDK 包。Unity 导出 Xcode 后，通过 Podfile 写入 `pod 'Name', :path => '...'` 来显式接入。

目标形态：

```text
<UnityProjectRoot>/LocalPods/
  FooCN/
    1.0.0/
      FooCN.podspec
      Vendor/
        Foo.framework
        FooResources.bundle
        PrivacyInfo.xcprivacy
```

Podfile 里引用的是本地路径：

```ruby
pod 'FooCN', :path => '<UnityProjectRoot>/LocalPods/FooCN/1.0.0'
```

转换判断表：

| 输入格式 | 能不能转成 `LocalPods` | 怎么改 |
| --- | --- | --- |
| `.framework` | 可以，最适合本工具 | 放进 `Vendor/`，podspec 写 `s.vendored_frameworks` |
| `.xcframework` | 可以，适合本工具 | 放进 `Vendor/`，podspec 写 `s.vendored_frameworks` |
| `.a` + `.h` | 可以 | 放进 `Vendor/`，podspec 写 `s.vendored_libraries`、`s.public_header_files` |
| `.bundle` / `.xcassets` | 可以，但通常作为资源跟随 SDK | 放进 `Vendor/`，podspec 写 `s.resources` |
| `.xcprivacy` | 可以 | 放进 `Vendor/` 或 SDK 根目录，podspec 写 `s.resources` |
| `.m` / `.mm` / `.c` / `.cc` / `.cpp` / `.cxx` / `.h` / `.hh` / `.hpp` | 可以，工具已支持 ObjC/C/C++ 源码 Pod | 放进 `Vendor/`，podspec 写 `s.source_files` 和 `s.public_header_files` |
| `.swift` | 可以，但更麻烦 | 手写 podspec，补 `s.source_files`、`s.swift_version`，处理 Swift module 和桥接 |
| 现成 `.podspec` | 可以 | 调整成 `s.source = { :path => '.' }`，修正 vendored/source/resource 相对路径 |
| 远程 Pod | 可以本地化，但不是简单复制 | 用官方 Pod 依赖，或把源码/二进制/vendor 资源整理成本地 Pod |
| `.xcodeproj` / `.xcworkspace` | 不建议直接转 | 先用 Xcode 构建出 `.framework` / `.xcframework`，或整理成源码 Pod |
| `.dylib` | 不推荐 | 改成 `.framework` / `.xcframework` 分发，再做成本地 Pod |

容易直接转的格式：

- SDK 已经是 `.framework` 或 `.xcframework`。
- SDK 是 `.a` 静态库，并且头文件完整。
- SDK 是 `.m/.mm/.c/.cpp` 这类 Objective-C/C/C++ 源码。
- SDK 的资源是 `.bundle`、`.xcassets`、`.plist`、`.storyboard`、`.xib`、`.xcprivacy`。
- 只是需要补系统 framework/library，例如 `Foundation`、`UIKit`、`z`、`c++`。

不好直接转的格式：

- SDK 只有 Swift 源码，还没有稳定的 Podspec。
- SDK 是完整 Xcode 工程，需要先编译或拆出源码文件。
- SDK 依赖其他 CocoaPods，但这些依赖没有在 podspec 里声明。
- SDK 需要复杂 Build Settings，例如 Swift version、module map、script phase、resource bundle 编译规则。

这些不好直接转的格式，要先变成标准 Pod 或标准 Apple SDK 产物：

```text
iOS Swift source / Xcode project / complex SDK
  -> 写 podspec 或用 Xcode 构建
  -> .framework / .xcframework 或源码 Pod
  -> 放进 LocalPods
  -> Podfile 用 :path 显式引用
```

用 NativeLibTool 转换 iOS SDK 目录：

1. 打开工具，设置 `Unity project root`。
2. iOS 页签里选择 `Source directory`。可以选到真正包含 `.framework`、`.xcframework`、`.a`、`.bundle`、`.xcprivacy`、`.m/.mm/.c/.cpp/.h` 的目录，也可以选它的父目录，让工具自动检测。
3. 填 Pod 信息：
   - `Pod name`：例如 `FooCN`
   - `Version`：例如 `1.0.0`
   - `Min iOS`：例如 `12.0`
   - `Frameworks`：例如 `Foundation, UIKit, SystemConfiguration`
   - `Libraries`：例如 `z, c++`
   - `Dependencies`：例如 `SolarEngineSDK, >=0`
4. 点击 `Generate`。
5. 工具会在 Unity 项目根目录生成 `LocalPods/<PodName>/<Version>`。

工具生成 podspec 时的映射规则：

| 原目录内容 | podspec 字段 |
| --- | --- |
| `*.framework` | `s.vendored_frameworks` |
| `*.xcframework` | `s.vendored_frameworks` |
| `*.a` | `s.vendored_libraries` |
| `*.m`, `*.mm`, `*.c`, `*.cc`, `*.cpp`, `*.cxx` | `s.source_files` |
| `*.h`, `*.hh`, `*.hpp` | `s.public_header_files`、`s.source_files` |
| `*.bundle` | `s.resources` |
| `*.xcassets` | `s.resources` |
| `*.plist` | `s.resources` |
| `*.storyboard` | `s.resources` |
| `*.xib` | `s.resources` |
| `*.xcprivacy` | `s.resources` |

如果 `Dependencies` 填：

```text
SolarEngineSDK, >=0
```

或者直接粘贴：

```ruby
pod 'SolarEngineSDK', '>=0'
```

生成的 podspec 会包含：

```ruby
s.dependency 'SolarEngineSDK', '>=0'
```

这表示当前 LocalPod 编译时和链接时依赖 `SolarEngineSDK`，CocoaPods 会自动把它加入 Pods。`s.dependency` 只能声明 pod 名和版本；如果依赖必须使用 `:path`、`:podspec` 或网络 podspec URL，就需要在 Podfile/custom Podfile 里直接写那条 pod。

生成后的文件：

```text
<UnityProjectRoot>/LocalPods/<PodName>/<Version>/<PodName>.podspec
<UnityProjectRoot>/LocalPods/<PodName>/<Version>/Vendor/...
```

例如：

```text
LocalPods/FooCN/1.0.0/FooCN.podspec
LocalPods/FooCN/1.0.0/Vendor/Foo.framework
LocalPods/FooCN/1.0.0/Vendor/FooResources.bundle
```

在 Unity iOS 导出的 Xcode 工程里接入：

```ruby
pod 'FooCN', :path => 'D:/YourUnityProject/LocalPods/FooCN/1.0.0'
```

本项目推荐不要手动改导出的 Podfile，而是在 `PostProcessBuild` 阶段按构建条件 patch Podfile。例如只在 CN iOS 包加入：

```json
{
  "enabled": true,
  "onlyWhenDefinesContain": [
    "UNITY_IOS"
  ],
  "pods": [
    {
      "name": "FooCN",
      "path": "LocalPods/FooCN/1.0.0"
    }
  ]
}
```

用工具生成自定义 Podfile 配置：

1. 点击 iOS 页签的 `Collect`。
2. 工具会扫描 `LocalPods` 下所有 `.podspec`。
3. 生成：

```text
<UnityProjectRoot>/LocalPods/custom.podfile
```

`custom.podfile` 只是给 Unity PostProcessBuild 使用或复制改名的配置，不会自动修改 Unity 工程。它包含两个目标：

```ruby
target 'Add' do
  pod 'FooCN', :path => '<UnityProject>/LocalPods/FooCN/1.0.0'
end

target 'Remove' do
end
```

如果输入本来就是一个本地 Pod，最小结构是：

```text
LocalPods/FooCN/1.0.0/FooCN.podspec
LocalPods/FooCN/1.0.0/Vendor/Foo.xcframework
```

对应 `FooCN.podspec` 至少需要：

```ruby
Pod::Spec.new do |s|
  s.name = 'FooCN'
  s.version = '1.0.0'
  s.summary = 'FooCN native SDK'
  s.homepage = 'https://internal.local/FooCN'
  s.license = { :type => 'Proprietary' }
  s.author = { 'Company' => 'dev@company.local' }
  s.platform = :ios, '12.0'
  s.source = { :path => '.' }
  s.vendored_frameworks = 'Vendor/Foo.xcframework'
  s.resources = 'Vendor/FooResources.bundle'
  s.frameworks = 'Foundation', 'UIKit'
  s.libraries = 'z', 'c++'
end
```

常见坑：

- `Pod name` 必须和 podspec 里的 `s.name` 一致。
- `:path` 要指向包含 podspec 的目录，不是指向 `Vendor/`。
- 如果 SDK 依赖其他 Pod，要在 podspec 里写 `s.dependency`，或者同时把依赖也变成本地 Pod 并确保 Podfile 能找到。
- Swift SDK 通常不能只复制 `.swift` 文件就结束，要处理 `s.swift_version`、module、bridging 和 Build Settings。
- 系统 framework/library 不要复制到 `LocalPods`，在 podspec 里用 `s.frameworks` / `s.libraries` 声明。
- 当前 NativeLibTool 支持 vendored SDK 和 Objective-C/C/C++ 源码 Pod；Swift Pod、复杂 podspec 需要手写 podspec 或扩展工具。

## Windows / macOS / Linux

桌面平台主要是预编译动态库。跨平台插件通常每个平台各提供一个文件，但 C# 使用同一个 `DllImport` 名字。

### Windows `.dll`

本质：Windows Dynamic Link Library，导出 C ABI 函数。

添加方式：

```text
Assets/Plugins/x86_64/FooPlugin.dll
```

或放在 `Assets/Plugins` 下，在 Inspector 里设置 Standalone、Windows、CPU。

怎么调用：

```csharp
[DllImport("FooPlugin")]
private static extern int Foo_Init();
```

常见坑：

- x86/x64 架构要和 Editor/Player 匹配。
- 依赖的其他 DLL 也要能被加载到。
- C++ 导出要处理 name mangling，建议 C wrapper。

### macOS `.bundle`

本质：macOS Bundle 插件，Unity 桌面 Native 插件的常见 macOS 形态。

添加方式：

```text
Assets/Plugins/FooPlugin.bundle
```

在 Inspector 里设置 Standalone、macOS。

怎么调用：

```csharp
[DllImport("FooPlugin")]
private static extern int Foo_Init();
```

### macOS `.dylib`

本质：macOS 动态库。

添加方式：

```text
Assets/Plugins/libFooPlugin.dylib
```

在 Inspector 里设置 Standalone、macOS。

怎么调用：

```csharp
[DllImport("FooPlugin")]
private static extern int Foo_Init();
```

常见坑：

- 签名、notarization、runtime search path 可能影响加载。
- 依赖其他 dylib 时要处理 `@rpath` / `@loader_path`。

### Linux `.so`

本质：Linux ELF shared object。

添加方式：

```text
Assets/Plugins/x86_64/libFooPlugin.so
```

在 Inspector 里设置 Standalone、Linux、CPU。

怎么调用：

```csharp
[DllImport("FooPlugin")]
private static extern int Foo_Init();
```

常见坑：

- Linux 默认不一定从当前目录找依赖 so，复杂依赖要处理 `rpath`，常见是链接时加 `$ORIGIN`。
- 文件名通常是 `libFooPlugin.so`，但 `DllImport` 写 `FooPlugin`。

## WebGL

WebGL 不能像桌面平台那样加载本地 `.dll` / `.so`。它的“Native 插件”本质是 Emscripten 构建链里的 JavaScript 或 C/C++ 代码。

### `.jslib`

本质：Emscripten JavaScript library。它把 JavaScript 函数合并到 WebGL 构建产物里，让 C# 可以像调用 native 函数一样调用 JS。

添加方式：

```text
Assets/Plugins/WebGL/Foo.jslib
```

示例：

```javascript
mergeInto(LibraryManager.library, {
    Foo_Alert: function () {
        window.alert("hello");
    }
});
```

```csharp
[DllImport("__Internal")]
private static extern void Foo_Alert();
```

适合场景：

- 调浏览器 API。
- 调页面上的 JavaScript SDK。
- WebGL 与网页宿主通信。

### `.jspre`

本质：Emscripten pre-js。它会被拼进 WebGL 生成的 framework JS 前部，用来放初始化脚本或第三方 JS 库。

添加方式：

```text
Assets/Plugins/WebGL/Foo.jspre
```

适合场景：

- `.jslib` 需要依赖的全局 JS 初始化。
- 提前注入第三方库或全局对象。

常见坑：

- C# 不能像调用 `.jslib` 函数一样直接调用 `.jspre`；通常 `.jspre` 提供环境，`.jslib` 暴露函数。
- WebGL 的字符串、数组、指针传递要按 Emscripten 规则处理。

## 怎么选择

| 需求 | 推荐方式 |
| --- | --- |
| Android SDK 有资源、Manifest、Java、so | `.aar` |
| Android SDK 是源码工程，还需要 Gradle 编译 | `.androidlib` |
| Android 只有少量 Java/Kotlin | `.java` / `.kt` |
| Android 只有 Java class，无资源 | `.jar` |
| Android 只有 NDK 动态库 | `.so` |
| Android Maven 坐标依赖 | Gradle dependency / EDM4U / `LocalMaven` |
| Android 旧插件要按区域显式接入 | 转成 AAR 后放进 `LocalMaven` |
| Android 源码工程要本地 Maven 化 | 先用 Gradle 构建 AAR，再放进 `LocalMaven` |
| iOS SDK 是静态库 | `.a` + `.h` + `.m/.mm` bridge |
| iOS SDK 是 framework | `.framework` |
| iOS SDK 是现代多架构包 | `.xcframework` 或 CocoaPods |
| iOS SDK 官方提供 Pod | CocoaPods / custom Podfile / `LocalPods` |
| iOS 二进制 SDK 要按区域显式接入 | 转成 `LocalPods` |
| iOS Objective-C/C/C++ 源码要本地 Pod 化 | 直接转成 `LocalPods` 源码 Pod |
| iOS Swift SDK 要本地 Pod 化 | 先写 podspec 或构建 `.xcframework`，再放进 `LocalPods` |
| Windows native | `.dll` |
| macOS native | `.bundle` 或 `.dylib` |
| Linux native | `.so` |
| WebGL 调浏览器 JS | `.jslib` |

## 和 NativeLibTool 的关系

这个工具主要解决“不要把区域专用 Native SDK 直接塞进 Unity 自动导入流程”的问题。

Android 侧：

- 输入 Unity/旧 Android 插件目录。
- 输出 AAR + POM + metadata 到 `LocalMaven`。
- Unity Android 构建时通过 Gradle dependency 显式引用。

iOS 侧：

- 输入 `.framework` / `.xcframework` / `.a` / `.bundle` / `.xcprivacy` / `.m` / `.mm` / `.c` / `.cpp` 等原生 SDK 目录。
- 输出本地 Pod 到 `LocalPods`。
- Unity 导出 Xcode 后，通过自定义 Podfile 配置显式引用。

这样做的好处是：

- 插件不会因为放在 `Assets/Plugins` 下就自动进所有平台包。
- 区域包、渠道包、flavor 可以显式控制 native 依赖。
- Android/iOS 都回到各自平台原生依赖系统，后续排查更接近标准工程。

## 参考

- Unity Manual: Native plug-ins  
  https://docs.unity.cn/Manual/plug-ins-native.html
- Unity Manual: Import and configure plug-ins  
  https://docs.unity.cn/Manual/PluginInspector.html
- Unity Manual: Android AAR plug-ins and Android Libraries  
  https://docs.unity3d.com/cn/2021.1/Manual/AndroidAARPlugins.html
- Unity Manual: Android Java and Kotlin source plug-ins  
  https://docs.unity3d.com/cn/2021.3/Manual/android-java-and-kotlin-plugins-create.html
- Unity Manual: Android native plug-ins  
  https://docs.unity3d.com/cn/2023.1/Manual/android-native-plugins-call.html
- Unity Manual: iOS automated plug-in integration  
  https://docs.unity3d.com/cn/current/Manual/ios-native-plugin-automated-integration.html
- Unity Manual: Desktop native plug-ins  
  https://docs.unity3d.com/cn/2022.2/Manual/PluginsForDesktop.html
- Unity Manual: Web JavaScript plug-ins  
  https://docs.unity.cn/Manual/web-interacting-browser-js.html
