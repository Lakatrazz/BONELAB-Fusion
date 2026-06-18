using LabFusion.Entities;

namespace LabFusion.Marrow.Data;

public interface IMarrowEntityRepresentationExtender : IEntityExtender
{
    public void OnRepresentationReceived(MarrowEntityRepresentation representation);
}
