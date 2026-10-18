using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

using LabFusion.Network;

namespace MarrowFusion.Epic;

internal struct EpicLobbyQuery : ILobbyQuery
{
    public LobbySearch LobbySearch { get; set; }

    public EpicLobbyQuery(LobbySearch lobbySearch)
    {
        LobbySearch = lobbySearch;
    }

    public readonly ILobbyQuery WithEqual(string key, string value)
    {
        var attributeData = new AttributeData()
        {
            Key = key,
            Value = new AttributeDataValue()
            {
                AsUtf8 = value,
            },
        };

        var setParameterOptions = new LobbySearchSetParameterOptions()
        {
            Parameter = attributeData,
            ComparisonOp = ComparisonOp.Equal,
        };

        var result = LobbySearch.SetParameter(ref setParameterOptions);
        if (result != Result.Success)
        {
            EpicModule.Logger.Error($"Failed to set lobby query: {result}");
        }

        return this;
    }

    public readonly ILobbyQuery WithComparison(string key, int value, LobbyQueryComparison comparison)
    {
        var attributeData = new AttributeData()
        {
            Key = key,
            Value = new AttributeDataValue()
            {
                AsUtf8 = value.ToString(),
            },
        };

        var setParameterOptions = new LobbySearchSetParameterOptions()
        {
            Parameter = attributeData,
            ComparisonOp = GetEOSLobbyComparison(comparison),
        };

        var result = LobbySearch.SetParameter(ref setParameterOptions);
        if (result != Result.Success)
        {
            EpicModule.Logger.Error($"Failed to set lobby query: {result}");
        }

        return this;
    }

    private static ComparisonOp GetEOSLobbyComparison(LobbyQueryComparison comparison)
    {
        return comparison switch
        {
            LobbyQueryComparison.LessThanOrEqualTo => ComparisonOp.Lessthanorequal,
            LobbyQueryComparison.LessThan => ComparisonOp.Lessthan,
            LobbyQueryComparison.GreaterThan => ComparisonOp.Greaterthan,
            LobbyQueryComparison.GreaterThanOrEqualTo => ComparisonOp.Greaterthanorequal,
            LobbyQueryComparison.NotEqual => ComparisonOp.Notequal,
            _ => ComparisonOp.Equal,
        };
    }
}