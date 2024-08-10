using LabFusion.Player;

namespace LabFusion.Entities;

public interface IEntityValidator
{
    bool Validate(NetworkEntity entity, PlayerId player);
}