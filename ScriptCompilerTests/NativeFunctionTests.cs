using System;
using System.Collections.Generic;
using ForgottenRPG.VM;
using ForgottenRPG.Entity;
using ForgottenRPG.Entity.Components;
using ForgottenRPG.Scripting;
using ForgottenRPG.NativeFunctions;
using Xunit;

namespace ScriptCompilerTests {
    public class NativeFunctionTests {
        // Simple test native function
        [NativeFunction("test_add")]
        public static int TestAdd(int a, int b) {
            return a + b;
        }

        [NativeFunction("test_multiply")]
        public static int TestMultiply(int a, int b) {
            return a * b;
        }

        [NativeFunction("test_float_add")]
        public static float TestFloatAdd(float a, float b) {
            return a + b;
        }

        [NativeFunction("test_bool")]
        public static bool TestBool(int value) {
            return value > 0;
        }

        [NativeFunction("test_void")]
        public static void TestVoid(int value) {
            // Does nothing, just for testing void functions
        }

        [Fact]
        public void NativeRegistryCanDiscoverFunctions() {
            var registry = new NativeFunctionRegistry();
            registry.ScanType(typeof(NativeFunctionTests));

            Assert.True(registry.HasFunction("test_add"));
            Assert.True(registry.HasFunction("test_multiply"));
            Assert.True(registry.HasFunction("test_float_add"));
            Assert.True(registry.HasFunction("test_bool"));
            Assert.True(registry.HasFunction("test_void"));
        }

        [Fact]
        public void NativeContextCanPushAndPopIntegers() {
            var stack = new Stack<uint>();
            var entityRegistry = new Dictionary<int, object>();
            var context = new ScriptNativeContext(stack, entityRegistry);

            context.PushInt(42);
            context.PushInt(100);

            Assert.Equal(100, context.PopInt());
            Assert.Equal(42, context.PopInt());
        }

        [Fact]
        public void NativeContextCanPushAndPopFloats() {
            var stack = new Stack<uint>();
            var entityRegistry = new Dictionary<int, object>();
            var context = new ScriptNativeContext(stack, entityRegistry);

            context.PushFloat(3.14f);
            context.PushFloat(2.71f);

            Assert.Equal(2.71f, context.PopFloat(), 2);
            Assert.Equal(3.14f, context.PopFloat(), 2);
        }

        [Fact]
        public void NativeContextCanRegisterAndRetrieveEntities() {
            var stack = new Stack<uint>();
            var entityRegistry = new Dictionary<int, object>();
            var ctx = new ScriptNativeContext(stack, new Dictionary<int, object>());
            var entity = new object();
            int id = ctx.RegisterObject(entity);

            Assert.Same(entity, ctx.GetObject(id));
        }

        [Fact]
        public void NativeFunctionRegistryMarshalsIntegerArguments() {
            var registry = new NativeFunctionRegistry();
            registry.ScanType(typeof(NativeFunctionTests));

            var stack = new Stack<uint>();
            var entityRegistry = new Dictionary<int, object>();
            var context = new ScriptNativeContext(stack, entityRegistry);

            // Push arguments in reverse order (last parameter on top)
            context.PushInt(5);
            context.PushInt(3);

            var funcInfo = registry.GetByName("test_add");
            funcInfo.MarshalledDelegate(context);

            // Result should be pushed to stack
            Assert.Equal(8, context.PopInt());
        }

        [Fact]
        public void NativeFunctionRegistryMarshalsFloatArguments() {
            var registry = new NativeFunctionRegistry();
            registry.ScanType(typeof(NativeFunctionTests));

            var stack = new Stack<uint>();
            var entityRegistry = new Dictionary<int, object>();
            var context = new ScriptNativeContext(stack, entityRegistry);

            // Push arguments in reverse order
            context.PushFloat(2.5f);
            context.PushFloat(1.5f);

            var funcInfo = registry.GetByName("test_float_add");
            funcInfo.MarshalledDelegate(context);

            // Result should be pushed to stack
            Assert.Equal(4.0f, context.PopFloat(), 2);
        }

        [Fact]
        public void NativeFunctionRegistryMarshalsBooleanReturns() {
            var registry = new NativeFunctionRegistry();
            registry.ScanType(typeof(NativeFunctionTests));

            var stack = new Stack<uint>();
            var entityRegistry = new Dictionary<int, object>();
            var context = new ScriptNativeContext(stack, entityRegistry);

            context.PushInt(10);

            var funcInfo = registry.GetByName("test_bool");
            funcInfo.MarshalledDelegate(context);

            // Boolean true should be 1
            Assert.Equal(1, context.PopInt());
        }

        [Fact]
        public void NativeFunctionWithVoidReturnDoesNotPushToStack() {
            var registry = new NativeFunctionRegistry();
            registry.ScanType(typeof(NativeFunctionTests));

            var stack = new Stack<uint>();
            var entityRegistry = new Dictionary<int, object>();
            var context = new ScriptNativeContext(stack, entityRegistry);

            context.PushInt(42);

            var funcInfo = registry.GetByName("test_void");
            funcInfo.MarshalledDelegate(context);

            // Stack should be empty after void function
            Assert.Equal(0, stack.Count);
        }
    }
}
