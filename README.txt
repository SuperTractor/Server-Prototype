* 双击 Server.sln 用 Visual Studio 打开工程
* 右键单击 Server 工程，设置为启动项目
* 在 Windows 开始菜单，输入 ipconfig，查看自己的 ipv4 地址，比如我现在的是 192.168.88.101
* 将 Server 工程下，Program.cs 的 m_isStr 改成你的 ipv4 地址
* 按 F5 启动调试
* 然后开 4 个 Unity 游戏实例，才可以开始游戏
* 注意一定要用 4 个不同的机子开 Unity，因为只有当窗口激活时 Unity 才会运行，在 1 个机子上不可能实现 4 个 Unity 游戏同时运行（除非用虚拟机）