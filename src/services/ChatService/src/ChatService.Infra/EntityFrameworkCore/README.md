﻿## 迁移命令

> 由于使用多租户或者领域事件方式，dbcontext初始化需要加载 依赖，所以迁移需要定义 对应的MigrationDbContext。
> 具体参考 ChatServiceDbContext  -->  ChatServiceMigrationDbContext 以下命令中的 -Context 需要换成 MigrationDbContext的。

```
-- DeviceCenterDbContext
Add-Migration InitialCreate -Context ChatServiceMigrationDbContext -Project ChatService.Infra -StartupProject ChatService.Infra -OutputDir Migrations/ChatServiceDb


Remove-Migration -Context ChatServiceMigrationDbContext -Project ChatService.Infra -StartupProject ChatService.Infra


Update-Database -Context ChatServiceMigrationDbContext -Project ChatService.Infra -StartupProject ChatService.Infra


-- 使用链接字符串应用迁移
Update-Database -Context ChatServiceMigrationDbContext -Project ChatService.Infra -StartupProject ChatService.Infra -Connection "Server=localhost,1433;Database=Pnct_T20210602000002;User Id=sa;Password=970307lBx;Trusted_Connection = False;"


```
