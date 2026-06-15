namespace LabFusion.Entities;

public interface IEntityDespawnableExtender : IEntityExtender
{
    void OnDespawnReceived();

    void PlayDespawnEffect();
}
