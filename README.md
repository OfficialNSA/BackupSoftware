# BackupSoftware

## Description

This software can create backup folders with only your incremental changes to save disk space on your backup drive.

## Usage

### Preparation

Create a folder where you copy all of your data to save to. This will be the original where the increments will be derived from.

In the sourcecode, change path `root1` to the contents you want to backup and path `root2` to your Original. It's important that these two are the root of your data as it will be compared relatively to these paths to find changes.

Set `destination` to the folder where the new increment should be saved to (currently with automatic date).

Add or remove elements the `ignorables` array if you don't want anything that has this string in it's path to be saved in the increment.
(Be wary: `ignorables` is used in a `String.contains` function, don't accidentally exclude things you would want to save)

### Action

Run the Software in your preferred C# IDE or as executable. In the log every single file that is being compared is listed. After all files have been compared, the ones that the software detected a change on or that are new will be copied to the new increment.

## Future

If you are interested in the expansion of this software hit me up then we can sort out how we can make it happen.

Things I have in mind right now but not enough time at hand:

- Take existing increments into account to create only the increment from the last increment and not all increments based on the original
- Only keep `n` increments, merge the oldest increment with the original
- Export paths/settings in json so that the source code doesn't have to be modified to change the paths