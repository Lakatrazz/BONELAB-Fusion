namespace LabFusion.Entities;

public interface IEntityExtender
{
    bool IsRegistered { get; }

    void OnExtenderRegistered();

    void OnExtenderUnregistered();
}