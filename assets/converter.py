from PIL import Image
import numpy as np
import copy
from shutil import copyfile
from dataclasses import dataclass

@dataclass
class DefaultColors:
    black: np.array         = np.array([  0,   0,   0, 255])
    white: np.array         = np.array([255, 255, 255, 255])
    bgr: np.array           = np.array([120, 135, 135, 255])
    inside: np.array        = np.array([254,   0,   0, 255])
    shadow: np.array        = np.array([  0,   0, 254, 255])
    vizor_inside: np.array  = np.array([  0, 255,   0, 255])
    vizor_shadow: np.array  = np.array([  0, 126,   0, 255])

@dataclass
class PlayerColors:
    red_inside: np.array    = np.array([197,  17,  17, 255])
    red_shadow: np.array    = np.array([122,   8,  56, 255])
    blue_inside: np.array   = np.array([ 19,  46, 209, 255])
    blue_shadow: np.array   = np.array([  9,  21, 142, 255])
    green_inside: np.array  = np.array([ 17, 127,  45, 255])
    green_shadow: np.array  = np.array([ 10,  77,  46, 255])
    pink_inside: np.array   = np.array([237,  84, 186, 255])
    pink_shadow: np.array   = np.array([171,  43, 173, 255])
    orange_inside: np.array = np.array([239, 125,  14, 255])
    orange_shadow: np.array = np.array([179,  63,  21, 255])
    yellow_inside: np.array = np.array([246, 246,  88, 255])
    yellow_shadow: np.array = np.array([195, 136,  35, 255])
    black_inside: np.array  = np.array([ 63,  71,  78, 255])
    black_shadow: np.array  = np.array([ 30,  31,  38, 255])
    white_inside: np.array  = np.array([214, 224, 240, 255])
    white_shadow: np.array  = np.array([131, 148, 191, 255])
    purple_inside: np.array = np.array([107,  49, 188, 255])
    purple_shadow: np.array = np.array([ 60,  23, 124, 255])
    brown_inside: np.array  = np.array([113,  73,  30, 255])
    brown_shadow: np.array  = np.array([ 94,  38,  21, 255])
    cyan_inside: np.array   = np.array([ 56, 254, 219, 255])
    cyan_shadow: np.array   = np.array([ 36, 168, 190, 255])
    lime_inside: np.array   = np.array([ 80, 239,  57, 255])
    lime_shadow: np.array   = np.array([ 21, 167,  66, 255])

    vizor_inside: np.array  = np.array([149, 201, 218, 255])
    vizor_shadow: np.array  = np.array([ 74, 100, 107, 255])

def get_concat_h_multi_resize(im_list, resample=Image.BICUBIC):
    min_height = min(im.height for im in im_list)
    im_list_resize = [im.resize((int(im.width * min_height / im.height), min_height),resample=resample)
                      for im in im_list]
    total_width = sum(im.width for im in im_list_resize)
    dst = Image.new('RGBA', (total_width, min_height))
    pos_x = 0
    for im in im_list_resize:
        dst.paste(im, (pos_x, 0))
        pos_x += im.width
    return dst

def get_concat_v_multi_resize(im_list, resample=Image.BICUBIC):
    min_width = min(im.width for im in im_list)
    im_list_resize = [im.resize((min_width, int(im.height * min_width / im.width)),resample=resample)
                      for im in im_list]
    total_height = sum(im.height for im in im_list_resize)
    dst = Image.new('RGBA', (min_width, total_height))
    pos_y = 0
    for im in im_list_resize:
        dst.paste(im, (0, pos_y))
        pos_y += im.height
    return dst

def get_concat_tile_resize(im_list_2d, resample=Image.BICUBIC):
    im_list_v = [get_concat_h_multi_resize(im_list_h, resample=resample) for im_list_h in im_list_2d]
    return get_concat_v_multi_resize(im_list_v, resample=resample)

def ProcessArray(insideToReplace, shadowToReplace, array):
    imArray = copy.deepcopy(array)
    for row in range(len(imArray)):
        for col in range(len(imArray[row])):
            # Check if close transperant
            if imArray[row,col][3] < 10:
                continue
            # Check if close to white
            elif np.sum(np.isclose(imArray[row,col][:3], DefaultColors.white[:3], atol=10)) > 2:
                continue
            # Check if close to black
            elif np.sum(np.isclose(imArray[row,col][:3], DefaultColors.black[:3], atol=30)) > 2:
                imArray[row,col] = DefaultColors.black
            elif np.sum(np.isclose(imArray[row,col][:3], DefaultColors.bgr[:3], atol=30)) > 2:
                continue
            # Replace shadow
            elif np.sum(np.isclose(imArray[row,col], DefaultColors.shadow, atol=10)) > 2:
                imArray[row,col] = shadowToReplace
            # Replace inside
            elif np.sum(np.isclose(imArray[row,col], DefaultColors.inside, atol=10)) > 2:
                imArray[row,col] = insideToReplace
            
            elif imArray[row,col][1] < 35:
                if imArray[row,col][0] > 100:
                    imArray[row,col] = insideToReplace
                elif imArray[row,col][2] > 100:
                    imArray[row,col] = shadowToReplace
            else:
                if np.sum(np.isclose(imArray[row,col][:3], DefaultColors.vizor_inside[:3], atol=10)) > 2:
                    imArray[row,col] = PlayerColors.vizor_inside
                elif np.sum(np.isclose(imArray[row,col][:3], DefaultColors.vizor_shadow[:3], atol=10)) > 2 or imArray[row,col][1] < 126:
                    imArray[row,col] = PlayerColors.vizor_shadow
                else:
                    imArray[row,col] = PlayerColors.vizor_inside
    
    return imArray

if __name__ == '__main__':
    imageFile = 'morphButton.png'
    inputImage = Image.open(imageFile)
    inputImage = inputImage.transpose(Image.FLIP_TOP_BOTTOM)
    inputArray = np.array(inputImage)

    print(f'Processing image {imageFile}')
    width, height = inputImage.size
    print(f'Input image width: {width}')
    print(f'Input image height: {height}')
    scaleDown = 8
    print(f'Input image scaled down by {scaleDown}\n')

    playerColors = np.array(list(PlayerColors.__dataclass_fields__.keys())[:-2]).reshape(-1,2)
    images = []
    progress = 0
    # TODO: Currently very slow.
    for color in playerColors:
        print(f'Progress: {progress+1} image out of {len(playerColors)}')
        progress += 1
        processedArray = ProcessArray(getattr(PlayerColors, color[0]), getattr(PlayerColors, color[1]), inputArray)
        processedImage = Image.fromarray(processedArray, 'RGBA')
        processedImage = processedImage.resize(
                (processedImage.width // scaleDown, processedImage.height // scaleDown),
                resample=Image.BICUBIC)
        images.append(processedImage)

    numOfSprites = len(images)
    images = np.reshape(np.array(images), (-1, int(numOfSprites)))
    rows, cols = images.shape

    outputImage = get_concat_tile_resize(images)
    #outputImage.save("morphButtonArray.png")
   
    # Convert output image to array
    outputArray = np.array(outputImage)

    width, height = outputImage.size
    print(f'Output image width: {width}')
    print(f'Output image height: {height}')
    print(f'Output image pixel count: {width*height}')
    print(f'Output image byte count: {width*height*4}\n')

    # Format hex data
    hexData = ["0x{:02X}".format(x) for x in outputArray.astype('uint8').tobytes()]
    hexData = [hexData[i:i + 12] for i in range(0, len(hexData), 12)]
    hexData = [", ".join(l)+",\n"+ " "*12 for l in hexData]
    hexData = "".join(hexData).rstrip()

    csCode = (
        f"// Auto generated file\n\n\n" \
        f"namespace Metamorphosis\n" \
        f"{{\n" \
        f"    public static class MorphButtonImage\n" \
        f"    {{\n" \
        f"        public static readonly int Width = {width};\n" \
        f"        public static readonly int Cols = {cols};\n" \
        f"        public static readonly int Height = {height};\n" \
        f"        public static readonly int Rows = {rows};\n" \
        f"        public static readonly int NumberOfSprites = {numOfSprites};\n" \
        f"        public static readonly byte[] Data = \n" \
        f"        {{\n" \
        f"            {hexData}\n" \
        f"        }};\n" \
        f"    }}\n" \
        f"}}\n" \
    )
   
    csFile = '../Metamorphosis/MorphButtonImage.cs'
    print(f'Writing c# array to {csFile}')
    csFile_ = open(csFile, 'w', encoding='utf-8')
    csFile_.write(csCode)
