using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using Qud.UI;
using ConsoleLib.Console;
using XRL;

namespace Plaidman.LightenMyLoad.Menus {
	public class ItemList {
		private static string GetItemLabel(bool selected, InventoryItem item) {
			var label = PopupLabelUtils.GetSelectionLabel(selected) + " ";
			// label += PopupLabelUtils.GetWeightLabel(item) + " ";
			label += PopupLabelUtils.GetValueLabel(item) + " ";
			label += item.DisplayName;
			
			return label;
		}
		
		public static int[] ShowPopup(InventoryItem[] options) {
			var defaultSelected = 0;
			var weightSelected = 0;
			var selectedItems = new HashSet<int>();
			
			// options.Sort(new WeightComparer());
			options.Sort(new ValueComparer());
			IRenderable[] itemIcons = options.Select((item) => { return item.Icon; }).ToArray();
			string[] itemLabels = options.Select((item) => {
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

				var mappedItem = options[selectedIndex];
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
