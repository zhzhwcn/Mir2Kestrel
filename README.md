# Mir2

传奇2使用Kestrel做为服务器实现。

客户端使用[https://github.com/Suprcode/mir2]

目前实现功能：
- [x] 链接监听
- [] 数据包处理
- [] 服务器主循环

__链接监听__

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://127.0.0.1:7000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: E:\dotnet\Mir2\Mir2\ConsoleServer\
info: ServerKestrel.MirConnectionHandler[0]
      127.0.0.1:56093 connected
info: ServerKestrel.MirConnectionHandler[0]
      Bytes Read:24
info: ServerKestrel.MirConnectionHandler[0]
      Received Packet[0]:{"Observable":true,"Index":0}
```

代码参考：
 * https://github.com/Suprcode/mir2
 * https://github.com/Suprcode/mir2