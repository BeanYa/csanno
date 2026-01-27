using System.Reflection;
using Csanno.Attributes;

namespace Csanno.Tests.Aop
{

    /// <summary>
    /// 异常测试注解
    /// </summary>
    public class ExceptionTestAttribute : BaseInterceptAttribute
    {
    }

    /// <summary>
    /// 在 OnBefore 中抛出异常的拦截器
    /// </summary>
    [BindWith<ExceptionTestAttribute>]
    public class ThrowingOnBeforeInterceptor : BaseInterceptor
    {
        /// <summary>
        /// 是否应该在 OnBefore 中抛出异常
        /// </summary>
        public static bool ShouldThrow { get; set; } = true;

        /// <summary>
        /// OnBeforeException 是否应返回 true（继续调用链）
        /// </summary>
        public static bool ShouldContinueOnException { get; set; } = true;

        /// <summary>
        /// 调用顺序记录
        /// </summary>
        public static List<string> CallOrder { get; } = [];

        /// <summary>
        /// 捕获的异常列表
        /// </summary>
        public static List<Exception> CaughtExceptions { get; } = [];

        /// <summary>
        /// 清除状态
        /// </summary>
        public static void Clear()
        {
            ShouldThrow = true;
            ShouldContinueOnException = true;
            CallOrder.Clear();
            CaughtExceptions.Clear();
        }

        /// <inheritdoc />
        public override bool OnBefore(MethodInfo method, object?[] args, InvokeResult invokeResult)
        {
            CallOrder.Add("OnBefore");
            if (ShouldThrow)
            {
                throw new InvalidOperationException("OnBefore exception");
            }
            return true;
        }

        /// <inheritdoc />
        public override void OnAfter(MethodInfo method, object? result)
        {
            CallOrder.Add("OnAfter");
        }

        /// <inheritdoc />
        public override bool OnBeforeException(MethodInfo method, object?[] args, Exception exception, InvokeResult invokeResult)
        {
            CallOrder.Add("OnBeforeException");
            CaughtExceptions.Add(exception);
            return ShouldContinueOnException;
        }

        /// <inheritdoc />
        public override bool OnAfterException(MethodInfo method, object? result, Exception exception)
        {
            CallOrder.Add("OnAfterException");
            CaughtExceptions.Add(exception);
            return true;
        }
    }

    /// <summary>
    /// 在 OnAfter 中抛出异常的拦截器
    /// </summary>
    [BindWith<ExceptionTestAttribute>]
    public class ThrowingOnAfterInterceptor : BaseInterceptor
    {
        /// <summary>
        /// 是否应该在 OnAfter 中抛出异常
        /// </summary>
        public static bool ShouldThrow { get; set; } = true;

        /// <summary>
        /// 调用顺序记录
        /// </summary>
        public static List<string> CallOrder { get; } = [];

        /// <summary>
        /// 捕获的异常列表
        /// </summary>
        public static List<Exception> CaughtExceptions { get; } = [];

        /// <summary>
        /// 清除状态
        /// </summary>
        public static void Clear()
        {
            ShouldThrow = true;
            CallOrder.Clear();
            CaughtExceptions.Clear();
        }

        /// <inheritdoc />
        public override bool OnBefore(MethodInfo method, object?[] args, InvokeResult invokeResult)
        {
            CallOrder.Add("OnBefore");
            return true;
        }

        /// <inheritdoc />
        public override void OnAfter(MethodInfo method, object? result)
        {
            CallOrder.Add("OnAfter");
            if (ShouldThrow)
            {
                throw new InvalidOperationException("OnAfter exception");
            }
        }

        /// <inheritdoc />
        public override bool OnBeforeException(MethodInfo method, object?[] args, Exception exception, InvokeResult invokeResult)
        {
            CallOrder.Add("OnBeforeException");
            CaughtExceptions.Add(exception);
            return true;
        }

        /// <inheritdoc />
        public override bool OnAfterException(MethodInfo method, object? result, Exception exception)
        {
            CallOrder.Add("OnAfterException");
            CaughtExceptions.Add(exception);
            return true;
        }
    }
}
