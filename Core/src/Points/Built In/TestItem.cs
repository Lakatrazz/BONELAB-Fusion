#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Points {
    public class TestItem : PointItem {
        public override string Title => "Test Item";

        public override string Author => BitEconomy.InternalAuthor;

        public override string Description => "A debugging item for the point shop.";

        public override int Price => 0;
    }
}
#endif