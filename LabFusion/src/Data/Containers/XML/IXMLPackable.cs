using System.Xml.Linq;

namespace LabFusion.XML
{
    public interface IXMLPackable
    {
        public void Pack(XElement element);

        public void Unpack(XElement element);
    }
}
