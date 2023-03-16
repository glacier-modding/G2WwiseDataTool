# G2WwiseDataTool
G2WwiseDataTool is a CLI application which can be used to export Audio Objects, Events, Switches and SoundBanks from a WWise project into files which are compatible with the Glacier 2 engine.

## Supported file types
This table shows which Wwise objects are supported and which Glacier 2 engine file types that they get exported to:
| Wwise         |  | Glacier 2 |
|---------------|--|-----------|
| Audio Objects |->| WWEM      |
| Events        |->| WWEV      |
| Switches      |->| WSWT/WSWB |
| SoundBanks    |->| WBNK      |

## Wwise setup
1. Download the example Wwise project from: https://github.com/glacier-modding/G2WwiseProject.
2. Download and install Wwise 2019.2.15.7667 from https://www.audiokinetic.com/en/download/ (You will need a Wwise account).
3. Setup your Audio Objects, Events, Switches and SoundBanks (make sure to untick the media field for each event that you reference in the SoundBank).
4. Generate SoundBanks.

## In-game setup
You will need to create a SoundBank entity which references your new SoundBank in the GlobalData brick (`[assembly:/_pro/scenes/bricks/globaldata.brick].pc_entitytype`) using QNE (QuickEntity Editor) and creating a entity.patch.json file for your SMF mod.

Here is an example of a SoundBank entity:
```json
{
	"parent": "abcd889eea2e16b2",
	"name": "Example_SoundBank",
	"factory": "[modules:/zsoundbankentity.class].pc_entitytype",
	"blueprint": "[modules:/zsoundbankentity.class].pc_entityblueprint",
	"properties": {
		"m_pBankResource": {
			"type": "ZRuntimeResourceID",
			"value": "[assembly:/sound/wwise/exportedwwisedata/soundbanks/globaldata/example_soundbank.wwisesoundbank].pc_wwisebank"
		}
	}
}
```

## Usage
```
G2WwiseDataTool 1.3.0
Copyright (C) Glacier 2 Modding Organisation

  -i, --input                      Path to SoundBanksInfoPath.xml file (Located in
                                   Wwise_Project_Root\GeneratedSoundBanks\Windows\).

  -o, --output                     Path to output files (Defaults to current working directory).

  -f, --output-folder-structure    Output to a folder structure instead of hashes.

  -s, --save-paths                 Save Event and SoundBank paths to a events.txt and soundbanks.txt file in the output
                                   path.

  -v, --verbose                    Set output to verbose messages.

  -l, --licenses                   Prints license information for G2WwiseDataTool and third party libraries that are
                                   used.

  --help                           Display this help screen.

  --version                        Display version information.
```

### Example Usage
G2WwiseDataTool.exe -i "InputPath\Windows\SoundbanksInfo.xml" -o "OutputPath" -s

**For automation with Wwise please see the automation section in the G2WwiseProject repository.**

## Credits
- [2kpr](https://github.com/2kpr) - For helping by adding support for events which contain multiple audio objects and events which contain multiple mixed stream type audio objects.