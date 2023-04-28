using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using System.Reflection;
using System.Linq.Expressions;

internal class ExpressionBenchmarks
{
    public MethodInfo method;

    [GlobalSetup]
    public void Setup()
    {
        method = typeof(ExpressionBenchmarks).GetMethods().Where((method) => method.Name == "TestAction").First();
    }

    [Benchmark]
    public void ExpressionCompile()
    {
    
    }

    public void TestAction() { Console.Write("Poop"); }
}
