﻿// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Voting.Stimmunterlagen.Core.Managers.VotingExport.Converters;

public class VotingCsvExportDateConverter : DefaultTypeConverter
{
    public override string ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
        => (value as DateTime?)?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
}
