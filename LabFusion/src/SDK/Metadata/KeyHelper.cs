using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Metadata;

public static class KeyHelper
{
    public static string GetPlayerKey(string variable, PlayerId player)
    {
        if (player == null)
        {
            return string.Empty;
        }

        return $"{variable}.{player.LongId}";
    }
}