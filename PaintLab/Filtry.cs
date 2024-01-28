using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace PaintLab
{
    internal class Filtry
    {
        public BitmapSource ApplyFilterGauss(BitmapSource source, double[,] filterMatrix)
        {
            FormatConvertedBitmap formattedBitmap = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            int stride = width * 4; // 4 bytes per pixel (BGRA)

            byte[] pixels = new byte[height * stride];
            formattedBitmap.CopyPixels(pixels, stride, 0);

            byte[] resultPixels = new byte[height * stride];

            int filterSize = filterMatrix.GetLength(0);
            int radius = filterSize / 2;

            for (int y = radius; y < height - radius; y++)
            {
                for (int x = radius; x < width - radius; x++)
                {
                    double red = 0, green = 0, blue = 0;

                    for (int i = 0; i < filterSize; i++)
                    {
                        for (int j = 0; j < filterSize; j++)
                        {
                            int offsetX = x + i - radius;
                            int offsetY = y + j - radius;

                            int index = (offsetY * stride) + (offsetX * 4);

                            red += pixels[index + 2] * filterMatrix[i, j];
                            green += pixels[index + 1] * filterMatrix[i, j];
                            blue += pixels[index] * filterMatrix[i, j];
                        }
                    }

                    int currentIndex = (y * stride) + (x * 4);
                    resultPixels[currentIndex] = (byte)blue;
                    resultPixels[currentIndex + 1] = (byte)green;
                    resultPixels[currentIndex + 2] = (byte)red;
                    resultPixels[currentIndex + 3] = 255; // Alpha channel
                }
            }

            BitmapSource result = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, resultPixels, stride);

            return result;
        }
        public BitmapSource ApplySobelFilter(BitmapSource source, double[,] sobelX, double[,] sobelY)
        {
            FormatConvertedBitmap formattedBitmap = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            int stride = width * 4; // 4 bytes per pixel (BGRA)

            byte[] pixels = new byte[height * stride];
            formattedBitmap.CopyPixels(pixels, stride, 0);

            byte[] resultPixels = new byte[height * stride];

            int filterSize = sobelX.GetLength(0);
            int radius = filterSize / 2;

            for (int y = radius; y < height - radius; y++)
            {
                for (int x = radius; x < width - radius; x++)
                {
                    double gradientXRed = 0, gradientYRed = 0;
                    double gradientXGreen = 0, gradientYGreen = 0;
                    double gradientXBlue = 0, gradientYBlue = 0;

                    for (int i = 0; i < filterSize; i++)
                    {
                        for (int j = 0; j < filterSize; j++)
                        {
                            int offsetX = x + i - radius;
                            int offsetY = y + j - radius;

                            int index = (offsetY * stride) + (offsetX * 4);

                            gradientXRed += pixels[index + 2] * sobelX[i, j];
                            gradientYRed += pixels[index + 2] * sobelY[i, j];

                            gradientXGreen += pixels[index + 1] * sobelX[i, j];
                            gradientYGreen += pixels[index + 1] * sobelY[i, j];

                            gradientXBlue += pixels[index] * sobelX[i, j];
                            gradientYBlue += pixels[index] * sobelY[i, j];
                        }
                    }

                    int currentIndex = (y * stride) + (x * 4);

                    double red = Math.Sqrt((gradientXRed * gradientXRed) + (gradientYRed * gradientYRed));
                    double green = Math.Sqrt((gradientXGreen * gradientXGreen) + (gradientYGreen * gradientYGreen));
                    double blue = Math.Sqrt((gradientXBlue * gradientXBlue) + (gradientYBlue * gradientYBlue));

                    resultPixels[currentIndex] = (byte)Math.Min(255, red);
                    resultPixels[currentIndex + 1] = (byte)Math.Min(255, green);
                    resultPixels[currentIndex + 2] = (byte)Math.Min(255, blue);
                    resultPixels[currentIndex + 3] = 255; // Alpha channel
                }
            }

            BitmapSource result = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, resultPixels, stride);

            return result;
        }
        public BitmapSource ApplySepiaFilter(BitmapSource source)
        {
            FormatConvertedBitmap formattedBitmap = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);

            int width = source.PixelWidth;
            int height = source.PixelHeight;

            int stride = width * 4; // 4 bytes per pixel (BGRA)

            byte[] pixels = new byte[height * stride];
            formattedBitmap.CopyPixels(pixels, stride, 0);

            byte[] resultPixels = new byte[height * stride];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int currentIndex = (y * stride) + (x * 4);

                    double red = pixels[currentIndex + 2] * 0.393 + pixels[currentIndex + 1] * 0.769 + pixels[currentIndex] * 0.189;
                    double green = pixels[currentIndex + 2] * 0.349 + pixels[currentIndex + 1] * 0.686 + pixels[currentIndex] * 0.168;
                    double blue = pixels[currentIndex + 2] * 0.272 + pixels[currentIndex + 1] * 0.534 + pixels[currentIndex] * 0.131;

                    resultPixels[currentIndex] = (byte)Math.Min(255, blue);
                    resultPixels[currentIndex + 1] = (byte)Math.Min(255, green);
                    resultPixels[currentIndex + 2] = (byte)Math.Min(255, red);
                    resultPixels[currentIndex + 3] = 255; // Alpha channel
                }
            }

            BitmapSource result = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, resultPixels, stride);

            return result;
        }
    }
    
}
