-- 切换到你的数据库
USE tankgame;

-- 如果存在旧的user表，先删除（方便重复运行）
DROP TABLE IF EXISTS user;

-- 创建用户表
CREATE TABLE user (
    userid INT PRIMARY KEY AUTO_INCREMENT COMMENT '用户ID',
    password VARCHAR(20) NOT NULL COMMENT '用户密码'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;