## 优惠券

```java
/**
     * 6.根据order计算优惠券折扣，最后体现在修改orderItem的dealPrice为折扣后价格
     * 内部接口（order）
     * 测试成功
     * @param
     * @param orderRequest
     * @return
     */
    @PostMapping("/coupon/calcDiscount")
		@ResponseBody
    public Object calculDiscount(@RequestBody Order orderRequest){
        return orderRequest;
    }
```

## 预售

```java
/**
*根据order获取预售价格
**/
@PostMapping("/presale/order")
@ResponseBody
public Object calculPresale(@RequestBody Order order) {
    return presaleService.cacuPresale(order);
}
```

```java
/**
* 根据Order中的OrderItem中的xxx预售是否合法
* 这里订单接到前端请求，将要创建预售订单（等待团购模块确认合法性）
**/
@PostMapping("/order/checkPresale")
@ResponseBody
public Object checkPresaleInLaw(@RequestBody Order order) {
    return ResponseUtil.ok();
}
```



## 团购

```java
/**
* 根据Order中的OrderItem中的goodsId、下单时间 来判断团购是否合法
* 这里订单接到前端请求，将要创建团购订单（等待团购模块确认合法性）
**/
@PostMapping("/order/checkGroupon")
@ResponseBody
public Object checkRuleInLaw(@RequestBody Order order) {
    return ResponseUtil.ok();
}
```

## 订单

```java
/**
* 传入grouponRule，根据grouponRule中的GoodsId，BeginTime,EndTime查找团购订单，并统计人数
* 根据grouponRule中的List<Strategy>自己解析应该返款多少钱，并对刚刚查找出来的订单进行返款
* 返回成功失败的boolean
**/
@PostMapping("/order/groupOnFinish")
@ResponseBody
public Object groupRefund(@RequestBody GroupOnRule groupOnRule) {
    return orderService.groupRefund(groupOnRule);
}
```

```java
/**
* 传入grouponRule，根据grouponRule中的GoodsId，BeginTime,EndTime查找团购订单，并统计人数
* 返回符合的人数
**/
@PostMapping("/groupon/getPeople")
@ResponseBody
public Object cacuGroupPeople(@RequestBody GroupOnRule groupOnRule) {
    return presaleService.cacuGroupPeople(groupOnRule);
}
```

```java
/**
* 进入预售xx1阶段，对订单进行xx1操作
**/
```

```java
/**
* 进入预售xx2阶段，对订单进行xx2操作
**/
```

