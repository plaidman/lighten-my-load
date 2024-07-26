using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using Qud.UI;
using ConsoleLib.Console;
using XRL.World;

namespace Plaidman.LightenMyLoad.Menus {
	public class ItemList {
		public class InventoryItem {
			public int Index { get; }
			public string DisplayName { get; }
			public IRenderable Icon { get; }

			public InventoryItem(int index, GameObject go) {
				Index = index;
				DisplayName = go.DisplayName;
				Icon = go.Render;
			}
		}

		public static int[] ShowPopup(InventoryItem[] options) {
			var defaultSelected = 0;
			var selectedItems = new HashSet<int>();
			string[] itemLabels = new string[options.Length];
			IRenderable[] itemIcons;
			
			itemIcons = options.Select((item) => { return item.Icon; }).ToArray();

			QudMenuItem[] menuCommands = new QudMenuItem[1]
			{
				new() {
					text = "{{W|[D]}} {{y|Drop Items}}",
					command = "option:-2",
					hotkey = "D"
				},
			};

			while (true) {
				for (int i = 0; i < itemLabels.Length; i++) {
					itemLabels[i] = (selectedItems.Contains(i) ? "{{W|[þ]}} " : "{{y|[ ]}} ") + options[i].DisplayName;
				}

				int selectedIndex = Popup.PickOption(
					Title: "Inventory Items",
					Intro: "Mark items here, press 'd' to drop them.",
					IntroIcon: null,
					Options: itemLabels,
					RespectOptionNewlines: false,
					Icons: itemIcons,
					DefaultSelected: defaultSelected,
					Buttons: menuCommands,
					AllowEscape: true
				);

				switch (selectedIndex) {
					case -1:  // Esc / Cancelled
						return null;

					case -2: // d drop items
						return selectedItems.ToArray();

					default:
						break;
				}

				if (selectedItems.Contains(selectedIndex)) {
					selectedItems.Remove(selectedIndex);
				} else {
					selectedItems.Add(selectedIndex);
				}

				defaultSelected = selectedIndex;
			}
		}
	}
};
