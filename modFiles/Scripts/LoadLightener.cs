using System;
using System.Collections.Generic;
using XRL.UI;
using Plaidman.LightenMyLoad.Menus;
using System.Linq;

// TODOs
// save most recent sort selection
//
// create skill
// show all ratio labels if you have the skill
// figure out how to check if PKAPP skill
// show all values if you have PKAPP skill
// always know if option is set
// remove auto-known torch in constructor
//
// adjust weight/ratio display to uniform width
//   - is this possible?

namespace XRL.World.Parts {
	[Serializable]
	public class LML_LoadLightener : IPlayerPart {
		public static readonly string ItemListCommand = "Plaidman_LightenMyLoad_Command_ShowItemList";
		public static readonly string UninstallCommand = "Plaidman_LightenMyLoad_Command_Uninstall";
		public static readonly string ShowValueOption = "Plaidman_LightenMyLoad_Option_AlwaysShowValue";
		public static readonly string AbilityOption = "Plaidman_LightenMyLoad_Option_UseAbility";
		public Guid AbilityGuid;
		public HashSet<string> KnownItems;
		public bool SortByWeight;
		
		public LML_LoadLightener() {
			KnownItems = new(50);
			KnownItems.Add("torch");
			SortByWeight = false;
		}

		public override void Register(GameObject go, IEventRegistrar registrar) {
			registrar.Register(CommandEvent.ID);
			registrar.Register(AfterPlayerBodyChangeEvent.ID);
			registrar.Register(StartTradeEvent.ID);
			base.Register(go, registrar);
		}

		public void ToggleAbility() {
			if (Options.GetOption(AbilityOption) == "Yes") {
				RequireAbility();
			} else {
				RemoveAbility();
			}
		}

		private void RequireAbility() {
			if (AbilityGuid == Guid.Empty) {
				AbilityGuid = ParentObject.AddActivatedAbility("Lighten Load", ItemListCommand, "Skill", Silent: true);
			}
		}
	
		private void RemoveAbility() {
			if (AbilityGuid != Guid.Empty) {
				ParentObject.RemoveActivatedAbility(ref AbilityGuid);
			}
		}

		public override bool HandleEvent(AfterPlayerBodyChangeEvent e) {
			ToggleAbility();
			return base.HandleEvent(e);
		}

		public override bool HandleEvent(StartTradeEvent e) {
			if (!e.Trader.IsCreature) {
				return base.HandleEvent(e);
			}
			
			foreach (var item in e.Actor.Inventory.GetObjects()) {
				KnownItems.Add(item.BaseDisplayName);
			}

			foreach (var item in e.Trader.Inventory.GetObjects()) {
				KnownItems.Add(item.BaseDisplayName);
			}

			return base.HandleEvent(e);
		}

		private void UninstallParts() {
			if (Popup.ShowYesNo("Are you sure you want to uninstall {{W|Lighten My Load}}?") == DialogResult.No) {
				Messages.MessageQueue.AddPlayerMessage("{{W|Lighten My Load}} uninstall was cancelled.");
				return;
			}

			if (AbilityGuid != Guid.Empty) {
				ParentObject.RemoveActivatedAbility(ref AbilityGuid);
				Messages.MessageQueue.AddPlayerMessage("{{W|Lighten My Load}}: removed ability");
			}

			ParentObject.RemovePart<LML_LoadLightener>();
			Messages.MessageQueue.AddPlayerMessage("{{W|Lighten My Load}}: removed player part");
			
			Popup.Show("Finished removing {{W|Lighten My Load}}. Please save and quit, then you can remove this mod.");
		}

		public override bool HandleEvent(CommandEvent e) {
			if (e.Command == ItemListCommand) {
				ListItems();
			}

			if (e.Command == UninstallCommand) {
				UninstallParts();
			}

			return base.HandleEvent(e);
		}
		
		private bool IsKnown(GameObject go) {
			return KnownItems.Contains(go.BaseDisplayName);
		}
		
		private void ListItems() {
			var objects = ParentObject.Inventory.GetObjects();
			var itemList = objects.Select((go, i) => {
				return new InventoryItem(i, go, IsKnown(go));
			}).ToArray();

			var selected = ItemList.ShowPopup(itemList, SortByWeight);
			if (selected == null || selected.Length == 0) {
				Messages.MessageQueue.AddPlayerMessage("no items selected");
				return;
			}
			
			if (selected.Length == 1) {
				var index = selected[0];
				InventoryActionEvent.Check(ParentObject, ParentObject, objects[index], "CommandDropObject");
			}

			foreach (var item in selected) {
				InventoryActionEvent.Check(ParentObject, ParentObject, objects[item], "CommandDropAllObject");
			}
		}
	}
}
