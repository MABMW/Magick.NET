﻿// Copyright 2013-2020 Dirk Lemstra <https://github.com/dlemstra/Magick.NET/>
//
// Licensed under the ImageMagick License (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
//
//   https://www.imagemagick.org/script/license.php
//
// Unless required by applicable law or agreed to in writing, software distributed under the
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System.IO;
using System.Linq;
using ImageMagick;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Magick.NET.Tests
{
    // TODO: Move methods to another class
    [TestClass]
    public partial class ExifProfileTests
    {
        [TestMethod]
        public void Test_Fraction()
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                double exposureTime = 1.0 / 1600;

                using (var image = new MagickImage(Files.FujiFilmFinePixS1ProJPG))
                {
                    var profile = image.GetExifProfile();

                    profile.SetValue(ExifTag.ExposureTime, new Rational(exposureTime));
                    image.SetProfile(profile);

                    image.Write(memStream);
                }

                memStream.Position = 0;
                using (var image = new MagickImage(memStream))
                {
                    var profile = image.GetExifProfile();

                    Assert.IsNotNull(profile);

                    var value = profile.GetValue(ExifTag.ExposureTime);
                    Assert.IsNotNull(value);
                    Assert.AreNotEqual(exposureTime, value.Value.ToDouble());
                }

                memStream.Position = 0;
                using (var image = new MagickImage(Files.FujiFilmFinePixS1ProJPG))
                {
                    var profile = image.GetExifProfile();

                    profile.SetValue(ExifTag.ExposureTime, new Rational(exposureTime, true));
                    image.SetProfile(profile);

                    image.Write(memStream);
                }

                memStream.Position = 0;
                using (var image = new MagickImage(memStream))
                {
                    var profile = image.GetExifProfile();

                    Assert.IsNotNull(profile);

                    var value = profile.GetValue(ExifTag.ExposureTime);
                    Assert.AreEqual(exposureTime, value.Value.ToDouble());
                }
            }
        }

        [TestMethod]
        public void Test_Infinity()
        {
            using (var image = new MagickImage(Files.FujiFilmFinePixS1ProJPG))
            {
                var profile = image.GetExifProfile();
                profile.SetValue(ExifTag.ExposureBiasValue, new SignedRational(double.PositiveInfinity));
                image.SetProfile(profile);

                profile = image.GetExifProfile();
                var value = profile.GetValue(ExifTag.ExposureBiasValue);
                Assert.IsNotNull(value);
                Assert.AreEqual(double.PositiveInfinity, value.Value.ToDouble());

                profile.SetValue(ExifTag.ExposureBiasValue, new SignedRational(double.NegativeInfinity));
                image.SetProfile(profile);

                profile = image.GetExifProfile();
                value = profile.GetValue(ExifTag.ExposureBiasValue);
                Assert.IsNotNull(value);
                Assert.AreEqual(double.NegativeInfinity, value.Value.ToDouble());

                profile.SetValue(ExifTag.FlashEnergy, new Rational(double.NegativeInfinity));
                image.SetProfile(profile);

                profile = image.GetExifProfile();
                var flashValue = profile.GetValue(ExifTag.FlashEnergy);
                Assert.IsNotNull(flashValue);
                Assert.AreEqual(double.PositiveInfinity, flashValue.Value.ToDouble());
            }
        }

        [TestMethod]
        public void Test_Values()
        {
            using (var image = new MagickImage(Files.FujiFilmFinePixS1ProJPG))
            {
                var profile = image.GetExifProfile();
                TestExifProfile(profile);

                using (var emptyImage = new MagickImage(Files.ImageMagickJPG))
                {
                    Assert.IsNull(emptyImage.GetExifProfile());
                    emptyImage.SetProfile(profile);

                    profile = emptyImage.GetExifProfile();
                    TestExifProfile(profile);
                }
            }
        }

        [TestMethod]
        public void Test_ExifTypeUndefined()
        {
            using (var image = new MagickImage(Files.ExifUndefTypeJPG))
            {
                var profile = image.GetExifProfile();
                Assert.IsNotNull(profile);

                foreach (var value in profile.Values)
                {
                    if (value.DataType == ExifDataType.Undefined)
                    {
                        byte[] data = (byte[])value.GetValue();
                        Assert.AreEqual(4, data.Length);
                    }
                }
            }
        }

        private static void TestExifProfile(IExifProfile profile)
        {
            Assert.IsNotNull(profile);

            Assert.AreEqual(44, profile.Values.Count());

            foreach (var value in profile.Values)
            {
                Assert.IsNotNull(value.GetValue());

                if (value.Tag == ExifTag.Software)
                    Assert.AreEqual("Adobe Photoshop 7.0", value.ToString());

                if (value.Tag == ExifTag.XResolution)
                    Assert.AreEqual(new Rational(300, 1), (Rational)value.GetValue());

                if (value.Tag == ExifTag.GPSLatitude)
                {
                    Rational[] pos = (Rational[])value.GetValue();
                    Assert.AreEqual(54, pos[0].ToDouble());
                    Assert.AreEqual(59.38, pos[1].ToDouble());
                    Assert.AreEqual(0, pos[2].ToDouble());
                }

                if (value.Tag == ExifTag.ShutterSpeedValue)
                    Assert.AreEqual(9.5, ((SignedRational)value.GetValue()).ToDouble());
            }
        }
    }
}
