// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Ech0007_5_0;
using Voting.Stimmunterlagen.Data.Models;

namespace Voting.Stimmunterlagen.Ech.Mapping.V4;

internal static class CantonMapping
{
    public static CantonAbbreviation ToCantonAbbreviation(this CantonAbbreviationType cantonAbbreviation)
    {
        return cantonAbbreviation switch
        {
            CantonAbbreviationType.Zh => CantonAbbreviation.ZH,
            CantonAbbreviationType.Be => CantonAbbreviation.BE,
            CantonAbbreviationType.Lu => CantonAbbreviation.LU,
            CantonAbbreviationType.Ur => CantonAbbreviation.UR,
            CantonAbbreviationType.Sz => CantonAbbreviation.SZ,
            CantonAbbreviationType.Ow => CantonAbbreviation.OW,
            CantonAbbreviationType.Nw => CantonAbbreviation.NW,
            CantonAbbreviationType.Gl => CantonAbbreviation.GL,
            CantonAbbreviationType.Zg => CantonAbbreviation.ZG,
            CantonAbbreviationType.Fr => CantonAbbreviation.FR,
            CantonAbbreviationType.So => CantonAbbreviation.SO,
            CantonAbbreviationType.Bs => CantonAbbreviation.BS,
            CantonAbbreviationType.Bl => CantonAbbreviation.BL,
            CantonAbbreviationType.Sh => CantonAbbreviation.SH,
            CantonAbbreviationType.Ar => CantonAbbreviation.AR,
            CantonAbbreviationType.Ai => CantonAbbreviation.AI,
            CantonAbbreviationType.Sg => CantonAbbreviation.SG,
            CantonAbbreviationType.Gr => CantonAbbreviation.GR,
            CantonAbbreviationType.Ag => CantonAbbreviation.AG,
            CantonAbbreviationType.Tg => CantonAbbreviation.TG,
            CantonAbbreviationType.Ti => CantonAbbreviation.TI,
            CantonAbbreviationType.Vd => CantonAbbreviation.VD,
            CantonAbbreviationType.Vs => CantonAbbreviation.VS,
            CantonAbbreviationType.Ne => CantonAbbreviation.NE,
            CantonAbbreviationType.Ge => CantonAbbreviation.GE,
            CantonAbbreviationType.Ju => CantonAbbreviation.JU,
            _ => throw new InvalidOperationException("invalid canton abbreviation " + cantonAbbreviation),
        };
    }

    public static CantonAbbreviationType ToEchCantonAbbreviation(this CantonAbbreviation cantonAbbreviation)
    {
        return cantonAbbreviation switch
        {
            CantonAbbreviation.ZH => CantonAbbreviationType.Zh,
            CantonAbbreviation.BE => CantonAbbreviationType.Be,
            CantonAbbreviation.LU => CantonAbbreviationType.Lu,
            CantonAbbreviation.UR => CantonAbbreviationType.Ur,
            CantonAbbreviation.SZ => CantonAbbreviationType.Sz,
            CantonAbbreviation.OW => CantonAbbreviationType.Ow,
            CantonAbbreviation.NW => CantonAbbreviationType.Nw,
            CantonAbbreviation.GL => CantonAbbreviationType.Gl,
            CantonAbbreviation.ZG => CantonAbbreviationType.Zg,
            CantonAbbreviation.FR => CantonAbbreviationType.Fr,
            CantonAbbreviation.SO => CantonAbbreviationType.So,
            CantonAbbreviation.BS => CantonAbbreviationType.Bs,
            CantonAbbreviation.BL => CantonAbbreviationType.Bl,
            CantonAbbreviation.SH => CantonAbbreviationType.Sh,
            CantonAbbreviation.AR => CantonAbbreviationType.Ar,
            CantonAbbreviation.AI => CantonAbbreviationType.Ai,
            CantonAbbreviation.SG => CantonAbbreviationType.Sg,
            CantonAbbreviation.GR => CantonAbbreviationType.Gr,
            CantonAbbreviation.AG => CantonAbbreviationType.Ag,
            CantonAbbreviation.TG => CantonAbbreviationType.Tg,
            CantonAbbreviation.TI => CantonAbbreviationType.Ti,
            CantonAbbreviation.VD => CantonAbbreviationType.Vd,
            CantonAbbreviation.VS => CantonAbbreviationType.Vs,
            CantonAbbreviation.NE => CantonAbbreviationType.Ne,
            CantonAbbreviation.GE => CantonAbbreviationType.Ge,
            CantonAbbreviation.JU => CantonAbbreviationType.Ju,
            _ => throw new InvalidOperationException("invalid canton abbreviation " + cantonAbbreviation),
        };
    }
}
