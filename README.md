# G2WwiseDataTool
G2WwiseDataTool is a CLI application which can be used to export SoundBanks and Events from a WWise project into files compatible with Glacier 2.

## Wwise setup
1. Download the example Wwise project from: https://github.com/glacier-modding/G2WwiseProject.
2. Download and install Wwise 2019.2.15.7667 from https://www.audiokinetic.com/en/download/ (You will need a Wwise account).
3. Setup your Audio Objects, Events and SoundBank (make sure to untick the media field for each event that you reference in the SoundBank).
4. Generate SoundBanks.

## Usage
```
G2WwiseDataTool 1.2.0
Copyright (C) Glacier 2 Modding Organisation

  -i, --input                      Path to SoundBanksInfoPath.xml file (Located in
                                   Wwise_Project_Root\GeneratedSoundBanks\Windows\).

  -o, --output                     Path to output files (Defaults to current working directory).

  -r, --rpkg                       Path to rpkg-cli for automatic .meta.json to .meta conversion.

  -f, --output-folder-structure    Output to a folder structure instead of hashes.

  -v, --verbose                    Set output to verbose messages.

  -l, --licenses                   Prints license information for G2WwiseDataTool and third party libraries that are
                                   used.

  --help                           Display this help screen.

  --version                        Display version information.
```