// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BenchmarkDotNet.Running;

namespace Microsoft.DependencyInjection.Benchmarks
{
    public class Program
    {
        public static void Main()
        {
            var _ = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
