using BenchmarkDotNet;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

ExpressionBenchmarks m = new ExpressionBenchmarks();
m.ExpressionCompile();
//BenchmarkRunner.Run<ExpressionBenchmarks>();