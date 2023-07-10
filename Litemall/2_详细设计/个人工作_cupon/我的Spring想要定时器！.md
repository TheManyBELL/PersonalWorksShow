# 我的Spring想要定时器！
@author YipWingchun
@date 2019/12/13


## 强大的@Scheduled注解
> Spring框架提供了一个叫做@Scheduled的定时器功能，虽然JAVA以及Spring还提供了三四种其他的定时器实现方式，但是综合看来，还是利用@Scheduled注解的方式来实现定时器最适合MVC框架的Spring项目

## 建立定时器类
* 我的话在根目录下建立了 util.TimeUtils.java 类来专门存放定时器服务
```java
package xmu.oomall.groupon.util;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.scheduling.annotation.Scheduled;
import xmu.oomall.groupon.controller.GroupOnRuleController;

import java.time.LocalDateTime;

@Component
public class TimeUtils {

    @Autowired
    private GroupOnRuleController groupOnRuleController;

    @Scheduled(cron="0 0/1 * * * ?")
    public void toController(){
        groupOnRuleController.minute();
    }
    
    @Scheduled(cron="0/1 * * * * ?")
    public void second(){
        System.out.println(LocalDateTime.now());
    }


}

```

* 注解@Component
> 这个注解使我们可以在这个定时器类里自动注入Controller

* 注解@Scheduled(cron="")
> 这是定时器的灵魂所在，下文解释
> 顺带一提：
> cron="0 0/1 * * * ?" 等于每分钟执行一次
> cron="0/1 * * * * ?"等于每秒执行一次

## 一起学习@Scheduled注解！
### 参数详解
1. corn表达式

corn参数接收一个cron表达式，cron表达式是一个字符串，字符串以5或6个空格隔开，分开共**6或7**个域，每一个域代表一个含义
2. corn表达语法

```java
[秒] [分] [时] [日] [月] [周] [年]
```
> 当写7个域的corn表达式，则每个域定义如上；
> 若写6个域的corn表达式，则省略年；

3. corn 的含义

> 当我们往corn表达式中插入数字或\*，我们其实可以得到一个时间，例如插入{1,2,3,4,5,\*}，能得到5月4日3时2分1秒
> 那么@Scheduled每次碰到5月4日3时2分1秒就会执行下面的方法
> 

### 常用的corn通配符
* ` *` 表示所有值。某个域填`*`则代表这个域取什么值都符合条件
* `	?`表示不关心这个值。某个域填`？`表示对这个域不关心，我也不知道跟`*`有个锤子区别
* `/`表示递归触发。如在秒上面设置”5/15” 表示从5秒开始，每增15秒触发，也就是(5,20,35,50)触发；在日字段上设置’1/3’所示每月1号开始，每隔三天触发一次。

## learn more

[@Scheduled注解各参数详解](https://www.jianshu.com/p/1defb0f22ed1)








