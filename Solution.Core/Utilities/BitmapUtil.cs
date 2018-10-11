using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using log4net;


namespace Solution.Core.Utilities
{
    /// <summary>
    /// Utility to resize and crop BMP image format
    /// </summary>
    public static class BitmapUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Resize an image if it's over the maxWitdth (proportional: yes, grainy: no)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="maxWidth"></param>
        /// <returns></returns>
        public static Bitmap ResizeMaxWidth(Bitmap src, int maxWidth)
        {
            // original dimensions
            int w = src.Width;
            int h = src.Height;

            if (w > maxWidth)
            {
                // longest and shortest dimension
                int longestDimension = (w > h) ? w : h;
                int shortestDimension = (w < h) ? w : h;

                // calculate newHeight
                double newHeight;
                // with propotionality factor
                float factor;
                if (w < h)
                {
                    factor = ((float)shortestDimension) / longestDimension;
                    newHeight = maxWidth / factor;
                }
                else
                {
                    factor = ((float)longestDimension) / shortestDimension;
                    newHeight = maxWidth / factor;
                }

                // Create new Bitmap at new dimensions
                Bitmap result = new Bitmap((int)maxWidth, (int)newHeight);
                using (Graphics g = Graphics.FromImage((System.Drawing.Image)result))
                    g.DrawImage(src, 0, 0, (int)maxWidth, (int)newHeight);

                return result;
            }
            else
            {
                return src;
            }
        }

        /// <summary>
        /// Resize an image to the newWidth (proportional: yes, grainy: yes)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="newWidth"></param>
        /// <returns></returns>
        public static Bitmap ResizeFixedWidth(Bitmap src, int newWidth)
        {
            // original dimensions
            int w = src.Width;
            int h = src.Height;

            // longest and shortest dimension
            int longestDimension = (w > h) ? w : h;
            int shortestDimension = (w < h) ? w : h;

            // calculate newHeight
            double newHeight;
            // with propotionality factor
            float factor;
            if (w < h)
            {
                factor = ((float)shortestDimension) / longestDimension;
                newHeight = newWidth / factor;
            }
            else
            {
                factor = ((float)longestDimension) / shortestDimension;
                newHeight = newWidth / factor;
            }

            // Create new Bitmap at new dimensions
            Bitmap result = new Bitmap((int)newWidth, (int)newHeight);
            using (Graphics g = Graphics.FromImage((System.Drawing.Image)result))
                g.DrawImage(src, 0, 0, (int)newWidth, (int)newHeight);

            return result;
        }

        /// <summary>
        /// Resize an image if it's over the maxHeight (proportional: yes, grainy: no)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        public static Bitmap ResizeMaxHeight(Bitmap src, int maxHeight)
        {
            // original dimensions
            int w = src.Width;
            int h = src.Height;

            if (h > maxHeight)
            {
                // longest and shortest dimension
                int longestDimension = (w > h) ? w : h;
                int shortestDimension = (w < h) ? w : h;

                // calculate newWidth
                double newWidth;
                // with propotionality factor
                float factor;
                if (w > h)
                {
                    factor = ((float)shortestDimension) / longestDimension;
                    newWidth = maxHeight / factor;
                }
                else
                {
                    factor = ((float)longestDimension) / shortestDimension;
                    newWidth = maxHeight / factor;
                }

                // Create new Bitmap at new dimensions
                Bitmap result = new Bitmap((int)newWidth, (int)maxHeight);
                using (Graphics g = Graphics.FromImage((System.Drawing.Image)result))
                    g.DrawImage(src, 0, 0, (int)newWidth, (int)maxHeight);

                return result;
            }
            else
            {
                return src;
            }
        }

        /// <summary>
        /// Resize an image to the newHeight (proportional: yes, grainy: yes)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        public static Bitmap ResizeFixedHeight(Bitmap src, int newHeight)
        {
            // original dimensions
            int w = src.Width;
            int h = src.Height;

            // longest and shortest dimension
            int longestDimension = (w > h) ? w : h;
            int shortestDimension = (w < h) ? w : h;

            // calculate newHeight
            double newWidth;
            // with propotionality factor
            float factor;
            if (w > h)
            {
                factor = ((float)shortestDimension) / longestDimension;
                newWidth = newHeight / factor;
            }
            else
            {
                factor = ((float)longestDimension) / shortestDimension;
                newWidth = newHeight / factor;
            }

            // Create new Bitmap at new dimensions
            Bitmap result = new Bitmap((int)newWidth, (int)newHeight);
            using (Graphics g = Graphics.FromImage((System.Drawing.Image)result))
                g.DrawImage(src, 0, 0, (int)newWidth, (int)newHeight);

            return result;
        }

        /// <summary>
        /// Stretch an image to a provided box size (proportional: yes, grainy: yes)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="boxWidth"></param>
        /// <param name="boxHeight"></param>
        /// <returns></returns>
        public static Bitmap ResizeBoxOverflow(Bitmap src, int boxWidth, int boxHeight)
        {
            Bitmap result = ResizeFixedWidth(src, boxWidth);
            if (result.Height >= boxHeight)
            {
                return result;
            }
            else
            {
                return ResizeFixedHeight(src, boxHeight);
            }
        }

        /// <summary>
        /// Reduce an image to a provided box size (proportional: yes, grainy: no)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="boxWidth"></param>
        /// <param name="boxHeight"></param>
        /// <returns></returns>
        public static Bitmap ResizeBoxContained(Bitmap src, int boxWidth, int boxHeight)
        {
            Bitmap result = ResizeMaxWidth(src, boxWidth);
            if (result.Height <= boxHeight)
            {
                return result;
            }
            else
            {
                return ResizeMaxHeight(src, boxHeight);
            }
        }

        /// <summary>
        /// Stretch an image to a new size (proportional: no, grainy: yes)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        public static Bitmap ResizeStretched(Bitmap src, int newWidth, int newHeight)
        {
            if (src.Width == newWidth && src.Height == newHeight)
            {
                return src;
            }
            else
            {
                // Create new Bitmap at new dimensions
                Bitmap result = new Bitmap(newWidth, newHeight);
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(src, 0, 0, newWidth, newHeight);
                }
                return result;
            }
        }

        /// <summary>
        /// Crop an image to specified size from start point
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="startPointX"></param>
        /// <param name="startPointY"></param>
        /// <param name="cropWidth"></param>
        /// <param name="cropHeight"></param>
        /// <returns></returns>
        public static Bitmap Crop(Bitmap bitmap, int startPointX, int startPointY, int cropWidth, int cropHeight)
        {
            //define cropping area
            Rectangle rect = new Rectangle(startPointX, startPointY, cropWidth, cropHeight);

            //clone cropping area
            Bitmap cropped = bitmap.Clone(rect, bitmap.PixelFormat);

            return cropped;
        }

        /// <summary>
        /// Crop an image to specified size starting from top left corner
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="cropWidth"></param>
        /// <param name="cropHeight"></param>
        /// <returns></returns>
        public static Bitmap CropFromTopLeft(Bitmap bitmap, int cropWidth, int cropHeight)
        {
            return Crop(bitmap, 0, 0, cropWidth, cropHeight);
        }

        /// <summary>
        /// Crop image to a square size starting from top left corner
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Bitmap CropSquare(Bitmap bitmap)
        {
            // original dimensions
            int w = bitmap.Width;
            int h = bitmap.Height;

            // find shortest dimension
            int shortestDimension = (w < h) ? w : h;

            return Crop(bitmap, 0, 0, shortestDimension, shortestDimension);
        }

        /// <summary>
        /// Resize an image to be fitted in specified box and crop it if something is outside 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="boxWidth"></param>
        /// <param name="boxHeight"></param>
        /// <returns></returns>
        public static Bitmap ResizeAndCrop(Bitmap bitmap, int boxWidth, int boxHeight)
        {
            Bitmap resized = ResizeBoxOverflow(bitmap, boxWidth, boxHeight);
            Bitmap cropped = CropFromTopLeft(resized, boxWidth, boxHeight);
            return cropped;
        }

    }
}
