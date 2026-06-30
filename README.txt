## 运行前置条件
- .NET 6+ / .NET Framework（看你控制台用的啥）
- MySQL 5.7+（需提前建库，建表脚本在 `sql/init.sql`）

## 配置修改
修改DBHelper里的数据库连接串：private static string contankgame = "server=127.0.0.1;port=3306;database=修改=root;password=修改;";