## 优惠券规则

规则上架 statusCode==true：

- 可以领取优惠券
  - 在规则的开始和结束时间之间，用户可以领取优惠券。
  - 优惠券的开始时间为领取时间，结束时间有两种计算方式
    - 优惠券规则的validPeriod为负数,则优惠券结束时间为优惠券规则结束时间（的24点）
    - 优惠券规则的validPeriod为正数，则优惠券结束时间为开始时间+validPeriod（的24点）
  - 优惠券的生命周期统一独立于优惠券规则
  - 每个用户只能领取一张
- 可以使用优惠券
- 不能修改优惠券规则

规则过期 statusCode==true

- 所有所属优惠券设置为`过期`，过期的优惠券用户可以查看
- 用户和界面都不能看到/显示过期的优惠券
- 管理员可以修改优惠券规则

规则作废 `statusCode==false`

- 管理员可以在优惠券规则生命周期内强行下架优惠券规则
- 在操作发生时所属优惠券全部设置为失效，失效的优惠券用户似乎不能查看





## 计算订单优惠

- 取出order中的couponId

  - 判断order是不是优惠券订单（couponId==null）	

    - null，为普通订单

      1. 计算商品总价goodPrice

      2. 填写优惠券折扣couponPrice为0

      3. 均摊返点折扣

         1. 检测返点折扣总额是否合法，小于0报错，等于0不需要均摊直接返回，大于0继续

         2. 获取商品总价；初始化还没有分配出去的返点notUsedRebate = rebate

         3. 开始遍历所有orderItem，逐个分配返点（循环）

            1. 计算当前orderItem的返点百分比（份额）。保留两位小数，向下取

               ```java
               percentage = orderItem.getPrice().divide(totalGoodsPrice,2,1);
               ```

            2. 计算可以获取的返点金额。会出现四位小数，保留两位小数，向下取

               ```java
               myRebate = rebate.multiply(percentage);
               ```

            3. 判断myrebate是否为0，为0表示不足1分钱，不分配，进入下一个OrderItem

            4. 不为0，表示大于等于1分钱，可以分。更新dealPrice

            5. 减少未分配的返点notUsedRebate

         4. 是否有返点没分完`notUsedRebate.compareTo(BigDecimal.ZERO)>0`

            1. 有返点没分完，遍历orderItem，更新第一个dealPrice减去剩余返点不为负数的orderItem，推出循环
            
         5. 判断是否有返点没分配完，没有OK，有则报错

      4. 计算最终成交价
      5.  生成payment
      
    - 非null，为有使用优惠券的普通订单
      
      1. 取出优惠券coupon
      2. 检测优惠券是否存在
      3. 检测优惠券使用状态、时间(判断有点不好看)
      4. 取出优惠券规则并装填抽象类
      5. 根据优惠券规则判断是否达到条件
      6. 计算goodsPrice
      7. 计算支付总价
      8. 检测支付总价是否正确
      9. 将优惠券折扣分配到适用orderItem的dealPrice中
         1. 获取可用优惠券的商品列表
         2. 第一次处理分配，有误差
         3. 
      10. 将返点分配到所有orderItem的dealPrice中
      11. 返回order
      
      
      
      
      
      
  
  

## PUT

先对属性做基本检测：判空、判合法、判是否存在（拉出记录）

statusCode

​	false:下架

​	true:上架

 和 beDeleted





```
{
    "errno": 0,
    "data": {
        "gmtModified": "2019-12-21T02:09:47.034",
        "orderSn": "201912210209470334",
        "orderItemList": [
            {
                "itemType": 0,
                "gmtModified": "2019-12-21T02:09:50.14",
                "product": {
                    "picUrl": "string",
                    "gmtModified": "2019-12-18T02:30:21",
                    "safetyStock": 58,
                    "goodsId": 60,
                    "price": 100.0,
                    "goods": {
                        "gmtModified": "2019-12-18T09:59:49.37",
                        "goodsSn": "string",
                        "description": "string",
                        "specialFreightId": 1,
                        "picUrl": "string",
                        "goodsCategoryId": 59,
                        "price": 1.0,
                        "id": 60,
                        "gallery": "string",
                        "brief": "string",
                        "weight": 10.0,
                        "gmtCreate": "2019-12-18T03:10:42.55",
                        "volume": "string",
                        "brandId": 56,
                        "name": "string",
                        "detail": "string",
                        "shortName": "string",
                        "beSpecial": true,
                        "beDeleted": false,
                        "statusCode": 1
                    },
                    "id": 84,
                    "gmtCreate": "2019-12-18T02:30:21",
                    "specifications": "string",
                    "beDeleted": false
                },
                "productId": 84,
                "orderId": 38051,
                "goodsId": 60,
                "dealPrice": 188.0,
                "gmtCreate": "2019-12-21T02:09:50.14",
                "number": 2,
                "picUrl": "string",
                "price": 200.0,
                "id": 39404,
                "beDeleted": false,
                "statusCode": 0
            }
        ],
        "couponId": 5,
        "addressObj": {
            "consignee": "Clines",
            "city": "厦门市",
            "postalCode": "361000",
            "mobile": "15860852866",
            "county": "思明区",
            "cityId": 147,
            "beDefault": true,
            "userId": 1,
            "provinceId": 13,
            "addressDetail": "曾厝安西路厦门大学海韵园科研楼1-106教室",
            "province": "福建省",
            "countyId": 1407,
            "id": 1,
            "beDeleted": false
        },
        "id": 38051,
        "freightPrice": 70.0,
        "exchangeOrder": false,
        "integralPrice": 458.0,
        "consignee": "Clines",
        "address": "福建省厦门市思明区曾厝安西路厦门大学海韵园科研楼1-106教室",
        "mobile": "15860852866",
        "message": "Just Test Api",
        "gmtCreate": "2019-12-21T02:09:47.034",
        "userId": 1,
        "paymentList": [
            {
                "actualPrice": 458.0,
                "orderId": 38051,
                "beginTime": "2019-12-21T02:09:54.659",
                "statusCode": 0
            }
        ],
        "goodsPrice": 400.0,
        "rebatePrice": 2.0,
        "couponPrice": 10.0,
        "beDeleted": false,
        "user": {
            "birthday": "2019-11-10T00:00:00",
            "gmtModified": "2019-12-20T10:53:03.78",
            "gender": 0,
            "rebate": 500,
            "wxOpenId": "1234567",
            "roleId": 4,
            "mobile": "15860852857",
            "avatar": "1.jpg",
            "lastLoginTime": "2019-12-19T17:29:42",
            "password": "123",
            "userLevel": 1,
            "name": "yuanyuan",
            "nickname": "weiwei",
            "id": 1,
            "beDeleted": false
        },
        "statusCode": 0
    },
    "errmsg": "成功"
}
```

```java
/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/bin/java -XX:TieredStopAtLevel=1 -noverify -Dspring.output.ansi.enabled=always -Dcom.sun.management.jmxremote -Dspring.liveBeansView.mbeanDomain -Dspring.application.admin.enabled=true "-javaagent:/Applications/IntelliJ IDEA.app/Contents/lib/idea_rt.jar=52557:/Applications/IntelliJ IDEA.app/Contents/bin" -Dfile.encoding=UTF-8 -classpath "/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/charsets.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/deploy.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/ext/cldrdata.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/ext/dnsns.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/ext/jaccess.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/ext/jfxrt.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/ext/localedata.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/ext/nashorn.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/ext/sunec.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/ext/sunjce_provider.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/ext/sunpkcs11.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/ext/zipfs.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/javaws.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/jce.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/jfr.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/jfxswt.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/jsse.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/management-agent.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/plugin.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/resources.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/jre/lib/rt.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/lib/ant-javafx.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/lib/dt.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/lib/javafx-mx.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/lib/jconsole.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/lib/packager.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/lib/sa-jdi.jar:/Library/Java/JavaVirtualMachines/jdk1.8.0_231.jdk/Contents/Home/lib/tools.jar:/Users/qinwen/Library/Mobile Documents/com~apple~CloudDocs/IDEAProject/discount121803/discount/target/classes:/Users/qinwen/.m2/repository/org/springframework/cloud/spring-cloud-starter-netflix-eureka-client/2.0.0.RELEASE/spring-cloud-starter-netflix-eureka-client-2.0.0.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/cloud/spring-cloud-netflix-core/2.0.0.RELEASE/spring-cloud-netflix-core-2.0.0.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter-aop/2.2.1.RELEASE/spring-boot-starter-aop-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/aspectj/aspectjweaver/1.9.4/aspectjweaver-1.9.4.jar:/Users/qinwen/.m2/repository/org/springframework/cloud/spring-cloud-netflix-eureka-client/2.0.0.RELEASE/spring-cloud-netflix-eureka-client-2.0.0.RELEASE.jar:/Users/qinwen/.m2/repository/com/netflix/eureka/eureka-client/1.9.2/eureka-client-1.9.2.jar:/Users/qinwen/.m2/repository/org/codehaus/jettison/jettison/1.3.7/jettison-1.3.7.jar:/Users/qinwen/.m2/repository/stax/stax-api/1.0.1/stax-api-1.0.1.jar:/Users/qinwen/.m2/repository/com/netflix/netflix-commons/netflix-eventbus/0.3.0/netflix-eventbus-0.3.0.jar:/Users/qinwen/.m2/repository/com/netflix/netflix-commons/netflix-infix/0.3.0/netflix-infix-0.3.0.jar:/Users/qinwen/.m2/repository/commons-jxpath/commons-jxpath/1.3/commons-jxpath-1.3.jar:/Users/qinwen/.m2/repository/joda-time/joda-time/2.10.5/joda-time-2.10.5.jar:/Users/qinwen/.m2/repository/org/antlr/antlr-runtime/3.4/antlr-runtime-3.4.jar:/Users/qinwen/.m2/repository/org/antlr/stringtemplate/3.2.1/stringtemplate-3.2.1.jar:/Users/qinwen/.m2/repository/antlr/antlr/2.7.7/antlr-2.7.7.jar:/Users/qinwen/.m2/repository/com/google/code/gson/gson/2.8.6/gson-2.8.6.jar:/Users/qinwen/.m2/repository/org/apache/commons/commons-math/2.2/commons-math-2.2.jar:/Users/qinwen/.m2/repository/com/netflix/archaius/archaius-core/0.7.6/archaius-core-0.7.6.jar:/Users/qinwen/.m2/repository/com/google/guava/guava/16.0/guava-16.0.jar:/Users/qinwen/.m2/repository/javax/ws/rs/jsr311-api/1.1.1/jsr311-api-1.1.1.jar:/Users/qinwen/.m2/repository/com/netflix/servo/servo-core/0.12.21/servo-core-0.12.21.jar:/Users/qinwen/.m2/repository/com/sun/jersey/jersey-core/1.19.1/jersey-core-1.19.1.jar:/Users/qinwen/.m2/repository/com/sun/jersey/jersey-client/1.19.1/jersey-client-1.19.1.jar:/Users/qinwen/.m2/repository/com/sun/jersey/contribs/jersey-apache-client4/1.19.1/jersey-apache-client4-1.19.1.jar:/Users/qinwen/.m2/repository/org/apache/httpcomponents/httpclient/4.5.10/httpclient-4.5.10.jar:/Users/qinwen/.m2/repository/org/apache/httpcomponents/httpcore/4.4.12/httpcore-4.4.12.jar:/Users/qinwen/.m2/repository/commons-codec/commons-codec/1.13/commons-codec-1.13.jar:/Users/qinwen/.m2/repository/com/google/inject/guice/4.1.0/guice-4.1.0.jar:/Users/qinwen/.m2/repository/javax/inject/javax.inject/1/javax.inject-1.jar:/Users/qinwen/.m2/repository/aopalliance/aopalliance/1.0/aopalliance-1.0.jar:/Users/qinwen/.m2/repository/com/github/vlsi/compactmap/compactmap/1.2.1/compactmap-1.2.1.jar:/Users/qinwen/.m2/repository/com/github/andrewoma/dexx/dexx-collections/0.2/dexx-collections-0.2.jar:/Users/qinwen/.m2/repository/com/netflix/eureka/eureka-core/1.9.2/eureka-core-1.9.2.jar:/Users/qinwen/.m2/repository/org/codehaus/woodstox/woodstox-core-asl/4.4.1/woodstox-core-asl-4.4.1.jar:/Users/qinwen/.m2/repository/javax/xml/stream/stax-api/1.0-2/stax-api-1.0-2.jar:/Users/qinwen/.m2/repository/org/codehaus/woodstox/stax2-api/3.1.4/stax2-api-3.1.4.jar:/Users/qinwen/.m2/repository/org/springframework/cloud/spring-cloud-starter-netflix-archaius/2.0.0.RELEASE/spring-cloud-starter-netflix-archaius-2.0.0.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/cloud/spring-cloud-netflix-ribbon/2.0.0.RELEASE/spring-cloud-netflix-ribbon-2.0.0.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/cloud/spring-cloud-netflix-archaius/2.0.0.RELEASE/spring-cloud-netflix-archaius-2.0.0.RELEASE.jar:/Users/qinwen/.m2/repository/commons-configuration/commons-configuration/1.8/commons-configuration-1.8.jar:/Users/qinwen/.m2/repository/commons-lang/commons-lang/2.6/commons-lang-2.6.jar:/Users/qinwen/.m2/repository/org/springframework/cloud/spring-cloud-starter-netflix-ribbon/2.0.0.RELEASE/spring-cloud-starter-netflix-ribbon-2.0.0.RELEASE.jar:/Users/qinwen/.m2/repository/com/netflix/ribbon/ribbon/2.2.5/ribbon-2.2.5.jar:/Users/qinwen/.m2/repository/com/netflix/ribbon/ribbon-transport/2.2.5/ribbon-transport-2.2.5.jar:/Users/qinwen/.m2/repository/io/reactivex/rxnetty-contexts/0.4.9/rxnetty-contexts-0.4.9.jar:/Users/qinwen/.m2/repository/io/reactivex/rxnetty-servo/0.4.9/rxnetty-servo-0.4.9.jar:/Users/qinwen/.m2/repository/com/netflix/hystrix/hystrix-core/1.5.12/hystrix-core-1.5.12.jar:/Users/qinwen/.m2/repository/org/hdrhistogram/HdrHistogram/2.1.9/HdrHistogram-2.1.9.jar:/Users/qinwen/.m2/repository/io/reactivex/rxnetty/0.4.9/rxnetty-0.4.9.jar:/Users/qinwen/.m2/repository/io/netty/netty-codec-http/4.1.43.Final/netty-codec-http-4.1.43.Final.jar:/Users/qinwen/.m2/repository/io/netty/netty-transport-native-epoll/4.1.43.Final/netty-transport-native-epoll-4.1.43.Final.jar:/Users/qinwen/.m2/repository/io/netty/netty-transport-native-unix-common/4.1.43.Final/netty-transport-native-unix-common-4.1.43.Final.jar:/Users/qinwen/.m2/repository/com/netflix/ribbon/ribbon-core/2.2.5/ribbon-core-2.2.5.jar:/Users/qinwen/.m2/repository/com/netflix/ribbon/ribbon-httpclient/2.2.5/ribbon-httpclient-2.2.5.jar:/Users/qinwen/.m2/repository/commons-collections/commons-collections/3.2.2/commons-collections-3.2.2.jar:/Users/qinwen/.m2/repository/com/netflix/netflix-commons/netflix-commons-util/0.3.0/netflix-commons-util-0.3.0.jar:/Users/qinwen/.m2/repository/com/netflix/ribbon/ribbon-loadbalancer/2.2.5/ribbon-loadbalancer-2.2.5.jar:/Users/qinwen/.m2/repository/com/netflix/netflix-commons/netflix-statistics/0.1.1/netflix-statistics-0.1.1.jar:/Users/qinwen/.m2/repository/io/reactivex/rxjava/1.3.8/rxjava-1.3.8.jar:/Users/qinwen/.m2/repository/com/netflix/ribbon/ribbon-eureka/2.2.5/ribbon-eureka-2.2.5.jar:/Users/qinwen/.m2/repository/com/thoughtworks/xstream/xstream/1.4.10/xstream-1.4.10.jar:/Users/qinwen/.m2/repository/xmlpull/xmlpull/1.1.3.1/xmlpull-1.1.3.1.jar:/Users/qinwen/.m2/repository/xpp3/xpp3_min/1.1.4c/xpp3_min-1.1.4c.jar:/Users/qinwen/.m2/repository/org/springframework/cloud/spring-cloud-starter/2.0.0.RELEASE/spring-cloud-starter-2.0.0.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter/2.2.1.RELEASE/spring-boot-starter-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter-logging/2.2.1.RELEASE/spring-boot-starter-logging-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/ch/qos/logback/logback-classic/1.2.3/logback-classic-1.2.3.jar:/Users/qinwen/.m2/repository/ch/qos/logback/logback-core/1.2.3/logback-core-1.2.3.jar:/Users/qinwen/.m2/repository/org/apache/logging/log4j/log4j-to-slf4j/2.12.1/log4j-to-slf4j-2.12.1.jar:/Users/qinwen/.m2/repository/org/slf4j/jul-to-slf4j/1.7.29/jul-to-slf4j-1.7.29.jar:/Users/qinwen/.m2/repository/jakarta/annotation/jakarta.annotation-api/1.3.5/jakarta.annotation-api-1.3.5.jar:/Users/qinwen/.m2/repository/org/yaml/snakeyaml/1.25/snakeyaml-1.25.jar:/Users/qinwen/.m2/repository/org/springframework/cloud/spring-cloud-context/2.0.0.RELEASE/spring-cloud-context-2.0.0.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/security/spring-security-crypto/5.2.1.RELEASE/spring-security-crypto-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/cloud/spring-cloud-commons/2.0.0.RELEASE/spring-cloud-commons-2.0.0.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/security/spring-security-rsa/1.0.5.RELEASE/spring-security-rsa-1.0.5.RELEASE.jar:/Users/qinwen/.m2/repository/org/bouncycastle/bcpkix-jdk15on/1.56/bcpkix-jdk15on-1.56.jar:/Users/qinwen/.m2/repository/org/bouncycastle/bcprov-jdk15on/1.56/bcprov-jdk15on-1.56.jar:/Users/qinwen/.m2/repository/org/projectlombok/lombok/1.18.4/lombok-1.18.4.jar:/Users/qinwen/.m2/repository/com/alibaba/druid/1.1.21/druid-1.1.21.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter-data-jdbc/2.2.1.RELEASE/spring-boot-starter-data-jdbc-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/data/spring-data-jdbc/1.1.1.RELEASE/spring-data-jdbc-1.1.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/data/spring-data-relational/1.1.1.RELEASE/spring-data-relational-1.1.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/data/spring-data-commons/2.2.1.RELEASE/spring-data-commons-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/spring-tx/5.2.1.RELEASE/spring-tx-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/spring-context/5.2.1.RELEASE/spring-context-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/spring-beans/5.2.1.RELEASE/spring-beans-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter-data-redis/2.2.1.RELEASE/spring-boot-starter-data-redis-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/data/spring-data-redis/2.2.1.RELEASE/spring-data-redis-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/data/spring-data-keyvalue/2.2.1.RELEASE/spring-data-keyvalue-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/spring-aop/5.2.1.RELEASE/spring-aop-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/spring-context-support/5.2.1.RELEASE/spring-context-support-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/io/lettuce/lettuce-core/5.2.1.RELEASE/lettuce-core-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/io/netty/netty-common/4.1.43.Final/netty-common-4.1.43.Final.jar:/Users/qinwen/.m2/repository/io/netty/netty-handler/4.1.43.Final/netty-handler-4.1.43.Final.jar:/Users/qinwen/.m2/repository/io/netty/netty-buffer/4.1.43.Final/netty-buffer-4.1.43.Final.jar:/Users/qinwen/.m2/repository/io/netty/netty-codec/4.1.43.Final/netty-codec-4.1.43.Final.jar:/Users/qinwen/.m2/repository/io/netty/netty-transport/4.1.43.Final/netty-transport-4.1.43.Final.jar:/Users/qinwen/.m2/repository/io/netty/netty-resolver/4.1.43.Final/netty-resolver-4.1.43.Final.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter-data-redis-reactive/2.2.1.RELEASE/spring-boot-starter-data-redis-reactive-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter-data-rest/2.2.1.RELEASE/spring-boot-starter-data-rest-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/data/spring-data-rest-webmvc/3.2.1.RELEASE/spring-data-rest-webmvc-3.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/data/spring-data-rest-core/3.2.1.RELEASE/spring-data-rest-core-3.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/hateoas/spring-hateoas/1.0.1.RELEASE/spring-hateoas-1.0.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/plugin/spring-plugin-core/2.0.0.RELEASE/spring-plugin-core-2.0.0.RELEASE.jar:/Users/qinwen/.m2/repository/org/atteo/evo-inflector/1.2.2/evo-inflector-1.2.2.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter-jdbc/2.2.1.RELEASE/spring-boot-starter-jdbc-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/com/zaxxer/HikariCP/3.4.1/HikariCP-3.4.1.jar:/Users/qinwen/.m2/repository/org/springframework/spring-jdbc/5.2.1.RELEASE/spring-jdbc-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter-web/2.2.1.RELEASE/spring-boot-starter-web-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter-json/2.2.1.RELEASE/spring-boot-starter-json-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/com/fasterxml/jackson/datatype/jackson-datatype-jdk8/2.10.0/jackson-datatype-jdk8-2.10.0.jar:/Users/qinwen/.m2/repository/com/fasterxml/jackson/datatype/jackson-datatype-jsr310/2.10.0/jackson-datatype-jsr310-2.10.0.jar:/Users/qinwen/.m2/repository/com/fasterxml/jackson/module/jackson-module-parameter-names/2.10.0/jackson-module-parameter-names-2.10.0.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter-tomcat/2.2.1.RELEASE/spring-boot-starter-tomcat-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/apache/tomcat/embed/tomcat-embed-core/9.0.27/tomcat-embed-core-9.0.27.jar:/Users/qinwen/.m2/repository/org/apache/tomcat/embed/tomcat-embed-el/9.0.27/tomcat-embed-el-9.0.27.jar:/Users/qinwen/.m2/repository/org/apache/tomcat/embed/tomcat-embed-websocket/9.0.27/tomcat-embed-websocket-9.0.27.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter-validation/2.2.1.RELEASE/spring-boot-starter-validation-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/jakarta/validation/jakarta.validation-api/2.0.1/jakarta.validation-api-2.0.1.jar:/Users/qinwen/.m2/repository/org/hibernate/validator/hibernate-validator/6.0.18.Final/hibernate-validator-6.0.18.Final.jar:/Users/qinwen/.m2/repository/org/jboss/logging/jboss-logging/3.4.1.Final/jboss-logging-3.4.1.Final.jar:/Users/qinwen/.m2/repository/com/fasterxml/classmate/1.5.1/classmate-1.5.1.jar:/Users/qinwen/.m2/repository/org/springframework/spring-web/5.2.1.RELEASE/spring-web-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/spring-webmvc/5.2.1.RELEASE/spring-webmvc-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/spring-expression/5.2.1.RELEASE/spring-expression-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/mybatis/spring/boot/mybatis-spring-boot-starter/2.1.1/mybatis-spring-boot-starter-2.1.1.jar:/Users/qinwen/.m2/repository/org/mybatis/spring/boot/mybatis-spring-boot-autoconfigure/2.1.1/mybatis-spring-boot-autoconfigure-2.1.1.jar:/Users/qinwen/.m2/repository/org/mybatis/mybatis/3.5.3/mybatis-3.5.3.jar:/Users/qinwen/.m2/repository/org/mybatis/mybatis-spring/2.0.3/mybatis-spring-2.0.3.jar:/Users/qinwen/.m2/repository/com/alibaba/fastjson/1.2.47/fastjson-1.2.47.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-devtools/2.2.1.RELEASE/spring-boot-devtools-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot/2.2.1.RELEASE/spring-boot-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-autoconfigure/2.2.1.RELEASE/spring-boot-autoconfigure-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/com/h2database/h2/1.4.200/h2-1.4.200.jar:/Users/qinwen/.m2/repository/mysql/mysql-connector-java/8.0.18/mysql-connector-java-8.0.18.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-configuration-processor/2.2.1.RELEASE/spring-boot-configuration-processor-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/com/jayway/jsonpath/json-path/2.4.0/json-path-2.4.0.jar:/Users/qinwen/.m2/repository/net/minidev/json-smart/2.3/json-smart-2.3.jar:/Users/qinwen/.m2/repository/net/minidev/accessors-smart/1.2/accessors-smart-1.2.jar:/Users/qinwen/.m2/repository/org/ow2/asm/asm/5.0.4/asm-5.0.4.jar:/Users/qinwen/.m2/repository/jakarta/xml/bind/jakarta.xml.bind-api/2.3.2/jakarta.xml.bind-api-2.3.2.jar:/Users/qinwen/.m2/repository/jakarta/activation/jakarta.activation-api/1.2.1/jakarta.activation-api-1.2.1.jar:/Users/qinwen/.m2/repository/org/springframework/spring-core/5.2.1.RELEASE/spring-core-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/spring-jcl/5.2.1.RELEASE/spring-jcl-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/io/projectreactor/reactor-core/3.3.0.RELEASE/reactor-core-3.3.0.RELEASE.jar:/Users/qinwen/.m2/repository/org/reactivestreams/reactive-streams/1.0.3/reactive-streams-1.0.3.jar:/Users/qinwen/.m2/repository/com/fasterxml/jackson/core/jackson-databind/2.10.0/jackson-databind-2.10.0.jar:/Users/qinwen/.m2/repository/com/fasterxml/jackson/core/jackson-annotations/2.10.0/jackson-annotations-2.10.0.jar:/Users/qinwen/.m2/repository/com/fasterxml/jackson/core/jackson-core/2.10.0/jackson-core-2.10.0.jar:/Users/qinwen/.m2/repository/org/springframework/boot/spring-boot-starter-web-services/2.2.1.RELEASE/spring-boot-starter-web-services-2.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/com/sun/xml/messaging/saaj/saaj-impl/1.5.1/saaj-impl-1.5.1.jar:/Users/qinwen/.m2/repository/jakarta/xml/soap/jakarta.xml.soap-api/1.4.1/jakarta.xml.soap-api-1.4.1.jar:/Users/qinwen/.m2/repository/org/jvnet/mimepull/mimepull/1.9.12/mimepull-1.9.12.jar:/Users/qinwen/.m2/repository/org/jvnet/staxex/stax-ex/1.8.1/stax-ex-1.8.1.jar:/Users/qinwen/.m2/repository/jakarta/xml/ws/jakarta.xml.ws-api/2.3.2/jakarta.xml.ws-api-2.3.2.jar:/Users/qinwen/.m2/repository/jakarta/jws/jakarta.jws-api/1.1.1/jakarta.jws-api-1.1.1.jar:/Users/qinwen/.m2/repository/org/springframework/spring-oxm/5.2.1.RELEASE/spring-oxm-5.2.1.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/ws/spring-ws-core/3.0.8.RELEASE/spring-ws-core-3.0.8.RELEASE.jar:/Users/qinwen/.m2/repository/org/springframework/ws/spring-xml/3.0.8.RELEASE/spring-xml-3.0.8.RELEASE.jar:/Users/qinwen/.m2/repository/org/mybatis/mybatis-typehandlers-jsr310/1.0.1/mybatis-typehandlers-jsr310-1.0.1.jar:/Users/qinwen/.m2/repository/org/apache/commons/commons-lang3/3.3.2/commons-lang3-3.3.2.jar:/Users/qinwen/.m2/repository/org/slf4j/slf4j-api/1.7.25/slf4j-api-1.7.25.jar:/Users/qinwen/.m2/repository/org/slf4j/slf4j-log4j12/1.7.5/slf4j-log4j12-1.7.5.jar:/Users/qinwen/.m2/repository/log4j/log4j/1.2.17/log4j-1.2.17.jar:/Users/qinwen/.m2/repository/org/apache/logging/log4j/log4j-api/2.0-rc1/log4j-api-2.0-rc1.jar:/Users/qinwen/.m2/repository/org/apache/logging/log4j/log4j-core/2.0-rc1/log4j-core-2.0-rc1.jar" xmu.oomall.coupon.DiscountApplication
SLF4J: Class path contains multiple SLF4J bindings.
SLF4J: Found binding in [jar:file:/Users/qinwen/.m2/repository/ch/qos/logback/logback-classic/1.2.3/logback-classic-1.2.3.jar!/org/slf4j/impl/StaticLoggerBinder.class]
SLF4J: Found binding in [jar:file:/Users/qinwen/.m2/repository/org/slf4j/slf4j-log4j12/1.7.5/slf4j-log4j12-1.7.5.jar!/org/slf4j/impl/StaticLoggerBinder.class]
SLF4J: See http://www.slf4j.org/codes.html#multiple_bindings for an explanation.
SLF4J: Actual binding is of type [ch.qos.logback.classic.util.ContextSelectorStaticBinder]
2019-12-21 04:56:27.274  INFO 30257 --- [  restartedMain] .e.DevToolsPropertyDefaultsPostProcessor : Devtools property defaults active! Set 'spring.devtools.add-properties' to 'false' to disable
2019-12-21 04:56:27.898  INFO 30257 --- [  restartedMain] trationDelegate$BeanPostProcessorChecker : Bean 'configurationPropertiesRebinderAutoConfiguration' of type [org.springframework.cloud.autoconfigure.ConfigurationPropertiesRebinderAutoConfiguration$$EnhancerBySpringCGLIB$$195b6f] is not eligible for getting processed by all BeanPostProcessors (for example: not eligible for auto-proxying)

  .   ____          _            __ _ _
 /\\ / ___'_ __ _ _(_)_ __  __ _ \ \ \ \
( ( )\___ | '_ | '_| | '_ \/ _` | \ \ \ \
 \\/  ___)| |_)| | | | | || (_| |  ) ) ) )
  '  |____| .__|_| |_|_| |_\__, | / / / /
 =========|_|==============|___/=/_/_/_/
 :: Spring Boot ::        (v2.2.1.RELEASE)

2019-12-21 04:56:28.326  INFO 30257 --- [  restartedMain] xmu.oomall.coupon.DiscountApplication    : The following profiles are active: dev
2019-12-21 04:56:29.302  INFO 30257 --- [  restartedMain] .s.d.r.c.RepositoryConfigurationDelegate : Multiple Spring Data modules found, entering strict repository configuration mode!
2019-12-21 04:56:29.302  INFO 30257 --- [  restartedMain] .s.d.r.c.RepositoryConfigurationDelegate : Bootstrapping Spring Data repositories in DEFAULT mode.
2019-12-21 04:56:29.322  INFO 30257 --- [  restartedMain] .s.d.r.c.RepositoryConfigurationDelegate : Finished Spring Data repository scanning in 15ms. Found 0 repository interfaces.
2019-12-21 04:56:29.338  INFO 30257 --- [  restartedMain] .s.d.r.c.RepositoryConfigurationDelegate : Multiple Spring Data modules found, entering strict repository configuration mode!
2019-12-21 04:56:29.339  INFO 30257 --- [  restartedMain] .s.d.r.c.RepositoryConfigurationDelegate : Bootstrapping Spring Data repositories in DEFAULT mode.
2019-12-21 04:56:29.355  INFO 30257 --- [  restartedMain] .s.d.r.c.RepositoryConfigurationDelegate : Finished Spring Data repository scanning in 7ms. Found 0 repository interfaces.
2019-12-21 04:56:29.858  INFO 30257 --- [  restartedMain] o.s.cloud.context.scope.GenericScope     : BeanFactory id=d6261323-d728-38ea-894d-e39d5af7ccd9
2019-12-21 04:56:30.009  INFO 30257 --- [  restartedMain] trationDelegate$BeanPostProcessorChecker : Bean 'org.springframework.ws.config.annotation.DelegatingWsConfiguration' of type [org.springframework.ws.config.annotation.DelegatingWsConfiguration$$EnhancerBySpringCGLIB$$88971d71] is not eligible for getting processed by all BeanPostProcessors (for example: not eligible for auto-proxying)
2019-12-21 04:56:30.069  INFO 30257 --- [  restartedMain] .w.s.a.s.AnnotationActionEndpointMapping : Supporting [WS-Addressing August 2004, WS-Addressing 1.0]
2019-12-21 04:56:30.096  INFO 30257 --- [  restartedMain] trationDelegate$BeanPostProcessorChecker : Bean 'org.springframework.transaction.annotation.ProxyTransactionManagementConfiguration' of type [org.springframework.transaction.annotation.ProxyTransactionManagementConfiguration] is not eligible for getting processed by all BeanPostProcessors (for example: not eligible for auto-proxying)
2019-12-21 04:56:30.151  INFO 30257 --- [  restartedMain] trationDelegate$BeanPostProcessorChecker : Bean 'org.springframework.cloud.autoconfigure.ConfigurationPropertiesRebinderAutoConfiguration' of type [org.springframework.cloud.autoconfigure.ConfigurationPropertiesRebinderAutoConfiguration$$EnhancerBySpringCGLIB$$195b6f] is not eligible for getting processed by all BeanPostProcessors (for example: not eligible for auto-proxying)
2019-12-21 04:56:30.666  INFO 30257 --- [  restartedMain] o.s.b.w.embedded.tomcat.TomcatWebServer  : Tomcat initialized with port(s): 8082 (http)
2019-12-21 04:56:30.676  INFO 30257 --- [  restartedMain] o.apache.catalina.core.StandardService   : Starting service [Tomcat]
2019-12-21 04:56:30.676  INFO 30257 --- [  restartedMain] org.apache.catalina.core.StandardEngine  : Starting Servlet engine: [Apache Tomcat/9.0.27]
2019-12-21 04:56:30.787  INFO 30257 --- [  restartedMain] o.a.c.c.C.[Tomcat].[localhost].[/]       : Initializing Spring embedded WebApplicationContext
2019-12-21 04:56:30.787  INFO 30257 --- [  restartedMain] o.s.web.context.ContextLoader            : Root WebApplicationContext: initialization completed in 2442 ms
2019-12-21 04:56:30.970  INFO 30257 --- [  restartedMain] com.alibaba.druid.pool.DruidDataSource   : {dataSource-1} inited
2019-12-21 04:56:31.895  INFO 30257 --- [  restartedMain] o.s.b.a.h2.H2ConsoleAutoConfiguration    : H2 console available at '/h2-console'. Database available at 'jdbc:mysql://106.14.83.157:3306/oomall'
2019-12-21 04:56:33.096  WARN 30257 --- [  restartedMain] c.n.c.sources.URLConfigurationSource     : No URLs will be polled as dynamic configuration sources.
2019-12-21 04:56:33.096  INFO 30257 --- [  restartedMain] c.n.c.sources.URLConfigurationSource     : To enable URLs as dynamic configuration sources, define System property archaius.configurationSource.additionalUrls or make config.properties available on classpath.
2019-12-21 04:56:33.101  WARN 30257 --- [  restartedMain] c.n.c.sources.URLConfigurationSource     : No URLs will be polled as dynamic configuration sources.
2019-12-21 04:56:33.101  INFO 30257 --- [  restartedMain] c.n.c.sources.URLConfigurationSource     : To enable URLs as dynamic configuration sources, define System property archaius.configurationSource.additionalUrls or make config.properties available on classpath.
2019-12-21 04:56:33.423  INFO 30257 --- [  restartedMain] o.s.s.concurrent.ThreadPoolTaskExecutor  : Initializing ExecutorService 'applicationTaskExecutor'
2019-12-21 04:56:34.588  WARN 30257 --- [  restartedMain] o.s.b.d.a.OptionalLiveReloadServer       : Unable to start LiveReload server
2019-12-21 04:56:34.876  INFO 30257 --- [  restartedMain] o.s.c.n.eureka.InstanceInfoFactory       : Setting initial instance status as: STARTING
2019-12-21 04:56:34.914  INFO 30257 --- [  restartedMain] com.netflix.discovery.DiscoveryClient    : Initializing Eureka in region us-east-1
2019-12-21 04:56:35.079  INFO 30257 --- [  restartedMain] c.n.d.provider.DiscoveryJerseyProvider   : Using JSON encoding codec LegacyJacksonJson
2019-12-21 04:56:35.080  INFO 30257 --- [  restartedMain] c.n.d.provider.DiscoveryJerseyProvider   : Using JSON decoding codec LegacyJacksonJson
2019-12-21 04:56:35.209  INFO 30257 --- [  restartedMain] c.n.d.provider.DiscoveryJerseyProvider   : Using XML encoding codec XStreamXml
2019-12-21 04:56:35.209  INFO 30257 --- [  restartedMain] c.n.d.provider.DiscoveryJerseyProvider   : Using XML decoding codec XStreamXml
2019-12-21 04:56:35.451  INFO 30257 --- [  restartedMain] c.n.d.s.r.aws.ConfigClusterResolver      : Resolving eureka endpoints via configuration
2019-12-21 04:56:35.468  INFO 30257 --- [  restartedMain] com.netflix.discovery.DiscoveryClient    : Disable delta property : false
2019-12-21 04:56:35.468  INFO 30257 --- [  restartedMain] com.netflix.discovery.DiscoveryClient    : Single vip registry refresh property : null
2019-12-21 04:56:35.468  INFO 30257 --- [  restartedMain] com.netflix.discovery.DiscoveryClient    : Force full registry fetch : false
2019-12-21 04:56:35.468  INFO 30257 --- [  restartedMain] com.netflix.discovery.DiscoveryClient    : Application is null : false
2019-12-21 04:56:35.468  INFO 30257 --- [  restartedMain] com.netflix.discovery.DiscoveryClient    : Registered Applications size is zero : true
2019-12-21 04:56:35.468  INFO 30257 --- [  restartedMain] com.netflix.discovery.DiscoveryClient    : Application version is -1: true
2019-12-21 04:56:35.468  INFO 30257 --- [  restartedMain] com.netflix.discovery.DiscoveryClient    : Getting all instance registry info from the eureka server
2019-12-21 04:56:36.130  INFO 30257 --- [  restartedMain] com.netflix.discovery.DiscoveryClient    : The response status is 200
2019-12-21 04:56:36.133  INFO 30257 --- [  restartedMain] com.netflix.discovery.DiscoveryClient    : Starting heartbeat executor: renew interval is: 10
2019-12-21 04:56:36.136  INFO 30257 --- [  restartedMain] c.n.discovery.InstanceInfoReplicator     : InstanceInfoReplicator onDemand update allowed rate per min is 4
2019-12-21 04:56:36.140  INFO 30257 --- [  restartedMain] com.netflix.discovery.DiscoveryClient    : Discovery Client initialized at timestamp 1576875396139 with initial instances count: 15
2019-12-21 04:56:36.146  INFO 30257 --- [  restartedMain] o.s.c.n.e.s.EurekaServiceRegistry        : Registering application 3-1-COUPON-SERVICE with eureka with status UP
2019-12-21 04:56:36.147  INFO 30257 --- [  restartedMain] com.netflix.discovery.DiscoveryClient    : Saw local status change event StatusChangeEvent [timestamp=1576875396147, current=UP, previous=STARTING]
2019-12-21 04:56:36.149  INFO 30257 --- [nfoReplicator-0] com.netflix.discovery.DiscoveryClient    : DiscoveryClient_3-1-COUPON-SERVICE/zhengqiwendeair:3-1-COUPON-SERVICE:8082: registering service...
2019-12-21 04:56:36.214  INFO 30257 --- [  restartedMain] o.s.b.w.embedded.tomcat.TomcatWebServer  : Tomcat started on port(s): 8082 (http) with context path ''
2019-12-21 04:56:36.218  INFO 30257 --- [  restartedMain] xmu.oomall.coupon.DiscountApplication    : Started DiscountApplication in 9.631 seconds (JVM running for 11.385)
2019-12-21 04:56:36.285  INFO 30257 --- [nfoReplicator-0] com.netflix.discovery.DiscoveryClient    : DiscoveryClient_3-1-COUPON-SERVICE/zhengqiwendeair:3-1-COUPON-SERVICE:8082 - registration status: 204
2019-12-21 04:58:27.247  INFO 30257 --- [nio-8082-exec-1] o.a.c.c.C.[Tomcat].[localhost].[/]       : Initializing Spring DispatcherServlet 'dispatcherServlet'
2019-12-21 04:58:27.248  INFO 30257 --- [nio-8082-exec-1] o.s.web.servlet.DispatcherServlet        : Initializing Servlet 'dispatcherServlet'
2019-12-21 04:58:27.285  INFO 30257 --- [nio-8082-exec-1] o.s.web.servlet.DispatcherServlet        : Completed initialization in 36 ms
2019-12-21 04:58:27.518  INFO 30257 --- [nio-8082-exec-1] x.o.coupon.controller.CouponController   : order: {"id":null,"userId":1,"orderSn":"201912210458222575","statusCode":0,"consignee":"Clines","mobile":"15860852866","message":"Just Test Api","goodsPrice":440.0,"couponPrice":0,"rebatePrice":2.00,"freightPrice":null,"integralPrice":null,"shipSn":null,"shipChannel":null,"shipTime":null,"confirmTime":null,"endTime":null,"payTime":null,"parentId":null,"address":"福建省厦门市思明区曾厝安西路厦门大学海韵园科研楼1-106教室","gmtCreate":[2019,12,21,4,58,22,257000000],"gmtModified":[2019,12,21,4,58,22,257000000],"beDeleted":false,"addressObj":{"id":1,"userId":1,"cityId":147,"provinceId":13,"countyId":1407,"addressDetail":"曾厝安西路厦门大学海韵园科研楼1-106教室","mobile":"15860852866","postalCode":"361000","consignee":"Clines","beDefault":true,"gmtCreate":null,"gmtModified":null,"beDeleted":false,"province":"福建省","city":"厦门市","county":"思明区"},"user":{"id":1,"name":"92998201300","nickname":null,"password":"123456","gender":0,"birthday":[2019,12,21,0,0],"mobile":"13959288888","rebate":12500,"avatar":"1.jpg","lastLoginTime":[2019,12,19,16,47,15],"lastLoginIp":"218.18.157.228","userLevel":1,"wxOpenId":null,"sessionKey":null,"roleId":4,"gmtCreate":[2019,12,19,16,47,15],"gmtModified":[2019,12,19,16,47,15],"beDeleted":false},"orderItemList":[{"id":null,"orderId":null,"itemType":null,"statusCode":0,"number":5,"price":150.0,"dealPrice":null,"productId":89,"goodsId":60,"nameWithSpecifications":null,"picUrl":"string","gmtCreate":[2019,12,21,4,58,24,696000000],"gmtModified":[2019,12,21,4,58,24,696000000],"beDeleted":false,"product":{"id":89,"goodsId":60,"picUrl":"string","specifications":"string","price":30.0,"safetyStock":0,"gmtCreate":[2019,12,21,2,16,22,580000000],"gmtModified":[2019,12,21,2,16,22,580000000],"beDeleted":false,"goods":{"id":60,"name":"string","goodsSn":"string","shortName":"string","description":"string","brief":"string","picUrl":"string","detail":"string","statusCode":1,"shareUrl":null,"gallery":"string","goodsCategoryId":59,"brandId":56,"weight":10.0,"volume":"string","specialFreightId":1,"beSpecial":true,"price":1.0,"beDeleted":false,"gmtCreate":[2019,12,18,3,10,42,550000000],"gmtModified":[2019,12,18,9,59,49,370000000],"brandPo":null,"goodsCategoryPo":null,"productPoList":null,"grouponRule":null,"shareRule":null,"presaleRule":null}}},{"id":null,"orderId":null,"itemType":null,"statusCode":0,"number":5,"price":200.0,"dealPrice":null,"productId":90,"goodsId":60,"nameWithSpecifications":null,"picUrl":"string","gmtCreate":[2019,12,21,4,58,25,172000000],"gmtModified":[2019,12,21,4,58,25,172000000],"beDeleted":false,"product":{"id":90,"goodsId":60,"picUrl":"string","specifications":"string","price":40.0,"safetyStock":2,"gmtCreate":[2019,12,21,2,16,26,690000000],"gmtModified":[2019,12,21,2,16,26,690000000],"beDeleted":false,"goods":{"id":60,"name":"string","goodsSn":"string","shortName":"string","description":"string","brief":"string","picUrl":"string","detail":"string","statusCode":1,"shareUrl":null,"gallery":"string","goodsCategoryId":59,"brandId":56,"weight":10.0,"volume":"string","specialFreightId":1,"beSpecial":true,"price":1.0,"beDeleted":false,"gmtCreate":[2019,12,18,3,10,42,550000000],"gmtModified":[2019,12,18,9,59,49,370000000],"brandPo":null,"goodsCategoryPo":null,"productPoList":null,"grouponRule":null,"shareRule":null,"presaleRule":null}}},{"id":null,"orderId":null,"itemType":null,"statusCode":0,"number":3,"price":90.0,"dealPrice":null,"productId":91,"goodsId":58,"nameWithSpecifications":null,"picUrl":"string","gmtCreate":[2019,12,21,4,58,25,684000000],"gmtModified":[2019,12,21,4,58,25,684000000],"beDeleted":false,"product":{"id":91,"goodsId":58,"picUrl":"string","specifications":"string","price":30.0,"safetyStock":50,"gmtCreate":[2019,12,21,2,16,58,940000000],"gmtModified":[2019,12,21,2,16,58,940000000],"beDeleted":false,"goods":{"id":58,"name":"string","goodsSn":"string","shortName":"string","description":"string","brief":"string","picUrl":"string","detail":"string","statusCode":1,"shareUrl":null,"gallery":"string","goodsCategoryId":59,"brandId":56,"weight":10.0,"volume":"string","specialFreightId":1,"beSpecial":false,"price":100.3,"beDeleted":false,"gmtCreate":[2019,12,18,3,10,39,950000000],"gmtModified":[2019,12,21,2,16,59,20000000],"brandPo":null,"goodsCategoryPo":null,"productPoList":null,"grouponRule":null,"shareRule":null,"presaleRule":null}}}],"couponId":8,"paymentList":null}
2019-12-21 04:58:27.518  INFO 30257 --- [nio-8082-exec-1] x.o.coupon.controller.CouponController   : 处理前的订单样子
2019-12-21 04:58:27.520  INFO 30257 --- [nio-8082-exec-1] x.o.coupon.controller.CouponController   : 订单详情:OrderItemPo{id=null, orderId=null, itemType=null, statusCode=0, number=5, price=150.0, dealPrice=null, productId=89, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:24.696, gmtModified=2019-12-21T04:58:24.696, beDeleted=false}
2019-12-21 04:58:27.520  INFO 30257 --- [nio-8082-exec-1] x.o.coupon.controller.CouponController   : 订单详情:OrderItemPo{id=null, orderId=null, itemType=null, statusCode=0, number=5, price=200.0, dealPrice=null, productId=90, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:25.172, gmtModified=2019-12-21T04:58:25.172, beDeleted=false}
2019-12-21 04:58:27.521  INFO 30257 --- [nio-8082-exec-1] x.o.coupon.controller.CouponController   : 订单详情:OrderItemPo{id=null, orderId=null, itemType=null, statusCode=0, number=3, price=90.0, dealPrice=null, productId=91, goodsId=58, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:25.684, gmtModified=2019-12-21T04:58:25.684, beDeleted=false}
2019-12-21 04:58:27.522  INFO 30257 --- [nio-8082-exec-1] x.o.c.service.impl.CouponServiceImpl     : couponID:8
2019-12-21 04:58:27.937  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.dao.CouponDao          : couponRulePo:CouponRule{discountStrategy=null, goodsList=[]}
2019-12-21 04:58:28.003  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.dao.CouponDao          : couponRuleStrategy:{"name":"CashOffStrategy", "obj":{"threshold":200.00, "offCash":50.00}}
2019-12-21 04:58:28.020  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.dao.CouponDao          : couponRuleGoodslist:[60]
2019-12-21 04:58:28.020  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.dao.CouponDao          : coupon:Coupon{couponRule=CouponRule{discountStrategy=CashOffStrategy{threshold=200.0, offCash=50.0}, goodsList=[60]}}
2019-12-21 04:58:28.021  INFO 30257 --- [nio-8082-exec-1] x.o.c.service.impl.CouponServiceImpl     : 优惠券详情:Coupon{couponRule=CouponRule{discountStrategy=CashOffStrategy{threshold=200.0, offCash=50.0}, goodsList=[60]}}
2019-12-21 04:58:28.033  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 适用优惠券的商品列表：[60]
2019-12-21 04:58:28.033  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.CashOffStrategy                : 可以使用优惠券的OrderItem:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=150.0, dealPrice=null, productId=89, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:24.696, gmtModified=2019-12-21T04:58:24.696, beDeleted=false}
2019-12-21 04:58:28.034  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.CashOffStrategy                : 可以使用优惠券的OrderItem:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=200.0, dealPrice=null, productId=90, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:25.172, gmtModified=2019-12-21T04:58:25.172, beDeleted=false}
2019-12-21 04:58:28.034  INFO 30257 --- [nio-8082-exec-1] x.o.c.service.impl.CouponServiceImpl     : 订单商品总价:440.0
2019-12-21 04:58:28.034  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : goodsPrice:440.0
2019-12-21 04:58:28.035  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : couponPrice:50.0
2019-12-21 04:58:28.035  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : rebatePrice:2.00
2019-12-21 04:58:28.036  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 适用优惠券的商品列表：[60]
2019-12-21 04:58:28.037  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.CashOffStrategy                : 可以使用优惠券的OrderItem:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=150.0, dealPrice=null, productId=89, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:24.696, gmtModified=2019-12-21T04:58:24.696, beDeleted=false}
2019-12-21 04:58:28.037  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.CashOffStrategy                : 可以使用优惠券的OrderItem:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=200.0, dealPrice=null, productId=90, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:25.172, gmtModified=2019-12-21T04:58:25.172, beDeleted=false}
2019-12-21 04:58:28.037  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 该orderItem总价:150.0
2019-12-21 04:58:28.038  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 占总价格的百分比：0.42
2019-12-21 04:58:28.038  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 可以拥有的折扣(处理前)：21.000
2019-12-21 04:58:28.043  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 可以拥有的折扣(处理后)：21.00
2019-12-21 04:58:28.043  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 成交价格：129.00
2019-12-21 04:58:28.044  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 该orderItem总价:200.0
2019-12-21 04:58:28.045  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 占总价格的百分比：0.57
2019-12-21 04:58:28.045  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 可以拥有的折扣(处理前)：28.500
2019-12-21 04:58:28.045  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 可以拥有的折扣(处理后)：28.50
2019-12-21 04:58:28.046  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 成交价格：171.50
2019-12-21 04:58:28.046  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 总价：350.0
2019-12-21 04:58:28.047  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 实际支付价格：300.50
2019-12-21 04:58:28.047  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 误差：0.50
2019-12-21 04:58:28.047  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 折扣订单详情：[OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=150.0, dealPrice=129.00, productId=89, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:24.696, gmtModified=2019-12-21T04:58:24.696, beDeleted=false}, OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=200.0, dealPrice=171.50, productId=90, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:25.172, gmtModified=2019-12-21T04:58:25.172, beDeleted=false}]
2019-12-21 04:58:28.047  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 总价：350.0
2019-12-21 04:58:28.048  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 实际支付价格：300.00
2019-12-21 04:58:28.048  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 误差：0.00
2019-12-21 04:58:28.048  INFO 30257 --- [nio-8082-exec-1] x.o.c.d.D.AbstractDiscountStrategy       : 折扣订单详情：[OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=150.0, dealPrice=128.50, productId=89, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:24.696, gmtModified=2019-12-21T04:58:24.696, beDeleted=false}, OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=200.0, dealPrice=171.50, productId=90, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:25.172, gmtModified=2019-12-21T04:58:25.172, beDeleted=false}]
2019-12-21 04:58:28.049  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 分返点的百分比:0.34
2019-12-21 04:58:28.050  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 可以分配的返点金额:0.6800
2019-12-21 04:58:28.050  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 可以分配的返点金额(处理后):0.68
2019-12-21 04:58:28.051  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 该订单项之前的dealPrice128.50
2019-12-21 04:58:28.055  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 该订单项处理后的的dealPrice127.82
2019-12-21 04:58:28.055  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 分返点的百分比:0.45
2019-12-21 04:58:28.056  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 可以分配的返点金额:0.9000
2019-12-21 04:58:28.056  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 可以分配的返点金额(处理后):0.90
2019-12-21 04:58:28.056  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 该订单项之前的dealPrice171.50
2019-12-21 04:58:28.057  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 该订单项处理后的的dealPrice170.60
2019-12-21 04:58:28.057  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 分返点的百分比:0.20
2019-12-21 04:58:28.057  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 可以分配的返点金额:0.4000
2019-12-21 04:58:28.057  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 可以分配的返点金额(处理后):0.40
2019-12-21 04:58:28.057  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 该订单项之前的dealPrice90.0
2019-12-21 04:58:28.057  INFO 30257 --- [nio-8082-exec-1] xmu.oomall.coupon.controller.DTO.Order   : 该订单项处理后的的dealPrice89.60
2019-12-21 04:58:28.068  INFO 30257 --- [nio-8082-exec-1] x.o.coupon.controller.CouponController   : payment:Payment{id=null, actualPrice=388.00, payChannel=null, statusCode=0, payTime=null, paySn='null', beginTime=2019-12-21T04:58:28.067, endTime=2019-12-21T05:28:28.067, orderId=null, gmtCreate=null, gmtModified=null, beDeleted=null}
2019-12-21 04:58:28.068  INFO 30257 --- [nio-8082-exec-1] x.o.coupon.controller.CouponController   : 处理后的订单样子
2019-12-21 04:58:28.068  INFO 30257 --- [nio-8082-exec-1] x.o.coupon.controller.CouponController   : 订单详情:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=150.0, dealPrice=127.80, productId=89, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:24.696, gmtModified=2019-12-21T04:58:24.696, beDeleted=false}
2019-12-21 04:58:28.069  INFO 30257 --- [nio-8082-exec-1] x.o.coupon.controller.CouponController   : 订单详情:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=200.0, dealPrice=170.60, productId=90, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:25.172, gmtModified=2019-12-21T04:58:25.172, beDeleted=false}
2019-12-21 04:58:28.069  INFO 30257 --- [nio-8082-exec-1] x.o.coupon.controller.CouponController   : 订单详情:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=3, price=90.0, dealPrice=89.60, productId=91, goodsId=58, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T04:58:25.684, gmtModified=2019-12-21T04:58:25.684, beDeleted=false}
2019-12-21 05:01:35.450  INFO 30257 --- [trap-executor-0] c.n.d.s.r.aws.ConfigClusterResolver      : Resolving eureka endpoints via configuration
2019-12-21 05:04:44.870  INFO 30257 --- [nio-8082-exec-3] x.o.coupon.controller.CouponController   : order: {"id":null,"userId":1,"orderSn":"201912210504406077","statusCode":0,"consignee":"Clines","mobile":"15860852866","message":"Just Test Api","goodsPrice":440.0,"couponPrice":0,"rebatePrice":2.00,"freightPrice":null,"integralPrice":null,"shipSn":null,"shipChannel":null,"shipTime":null,"confirmTime":null,"endTime":null,"payTime":null,"parentId":null,"address":"福建省厦门市思明区曾厝安西路厦门大学海韵园科研楼1-106教室","gmtCreate":[2019,12,21,5,4,40,607000000],"gmtModified":[2019,12,21,5,4,40,607000000],"beDeleted":false,"addressObj":{"id":1,"userId":1,"cityId":147,"provinceId":13,"countyId":1407,"addressDetail":"曾厝安西路厦门大学海韵园科研楼1-106教室","mobile":"15860852866","postalCode":"361000","consignee":"Clines","beDefault":true,"gmtCreate":null,"gmtModified":null,"beDeleted":false,"province":"福建省","city":"厦门市","county":"思明区"},"user":{"id":1,"name":"92998201300","nickname":null,"password":"123456","gender":0,"birthday":[2019,12,21,0,0],"mobile":"13959288888","rebate":12500,"avatar":"1.jpg","lastLoginTime":[2019,12,19,16,47,15],"lastLoginIp":"218.18.157.228","userLevel":1,"wxOpenId":null,"sessionKey":null,"roleId":4,"gmtCreate":[2019,12,19,16,47,15],"gmtModified":[2019,12,19,16,47,15],"beDeleted":false},"orderItemList":[{"id":null,"orderId":null,"itemType":null,"statusCode":0,"number":5,"price":150.0,"dealPrice":null,"productId":89,"goodsId":60,"nameWithSpecifications":null,"picUrl":"string","gmtCreate":[2019,12,21,5,4,42,829000000],"gmtModified":[2019,12,21,5,4,42,829000000],"beDeleted":false,"product":{"id":89,"goodsId":60,"picUrl":"string","specifications":"string","price":30.0,"safetyStock":0,"gmtCreate":[2019,12,21,2,16,22,580000000],"gmtModified":[2019,12,21,2,16,22,580000000],"beDeleted":false,"goods":{"id":60,"name":"string","goodsSn":"string","shortName":"string","description":"string","brief":"string","picUrl":"string","detail":"string","statusCode":0,"shareUrl":null,"gallery":"string","goodsCategoryId":59,"brandId":56,"weight":10.0,"volume":"string","specialFreightId":1,"beSpecial":true,"price":1.0,"beDeleted":false,"gmtCreate":[2019,12,18,3,10,42,550000000],"gmtModified":[2019,12,18,9,59,49,370000000],"brandPo":null,"goodsCategoryPo":null,"productPoList":null,"grouponRule":null,"shareRule":null,"presaleRule":null}}},{"id":null,"orderId":null,"itemType":null,"statusCode":0,"number":5,"price":200.0,"dealPrice":null,"productId":90,"goodsId":60,"nameWithSpecifications":null,"picUrl":"string","gmtCreate":[2019,12,21,5,4,43,196000000],"gmtModified":[2019,12,21,5,4,43,196000000],"beDeleted":false,"product":{"id":90,"goodsId":60,"picUrl":"string","specifications":"string","price":40.0,"safetyStock":2,"gmtCreate":[2019,12,21,2,16,26,690000000],"gmtModified":[2019,12,21,2,16,26,690000000],"beDeleted":false,"goods":{"id":60,"name":"string","goodsSn":"string","shortName":"string","description":"string","brief":"string","picUrl":"string","detail":"string","statusCode":0,"shareUrl":null,"gallery":"string","goodsCategoryId":59,"brandId":56,"weight":10.0,"volume":"string","specialFreightId":1,"beSpecial":true,"price":1.0,"beDeleted":false,"gmtCreate":[2019,12,18,3,10,42,550000000],"gmtModified":[2019,12,18,9,59,49,370000000],"brandPo":null,"goodsCategoryPo":null,"productPoList":null,"grouponRule":null,"shareRule":null,"presaleRule":null}}},{"id":null,"orderId":null,"itemType":null,"statusCode":0,"number":3,"price":90.0,"dealPrice":null,"productId":91,"goodsId":58,"nameWithSpecifications":null,"picUrl":"string","gmtCreate":[2019,12,21,5,4,43,747000000],"gmtModified":[2019,12,21,5,4,43,747000000],"beDeleted":false,"product":{"id":91,"goodsId":58,"picUrl":"string","specifications":"string","price":30.0,"safetyStock":50,"gmtCreate":[2019,12,21,2,16,58,940000000],"gmtModified":[2019,12,21,2,16,58,940000000],"beDeleted":false,"goods":{"id":58,"name":"string","goodsSn":"string","shortName":"string","description":"string","brief":"string","picUrl":"string","detail":"string","statusCode":1,"shareUrl":null,"gallery":"string","goodsCategoryId":59,"brandId":56,"weight":10.0,"volume":"string","specialFreightId":1,"beSpecial":false,"price":100.3,"beDeleted":false,"gmtCreate":[2019,12,18,3,10,39,950000000],"gmtModified":[2019,12,21,2,16,59,20000000],"brandPo":null,"goodsCategoryPo":null,"productPoList":null,"grouponRule":null,"shareRule":null,"presaleRule":null}}}],"couponId":8,"paymentList":null}
2019-12-21 05:04:44.870  INFO 30257 --- [nio-8082-exec-3] x.o.coupon.controller.CouponController   : 处理前的订单样子
2019-12-21 05:04:44.871  INFO 30257 --- [nio-8082-exec-3] x.o.coupon.controller.CouponController   : 订单详情:OrderItemPo{id=null, orderId=null, itemType=null, statusCode=0, number=5, price=150.0, dealPrice=null, productId=89, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:42.829, gmtModified=2019-12-21T05:04:42.829, beDeleted=false}
2019-12-21 05:04:44.871  INFO 30257 --- [nio-8082-exec-3] x.o.coupon.controller.CouponController   : 订单详情:OrderItemPo{id=null, orderId=null, itemType=null, statusCode=0, number=5, price=200.0, dealPrice=null, productId=90, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:43.196, gmtModified=2019-12-21T05:04:43.196, beDeleted=false}
2019-12-21 05:04:44.871  INFO 30257 --- [nio-8082-exec-3] x.o.coupon.controller.CouponController   : 订单详情:OrderItemPo{id=null, orderId=null, itemType=null, statusCode=0, number=3, price=90.0, dealPrice=null, productId=91, goodsId=58, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:43.747, gmtModified=2019-12-21T05:04:43.747, beDeleted=false}
2019-12-21 05:04:44.871  INFO 30257 --- [nio-8082-exec-3] x.o.c.service.impl.CouponServiceImpl     : couponID:8
2019-12-21 05:04:45.111  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.dao.CouponDao          : couponRulePo:CouponRule{discountStrategy=null, goodsList=[]}
2019-12-21 05:04:45.115  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.dao.CouponDao          : couponRuleStrategy:{"name":"CashOffStrategy", "obj":{"threshold":200.00, "offCash":50.00}}
2019-12-21 05:04:45.116  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.dao.CouponDao          : couponRuleGoodslist:[60]
2019-12-21 05:04:45.116  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.dao.CouponDao          : coupon:Coupon{couponRule=CouponRule{discountStrategy=CashOffStrategy{threshold=200.0, offCash=50.0}, goodsList=[60]}}
2019-12-21 05:04:45.116  INFO 30257 --- [nio-8082-exec-3] x.o.c.service.impl.CouponServiceImpl     : 优惠券详情:Coupon{couponRule=CouponRule{discountStrategy=CashOffStrategy{threshold=200.0, offCash=50.0}, goodsList=[60]}}
2019-12-21 05:04:45.116  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 适用优惠券的商品列表：[60]
2019-12-21 05:04:45.116  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.CashOffStrategy                : 可以使用优惠券的OrderItem:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=150.0, dealPrice=null, productId=89, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:42.829, gmtModified=2019-12-21T05:04:42.829, beDeleted=false}
2019-12-21 05:04:45.116  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.CashOffStrategy                : 可以使用优惠券的OrderItem:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=200.0, dealPrice=null, productId=90, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:43.196, gmtModified=2019-12-21T05:04:43.196, beDeleted=false}
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] x.o.c.service.impl.CouponServiceImpl     : 订单商品总价:440.0
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : goodsPrice:440.0
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : couponPrice:50.0
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : rebatePrice:2.00
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 适用优惠券的商品列表：[60]
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.CashOffStrategy                : 可以使用优惠券的OrderItem:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=150.0, dealPrice=null, productId=89, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:42.829, gmtModified=2019-12-21T05:04:42.829, beDeleted=false}
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.CashOffStrategy                : 可以使用优惠券的OrderItem:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=200.0, dealPrice=null, productId=90, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:43.196, gmtModified=2019-12-21T05:04:43.196, beDeleted=false}
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 该orderItem总价:150.0
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 占总价格的百分比：0.42
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 可以拥有的折扣(处理前)：21.000
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 可以拥有的折扣(处理后)：21.00
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 成交价格：129.00
2019-12-21 05:04:45.117  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 该orderItem总价:200.0
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 占总价格的百分比：0.57
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 可以拥有的折扣(处理前)：28.500
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 可以拥有的折扣(处理后)：28.50
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 成交价格：171.50
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 总价：350.0
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 实际支付价格：300.50
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 误差：0.50
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 折扣订单详情：[OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=150.0, dealPrice=129.00, productId=89, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:42.829, gmtModified=2019-12-21T05:04:42.829, beDeleted=false}, OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=200.0, dealPrice=171.50, productId=90, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:43.196, gmtModified=2019-12-21T05:04:43.196, beDeleted=false}]
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 总价：350.0
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 实际支付价格：300.00
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 误差：0.00
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] x.o.c.d.D.AbstractDiscountStrategy       : 折扣订单详情：[OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=150.0, dealPrice=128.50, productId=89, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:42.829, gmtModified=2019-12-21T05:04:42.829, beDeleted=false}, OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=200.0, dealPrice=171.50, productId=90, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:43.196, gmtModified=2019-12-21T05:04:43.196, beDeleted=false}]
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 分返点的百分比:0.34
2019-12-21 05:04:45.118  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 可以分配的返点金额:0.6800
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 可以分配的返点金额(处理后):0.68
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 该订单项之前的dealPrice128.50
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 该订单项处理后的的dealPrice127.82
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 分返点的百分比:0.45
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 可以分配的返点金额:0.9000
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 可以分配的返点金额(处理后):0.90
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 该订单项之前的dealPrice171.50
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 该订单项处理后的的dealPrice170.60
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 分返点的百分比:0.20
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 可以分配的返点金额:0.4000
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 可以分配的返点金额(处理后):0.40
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 该订单项之前的dealPrice90.0
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] xmu.oomall.coupon.controller.DTO.Order   : 该订单项处理后的的dealPrice89.60
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] x.o.coupon.controller.CouponController   : payment:Payment{id=null, actualPrice=388.00, payChannel=null, statusCode=0, payTime=null, paySn='null', beginTime=2019-12-21T05:04:45.119, endTime=2019-12-21T05:34:45.119, orderId=null, gmtCreate=null, gmtModified=null, beDeleted=null}
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] x.o.coupon.controller.CouponController   : 处理后的订单样子
2019-12-21 05:04:45.119  INFO 30257 --- [nio-8082-exec-3] x.o.coupon.controller.CouponController   : 订单详情:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=150.0, dealPrice=127.80, productId=89, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:42.829, gmtModified=2019-12-21T05:04:42.829, beDeleted=false}
2019-12-21 05:04:45.120  INFO 30257 --- [nio-8082-exec-3] x.o.coupon.controller.CouponController   : 订单详情:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=5, price=200.0, dealPrice=170.60, productId=90, goodsId=60, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:43.196, gmtModified=2019-12-21T05:04:43.196, beDeleted=false}
2019-12-21 05:04:45.120  INFO 30257 --- [nio-8082-exec-3] x.o.coupon.controller.CouponController   : 订单详情:OrderItemPo{id=null, orderId=null, itemType=0, statusCode=0, number=3, price=90.0, dealPrice=89.60, productId=91, goodsId=58, nameWithSpecifications='null', picUrl='string', gmtCreate=2019-12-21T05:04:43.747, gmtModified=2019-12-21T05:04:43.747, beDeleted=false}
2019-12-21 05:05:41.181  INFO 30257 --- [extShutdownHook] o.s.c.n.e.s.EurekaServiceRegistry        : Unregistering application 3-1-COUPON-SERVICE with eureka with status DOWN
2019-12-21 05:05:41.182  WARN 30257 --- [extShutdownHook] com.netflix.discovery.DiscoveryClient    : Saw local status change event StatusChangeEvent [timestamp=1576875941182, current=DOWN, previous=UP]
2019-12-21 05:05:41.184  INFO 30257 --- [nfoReplicator-0] com.netflix.discovery.DiscoveryClient    : DiscoveryClient_3-1-COUPON-SERVICE/zhengqiwendeair:3-1-COUPON-SERVICE:8082: registering service...
2019-12-21 05:05:41.186  INFO 30257 --- [extShutdownHook] com.netflix.discovery.DiscoveryClient    : Shutting down DiscoveryClient ...
2019-12-21 05:05:41.297  INFO 30257 --- [nfoReplicator-0] com.netflix.discovery.DiscoveryClient    : DiscoveryClient_3-1-COUPON-SERVICE/zhengqiwendeair:3-1-COUPON-SERVICE:8082 - registration status: 204
2019-12-21 05:05:41.299  INFO 30257 --- [extShutdownHook] com.netflix.discovery.DiscoveryClient    : Unregistering ...
2019-12-21 05:05:41.394  INFO 30257 --- [extShutdownHook] com.netflix.discovery.DiscoveryClient    : DiscoveryClient_3-1-COUPON-SERVICE/zhengqiwendeair:3-1-COUPON-SERVICE:8082 - deregister  status: 200
2019-12-21 05:05:41.413  INFO 30257 --- [extShutdownHook] com.netflix.discovery.DiscoveryClient    : Completed shut down of DiscoveryClient
2019-12-21 05:05:41.461  INFO 30257 --- [extShutdownHook] o.s.s.concurrent.ThreadPoolTaskExecutor  : Shutting down ExecutorService 'applicationTaskExecutor'
2019-12-21 05:05:41.466  INFO 30257 --- [extShutdownHook] com.alibaba.druid.pool.DruidDataSource   : {dataSource-1} closing ...
2019-12-21 05:05:41.475  INFO 30257 --- [extShutdownHook] com.alibaba.druid.pool.DruidDataSource   : {dataSource-1} closed

Process finished with exit code 130 (interrupted by signal 2: SIGINT)

```

