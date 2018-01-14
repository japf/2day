import fontforge
import os

# create empty font
font = fontforge.font()

directory = "export"

# set font names
font.fontname = "2Day"
font.fullname = "2Day"
font.familyname = "2Day Font"

# import svgs
i = 0
offset = 0
import os
for file in os.listdir(directory):
    if file.endswith(".svg") == False:
        continue
      
    if "folder" in file and offset == 0:
        offset = 64 * 1
        i = 0      
    if "group" in file and offset == 64 * 1:
        offset = 64 * 2
        i = 0
    if "logo" in file and offset == 64 * 2:
        offset = 64 * 3
        i = 0
    if "priority" in file and offset == 64 * 3:
        offset = 64 * 4
        i = 0
    if "setting" in file and offset == 64 * 4:
        offset = 64 * 5
        i = 0
    if "view" in file and offset == 64 * 5:
        offset = 64 * 6
        i = 0
    
    index = i + offset;
    print(directory + "\\" +  file + "  offset: " + str(index))
    
    # create a new glyph with the code point i
    glyph = font.createChar(index + 0xE000)

    # import svg file into it
    glyph.importOutlines(directory + "\\" + file)

    # make the glyph rest on the baseline
    ymin = glyph.boundingBox()[1]
    glyph.transform([1, 0, 0, 1, 0, -ymin])

    # set glyph side bearings, can be any value or even 0
    glyph.left_side_bearing = glyph.right_side_bearing = 10
    
    i = i + 1

font.generate(directory + "\\..\\..\\src\\2Day.App\\Fonts\\" + font.fontname + ".ttf") # truetype