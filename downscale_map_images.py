import cv2
import os.path

path = '.\\resources\\images'
files = os.listdir(path)
dim128 = (128, 128)
dim256 = (256, 256)
for item in files:
    itempath = os.path.join(path,item)
    f, e = os.path.splitext(itempath)
    if os.path.isfile(itempath):
        im = cv2.imread(itempath)
        cv2.imwrite(f + "_128x128.jpg", cv2.resize(im, dim128, interpolation=cv2.INTER_AREA))
        cv2.imwrite(f + "_256x256.jpg", cv2.resize(im, dim256, interpolation=cv2.INTER_AREA))