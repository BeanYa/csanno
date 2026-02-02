using Csanno.Attributes;

namespace Csanno.Tests.Aop
{

    /// <summary>
    /// 示例服务类，用于测试 AOP 功能
    /// </summary>
    [Component]
    public class SampleService
    {
        /// <summary>
        /// 加法运算（带日志拦截）
        /// </summary>
        [Logging("Sample method execution")]
        public virtual int Add(int a, int b)
        {
            return a + b;
        }

        /// <summary>
        /// 问候方法（带日志拦截）
        /// </summary>
        [Logging]
        public virtual string Greet(string name)
        {
            return $"Hello, {name}!";
        }

        /// <summary>
        /// 无拦截注解的方法
        /// </summary>
        public virtual void NoInterception()
        {
            // 不会被拦截
        }

        /// <summary>
        /// 非 virtual 方法（不能被拦截）
        /// </summary>
        [Logging]
        public int NonVirtualMethod(int x)
        {
            return x * 2;
        }

        /// <summary>
        /// 模拟耗时计算（带缓存拦截）
        /// </summary>
        [Caching(expirationSeconds: 300)]
        public virtual int ExpensiveCalculation(int input)
        {
            // 模拟耗时操作
            return input * input;
        }

        /// <summary>
        /// 获取时间戳（带缓存拦截）
        /// 每次调用都会生成新的时间戳，但缓存会保证相同参数返回相同值
        /// </summary>
        [Caching]
        public virtual long GetTimestamp(string key)
        {
            return DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// 获取时间戳（无参数版本，用于缓存测试）
        /// </summary>
        [Caching]
        public virtual long GetTimestamp()
        {
            return DateTime.UtcNow.Ticks;
        }
    }

    /// <summary>
    /// 重载方法测试服务：相同参数数量但不同参数类型顺序
    /// </summary>
    [Component]
    public class OverloadService
    {
        [Logging]
        public virtual string Echo(int number, string text)
        {
            return $"{number}:{text}";
        }

        [Logging]
        public virtual string Echo(string text, int number)
        {
            return $"{text}:{number}";
        }
    }

    /// <summary>
    /// 带构造函数参数的服务
    /// </summary>
    [Component]
    public class ServiceWithDependency(string prefix)
    {
        private readonly string _prefix = prefix;

        [Logging]
        public virtual string Format(string message)
        {
            return $"{_prefix}: {message}";
        }
    }

    /// <summary>
    /// 调用链测试服务
    /// </summary>
    [Component]
    public class ChainTestService
    {
        /// <summary>
        /// 原生方法是否被调用的标记
        /// </summary>
        public static bool OriginalMethodCalled { get; private set; }

        /// <summary>
        /// 清除状态
        /// </summary>
        public static void Clear() => OriginalMethodCalled = false;

        /// <summary>
        /// 测试方法（带调用链测试注解）
        /// </summary>
        [ChainTest]
        public virtual int TestMethod(int value)
        {
            OriginalMethodCalled = true;
            return value * 2;
        }
    }

    /// <summary>
    /// 异常处理测试服务
    /// </summary>
    [Component]
    public class ExceptionTestService
    {
        /// <summary>
        /// 原生方法是否被调用的标记
        /// </summary>
        public static bool OriginalMethodCalled { get; private set; }

        /// <summary>
        /// 清除状态
        /// </summary>
        public static void Clear() => OriginalMethodCalled = false;

        /// <summary>
        /// 测试方法（带异常测试注解）
        /// </summary>
        [ExceptionTest]
        public virtual int TestMethod(int value)
        {
            OriginalMethodCalled = true;
            return value * 2;
        }

        /// <summary>
        /// 无返回值的测试方法
        /// </summary>
        [ExceptionTest]
        public virtual void VoidMethod()
        {
            OriginalMethodCalled = true;
        }
    }

    /// <summary>
    /// 继承场景基类
    /// </summary>
    public class BaseService
    {
        /// <summary>
        /// 基类虚方法（可被子类覆盖和拦截）
        /// </summary>
        [Logging]
        public virtual string BaseVirtualMethod(string input)
        {
            return $"Base: {input}";
        }

        /// <summary>
        /// 基类非虚方法（不可被覆盖）
        /// </summary>
        [Logging]
        public string BaseNonVirtualMethod(string input)
        {
            return $"BaseNonVirtual: {input}";
        }
    }

    /// <summary>
    /// 继承场景子类
    /// </summary>
    [Component]
    public class DerivedService : BaseService
    {
        /// <summary>
        /// 覆盖基类虚方法
        /// </summary>
        [Logging]
        public override string BaseVirtualMethod(string input)
        {
            return $"Derived: {input}";
        }

        /// <summary>
        /// 子类自己的虚方法
        /// </summary>
        [Logging]
        public virtual string DerivedVirtualMethod(string input)
        {
            return $"DerivedOwn: {input}";
        }
    }

    /// <summary>
    /// 用于测试拦截器异常传播的服务
    /// </summary>
    [Component]
    public class InterceptorExceptionPropagationService
    {
        public static bool OriginalMethodCalled { get; private set; }
        public static void Clear() => OriginalMethodCalled = false;

        [PropagatingException]
        public virtual int TestMethod(int value)
        {
            OriginalMethodCalled = true;
            return value * 2;
        }
    }

}
