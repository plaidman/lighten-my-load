using System.Collections.Generic;

namespace Plaidman.LightenMyLoad.Menus {
	class ValueComparer : IComparer<InventoryItem> {
		public int Compare(InventoryItem x, InventoryItem y) {
			var xCat = Category(x);
			var yCat = Category(y);

			if (xCat != yCat) {
				return xCat - yCat;
			}

            return xCat switch
            {
                1 => CompareDouble((double)x.Ratio, (double)y.Ratio),
                3 => CompareDouble((double)x.Value, (double)y.Value),
                2 or 4 => y.Weight - x.Weight,
                _ => 0,
            };
        }
		
		private int CompareDouble(double x, double y) {
			if (x > y) { return 1; }
			if (x < y) { return -1; }
			return 0;
		}
		
		private int Category(InventoryItem item) {
			// 1: known items sort by ratio, lowest first
			// 2: unknown items sort by weight, highest first
			// 3: zero weight sort by value, lowest first
			// 4: unsellable sort by weight, highest first

			if (item.Weight <= 0) {
				return 3;
			}

			if (item.Value == null) {
				return 4;
			}

			if (item.Known) {
				return 1;
			}
			
			return 2;
		}
	}
}