# TextSpriteExport

## A WPF application that will export monospaced fonts as a .png image sprite sheet

### Guide

1. Copy your monospaced fonts into the program directory's Font folder
2. Run the program
3. Select a font from the drop down, if it detects a font is not monospaced there will be a warning
      * It's likely there will be an issue with the character spacing, the exported image will need manually edited for consistency. You can turn the grid on as a reference
4. Modify the parameters as desired
    * __Text Format Dropdown:__ Display/Ideal will set the text format, Display is recommended as it is more consistent
    * __Text Rendering Dropdown:__ Set the text rendering according to preference. ClearType/Grayscale anti-aliasing or aliased. Anti-aliasing may cause issues with colored backgrounds
    * __Text Hinting Dropdown:__ Set the text hinting according to preference. Fixed is recommended as it appears to be more clear
    * __Show Grid checkbox:__ will show/hide a grid overlay for the sprites
    * __Snap to Device Pixels checkbox:__ will enable "Snap to Device Pixels" setting on all children of the export preview (Default is on, but can't see a difference for myself)
    * __Use ISO-8859-1 checkbox:__ will allow up to 256 ISO-8859-1 encoded characters versus the standard 128 character UTF8 encoding (Affects char minimum and maximum limits, not supported by all fonts)
    * __Columns field:__ will be how many columns to split the characters into (0 is a single line)
    * __Char Min field:__ is where to begin looking for characters (0-32 is typically whitespace, default to 33)
    * __Char Max field:__ is where to stop looking for characters (default is max count, typically whitespace though)
    * __Show Background checkbox:__ will enable or disable the colored background for the exported image
    * __Select Background Color button and corresponding Alpha field:__ will allow to select the color and alpha values accordingly
    * __Character Width and Height labels:__ will show what the current value is for each character. If using non-monospaced fonts this will be inaccurate. Rounded to two decimal places. The height will always use the highest value height of all the characters.
    
### FYI
* Export Preview lead/end spacing may look truncated, but should be exported correctly. 
* If you have any issues or find any bugs, please let me know
* If you have any ideas for improvements, feel free to suggest them
