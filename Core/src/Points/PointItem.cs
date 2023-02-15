using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Points {
    public abstract class PointItem {
        // The title of the item
        public abstract string Title { get; }

        // The author of the item
        public abstract string Author { get; }

        // The version of the item
        public virtual string Version => "1.0.0";

        // The description of the item
        public abstract string Description { get; }

        // The barcode pointing to the item
        public virtual string Barcode => $"{Author}.{Title}.Item";

        // The price of the item in bits (currency)
        public abstract int Price { get; }

        // The rarity level of the item.
        public virtual RarityLevel Rarity => RarityLevel.White;

        // The preview image of the item in the menu. (Optional)
        public virtual Texture2D PreviewImage => null;

        public bool IsUnlocked => PointSaveManager.IsUnlocked(Barcode);
    }
}
