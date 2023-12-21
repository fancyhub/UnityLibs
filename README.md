# UnityLibs

## 概述
从2015年开始使用Unity, 一直到现在2023年, 以后不知道还是否用Unity, 下面的代码库都是自己这么多年一行一行写出来的
1. 换了几个项目, 觉得这些代码丢了比较可惜
2. 有时碰到老问题, 想从旧代码扒一些出来用, 觉得耦合太高
所以把这些代码重写整理, 重构, 弄成一个一个库, 尽量减少他们之间的耦合, UI框架, 战斗, 网络, Shader, 每个项目都不太相同, 大概率是不放的  

框架和组件, 我自己理解的区别是:  
1. 框架需要大量依赖注入,插件代码是被调用的, 缺了这些插件, 框架就没有意义
2. 组件相对独立,不需要太多的依赖注入


## Log
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.log)

## Clock
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.clock)

## TimeUtil
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.timeutil)

## str
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.str)

## Memory Pool
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.memorypool)

## Time Wheel
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.timewheel)

## Csv
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.csv)

## Event Set 
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.eventset)

## UI View Gen
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.uiviewgen)

## Msg Queue
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.msgqueue)

## StreamingAssets File System
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.streamingassetsfilesystem)

## Resource Manager
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.resourcemanager)

## Scene Manager
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.scenemanager)

## Asset Bundle Manager
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.assetbundlemanager)

## Asset Bundle Builder
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.assetbundlebuilder)

## Resource Service
[README](./UnityLibs/Packages/com.github.fancyhub.unitylibs.resourceservice)  
这个是聚合包,聚合了  
1. Resource Manager
2. Scene Manager
3. Asset Bundle Manager
4. Asset Bundle Builder


