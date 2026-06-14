namespace LabFusion.Entities;

public interface IEntityExtender
{
    void OnExtenderRegistered();

    void OnExtenderUnregistered();
}