# TextSpriteExport

## A WPF application that will export monospaced fonts as a .png image

### Guide

1. Copy your monospaced fonts into the program directory's Font folder
2. Run the program
3. Select a font from the drop down, if it detects a font is not monospaced there will be a warning
4. Modify the parameters as desired
    * Text Format Dropdown: Display/Ideal will set the text format, Display is recommended as it is more consistent
    * Show Grid checkbox: will show/hide a grid overlay for the sprites
    * Snap to Device Pixels checkbox: will enable "Snap to Device Pixels" setting on all children of the export preview (Default is on, but can't see a difference for myself)
    * Use Ascii checkbox: will allow up to 256 ascii encoded chars versus the standard 128 character UTF8 encoding (Affects char minimum and maximum limits, not supported by all fonts)
    * Columns field: will be how many columns to split the characters into (0 is a single line)
    * Char Min field: is where to begin looking for characters (0-32 is typically whitespace, default to 33)
    * Char Max field: is where to stop looking for characters (default is max count, typically whitespace though)
    * Show Background checkbox: will enable or disable the colored background for the exported image
    * Select Background Color button and corresponding Alpha field: will allow to select the color and alpha values accordingly
    * Character Width and Height labels: will show what the current value is for each character. If using non-monospaced fonts this will be inaccurate. Rounded to two decimal places. The height will always use the highest value height of all the characters.
    
### FYI
* Export Preview spacing may not appear correct in the box, but should be exported correctly. 
* If you have any issues or find any bugs, please let me know
* If you have any ideas for improvements, feel free to suggest them
