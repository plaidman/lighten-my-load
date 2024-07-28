using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using Qud.UI;
using ConsoleLib.Console;
using XRL;

namespace Plaidman.LightenMyLoad.Menus {
	public class ItemList {
		private static string GetItemLabel(bool selected, InventoryItem item, bool sortByWeight) {
			var label = PopupLabelUtils.GetSelectionLabel(selected) + " ";
			
			if (sortByWeight) {
				label += PopupLabelUtils.GetWeightLabel(item) + " ";
			} else {
				label += PopupLabelUtils.GetValueLabel(item) + " ";
			}

			return label + item.DisplayName;
		}
		
		public static int[] ShowPopup(
			InventoryItem[] options,
			bool sortByWeight
		) {
			var defaultSelected = 0;
			var weightSelected = 0;
			var selectedItems = new HashSet<int>();
			var sortString = "";
			
			if (sortByWeight) {
				options.Sort(new WeightComparer());
				sortString = "weight";
			} else {
				options.Sort(new ValueComparer());
				sortString = "value";
			}

			IRenderable[] itemIcons = options.Select((item) => { return item.Icon; }).ToArray();
			string[] itemLabels = options.Select((item) => {
				var selected = selectedItems.Contains(item.Index);
				return GetItemLabel(selected, item, sortByWeight);
			}).ToArray();

			QudMenuItem[] menuCommands = new QudMenuItem[2]
			{
				new() {
					text = "{{W|[D]}} {{y|Drop Items}}",
					command = "option:-2",
					hotkey = "D"
				},
				new() {
					text = "{{W|[Tab]}} {{y|Sort Mode: " + sortString + "}}",
					command = "option:-3",
					hotkey = "Tab"
				},
			};

			while (true) {
				var intro = "Mark items here, press 'd' to drop them.\n";
				intro += "Selected Item Weight: {{w|" + weightSelected + "#}}\n\n";
				
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

					case -2: // D drop items
						return selectedItems.ToArray();

					default:
						break;
				}
				
				if (selectedIndex == -3) {
					sortByWeight = !sortByWeight;
					if (sortByWeight) {
						options.Sort(new WeightComparer());
						sortString = "weight";
					} else {
						options.Sort(new ValueComparer());
						sortString = "value";
					}

					menuCommands[1].text = "{{W|[Tab]}} {{y|Sort Mode: " + sortString + "}}";
					itemIcons = options.Select((item) => { return item.Icon; }).ToArray();
					itemLabels = options.Select((item) => {
						var selected = selectedItems.Contains(item.Index);
						return GetItemLabel(selected, item, sortByWeight);
					}).ToArray();
					
					continue;
				}

				var mappedItem = options[selectedIndex];
				if (selectedItems.Contains(mappedItem.Index)) {
					selectedItems.Remove(mappedItem.Index);
					weightSelected -= mappedItem.Weight;
					itemLabels[selectedIndex] = GetItemLabel(false, mappedItem, sortByWeight);
				} else {
					selectedItems.Add(mappedItem.Index);
					weightSelected += mappedItem.Weight;
					itemLabels[selectedIndex] = GetItemLabel(true, mappedItem, sortByWeight);
				}

				defaultSelected = selectedIndex;
			}
		}
	}
};
