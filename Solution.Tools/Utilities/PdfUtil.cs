using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

using log4net;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;


namespace Solution.Tools.Utilities
{
    /// <summary>
    /// Utility to manage pdf using ItextSharp
    /// REQUIRE: iTextSharp
    /// </summary>
    public static class PdfUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Extracts all images (of types that iTextSharp knows how to decode) from a PDF file.
        /// </summary>
        /// <param name="fileName">Full path to pdf file</param>
        /// <returns></returns>
        public static List<ImageInfo> ExtractImagesInfo(string fileName)
        {
            List<ImageInfo> eiiList = new List<ImageInfo>();
            PdfReader.unethicalreading = true;

            using (var reader = new PdfReader(fileName))
            {
                var parser = new PdfReaderContentParser(reader);
                ImageRenderListener listener = null;

                for (var i = 1; i <= reader.NumberOfPages; i++)
                {
                    parser.ProcessContent(i, (listener = new ImageRenderListener()));
                    if (listener.ImagesInfo.Count > 0)
                    {
                        foreach (ImageInfo imageInfo in listener.ImagesInfo)
                        {
                            imageInfo.pageNum = i;
                            eiiList.Add(imageInfo);
                        }
                    }
                }
                return eiiList;
            }
        }
    }

    internal class ImageRenderListener : IRenderListener
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public List<ImageInfo> ImagesInfo { get; } = new List<ImageInfo>();


        public void BeginTextBlock() { }

        public void EndTextBlock() { }

        public void RenderText(TextRenderInfo renderInfo) { }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
            PdfImageObject image = renderInfo.GetImage();
            ImageInfo eii = new ImageInfo();
            Matrix m = renderInfo.GetImageCTM();

            try
            {
                Image drawingImage = image.GetDrawingImage();
                double hPoints = m[0];
                double vPoints = m[4];

                //72 Points = 1 inch so...
                double widthInches = hPoints / 72;
                double heightInches = vPoints / 72;
                double hDPI = drawingImage.Width / widthInches;
                double vDPI = drawingImage.Height / heightInches;

                eii.hDPI = Math.Round(hDPI);
                eii.vDPI = Math.Round(vDPI);
                eii.width = drawingImage.Width;
                eii.height = drawingImage.Height;
                eii.pixelFormat = drawingImage.PixelFormat;
            }
            catch (Exception e)
            {
                //It was not possible to extract image with image.GetDrawingImage();
                //Don't throw exception to continue parsing the document
                log.Warn(e.Message, e);
            }
            ImagesInfo.Add(eii);
        }
    }

    /// <summary>
    /// Class containing all the metadata extracted from image
    /// </summary>
    public class ImageInfo
    {
        public double hDPI;
        public double vDPI;
        public int height;
        public int width;
        public PixelFormat pixelFormat;
        public int pageNum;
    }
}
