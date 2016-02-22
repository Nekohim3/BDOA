using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Image = System.Windows.Controls.Image;

namespace BDOA
{
    internal class AItem
    {
        internal Grid G;
        internal Image ImgL;
        internal Image ImgX;
        internal Sbmp Img;
        internal Sbmp ImgS;
        internal bool _un;
        internal AItem(Sbmp b, int Width)
        {
            Img = b;
            ImgS = b.Crop(new Rectangle(13, 12, 150, 37));
            G = new Grid
            {
                Width = Width,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            ImgL = new Image
            {
                Width = 200,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Source = ImgHelpers.BitmapToImageSource(Img.GetBmp()),
                Margin = new Thickness(0, 0, 0, 0)
            };
            ImgX = new Image
            {
                Width = 46,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Source = ImgHelpers.BitmapToImageSource(Properties.Resources.del),
                Margin = new Thickness(0, 0, 0, 0)
            };
            RenderOptions.SetBitmapScalingMode(ImgL, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(ImgX, BitmapScalingMode.HighQuality);
            G.Children.Add(ImgL);
            G.Children.Add(ImgX);
        }
        internal AItem()
        {
            _un = true;
            Img = new Sbmp(Properties.Resources.unk);
            ImgS = Img.Crop(new Rectangle(13, 12, 37, 37));
            G = new Grid
            {
                Width = 246,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            ImgL = new Image
            {
                Width = 200,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Source = ImgHelpers.BitmapToImageSource(Img.GetBmp()),
                Margin = new Thickness(0, 0, 0, 0)
            };
            ImgX = new Image
            {
                Width = 46,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Source = ImgHelpers.BitmapToImageSource(Properties.Resources.del),
                Margin = new Thickness(200, 0, 0, 0)
            };
            RenderOptions.SetBitmapScalingMode(ImgL, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(ImgX, BitmapScalingMode.HighQuality);
            G.Children.Add(ImgL);
            G.Children.Add(ImgX);
        }
    }
}
