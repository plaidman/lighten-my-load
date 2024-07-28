using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using Qud.UI;
using ConsoleLib.Console;
using XRL.World;
using XRL.World.Parts;
using XRL;

namespace Plaidman.LightenMyLoad.Menus {
	public class ItemList {
		public class InventoryItem {
			public int Index { get; }
			public string DisplayName { get; }
			public IRenderable Icon { get; }
			public int? Weight { get; }
			public double? Value { get; }

			public InventoryItem(int index, GameObject go) {
				Index = index;
				DisplayName = go.DisplayName;
				Icon = go.Render;
				Weight = go.GetPart<Physics>()?.Weight ?? null;
				Value = GetValue(go);
			}
		}
		
		private static double? GetValue(GameObject go) {
			if (go.ContainsFreshWater()) {
				return null;
			}

			var value = go.Value;
			var multiple = 1.0;
			
			if (!go.IsCurrency) {
				// subtract 0.21 (3 * 0.07) because the player's reputation with themself is uncommonly high
				multiple = GetTradePerformanceEvent.GetFor(The.Player, The.Player) - 0.21;
			}

			return value * multiple;
		}
		
		private static string GetItemLabel(bool selected, InventoryItem item) {
			var label = GetSelectionLabel(selected) + " ";
			// label += GetWeightLabel(item) + " ";
			label += GetValueLabel(item) + " ";
			label += item.DisplayName;
			
			return label;
		}
		
		private static string GetValueLabel(InventoryItem item) {
			return item.Value.ToString();
		}
		
		private static string GetWeightLabel(InventoryItem item) {
			var weight = item.Weight;
			
			if (weight == null) {
				return "{{W||??#|}}";
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
			
			var sortedOptions = options.OrderByDescending((item) => { return item.Weight ?? 0; }).ToArray();
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
					weightSelected -= mappedItem.Weight ?? 0;
					itemLabels[selectedIndex] = GetItemLabel(false, mappedItem);
				} else {
					selectedItems.Add(mappedItem.Index);
					weightSelected += mappedItem.Weight ?? 0;
					itemLabels[selectedIndex] = GetItemLabel(true, mappedItem);
				}

				defaultSelected = selectedIndex;
			}
		}
	}
};
