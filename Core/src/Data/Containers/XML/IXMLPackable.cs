using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LabFusion.XML
{
    public interface IXMLPackable {
        public void Pack(XElement element);

        public void Unpack(XElement element);
    }
}
