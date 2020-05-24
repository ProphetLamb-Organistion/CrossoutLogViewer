from PIL import Image
import os.path, sys

path = ".\\screenshots"
dirs = os.listdir(path)

def crop():
    for item in dirs:
        fullpath = os.path.join(path,item)
        if os.path.isfile(fullpath):
            im = Image.open(fullpath)
            f, e = os.path.splitext(fullpath)
            imCrop = im.crop((997, 844, 1989, 1836))
            imCrop.save(f + '_cropped.jpg', "JPEG", quality=100)

crop()