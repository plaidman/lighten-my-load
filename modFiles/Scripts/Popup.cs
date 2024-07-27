using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using Qud.UI;
using ConsoleLib.Console;
using XRL.World;
using XRL.World.Parts;

namespace Plaidman.LightenMyLoad.Menus {
	public class ItemList {
		public class InventoryItem {
			public int Index { get; }
			public string DisplayName { get; }
			public int? Weight { get; }
			public IRenderable Icon { get; }

			public InventoryItem(int index, GameObject go) {
				Index = index;
				DisplayName = go.DisplayName;
				Icon = go.Render;
				Weight = go.GetPart<Physics>()?.Weight ?? null;
			}
		}
		
		private static string GetItemLabel(bool selected, InventoryItem item) {
			var label = GetSelectionLabel(selected) + " ";
			label += GetWeightLabel(item) + " ";
			label += item.DisplayName;
			
			return label;
		}
		
		private static string GetWeightLabel(InventoryItem item) {
			var weight = item.Weight;
			
			if (weight == null) {
				return "{{W||???#|}}";
			}
			
			if (weight > 999) {
				return "{{W||999+|}}";
			}

			if (weight < -99) {
				return "{{W||-99+|}}";
			}

			return "{{W||" + weight + "#|}}";
		}
		
		private static string GetSelectionLabel(bool selected) {
			if (selected) {
				return "{{W|[þ]}}";
			}

			return "{{y|[ ]}}";
		}

		public static int[] ShowPopup(InventoryItem[] options) {
			var defaultSelected = 0;
			var weightSelected = 0;
			var selectedItems = new HashSet<int>();
			string[] itemLabels = new string[options.Length];
			IRenderable[] itemIcons = options.Select((item) => { return item.Icon; }).ToArray();

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
					itemLabels[i] = GetItemLabel(selectedItems.Contains(i), options[i]);
				}
				
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

				if (selectedItems.Contains(selectedIndex)) {
					selectedItems.Remove(selectedIndex);
					weightSelected -= options[selectedIndex].Weight ?? 0;
				} else {
					selectedItems.Add(selectedIndex);
					weightSelected += options[selectedIndex].Weight ?? 0;
				}

				defaultSelected = selectedIndex;
			}
		}
	}
};
