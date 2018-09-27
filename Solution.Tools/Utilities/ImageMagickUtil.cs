using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using log4net;
using ImageMagick;


namespace Solution.Tools.Utilities
{
    public static class ImageMagickUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        #region Public methods

        public static byte[] ScaleImage(byte[] imgData, int desiredWidth, int desiredHeight)
        {
            using (MemoryStream ms = new MemoryStream(imgData))
            using (Bitmap bmp = new Bitmap(ms))
            {
                string mimeType = GetImageMimeType(bmp);
                ImageCodecInfo imageCodecInfo = GetImageCodecInfo(mimeType);

                switch (mimeType)
                {
                    case "image/tiff":
                        return ScaleTiff(bmp, imageCodecInfo, desiredWidth, desiredHeight);
                    case "image/jpeg":
                    case "image/png":
                    //case "image/gif":
                    case "image/bmp":
                        return ScaleSingleFrameBitmap(bmp, imageCodecInfo, desiredWidth, desiredHeight);
                    default:
                        throw new ArgumentException($"Image format {mimeType} not supported.");
                }
            }
        }

        public static void ScaleImage(string imageFile, string outputFile, int desiredWidth, int desiredHeight)
        {
            File.WriteAllBytes(outputFile, ScaleImage(File.ReadAllBytes(imageFile), desiredWidth, desiredHeight));
        }

        public static byte[] ConvertImageToGrayScale(byte[] imgData)
        {
            using (MemoryStream ms = new MemoryStream(imgData))
            using (Bitmap bmp = new Bitmap(ms))
            {
                string mimeType = GetImageMimeType(bmp);
                ImageCodecInfo imageCodecInfo = GetImageCodecInfo(mimeType);

                switch (mimeType)
                {
                    case "image/tiff":
                        return ConvertTiffToGrayScale(bmp, imageCodecInfo);
                    case "image/jpeg":
                    case "image/png":
                    case "image/gif":
                    case "image/bmp":
                        return ConvertSingleFrameBitmapToGrayScale(bmp, imageCodecInfo);
                    default:
                        throw new ArgumentException($"Image format {mimeType} not supported.");
                }
            }
        }

        public static void ConvertImageToGrayScale(string imageFile, string outputFile)
        {
            File.WriteAllBytes(outputFile, ConvertImageToGrayScale(File.ReadAllBytes(imageFile)));
        }

        #endregion


        #region Private manipulations

        private static byte[] ScaleTiff(Bitmap bmp, ImageCodecInfo imageCodecInfo, int desiredWidth, int desiredHeight)
        {
            if (bmp.GetFrameCount(FrameDimension.Page) == 1)
            {
                return ScaleSingleFrameBitmap(bmp, imageCodecInfo, desiredWidth, desiredHeight);
            }
            else
            {
                string tempTif = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                try
                {
                    using (Bitmap scaled = ByteArrayToImage(ScaleSingleFrameBitmap(bmp, imageCodecInfo, desiredWidth, desiredHeight)))
                    using (EncoderParameters encoderParams = new EncoderParameters(3))
                    using (EncoderParameter compressionEncoderParam = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW))
                    {
                        using (EncoderParameter colorDepthParam = new EncoderParameter(Encoder.ColorDepth, Image.GetPixelFormatSize(bmp.PixelFormat)))
                        using (EncoderParameter saveEncoderParam = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame))
                        {
                            encoderParams.Param[0] = compressionEncoderParam;
                            encoderParams.Param[1] = saveEncoderParam;
                            encoderParams.Param[2] = colorDepthParam;
                            scaled.Save(tempTif, imageCodecInfo, encoderParams);
                        }
                        using (EncoderParameter saveEncoderParam = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage))
                        {
                            encoderParams.Param[1] = saveEncoderParam;
                            //parto da 1 perchè il Frame di indice 0 si è inserito con il primo save in modalità encoder MultiFrame
                            for (int i = 1; i < bmp.GetFrameCount(FrameDimension.Page); i++)
                            {
                                bmp.SelectActiveFrame(FrameDimension.Page, i);
                                using (EncoderParameter colorDepthParam = new EncoderParameter(Encoder.ColorDepth, Image.GetPixelFormatSize(bmp.PixelFormat)))
                                {
                                    encoderParams.Param[0] = compressionEncoderParam;
                                    encoderParams.Param[2] = colorDepthParam;
                                    using (Bitmap scaledFrame = ByteArrayToImage(ScaleSingleFrameBitmap(bmp, imageCodecInfo, desiredWidth, desiredHeight)))
                                    {
                                        scaled.SaveAdd(scaledFrame, encoderParams);
                                    }
                                }
                            }
                        }
                        using (EncoderParameter saveEncoderParam = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush))
                        {
                            encoderParams.Param[1] = saveEncoderParam;
                            scaled.SaveAdd(encoderParams);
                        }
                    }
                    return File.ReadAllBytes(tempTif);
                }
                finally
                {
                    File.Delete(tempTif);
                }
            }
        }

        private static byte[] ScaleSingleFrameBitmap(Bitmap bmp, ImageCodecInfo imageCodecInfo, int desiredWidth, int desiredHeight)
        {
            if (bmp.Width >= bmp.Height)
            {
                if (desiredWidth < desiredHeight)
                {
                    int tmp;
                    tmp = desiredWidth;
                    desiredWidth = desiredHeight;
                    desiredHeight = tmp;
                }
            }
            else
            {
                if (desiredWidth > desiredHeight)
                {
                    int tmp;
                    tmp = desiredHeight;
                    desiredHeight = desiredWidth;
                    desiredWidth = tmp;
                }
            }
            if (bmp.Width <= desiredWidth && bmp.Height <= desiredHeight)
            {
                return ImageToByteArray(bmp);
            }

            MagickImage objMagick = new MagickImage();
            objMagick.Read(bmp);
            objMagick.Quality = 100;
            objMagick.Resize(new MagickGeometry(new System.Drawing.Rectangle(0, 0, desiredWidth, desiredHeight)));
            using (MemoryStream ms = new MemoryStream())
            {
                objMagick.Write(ms);
                return ms.ToArray();
            }
        }

        private static byte[] ConvertTiffToGrayScale(Bitmap bmp, ImageCodecInfo imageCodecInfo)
        {
            if (bmp.GetFrameCount(FrameDimension.Page) == 1)
            {
                return ConvertSingleFrameBitmapToGrayScale(bmp, imageCodecInfo);
            }
            else
            {
                string tempTif = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                try
                {
                    using (Bitmap bmpGrayScaled = ByteArrayToImage(ConvertSingleFrameBitmapToGrayScale(bmp, imageCodecInfo)))
                    using (EncoderParameters encoderParams = new EncoderParameters(3))
                    using (EncoderParameter compressionEncoderParam = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW))
                    {
                        using (EncoderParameter colorDepthParam = new EncoderParameter(Encoder.ColorDepth, Image.GetPixelFormatSize(bmp.PixelFormat)))
                        using (EncoderParameter saveEncoderParam = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame))
                        {
                            encoderParams.Param[0] = compressionEncoderParam;
                            encoderParams.Param[1] = saveEncoderParam;
                            encoderParams.Param[2] = colorDepthParam;
                            bmpGrayScaled.Save(tempTif, imageCodecInfo, encoderParams);
                        }
                        using (EncoderParameter saveEncoderParam = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage))
                        {
                            encoderParams.Param[1] = saveEncoderParam;
                            //parto da 1 perchè il Frame di indice 0 si è inserito con il primo save in modalità encoder MultiFrame
                            for (int i = 1; i < bmp.GetFrameCount(FrameDimension.Page); i++)
                            {
                                bmp.SelectActiveFrame(FrameDimension.Page, i);
                                using (EncoderParameter colorDepthParam = new EncoderParameter(Encoder.ColorDepth, Image.GetPixelFormatSize(bmp.PixelFormat)))
                                {
                                    encoderParams.Param[0] = compressionEncoderParam;
                                    encoderParams.Param[2] = colorDepthParam;
                                    using (Bitmap scaledFrame = ByteArrayToImage(ConvertSingleFrameBitmapToGrayScale(bmp, imageCodecInfo)))
                                    {
                                        bmpGrayScaled.SaveAdd(scaledFrame, encoderParams);
                                    }
                                }
                            }
                        }
                        using (EncoderParameter saveEncoderParam = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush))
                        {
                            encoderParams.Param[1] = saveEncoderParam;
                            bmpGrayScaled.SaveAdd(encoderParams);
                        }
                        return File.ReadAllBytes(tempTif);
                    }
                }
                finally
                {
                    File.Delete(tempTif);
                }
            }
        }

        private static byte[] ConvertSingleFrameBitmapToGrayScale(Bitmap bmp, ImageCodecInfo imageCodecInfo)
        {
            MagickImage objMagick = new MagickImage();
            objMagick.Read(bmp);
            objMagick.Quality = 100;
            objMagick.Grayscale(PixelIntensityMethod.Undefined);
            using (MemoryStream ms = new MemoryStream())
            {
                objMagick.Write(ms);
                return ms.ToArray();
            }
        }

        #endregion


        #region  Support

        public static ImageCodecInfo GetImageCodecInfo(string mimeType)
        {
            ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders()
                .First(c => c.MimeType.Equals(mimeType, System.StringComparison.InvariantCultureIgnoreCase));
            return codec;
        }

        public static ImageCodecInfo GetImageCodecInfo(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public static string GetImageMimeType(Image image)
        {
            ImageFormat format = image.RawFormat;
            ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders()
                .First(c => c.FormatID == format.Guid);
            return codec.MimeType;
        }

        public static Bitmap ByteArrayToImage(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            return new Bitmap(ms);
        }

        public static byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                return ms.ToArray();
            }
        }

        #endregion

    }
}
