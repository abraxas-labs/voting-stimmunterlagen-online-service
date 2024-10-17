// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.IO;

namespace Voting.Stimmunterlagen.Core.Utils;

public static class StimmunterlagenOutDirectoryUtils
{
    public static string OutDirectoryBasePath => Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "voting-stimmunterlagen");
}
