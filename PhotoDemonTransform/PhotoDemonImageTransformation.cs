using System;
using System.Diagnostics;
using System.Drawing;

namespace PhotoDemonTransform
{
    /*********
     * Salvador Silva: 05/Sept/2016
     This code was based on the project "PhotoDemon" (http://photodemon.org/) The source code I used is at: https://github.com/tannerhelland/PhotoDemon
     I implemented the algorythim it uses to disort an image using the edges 4 points.
     I translated it from VB6 to C#.
     This is a first draft, there is much improvement to be done, that's why I wanted to opensource it.
     I have found other implementations on C# but none of them were fast or with high quality.
     I used ImageMagick (https://magick.codeplex.com/) in c# when I needed to do this sort of 4 points distort with perspective but I didn't like the fact that I had all the overhead, that's where I found "PhotoDemon" and I loved the project and the distort algorythim.
     I hope this helps somebody else.
     
     
    ********/
    public class PhotoDemonImageTransformation
    {
        public Image Transform(Bitmap image, double x0, double x1, double x2, double x3, double y0, double y1, double y2, double y3)
        {
            
            ImageUtils utils = new ImageUtils();

            FilterSupport filterSuport = new FilterSupport();

            var data = ImageUtils.GetByteArray(image); 

            byte[,] dstImageData = data;


            byte[,] srcImageData = ImageUtils.GetByteArray(image); 

            long initX = 0;
            long initY = 0;
            long x = 0;
            long y = 0;
            long finalX = image.Width - 1;
            long finalY = image.Height - 1;


            long QuickVal = 0;
            long qvDepth = 0;
            qvDepth = 3; //TODO: MOVERLO

            bool correctiveProjection = false; //TODO: TEST

            bool interpolate = true; //TODO: TEST

            filterSuport.setDistortParameters(qvDepth, EDGE_OPERATOR.EDGE_ERASE, interpolate, finalX, finalY);
            /*
            
            '***************************************
            '  BEGIN SUPERSAMPLING PREPARATION 
            */

            long superSamplingAmount = 0;
            superSamplingAmount = 5;

            long newR = 0;
            long newG = 0;
            long newB = 0;
            long newA = 0;
            long r = 0;
            long g = 0;
            long b = 0;
            long a = 0;
            long tmpSum = 0;
            long tmpSumFirst = 0;

            long numSamples = 0;
            double[] ssX = null;
            double[] ssY = null;

            getSupersamplingTable(superSamplingAmount, ref numSamples, ref ssX, ref ssY);

            numSamples = numSamples - 1;

            double j = 0;
            double k = 0;
            //long sampleIndex = 0;
            long numSamplesUsed = 0;
            long superSampleVerify = 0;
            long ssVerificationLimit = 0;

            superSampleVerify = superSamplingAmount - 2;

            ssVerificationLimit = superSampleVerify * 6;

            if (superSampleVerify <= 0)
            {
                superSampleVerify = 1112121212121212121; //LONG_MAX
            }
            /*            
               END SUPERSAMPLING PREPARATIO
            '*************************************/

            double imgWidth = 0;
            double imgHeight = 0;
            imgWidth = finalX - initX;
            imgHeight = finalY - initY;

            double invWidth = 0;
            double invHeight = 0;
            invWidth = 1 / imgWidth;
            invHeight = 1 / imgHeight;          

            x0 = x0 * invWidth;
            y0 = y0 * invHeight;
            x1 = x1 * invWidth;
            y1 = y1 * invHeight;
            x2 = x2 * invWidth;
            y2 = y2 * invHeight;
            x3 = x3 * invWidth;
            y3 = y3 * invHeight;


            double dx1 = 0;
            double dy1 = 0;
            double dx2 = 0;
            double dy2 = 0;
            double dx3 = 0;
            double dy3 = 0;
            dx1 = x1 - x2;
            dy1 = y1 - y2;
            dx2 = x3 - x2;
            dy2 = y3 - y2;
            dx3 = x0 - x1 + x2 - x3;
            dy3 = y0 - y1 + y2 - y3;

            double h11 = 0;
            double h21 = 0;
            double h31 = 0;
            double h12 = 0;
            double h22 = 0;
            double h32 = 0;
            double h13 = 0;
            double h23 = 0;
            double h33 = 0;


            //Certain values can lead to divide-by-zero problems - check those in advance and convert 0 to something like 0.000001
            double chkDenom = 0;
            chkDenom = (dx1 * dy2 - dy1 * dx2);
            if (chkDenom == 0)
                chkDenom = 1E-09;


            h13 = (dx3 * dy2 - dx2 * dy3) / chkDenom;
            h23 = (dx1 * dy3 - dy1 * dx3) / chkDenom;
            h11 = x1 - x0 + h13 * x1;
            h21 = x3 - x0 + h23 * x3;
            h31 = x0;
            h12 = y1 - y0 + h13 * y1;
            h22 = y3 - y0 + h23 * y3;
            h32 = y0;
            h33 = 1;


            //Next, we need to calculate the key set of transformation parameters, using the reverse-map data we just generated.
            // Again, these are technically just matrix entries, but we get better performance by declaring them individually.
            double hA = 0;
            double hB = 0;
            double hC = 0;
            double hD = 0;
            double hE = 0;
            double hF = 0;
            double hG = 0;
            double hH = 0;
            double hI = 0;


            hA = h22 * h33 - h32 * h23;
            hB = h31 * h23 - h21 * h33;
            hC = h21 * h32 - h31 * h22;
            hD = h32 * h13 - h12 * h33;
            hE = h11 * h33 - h31 * h13;
            hF = h31 * h12 - h11 * h32;
            hG = h12 * h23 - h22 * h13;
            hH = h21 * h13 - h11 * h23;
            hI = h11 * h22 - h21 * h12;

            hA = hA * invWidth;
            hD = hD * invWidth;
            hG = hG * invWidth;
            hB = hB * invHeight;
            hE = hE * invHeight;
            hH = hH * invHeight;


            //With all that data calculated in advanced, the actual transform is quite simple.


            //Source X and Y values, which may or may not be used as part of a bilinear interpolation function
            double srcX = 0;
            double srcY = 0;


            double newX = 0;
            double newY = 0;


            //Loop through each pixel in the image, converting values as we go
            try
            {
                for (x = initX; x <= finalX; x++)
                {
                    QuickVal = x * qvDepth;

                    for (y = initY; y <= finalY; y++)
                    {
                        //Reset all supersampling values

                        newR = 0;
                        newG = 0;
                        newB = 0;
                        newA = 0;
                        numSamplesUsed = 0;

                        //'Sample a number of source pixels corresponding to the user's supplied quality value; more quality means
                        //' more samples, and much better representation in the final output.
                        for (var sampleIndex = 0; sampleIndex <= numSamples; sampleIndex++)
                        {
                            //Pull coordinates from the lookup table
                            newX = x + ssX[sampleIndex];
                            newY = y + ssY[sampleIndex];


                            //Reverse-map the coordinates back onto the original image (to allow for resampling)
                            chkDenom = (hG * newX + hH * newY + hI);
                            if (chkDenom == 0)
                                chkDenom = 1E-09;


                            srcX = imgWidth * (hA * newX + hB * newY + hC) / chkDenom;
                            srcY = imgHeight * (hD * newX + hE * newY + hF) / chkDenom;

                            //Use the filter support class to interpolate and edge-wrap pixels as necessary
                            filterSuport.getColorsFromSource(ref r, ref g, ref b, ref a, ref srcX, ref srcY, ref srcImageData, ref x, ref y);

                            if (sampleIndex == superSampleVerify)
                            {

                                //Calculate variance for the first two pixels (Q3), three pixels (Q4), or four pixels (Q5)
                                tmpSum = (r + g + b + a) * superSampleVerify;
                                tmpSumFirst = newR + newG + newB + newA;


                                //If variance is below 1.5 per channel per pixel, abort further supersampling
                                if (Math.Abs(tmpSum - tmpSumFirst) < ssVerificationLimit)
                                    break; // TODO: might not be correct. Was : Exit For

                            }

                            numSamplesUsed = numSamplesUsed + 1;


                            //Add the retrieved values to our running averages
                            newR = newR + r;
                            newG = newG + g;
                            newB = newB + b;
                            if (qvDepth == 4)
                                newA = newA + a;

                        }

                        newR = newR / numSamplesUsed;
                        newG = newG / numSamplesUsed;
                        newB = newB / numSamplesUsed;

                        dstImageData[QuickVal + 2, y] = (byte)newR;
                        dstImageData[QuickVal + 1, y] = (byte)newG;
                        dstImageData[QuickVal, y] = (byte)newB;
                        
                        if (qvDepth == 4)
                        {
                            newA = newA / numSamplesUsed;
                            dstImageData[QuickVal + 3, y] = (byte)newA;
                        }
                    } //Next y

                }//NEXT X
            }
            catch (Exception ex)
            {
                Debug.WriteLine("X " + x);
                Debug.WriteLine("Y " + y);
            }

            //return ImageUtils.CreateImageFromArrayFast(image.Height, image.Width, dstImageData);
            return ImageUtils.CreateImageFromArray(image.Height, image.Width, ImageUtils.RotateMatrix(dstImageData, image.Height, image.Width * 3));

        }

        public void getSupersamplingTable(long userQuality, ref long numAASamples, ref double[] ssOffsetsX, ref double[] ssOffsetsY)
        {

            //Old PD versions used a Boolean value for quality.  As such, if the user enabled interpolation, and saved it as part of a preset,
            // this function may get passed a "-1" for userQuality.  In that case, activate an identical method in the new supersampler.
            if (userQuality < 1)
                userQuality = 2;

            //Quality is typically presented to the user on a 1-5 scale.  1 = lowest quality/highest speed, 5 = highest quality/lowest speed.
            switch (userQuality)
            {

                //Quality settings of 1 and 2 both suspend supersampling.  The only difference is that the calling function, per PD convention,
                // will disable antialising.
                case 1:
                case 2:

                    numAASamples = 1;
                    ssOffsetsX = new double[1];
                    ssOffsetsY = new double[1];

                    ssOffsetsX[0] = 0;
                    ssOffsetsY[0] = 0;
                    break;

                //Cases 3, 4, 5: use rotated grid supersampling, at the recommended rotation of arctan(1/2), with 4 additional sample points
                // per quality level.
                default:

                    //Four additional samples are provided at each quality level
                    numAASamples = (userQuality - 2) * 4 + 1;
                    ssOffsetsX = new double[numAASamples];
                    ssOffsetsY = new double[numAASamples];

                    //The first sample point is always the origin pixel.  This is used as the basis of adaptive supersampling,
                    // and should not be changed.
                    ssOffsetsX[0] = 0;
                    ssOffsetsY[0] = 0;

                    //The other 4 sample points are calculated as follows:
                    // - Rotate (0, 0.5) around (0, 0) by arctan(1/2) radians
                    // - Repeat the above step, but increasing each rotation by 90.
                    ssOffsetsX[1] = 0.447077;
                    ssOffsetsY[1] = 0.22388;

                    ssOffsetsX[2] = -0.447077;
                    ssOffsetsY[2] = -0.22388;

                    ssOffsetsX[3] = -0.22388;
                    ssOffsetsY[3] = 0.447077;

                    ssOffsetsX[4] = 0.22388;
                    ssOffsetsY[4] = -0.447077;

                    //For quality levels 4 and 5, we add a second set of sampling points, closer to the origin, and offset from the originals
                    // by 45 degrees
                    if (userQuality > 3)
                    {

                        ssOffsetsX[5] = 0.0789123;
                        ssOffsetsY[5] = 0.237219;

                        ssOffsetsX[6] = -0.237219;
                        ssOffsetsY[6] = 0.0789123;

                        ssOffsetsX[7] = -0.0789123;
                        ssOffsetsY[7] = -0.237219;

                        ssOffsetsX[8] = 0.237219;
                        ssOffsetsY[8] = -0.0789123;

                        //For the final quality level, add a set of 4 more points, calculated by rotating (0, 0.67) around the
                        // origin in 45 degree increments.  The benefits of this are minimal for all but the most extreme
                        // zoom-out situations.
                        if (userQuality > 4)
                        {

                            ssOffsetsX[9] = 0.473762;
                            ssOffsetsY[9] = 0.473762;

                            ssOffsetsX[10] = -0.473762;
                            ssOffsetsY[10] = 0.473762;

                            ssOffsetsX[11] = -0.473762;
                            ssOffsetsY[11] = -0.473762;

                            ssOffsetsX[12] = 0.473762;
                            ssOffsetsY[12] = -0.473762;

                        }

                    }
                    break;

            }

        }
    }
}