# UeSaveConverter

A command line program that converts Unreal Engine save game files to and from a human editable json files.

## Releases

Releases can be found [here](https://github.com/CrystalFerrai/UeSaveConverter/releases). There is no installer, just unzip the contents to a location on your hard drive.

You will need to have the .NET Runtime 6.0 x64 installed. You can find the latest .NET 6 downloads [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0). Look for ".NET Runtime" or ".NET Desktop Runtime" (which includes .NET Runtime). Download and install the x64 version for your OS.

## How to Use

Prerequisite: You should have some familiarity with using command line programs or you may struggle to run this. You will need to pass various command line arguments to the program to tell it what you want to do.

**BACKUP YOUR SAVE FILES BEFORE USING THIS PROGRAM.** If something goes wrong, there is no way to recover your save unless you have a backup.

### Step 1: Convert a binary save file (.sav) into a json file

This will output a json version of the save file next to the original save file.
```
UeSaveConverter --to-json --overwrite path\to\savefile.sav
```

### Step 2: Make changes to the json file

Using a text editor, make any changes you want to the json file that was reated. Usually, you only care about the `Properties` section of the file and should not touch the `Header` or `CustomVersions` sections. Be careful that you do not break the json formatting of the file.

### Step 3: Convert the json file back into a binary save file

This will replace the original save file with the modified one. Make sure you backed up the original first!
```
UeSaveConverter --to-sav --overwrite path\to\savefile.sav.json
```

### More options

The program offers more command line options for more use cases. For example, you can change where the output file goes or can convert entire folders full of save files at once. To see the full list of options, run the program in a command window with no parameters. Here what currently prints at the time of writing this:
```
Usage: UeSaveConverter [[options]] [input path] [output path]

  input path   A file or directory containing files to convert to or from json.

  output path  A file or directory that will receive the converted file(s). Optional if
               input is a file. If not specified, output will be placed next to input.

Options

  --to-json                 Convert sav to json. Implied if input is a .sav file.

  --to-sav                  Convert json to sav. Implied if input is a .json file.

  --include-subdirectories  If input is a directory, also process subdirectories.

  --file-filter [filter]    If input is a directory, use this filter to match files.
                            Default is *.sav for to-json and *.sav.json for to-sav.

  --overwrite               Allow overwriting existing output file(s).
```

## How to Build

If you want to build, from source, follow these steps.
1. Clone the repo, including submodules.
    ```
    git clone --recursive https://github.com/CrystalFerrai/UeSaveConverter.git
    ```
2. Open the file `UeSaveConverter.sln` in Visual Studio.
3. Right click the solution in the Solution Explorer panel and select "Restore NuGet Dependencies".
4. Build the solution.

## Disclaimer

For various reasons, this program will not work for all games. Make sure you have backups of your save files before replacing them using this tool.

## Support

This is just one of my many free time projects. No support or documentation is offered beyond this readme. If you find a bug in the program, you can [submit as issue on Github](https://github.com/CrystalFerrai/UeSaveConverter/issues), but I make no promises about when or if I will address issues.
