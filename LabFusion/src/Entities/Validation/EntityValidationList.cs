using LabFusion.Player;

namespace LabFusion.Entities;

public class EntityValidationList
{
    private readonly HashSet<IEntityValidator> _validators = new();
    public HashSet<IEntityValidator> Validators => _validators;

    public void Register(IEntityValidator validator)
    {
        if (_validators.Contains(validator))
        {
            return;
        }

        _validators.Add(validator);
    }

    public void Unregister(IEntityValidator validator)
    {
        _validators.Remove(validator);
    }

    public bool Validate(NetworkEntity entity, PlayerID player)
    {
        foreach (var validator in Validators) 
        {
            if (!validator.Validate(entity, player))
            {
                return false;
            }
        }

        return true;
    }
}
