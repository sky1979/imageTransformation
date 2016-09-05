using System.Diagnostics;
using System.Drawing;
using NUnit.Framework;
using PhotoDemonTransform;

namespace PhotoDemonTransformTests
{
    [TestFixture]
    public class TransformTests
    {
        [Test]
        public void Can_transform_Image()
        {
            var distortion = new PhotoDemonImageTransformation();

            var myimage = (Bitmap)Image.FromFile("darth.png");

            double x0 = 218.875;
            double y0 = 153.375;
            double x1 = 334.625;
            double y1 = 148.375;
            double x2 = 332.75;
            double y2 = 264.5;
            double x3 = 220.75;
            double y3 = 277.75;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            var imageresult = distortion.Transform(myimage, x0, x1, x2, x3, y0, y1, y2, y3);

            watch.Stop();
            Debug.WriteLine("Time:" + watch.Elapsed.TotalSeconds.ToString());

            imageresult.Save("darth_distort.jpg");
        }
    }
}