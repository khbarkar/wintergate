.DEFAULT_GOAL := help

PROTOTYPE := $(CURDIR)/advent-game-concept/dist/index.html
UNITY_PROJECT := $(CURDIR)/wintergate-unity-prototype
UNITY_APP ?= $(firstword $(wildcard /Applications/Unity/Hub/Editor/6000.3.25f1/Unity.app /Applications/Unity/Hub/Editor/6000.*/Unity.app))

.PHONY: help prototype unity

help: ## Show the available commands
	@echo "Wintergate prototype"
	@echo ""
	@echo "Usage:"
	@echo "  make prototype   Open the interactive mock site"
	@echo "  make unity       Open the Quest calendar prototype in Unity"
	@echo "  make help        Show this help"

prototype: ## Open the interactive mock site in the default browser
	@test -f "$(PROTOTYPE)" || { echo "Prototype not found: $(PROTOTYPE)"; exit 1; }
	@open "$(PROTOTYPE)"

unity: ## Open the Quest calendar prototype in Unity
	@test -n "$(UNITY_APP)" && test -d "$(UNITY_APP)" || { echo "Unity Editor was not found."; exit 1; }
	@test -f "$(UNITY_PROJECT)/ProjectSettings/ProjectVersion.txt" || { echo "Unity project not found: $(UNITY_PROJECT)"; exit 1; }
	@"$(UNITY_APP)/Contents/MacOS/Unity" -projectPath "$(UNITY_PROJECT)"

