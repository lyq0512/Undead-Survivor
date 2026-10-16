\# Undead Survivor 项目开发与架构规范



\## 1. 运行环境与技术栈

\- 引擎版本：Unity 6 (6000.5.7f1)

\- 编程语言：C# (.NET Standard 2.1)

\- 渲染管线：Universal Render Pipeline (URP)

\- 视图维度：2D 俯视角动作肉鸽 (Top-down Roguelike)



\## 2. 现有核心架构模式

\- \*\*单例管理器\*\*：

&#x20; - `GameManager.instance`：负责全局游戏状态、计时与核心生命周期。

&#x20; - `PoolManager.instance`：基于 `Queue<GameObject>` 驱动的对象池。

&#x20; - `AudioManager.instance`：负责 BGM/SFX 双音轨播放，统一通过枚举调度。

\- \*\*高频实体生命周期\*\*：

&#x20; - 严禁对敌人、弹幕、特效调用 `Instantiate` 与 `Destroy`。

&#x20; - 所有生成与回收必须接入 `PoolManager`。

&#x20; - 实体越界必须配合屏幕外围 Cleaner 物理触发器执行回池。

\- \*\*物理与碰撞\*\*：

&#x20; - 严格限制使用 `Physics2D`、`Rigidbody2D`、`Collider2D`、`OnTriggerEnter2D`。

&#x20; - 索敌计算优先使用 `Physics2D.OverlapCircleNonAlloc`，避免分配垃圾。



\## 3. 代码修改约束（防断链原则）

\- \*\*禁止随意重命名序列化字段\*\*：任何带有 `\[SerializeField]` 或 `public` 的字段在未获许可前不得重命名，防止 Unity Inspector 丢失引用。

\- \*\*严禁在 Update / FixedUpdate 中分配堆内存\*\*：禁止在主循环中书写 LINQ 表达式、`new` 集合对象、`GetComponent` 或字符串高频拼接。

\- \*\*事件驱动解耦\*\*：数值变化（如击杀数、经验值）统一使用 C# `Action` 或 UnityEvent 广播，禁止每帧轮询。

