from PIL import Image
import os.path, sys

path = ".\\screenshots"
files = os.listdir(path)

for item in files:
    fullpath = os.path.join(path,item)
    if os.path.isfile(fullpath):
        im = Image.open(fullpath)
        f, e = os.path.splitext(fullpath)
        imCrop = im.crop((997, 844, 1989, 1836))
        imCrop.save(f + '_cropped.jpg', "JPEG", quality=100)