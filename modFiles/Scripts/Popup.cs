using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using Qud.UI;
using ConsoleLib.Console;

namespace Plaidman.LightenMyLoad.Menus {
	public class ItemList {
		private static string GetItemLabel(bool selected, InventoryItem item) {
			var label = PopupUtils.GetSelectionLabel(selected) + " ";
			// label += PopupUtils.GetWeightLabel(item) + " ";
			label += PopupUtils.GetValueLabel(item) + " ";
			label += item.DisplayName;
			
			return label;
		}
		
		public static int[] ShowPopup(InventoryItem[] options) {
			var defaultSelected = 0;
			var weightSelected = 0;
			var selectedItems = new HashSet<int>();
			
			var sortedOptions = options.OrderByDescending((item) => { return item.Weight; }).ToArray();
			IRenderable[] itemIcons = sortedOptions.Select((item) => { return item.Icon; }).ToArray();
			string[] itemLabels = sortedOptions.Select((item) => {
				var selected = selectedItems.Contains(item.Index);
				return GetItemLabel(selected, item);
			}).ToArray();

			QudMenuItem[] menuCommands = new QudMenuItem[1]
			{
				new() {
					text = "{{W|[D]}} {{y|Drop Items}}",
					command = "option:-2",
					hotkey = "D"
				},
			};

			while (true) {
				var intro = "Mark items here, press 'd' to drop them.\n";
				intro += "Weight selected: {{W|" + weightSelected + "#}}\n\n";

				int selectedIndex = Popup.PickOption(
					Title: "Inventory Items",
					Intro: intro,
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

				var mappedItem = sortedOptions[selectedIndex];
				if (selectedItems.Contains(mappedItem.Index)) {
					selectedItems.Remove(mappedItem.Index);
					weightSelected -= mappedItem.Weight;
					itemLabels[selectedIndex] = GetItemLabel(false, mappedItem);
				} else {
					selectedItems.Add(mappedItem.Index);
					weightSelected += mappedItem.Weight;
					itemLabels[selectedIndex] = GetItemLabel(true, mappedItem);
				}

				defaultSelected = selectedIndex;
			}
		}
	}
};
