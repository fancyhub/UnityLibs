# Str

## 概述
1. 主要为了解决 处理String产生的GC问题
2. 一个结构体的Str实现,包含了 OrigString, StartOffset,Len, 解决String解析的问题
3. 主要用于 CSV的读取, 让Csv读取的时候, 不需要频繁 SubString

