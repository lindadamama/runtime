// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using System.Collections.Generic;

#nullable enable

namespace Wasm.Build.Tests;

public class IcuShardingTests2 : IcuTestsBase
{
    public IcuShardingTests2(ITestOutputHelper output, SharedBuildPerTestClassFixture buildContext)
        : base(output, buildContext) { }

    public static IEnumerable<object[]> IcuExpectedAndMissingShardFromRuntimePackTestData(Configuration config)
    {
        var locales = new Dictionary<string, string>
        {
            { "icudt.dat", $@"new Locale[] {{
                                    new Locale(""en-GB"", ""{SundayNames.English}""), new Locale(""zh-CN"", ""{SundayNames.Chinese}""), new Locale(""sk-SK"", ""{SundayNames.Slovak}""),
                                    new Locale(""xx-yy"", null) }}" },
            { "icudt_EFIGS.dat", GetEfigsTestedLocales() },
            { "icudt_CJK.dat", GetCjkTestedLocales() },
            { "icudt_no_CJK.dat", GetNocjkTestedLocales() }
        }; 
        return
            from aot in boolOptions
            from locale in locales
            select new object[] { config, aot, locale.Key, locale.Value };
    }

    [Theory]
    [MemberData(nameof(IcuExpectedAndMissingShardFromRuntimePackTestData), parameters: new object[] { Configuration.Release })]
    public async Task DefaultAvailableIcuShardsFromRuntimePack(Configuration config, bool aot, string shardName, string testedLocales) =>
        await TestIcuShards(config, Template.WasmBrowser, aot, shardName, testedLocales, GlobalizationMode.Custom);
}